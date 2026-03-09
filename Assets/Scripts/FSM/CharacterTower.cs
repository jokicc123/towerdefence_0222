using UnityEngine;

namespace CHANG
{
    public class CharacterTower : MonoBehaviour

    {
        protected StateMachine stateMachine;


        protected virtual void Awake()
        {
            stateMachine = new StateMachine();
        }
        protected virtual void Update()

        {

            stateMachine?.Update();

        }


    }
}
