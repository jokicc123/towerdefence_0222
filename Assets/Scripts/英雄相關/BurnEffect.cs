using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 燃燒狀態效果。
    /// 持續對敵人造成傷害，並受到 BurnDamageSystem 的全域倍率影響。
    /// </summary>
    public class BurnEffect : StatusEffect
    {
        #region 執行期間資料

        private readonly Enemy burnTarget;
        private readonly float damagePerSecond;

        #endregion

        #region 常數

        private static readonly Color BurnColor =
            new Color(1f, 0.3f, 0f);

        #endregion

        #region 建構函式

        public BurnEffect(
            Enemy target,
            float duration,
            float damagePerSecond)
            : base(target, duration)
        {
            burnTarget = target;

            this.damagePerSecond =
                Mathf.Max(0f, damagePerSecond);
        }

        #endregion

        #region 狀態效果生命週期

        public override void OnApply()
        {
            if (burnTarget == null)
                return;

            burnTarget.SetEffectColor(
                BurnColor
            );
        }

        protected override void OnTick(float deltaTime)
        {
            if (burnTarget == null)
                return;

            float finalDamagePerSecond =
                damagePerSecond *
                BurnDamageSystem.CurrentMultiplier;

            burnTarget.TakeDamage(
                finalDamagePerSecond *
                deltaTime
            );
        }

        public override void OnExpire()
        {
            if (burnTarget == null)
                return;

            burnTarget.ResetColor();
        }

        #endregion
    }
}