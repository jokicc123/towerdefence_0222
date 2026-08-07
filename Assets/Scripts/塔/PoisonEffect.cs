using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 中毒效果。
    /// 持續造成傷害並降低移動速度。
    /// </summary>
    public class PoisonEffect : StatusEffect
    {
        #region 欄位

        private readonly float damagePerSecond;
        private readonly float slowPercent;

        #endregion

        #region 建構式

        public PoisonEffect(
            Enemy enemy,
            float duration,
            float damagePerSecond,
            float slowPercent)
            : base(enemy, duration)
        {
            this.damagePerSecond =
                Mathf.Max(0f, damagePerSecond);

            this.slowPercent =
                Mathf.Clamp01(slowPercent);
        }

        #endregion

        #region 狀態效果

        public override void OnApply()
        {
            if (enemy == null)
                return;

            enemy.ApplySlow(
                slowPercent
            );

            enemy.SetEffectColor(
                new Color(
                    0.2f,
                    0.8f,
                    0.2f
                )
            );

#if UNITY_EDITOR
            Debug.Log(
                "中毒開始"
            );
#endif
        }

        protected override void OnTick(
            float deltaTime)
        {
            if (enemy == null)
                return;

            enemy.TakeDamage(
                damagePerSecond *
                deltaTime
            );

#if UNITY_EDITOR
            Debug.Log(
                $"Poison Damage：" +
                $"{damagePerSecond * deltaTime:0.0}"
            );
#endif
        }

        public override void OnExpire()
        {
            if (enemy == null)
                return;

            enemy.RemoveSlow();

            enemy.ResetColor();

#if UNITY_EDITOR
            Debug.Log(
                "中毒結束"
            );
#endif
        }

        #endregion
    }
}