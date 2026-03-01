using Unity.VisualScripting;
using UnityEngine;
namespace CHANG 
{
    public abstract class State

    {
        protected string name;
        protected StateMachine stateMachine;

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();

    }
}
