using UnityEngine;
namespace CHANG 
{
    public class Tower : CharacterTower
    {
        #region 可調整數值
        [field: Header("防禦塔數值")]
        [field: SerializeField, Range(0, 30)]
        public float attackRange { get; private set; }
        [field: SerializeField, Range(0, 30)]
        public float damage { get; private set; }
        [field: SerializeField, Range(0, 5)]
        public float attackSpeed { get; private set; }
        #endregion
        public float AttackInterval => attackSpeed <= 0 ? 999f : 1f / attackSpeed;

        private float attackTimer;


        #region 狀態機
        public TowerIdle Idle { get; private set; }
        public TowerAttack Attack { get; private set; }
        public TowerCooldown Cooldown { get; private set; }
        #endregion




        protected override void Awake()
        {
            base.Awake();
            Idle = new TowerIdle("待機", stateMachine, this);
            Attack = new TowerAttack("攻擊", stateMachine, this);
            Cooldown = new TowerCooldown("冷卻", stateMachine, this);
            stateMachine.Initialize(Idle);
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

    }
}
