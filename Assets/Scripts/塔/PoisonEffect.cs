using UnityEngine;

namespace CHANG
{
    public class PoisonEffect : StatusEffect
    {
        private float damagePerSecond;
        private float slowPercent;

        public PoisonEffect(
            Enemy enemy,
            float duration,
            float damagePerSecond,
            float slowPercent
        ) : base(enemy, duration)
        {
            this.damagePerSecond = damagePerSecond;
            this.slowPercent = slowPercent;
        }

        public override void OnApply()
        {
            enemy.ApplySlow(slowPercent);
            enemy.SetEffectColor(new Color(0.2f, 0.8f, 0.2f)); // 🟢 綠色
            Debug.Log("☠️ 中毒開始");
        }

        protected override void OnTick(float dt)
        {
            float damage = damagePerSecond * dt;

            enemy.TakeDamage(damage);
            Debug.Log($"☠️ Poison Damage: {damage}");
        }

        public override void OnExpire()
        {
            enemy.RemoveSlow();
            enemy.ResetColor(); // 恢復原色
            Debug.Log("☠️ 中毒結束");
        }
    }
}