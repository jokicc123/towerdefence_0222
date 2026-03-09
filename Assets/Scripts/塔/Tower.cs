
using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    public class Tower : CharacterTower
    {
        // 1. 引入數據容器
        [SerializeField] public TowerData data;

        #region 屬性對接 (將原本寫死的數值轉向 Data)
        // 使用 => 讓這些屬性去讀取 ScriptableObject 的數值
        public float attackRange => data != null ? data.attackRange : 5f;
        public float damage => data != null ? data.damage : 10f;
        public float attackSpeed => data != null ? data.attackSpeed : 2f;

        // 子彈與點位
        private GameObject bulletPrefab => data?.bulletPrefab;
        [SerializeField] private Transform firePoint;
        #endregion

        private float attackTimer;
        private float AttackInterval => 1f / attackSpeed;

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
                rangeCollider.radius = attackRange; // 這裡會自動讀到 data 的數值
            }

            Idle = new TowerIdle("待機", stateMachine, this);
            Attack = new TowerAttack("攻擊", stateMachine, this);
            Cooldown = new TowerCooldown("冷卻", stateMachine, this);
            stateMachine.Initialize(Idle);
        }

        // 2. 新增一個初始化方法，讓 Manager 生成後可以注入 Data
        public void Initialize(TowerData towerData)
        {
            this.data = towerData;
            if (rangeCollider != null) rangeCollider.radius = attackRange;
        }

        public bool HasTarget() => enemiesInRange.Count > 0;

        public Enemy GetTarget() => HasTarget() ? enemiesInRange[0] : null;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemiesInRange.Add(enemy);
            }
            
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemiesInRange.Remove(enemy);
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

        public void Fire(Enemy target)
        {
            if (target == null || bulletPrefab == null || firePoint == null) return;

            GameObject bulletObj = Instantiate(bulletPrefab.gameObject, firePoint.position, Quaternion.identity);
            Bullet b = bulletObj.GetComponent<Bullet>();

            if (b != null)
            {
                b.SetTarget(target, damage);
            }
        }

    }
}
    
  



        