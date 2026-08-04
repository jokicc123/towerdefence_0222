using System.Collections.Generic;
using UnityEngine;
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
        public float attackSpeed => CurrenData.attackSpeed * attackSpeedMultiplier;
        public float cost => CurrenData.cost;
        public TowerEffectType EffectType => CurrenData.effectType; // ⭐給英雄被動光環判斷屬性用
        public float damage =>
            CurrenData.damage *
            damageMultiplier *
            ShopBonus.TowerDamageMultiplier;
        #endregion

        private GameObject bulletPrefab => CurrenData.bulletPrefab;
        public Transform FirePoint { get; private set; }
        public Transform Head { get; private set; }

        // ⭐ 攻擊計時
        private float attackTimer;
        private float AttackInterval => 1f / Mathf.Max(attackSpeed, 0.01f);

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
            if (towerData == null)
            {
                Debug.LogError($"{name} 初始化失敗：TowerData 為空");
                enabled = false;
                return;
            }

            if (towerData.levels == null ||
                towerData.levels.Length == 0)
            {
                Debug.LogError(
                    $"{towerData.name} 沒有設定任何塔等級資料"
                );

                enabled = false;
                return;
            }

            Debug.Log($"Initialize 被呼叫於 {gameObject.name}");

            data = towerData;
            currentLevel = 0;

            // 初始化時重置所有倍率
            damageMultiplier = 1f;
            attackSpeedMultiplier = 1f;
            rangeMultiplier = 1f;

            ApplyLevel();
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
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameRunning())
            {
                return;
            }

            if (data == null ||
                data.levels == null ||
                data.levels.Length == 0)
            {
                return;
            }

            base.Update();

            UpdateEnemiesInRange();

            if (HasTarget() && stateMachine.currentState == Idle)
            {
                stateMachine.ChangeState(Attack);
            }
            else if (!HasTarget() &&
                     stateMachine.currentState == Attack)
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

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                attackRange
            );

            HashSet<Enemy> foundEnemies = new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                // 防止同一個敵人有多個 Collider 而重複加入
                if (foundEnemies.Add(enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }

            // 依照距離排序，優先攻擊最近的敵人
            enemiesInRange.Sort((a, b) =>
            {
                float distanceA =
                    (a.transform.position - transform.position).sqrMagnitude;

                float distanceB =
                    (b.transform.position - transform.position).sqrMagnitude;

                return distanceA.CompareTo(distanceB);
            });
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
            if (target == null)
                return;

            if (bulletPrefab == null || FirePoint == null)
            {
                Debug.LogWarning(
                    $"{name} 無法攻擊，缺少 Bullet Prefab 或 FirePoint"
                );

                return;
            }
            if (SoundManager.Instance != null &&
                    data.attackSFX != null)
            {
                SoundManager.Instance.PlaySFX(
                    data.attackSFX
                );
            }

            Vector3 direction =
                target.transform.position - FirePoint.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                FirePoint.rotation =
                    Quaternion.LookRotation(direction.normalized);
            }

            GameObject bulletObj = Instantiate(
                bulletPrefab,
                FirePoint.position,
                FirePoint.rotation
            );

            if (bulletObj.TryGetComponent(out Bullet bullet))
            {
                bullet.SetTarget(
                    target,
                    damage,
                    CurrenData.effectType,
                    CurrenData.effectDuration,
                    CurrenData.effectDamagePerSecond,
                    CurrenData.blastRadius
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{bulletObj.name} 沒有 Bullet 腳本"
                );

                Destroy(bulletObj);
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
        public void ApplyBuff(
       PassiveBuffType buffType,
       float multiplier)
        {
            if (multiplier <= 0f)
            {
                Debug.LogWarning($"{name} 收到無效 Buff 倍率：{multiplier}");
                return;
            }

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

            Debug.Log(
                $"{name} 套用 {buffType} Buff，倍率：{multiplier}，" +
                $"目前傷害：{damage:0.0}，攻速：{attackSpeed:0.00}，射程：{attackRange:0.0}"
            );
        }

        public void RemoveBuff(
            PassiveBuffType buffType,
            float multiplier)
        {
            if (multiplier <= 0f)
            {
                Debug.LogWarning($"{name} 收到無效 Buff 倍率：{multiplier}");
                return;
            }

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

            Debug.Log(
                $"{name} 移除 {buffType} Buff，" +
                $"目前傷害：{damage:0.0}，攻速：{attackSpeed:0.00}，射程：{attackRange:0.0}"
            );
        }
        private void RefreshRangeCollider()
        {
            if (rangeCollider != null)
            {
                rangeCollider.isTrigger = true;
                rangeCollider.radius = attackRange;
            }

            if (rangeCircle != null)
            {
                rangeCircle.DrawCircle(attackRange);
            }
        }
        #region 外觀系統    
        private void UpdateModel()
        {
            if (modelRoot == null)
            {
                Debug.LogWarning($"{name} 沒有設定 Model Root");
                return;
            }

            if (data == null ||
                data.levelModelPrefabs == null ||
                data.levelModelPrefabs.Length == 0)
            {
                Debug.LogWarning($"{name} 沒有設定等級模型 Prefab");
                return;
            }

            if (currentLevel < 0 ||
                currentLevel >= data.levelModelPrefabs.Length)
            {
                Debug.LogError(
                    $"{name} 找不到第 {currentLevel + 1} 級模型，" +
                    $"模型陣列只有 {data.levelModelPrefabs.Length} 個"
                );

                return;
            }

            for (int i = modelRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(modelRoot.GetChild(i).gameObject);
            }

            GameObject prefab =
                data.levelModelPrefabs[currentLevel];

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"{name} 第 {currentLevel + 1} 級模型 Prefab 為空"
                );

                return;
            }

            currentModel = Instantiate(prefab, modelRoot);

            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
            currentModel.transform.localScale = Vector3.one;

            TowerModelRef modelRef =
                currentModel.GetComponentInChildren<TowerModelRef>();

            if (modelRef != null)
            {
                FirePoint = modelRef.FirePoint;
                Head = modelRef.Head;
            }
            else
            {
                FirePoint = null;
                Head = null;

                Debug.LogWarning(
                    $"{currentModel.name} 找不到 TowerModelRef",
                    currentModel
                );
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
        // ⭐ 賣塔取得回收價格（70%）    
        public int GetSellPrice()
        {
            int totalCost = 0;

            for (int i = 0; i <= currentLevel; i++)
            {
                totalCost += data.levels[i].cost;
            }

            // 回收 70%
            return Mathf.RoundToInt(totalCost * 0.7f);
        }
        [ContextMenu("測試塔傷害加成")]
        private void TestTowerDamageBonus()
        {
            Debug.Log(
                $"塔基礎傷害：{CurrenData.damage}\n" +
                $"英雄光環倍率：{damageMultiplier}\n" +
                $"商店倍率：{ShopBonus.TowerDamageMultiplier}\n" +
                $"最終傷害：{damage}"
            );
        }
    }
}