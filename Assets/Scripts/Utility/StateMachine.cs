namespace CHANG
{
    /// <summary>
    /// 狀態機。
    /// 負責初始化狀態、切換狀態與更新目前狀態。
    /// </summary>
    public class StateMachine
    {
        #region 執行期間資料

        public State CurrentState
        {
            get;
            private set;
        }

        #endregion

        #region 初始化

        public void Initialize(
            State startingState)
        {
            if (startingState == null)
                return;

            CurrentState =
                startingState;

            CurrentState.Enter();
        }

        #endregion

        #region 狀態切換

        public void ChangeState(
            State newState)
        {
            if (newState == null)
                return;

            if (CurrentState == newState)
                return;

            CurrentState?.Exit();

            CurrentState =
                newState;

            CurrentState.Enter();
        }

        #endregion

        #region 狀態更新

        public void Update()
        {
            CurrentState?.Update();
        }

        #endregion
    }
}