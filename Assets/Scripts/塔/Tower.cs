using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static CHANG.TowerData;

namespace CHANG
{
    public class Tower : CharacterTower
    {
        #region 資料
        [Header("資料")]
        [SerializeField] public TowerData data;

        // ⭐ 等級
        public int currentLevel = 0;

        // ⭐ 當前等級資料
        private TowerLevel CurrenData => data.levels[currentLevel];
        #endregion

        // ============================================================
        // ⭐【英雄系統新增】被動光環倍率
        // 不快取原始數值，改用倍率相乘，這樣塔升級中途被buff也不會出錯，
        // 且天然支援多重buff疊加（連乘）。
        // ============================================================
        #region 被動光環倍率
        private float damageMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private float rangeMultiplier = 1f;
        #endregion

        #region 屬性（全部改成吃等級 + 被動光環倍率）
        public float attackRange => CurrenData.attackRange * rangeMultiplier;
        public float damage => CurrenData.damage * damageMultiplier;
        public float attackSpeed => CurrenData.attackSpeed * attackSpeedMultiplier;
        public float cost => CurrenData.cost;
        public TowerEffectType EffectType => CurrenData.effectType; // ⭐給英雄被動光環判斷屬性用
        #endregion

        private GameObject bulletPrefab => CurrenData.bulletPrefab;
        public Transform FirePoint { get; private set; }
        public Transform Head { get; private set; }

        // ⭐ 攻擊計時
        private float attackTimer;
        private float AttackInterval => 1f / attackSpeed;

        // ⭐ 模型（外觀）
        [Header("模型")]
        [SerializeField] private Transform modelRoot;
        private GameObject currentModel;

        // ✨【新增】範圍圓圈組件連結
        [Header("範圍顯示")]
        [SerializeField] private RangeCircle rangeCircle;

        #region 狀態機
        public TowerIdle Idle { get; private set; }
        public TowerAttack Attack { get; private set; }
        public TowerCooldown Cooldown { get; private set; }
        #endregion

        private List<Enemy> enemiesInRange = new List<Enemy>();
        private SphereCollider rangeCollider;

        protected override void Awake()
        {
            base.Awake();

            rangeCollider = GetComponent<SphereCollider>();
            if (rangeCollider != null)
            {
                rangeCollider.isTrigger = true;
            }

            Idle = new TowerIdle("待機", stateMachine, this);
            Attack = new TowerAttack("攻擊", stateMachine, this);
            Cooldown = new TowerCooldown("冷卻", stateMachine, this);
            stateMachine.Initialize(Idle);
        }

        // ⭐ 初始化（由 TowerManager 呼召）
        public void Initialize(TowerData towerData)
        {
            Debug.Log($"Initialize 被呼叫於 {gameObject.name}");
            data = towerData;
            currentLevel = 0;          // 初始等級
            ApplyLevel();       // ⭐ 套用數值

            // ✨【新增】初始化時預設把範圍圈隱藏
            HideRangeCircle();
        }

        // ⭐ 套用等級（核心）
        private void ApplyLevel()
        {
            // 更新範圍碰撞器
            if (rangeCollider != null)
            {
                rangeCollider.isTrigger = true; // 確保一定是 Trigger
                rangeCollider.radius = attackRange;
            }

            // ✨【新增】讓 UI 圓圈動態去畫出當前等級的攻擊範圍大小
            if (rangeCircle != null)
            {
                rangeCircle.DrawCircle(attackRange);
                rangeCircle.SetColor(new Color(0f, 1f, 0f, 0.4f)); // 預設給牠半透明綠色
            }

            // 重置攻擊節奏
            attackTimer = 0f;

            // 更新外觀
            UpdateModel();

            Debug.Log($"套用等級 {currentLevel + 1}");
        }

        public bool CanUpgrade()
        {
            return currentLevel < data.levels.Length - 1;
        }

        // ⭐ 取得升級費用（下一級）
        public int GetUpgradeCost()
        {
            if (!CanUpgrade()) return 0;
            return data.levels[currentLevel + 1].cost;
        }

        // ⭐ 升級
        public void Upgrade()
        {
            // ⭐ 遊戲不在進行中時，禁止升級
            if (GameManager.Instance != null && !GameManager.Instance.IsGameRunning())
            {
                Debug.Log("遊戲已結束，無法升級");
                return;
            }

            if (currentLevel >= data.levels.Length - 1)
            {
                Debug.Log("已滿級");
                return;
            }

            int upgradeCost = data.levels[currentLevel + 1].cost;

            if (!GameManager.Instance.SpendGold(upgradeCost))
            {
                Debug.Log("金幣不足");
                return;
            }

            currentLevel++;
            ApplyLevel();

            Debug.Log($"升級完成 → 等級 {currentLevel + 1}");
        }
        public override void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.CanBuildTower())
                return;   // 遊戲結束就不再更新攻擊邏輯

            base.Update();
            UpdateEnemiesInRange();

            if (HasTarget() && stateMachine.currentState == Idle)
            {
                stateMachine.ChangeState(Attack);
            }
            else if (!HasTarget() && stateMachine.currentState == Attack)
            {
                stateMachine.ChangeState(Idle);
            }
        }

        #region 攻擊邏輯
        public bool HasTarget()
        {
            enemiesInRange.RemoveAll(e => e == null);
            return enemiesInRange.Count > 0;
        }

        public Enemy GetTarget() => HasTarget() ? enemiesInRange[0] : null;

        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Enemy enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        public void ResetAttackTimer()
        {
            attackTimer = AttackInterval;
        }

        public void TickAttackTimer()
        {
            attackTimer -= Time.deltaTime;
        }

        public bool IsAttackReady()
        {
            return attackTimer <= 0f;
        }

        public virtual void Fire(Enemy target)
        {
            if (bulletPrefab == null || FirePoint == null)
            {
                Debug.LogWarning("塔無法攻擊");
                return;
            }

            GameObject bulletObj = Instantiate(bulletPrefab, FirePoint.position, FirePoint.rotation);
            if (bulletObj.TryGetComponent(out Bullet b))
            {
                b.SetTarget(
                    target,
                    damage,
                    CurrenData.effectType,
                    CurrenData.effectDuration,
                    CurrenData.effectDamagePerSecond,
                    CurrenData.blastRadius  // ⭐ 告訴子彈是否為 AoE
                );
            }
        }

        protected void ApplyEffect(Enemy enemy)
        {
            switch (CurrenData.effectType)
            {
                case TowerEffectType.Burn:
                    enemy.AddEffect(new BurnEffect(enemy, CurrenData.effectDuration, CurrenData.effectDamagePerSecond));
                    break;
                case TowerEffectType.Poison:
                    enemy.AddEffect(new PoisonEffect(enemy, CurrenData.effectDuration, CurrenData.effectDamagePerSecond, CurrenData.slowPercent));
                    break;
            }
        }
        #endregion

        // ============================================================
        // ⭐【英雄系統新增】被動光環套用/移除
        // ============================================================
        #region 被動光環（英雄系統）
        public void ApplyBuff(PassiveBuffType buffType, float multiplier)
        {
            switch (buffType)
            {
                case PassiveBuffType.Damage:
                    damageMultiplier *= multiplier;
                    break;
                case PassiveBuffType.AttackSpeed:
                    attackSpeedMultiplier *= multiplier;
                    break;
                case PassiveBuffType.Range:
                    rangeMultiplier *= multiplier;
                    RefreshRangeCollider();
                    break;
            }
        }

        public void RemoveBuff(PassiveBuffType buffType, float multiplier)
        {
            switch (buffType)
            {
                case PassiveBuffType.Damage:
                    damageMultiplier /= multiplier;
                    break;
                case PassiveBuffType.AttackSpeed:
                    attackSpeedMultiplier /= multiplier;
                    break;
                case PassiveBuffType.Range:
                    rangeMultiplier /= multiplier;
                    RefreshRangeCollider();
                    break;
            }
        }

        private void RefreshRangeCollider()
        {
            if (rangeCollider != null)
                rangeCollider.radius = attackRange;

            if (rangeCircle != null)
                rangeCircle.DrawCircle(attackRange);
        }
        #endregion

        #region 外觀系統    
        private void UpdateModel()
        {
            if (modelRoot == null || data.levelModelPrefabs == null) return;

            // ⭐ 直接清掉所有舊模型（最重要）
            for (int i = modelRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(modelRoot.GetChild(i).gameObject);
            }

            GameObject prefab = data.levelModelPrefabs[currentLevel];

            if (prefab == null)
            {
                Debug.LogWarning("沒有模型 prefab");
                return;
            }

            currentModel = Instantiate(prefab, modelRoot);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            if (currentModel.TryGetComponent(out TowerModelRef modelRef))
            {
                FirePoint = modelRef.FirePoint;
                Head = modelRef.Head;
            }
        }
        #endregion

        #region 範圍顯示控制
        // ✨【新增】給外部（如 TowerManager 或點擊事件）呼叫的開關方法
        public void ShowRangeCircle()
        {
            if (rangeCircle != null)
            {
                rangeCircle.gameObject.SetActive(true);
            }
        }

        public void HideRangeCircle()
        {
            if (rangeCircle != null)
            {
                rangeCircle.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}