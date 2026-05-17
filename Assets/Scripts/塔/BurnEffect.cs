using UnityEngine;
namespace CHANG
{
    public class BurnEffect : StatusEffect
    {
        private float damagePerSecond;

        public BurnEffect(Enemy enemy, float duration, float dps)
            : base(enemy, duration)
        {
            damagePerSecond = dps;
            //Debug.Log($"🔥 BurnEffect 建立！duration={duration}, dps={dps}"); // ⭐ 加這行
        }
        public override void OnApply()
        {
            enemy.SetEffectColor(new Color(1f, 0.3f, 0f)); // 🔥 橘紅色
        }

        public override void OnExpire()
        {
            enemy.ResetColor(); // 恢復原色
        }
        protected override void OnTick(float dt)
        {
            enemy.TakeDamage(damagePerSecond);
            //Debug.Log($"🔥 Burn Damage: {damagePerSecond}");
        }
    }
}