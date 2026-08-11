namespace CHANG
{
    /// <summary>
    /// 防禦塔狀態的共同基底類別。
    /// </summary>
    public class StateTower : State
    {
        #region 執行期間資料

        protected readonly Tower tower;

        #endregion

        #region 建構式

        public StateTower(
            string name,
            StateMachine stateMachine,
            Tower tower)
            : base(
                name,
                stateMachine
            )
        {
            this.tower = tower;
        }

        #endregion
    }
}