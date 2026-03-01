using UnityEngine;

namespace CHANG
{
    public class TowerCooldown : StateTower
    {
        public TowerCooldown(string name, StateMachine stateMachine, Tower tower) : base(name, stateMachine, tower)
        {

        }

        public override void Enter()
        {
            base.Enter();
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
