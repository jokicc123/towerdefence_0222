namespace CHANG
{
    /// <summary>
    /// 所有狀態的基底類別。
    /// </summary>
    public abstract class State
    {
        #region 狀態資料

        public string Name
        {
            get;
            protected set;
        }

        protected readonly StateMachine stateMachine;

        #endregion

        #region 建構式

        protected State(
            string name,
            StateMachine stateMachine)
        {
            Name = name;
            this.stateMachine = stateMachine;
        }

        #endregion

        #region 狀態生命週期

        public virtual void Enter()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Exit()
        {
        }

        #endregion
    }
}