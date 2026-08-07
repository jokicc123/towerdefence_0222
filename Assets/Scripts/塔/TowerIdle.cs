namespace CHANG
{
    public class TowerIdle : StateTower
    {
        #region 建構式

        public TowerIdle(
            string name,
            StateMachine stateMachine,
            Tower tower)
            : base(
                name,
                stateMachine,
                tower
            )
        {
        }

        #endregion

        #region 狀態更新

        public override void Update()
        {
            if (!tower.HasTarget())
                return;

            stateMachine.ChangeState(
                tower.Attack
            );
        }

        #endregion
    }
}