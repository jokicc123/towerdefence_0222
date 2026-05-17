using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        private float currentHealth;
        private bool isDead = false;
        private Renderer[] renderers;
        private Color originalColor;

        // --- 路徑相關 ---
        private Transform targetPoint;
        private int wavePointIndex = 0;
        Rigidbody rb;

        // ⭐ 效果系統
        private List<StatusEffect> effects = new List<StatusEffect>();
        private float slowMultiplier = 1f;

        public float MoveSpeed => data != null ? data.moveSpeed * slowMultiplier : 3f;

        private void Awake()
        {
            if (data != null) currentHealth = data.maxHealth;
            rb = GetComponent<Rigidbody>();
            renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                originalColor = renderers[0].material.color;
            }
        }
        public void SetEffectColor(Color color)
        {
            foreach(var r in renderers)
            {
                r.material.color = color;
            }
        }

        public void ResetColor()
        {
            foreach(var r in renderers)
            {
                r.material.color = originalColor;
            }
        }
        private void Start()
        {
            if (Waypoints.Points == null || Waypoints.Points.Length == 0)
            {
                Waypoints wp = Object.FindFirstObjectByType<Waypoints>();
                if (wp != null) wp.InitializePoints();
            }

            if (Waypoints.Points != null && Waypoints.Points.Length > 0)
            {
                targetPoint = Waypoints.Points[0];
            }
            else
            {
                Debug.LogError("場景中找不到任何路徑點！");
            }
        }

        private void Update()
        {
            if (targetPoint == null) return;
            Move();
            TickEffects(); // ⭐ 每幀更新效果
        }

        // ⭐ 新增效果
        [SerializeField] private EnemyStatusUI statusUI;
        public void AddEffect(StatusEffect effect)
        {
            var existing = effects.Find(e => e.GetType() == effect.GetType());
            if (existing != null)
            {
                existing.OnExpire();
                effects.Remove(existing);
            }
            effect.OnApply();
            effects.Add(effect);
            RefreshStatusUI();
        }

        // ⭐ 每幀 Tick
        private void TickEffects()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Tick(Time.deltaTime);
                if (effects[i].IsExpired)
                {
                    effects[i].OnExpire();
                    effects.RemoveAt(i);
                    RefreshStatusUI();
                }
            }
        }
        private void RefreshStatusUI()
        {
            if (statusUI == null) return;
            statusUI.SetBurn(effects.Exists(e => e is BurnEffect));
            statusUI.SetPoison(effects.Exists(e => e is PoisonEffect));
        }

        // ⭐ 減速
        public void ApplySlow(float percent)
        {
            slowMultiplier = 1f - percent;
        }

        public void RemoveSlow()
        {
            slowMultiplier = 1f;
        }

        private void Move()
        {
            if (targetPoint == null) return;

            Vector3 direction =
                (targetPoint.position - transform.position).normalized;

            // ⭐ 只轉 Y 軸
            direction.y = 0;

            // ⭐ 面向移動方向
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    10f * Time.deltaTime
                );
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                MoveSpeed * Time.deltaTime
            );

            Vector3 flatPosition =
                new Vector3(transform.position.x, 0, transform.position.z);

            Vector3 flatTarget =
                new Vector3(targetPoint.position.x, 0, targetPoint.position.z);

            if (Vector3.Distance(flatPosition, flatTarget) <= 0.2f)
            {
                GetNextWaypoint();
            }
        }
        private void GetNextWaypoint()
        {
            if (wavePointIndex >= Waypoints.Points.Length - 1)
            {
                GameManager.Instance.TakeDamege(data.damage);
                ReachGoal();
                return;
            }
            wavePointIndex++;
            targetPoint = Waypoints.Points[wavePointIndex];
         
        }


        

        public void TakeDamage(float amount)
        {
            if (isDead) return;
            currentHealth -= amount;
            if (currentHealth <= 0) KillByTower();
        }

        public void KillByTower()
        {
            isDead = true;
            GameManager.Instance.AddGold(data.goldReward);
            Destroy(gameObject);
        }

        public void ReachGoal()
        {
            GameManager.Instance.TakeDamege(data.damage);
            Destroy(gameObject);
        }
    }
}