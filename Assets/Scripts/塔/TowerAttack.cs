using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 防禦塔攻擊狀態。
    /// 負責鎖定目標、旋轉塔頭與執行攻擊。
    /// </summary>
    public class TowerAttack : StateTower
    {
        #region 建構式

        public TowerAttack(
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
            {
                stateMachine.ChangeState(
                    tower.Idle
                );

                return;
            }

            Enemy target =
                tower.GetTarget();

            if (target == null ||
                target.IsDead)
            {
                stateMachine.ChangeState(
                    tower.Idle
                );

                return;
            }

            RotateHeadToTarget(
                target
            );

            tower.TickAttackTimer();

            if (!tower.IsAttackReady())
                return;

            tower.Fire(
                target
            );

            tower.ResetAttackTimer();
        }

        #endregion

        #region 旋轉

        private void RotateHeadToTarget(
            Enemy target)
        {
            if (tower.Head == null ||
                target == null ||
                target.IsDead)
            {
                return;
            }

            Vector3 direction =
                target.transform.position -
                tower.Head.position;

            if (direction.sqrMagnitude <=
                0.001f)
            {
                return;
            }

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction
                );

            tower.Head.rotation =
                Quaternion.Lerp(
                    tower.Head.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
        }

        #endregion
    }
}