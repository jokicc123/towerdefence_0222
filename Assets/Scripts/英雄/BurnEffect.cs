using UnityEngine;

namespace CHANG
{
    public class BurnEffect : StatusEffect
    {
        private readonly Enemy burnTarget;
        private readonly float damagePerSecond;

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

        public override void OnApply()
        {
            if (burnTarget == null)
                return;

            // 燒傷顯示：讓敵人變成橘紅色
            burnTarget.SetEffectColor(
                new Color(1f, 0.3f, 0f)
            );
        }

        protected override void OnTick(float deltaTime)
        {
            if (burnTarget == null)
                return;

            float finalDamagePerSecond =
                damagePerSecond *
                BurnDamageSystem.CurrentMultiplier;

            // DPS 必須乘上 deltaTime
            burnTarget.TakeDamage(
                finalDamagePerSecond *
                deltaTime
            );
        }

        public override void OnExpire()
        {
            if (burnTarget == null)
                return;

            // 燒傷結束後恢復顏色
            burnTarget.ResetColor();
        }
    }
}