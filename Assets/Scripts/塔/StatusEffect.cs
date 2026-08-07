using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 所有敵人狀態效果的基底類別。
    /// 負責持續時間與固定間隔 Tick。
    /// </summary>
    public abstract class StatusEffect
    {
        #region 執行期間資料

        protected readonly Enemy enemy;

        private float timer;
        private float tickTimer;

        private readonly float tickInterval = 1f;

        #endregion

        #region 公開屬性

        public bool IsExpired =>
            timer <= 0f;

        #endregion

        #region 建構式

        protected StatusEffect(
            Enemy enemy,
            float duration)
        {
            this.enemy = enemy;

            timer =
                Mathf.Max(
                    0f,
                    duration
                );
        }

        #endregion

        #region 狀態生命週期

        public virtual void OnApply()
        {
        }

        public virtual void OnExpire()
        {
        }

        protected abstract void OnTick(
            float deltaTime);

        #endregion

        #region 更新流程

        public void Tick(
            float deltaTime)
        {
            if (IsExpired ||
                deltaTime <= 0f)
            {
                return;
            }

            timer -= deltaTime;
            tickTimer += deltaTime;

            while (tickTimer >= tickInterval)
            {
                OnTick(
                    tickInterval
                );

                tickTimer -=
                    tickInterval;
            }
        }

        #endregion
    }
}