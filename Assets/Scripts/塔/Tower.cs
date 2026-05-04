using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CHANG
{
    public class Tower : CharacterTower
    {
        [Header("資料")]
        [SerializeField] public TowerData data;

        // ⭐ 等級
        public int currentLevel = 0;

        // ⭐ 當前等級資料
        private TowerLevel CurrenData => data.levels[currentLevel];

        #region 屬性（全部改成吃等級）
        public float attackRange => CurrenData.attackRange;
        public float damage => CurrenData.damage;
        public float attackSpeed => CurrenData.attackSpeed;
        public float cost => CurrenData.cost;

        private GameObject bulletPrefab => CurrenData.bulletPrefab;

        [SerializeField] private Transform firePoint;
        public Transform FirePoint => firePoint;

        [SerializeField] private Transform head;
        public Transform Head => head;
        #endregion

        // ⭐ 攻擊計時
        private float attackTimer;
        private float AttackInterval => 1f / attackSpeed;

        // ⭐ 模型（外觀）
        [Header("模型")]
        [SerializeField] private Transform modelRoot;
        private GameObject currentModel;

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

        // ⭐ 初始化（由 TowerManager 呼叫）
        public void Initialize(TowerData towerData)
        {
            data = towerData;

            currentLevel = 0;          // 初始等級
            ApplyLevel();       // ⭐ 套用數值
        }

        // ⭐ 套用等級（核心）
        private void ApplyLevel()
        {
            // 更新範圍碰撞器
            if (rangeCollider != null)
            {
                rangeCollider.radius = attackRange;
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
            base.Update();

            UpdateEnemiesInRange();

            if (HasTarget())
            {
                if (IsAttackReady())
                {
                    Fire(GetTarget());
                    ResetAttackTimer();
                }

                TickAttackTimer();
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
            if (target == null || bulletPrefab == null || firePoint == null)
            {
                Debug.LogWarning("塔無法攻擊");
                return;
            }

            GameObject bulletObj = Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            if (bulletObj.TryGetComponent(out Bullet b))
            {
                b.SetTarget(target, damage); // ⭐ 用當前等級 damage
            }
        }
        #endregion

        #region 外觀系統
        private void UpdateModel()
        {
            if (modelRoot == null || data.levelModelPrefabs == null) return;

            if (currentModel != null)
                Destroy(currentModel);

            if (currentLevel >= data.levelModelPrefabs.Length)
            {
                Debug.LogWarning("沒有對應等級模型");
                return;
            }

            currentModel = Instantiate(
                data.levelModelPrefabs[currentLevel],
                modelRoot.position,
                modelRoot.rotation,
                modelRoot
            );
        }
        #endregion


    }
}