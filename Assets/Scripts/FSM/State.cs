using Unity.VisualScripting;
using UnityEngine;
namespace CHANG 
{
    public abstract class State

    {
        public string Name { get; protected set; }
        protected StateMachine stateMachine;

        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();

    }
}
