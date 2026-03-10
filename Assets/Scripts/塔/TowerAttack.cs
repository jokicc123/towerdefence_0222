using UnityEngine;

namespace CHANG
{
    public class TowerAttack : StateTower
    {
        public TowerAttack(string name, StateMachine stateMachine, Tower tower) : base(name, stateMachine, tower)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update()
        {
            base.Update();
            Debug.Log("TowerAttack 狀態運行中");

            if (!tower.HasTarget())
            {
                stateMachine.ChangeState(tower.Idle);
                return;
            }

            tower.TickAttackTimer();

            if (tower.IsAttackReady())
            {
                Enemy target = tower.GetTarget();

                if (target != null)
                {
                    Debug.Log("塔攻擊");
                    tower.Fire(target);
                    tower.ResetAttackTimer();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}

