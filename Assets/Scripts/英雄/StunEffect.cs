using UnityEngine;

namespace CHANG
{
    public class StunEffect : StatusEffect
    {
        private readonly Enemy stunTarget;

        public StunEffect(
            Enemy target,
            float duration)
            : base(target, duration)
        {
            stunTarget = target;
        }

        public override void OnApply()
        {
            if (stunTarget != null)
            {
                stunTarget.AddStun();
            }
        }

        public override void OnExpire()
        {
            if (stunTarget != null)
            {
                stunTarget.RemoveStun();
            }
        }

        protected override void OnTick(float deltaTime)
        {
            // 暈眩只需要等待時間結束，
            // 不需要每幀造成傷害。
        }
    }
}