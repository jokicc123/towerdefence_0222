using UnityEngine;

namespace CHANG
{
    public class CharacterHero : MonoBehaviour
    {
        protected StateMachine stateMachine;

        protected virtual void Awake()
        {
            stateMachine = new StateMachine();
        }

        public virtual void Update()
        {
            stateMachine?.Update();
        }
    }
}
