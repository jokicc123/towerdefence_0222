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
        private Transform[] myPath;    // ✨ 新增：儲存這隻怪物專屬的路徑點
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

        // ✨ 新增：提供給 Spawner 在生成怪物時，動態塞入專屬路線的方法
        public void InitializePath(Transform[] specificPath)
        {
            myPath = specificPath;
            wavePointIndex = 0;
            if (myPath != null && myPath.Length > 0)
            {
                targetPoint = myPath[0];
            }
        }

        public void SetEffectColor(Color color)
        {
            foreach (var r in renderers)
            {
                r.material.color = color;
            }
        }

        public void ResetColor()
        {
            foreach (var r in renderers)
            {
                r.material.color = originalColor;
            }
        }

        private void Start()
        {
            // ✨ 擴充：如果 Spawner 還沒透過 InitializePath 餵路線進來（相容原本的舊做法）
            if (myPath == null || myPath.Length == 0)
            {
                if (Waypoints.Points == null || Waypoints.Points.Length == 0)
                {
                    Waypoints wp = Object.FindFirstObjectByType<Waypoints>();
                    if (wp != null) wp.InitializePoints();
                }

                if (Waypoints.Points != null && Waypoints.Points.Length > 0)
                {
                    myPath = Waypoints.Points; // 沒設定就用全域預設路線
                }
            }

            // 初始化第一個目標點
            if (myPath != null && myPath.Length > 0)
            {
                targetPoint = myPath[0];
            }
            else
            {
                Debug.LogError($"{gameObject.name} 找不到任何可以前進的路徑點！");
            }
        }

        private void Update()
        {
            if (targetPoint == null) return;
            Move();
            TickEffects(); // ⭐ 每幀更新效果
        }

        // ⭐ 新增效果
        
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
                  
                }
            }
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

            Vector3 direction = (targetPoint.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
            }

            // ⭐ 目標點強制拉平到敵人自己當前的 Y，徹底跟 waypoint 高度脫鉤
            Vector3 flatTargetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);

            transform.position = Vector3.MoveTowards(
                transform.position,
                flatTargetPos,
                MoveSpeed * Time.deltaTime
            );

            Vector3 flatPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(targetPoint.position.x, 0, targetPoint.position.z);

            if (Vector3.Distance(flatPosition, flatTarget) <= 0.2f)
            {
                GetNextWaypoint();
            }
        }

        private void GetNextWaypoint()
        {
            // ✨ 修改：改用我自己的路徑 myPath 來做長度判定
            if (myPath == null || myPath.Length == 0) return;

            if (wavePointIndex >= myPath.Length - 1)
            {
                
               
                // 調整後：直接交給 ReachGoal 處理即可
                ReachGoal();
                return;
            }
            wavePointIndex++;
            targetPoint = myPath[wavePointIndex];
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