using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 所有可使用狀態機角色的共同基底。
    /// </summary>
    public abstract class Character : MonoBehaviour
    {
        #region 狀態機

        protected StateMachine stateMachine;

        #endregion

        #region Unity 生命週期

        protected virtual void Awake()
        {
            stateMachine = new StateMachine();
        }

        protected virtual void Update()
        {
            stateMachine?.Update();
        }

        #endregion
    }

}