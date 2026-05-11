using UnityEngine;
namespace CHANG 
{
    public class StateTower : State
    {
        protected Tower tower;

        public StateTower(string name, StateMachine stateMachine, Tower tower)
        {
            this.Name = name;
            this.tower = tower;
            this.stateMachine = stateMachine;


        }


        public override void Enter()
        {
            //Log.Text($": {tower.name}進入{name}", "#6f6");

        }

        public override void Exit()
        {
        

        }   



        public override void Update()
        {


        }

    }
}
