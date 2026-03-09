using UnityEngine;

namespace CHANG
{
    public class TowerAttack : StateTower
    {
        public TowerAttack(string name, StateMachine stateMachine, Tower tower) : base(name, stateMachine, tower)
        {

        }

        // TowerAttack.cs
        public override void Enter()
        {
            base.Enter();
            Enemy target = tower.GetTarget();

            if (target != null)
            {
                tower.Fire(target); // 傳入 target 物件，不要加 .transform
                tower.ResetAttackTimer();
            }
            else
            {
                stateMachine.ChangeState(tower.Idle);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
