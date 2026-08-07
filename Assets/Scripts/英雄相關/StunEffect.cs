using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 暈眩狀態效果。
    /// 持續期間內使敵人無法移動。
    /// </summary>
    public class StunEffect : StatusEffect
    {
        #region 執行期間資料

        private readonly Enemy stunTarget;

        #endregion

        #region 建構函式

        public StunEffect(
            Enemy target,
            float duration)
            : base(target, duration)
        {
            stunTarget = target;
        }

        #endregion

        #region 狀態效果生命週期

        public override void OnApply()
        {
            if (stunTarget == null)
                return;

            stunTarget.AddStun();
        }

        protected override void OnTick(float deltaTime)
        {
            // 暈眩效果不需要每幀更新。
        }

        public override void OnExpire()
        {
            if (stunTarget == null)
                return;

            stunTarget.RemoveStun();
        }

        #endregion
    }
}