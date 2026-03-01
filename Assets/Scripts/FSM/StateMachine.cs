using UnityEngine;

namespace CHANG
{
    public class StateMachine
    {
        public State currentState;
        /// <summary>
        /// 初始化狀態
        /// </summary>
        /// <param name="startingState"></param>
        public void Initialize(State startingState)
        {
            currentState = startingState;
            currentState?.Enter();
        }
        /// <summary>
        /// 變更狀態
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(State newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }


        public void Update()
        {
            currentState?.Update();

        }
    }
}
