using UnityEngine;
namespace CHANG
{
    public abstract class StatusEffect
    {
        protected Enemy enemy;
        private float timer;
        private float tickTimer;
        private float tickInterval = 1f;

        public bool IsExpired => timer <= 0f;

        public StatusEffect(Enemy enemy, float duration)
        {
            this.enemy = enemy;
            timer = duration;
        }
        public virtual void OnApply() { }
        public virtual void OnExpire() { }

        public void Tick(float dt)
        {
            timer -= dt;

            tickTimer += dt;


            if (tickTimer >= tickInterval)
            {
                OnTick(tickInterval);
                tickTimer = 0f;
            }
        }

        protected abstract void OnTick(float dt);
    }
}