using UnityEngine;

namespace CHANG
{
    public class StunEffect : StatusEffect
    {
        private readonly Enemy stunTarget;

        public StunEffect(Enemy target, float duration) : base(target, duration)
        {
            stunTarget = target; // 自己存一份，避免依賴基底類別的欄位名稱
        }

        public override void OnApply()
        {
            // 暈眩 = 減速100%，借用現有的減速系統，不用另外做移動鎖定
            stunTarget.ApplySlow(1f);
        }

        public override void OnExpire()
        {
            stunTarget.RemoveSlow();
        }

        protected override void OnTick(float deltaTime)
        {
            // 暈眩不需要每幀邏輯，留空即可
        }
    }
}