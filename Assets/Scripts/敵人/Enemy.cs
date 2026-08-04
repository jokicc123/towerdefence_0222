using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        public EnemyData Data => data;
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
        // ⭐ 效果系統
        private List<StatusEffect> effects = new List<StatusEffect>();

        // 減速倍率
        private float slowMultiplier = 1f;
        private float speedBuffMultiplier = 1f;
        private float damageBuffMultiplier = 1f;
        private float damageTakenMultiplier = 1f;
        // ⭐ 暈眩計數（可支援多個暈眩）
        private int stunCount;

        // 是否處於暈眩
        public bool IsStunned => stunCount > 0;

        // 真正移動速度
        public float MoveSpeed
        {
            get
            {
                // 暈眩時完全不能移動
                if (IsStunned)
                    return 0f;

                return data != null
                    ? data.moveSpeed * slowMultiplier
                    : 3f;
            }
        }
            public void ApplySpeedBuff(float multiplier)
        {
            speedBuffMultiplier *= Mathf.Max(multiplier, 0.01f);
        }

        public void RemoveSpeedBuff(float multiplier)
        {
            if (multiplier <= 0f)
                return;

            speedBuffMultiplier /= multiplier;
        }

        public void ApplyDamageBuff(float multiplier)
        {
            damageBuffMultiplier *= Mathf.Max(multiplier, 0.01f);
        }

        public void RemoveDamageBuff(float multiplier)
        {
            if (multiplier <= 0f)
                return;

            damageBuffMultiplier /= multiplier;
        }

        public void ApplyDefenseBuff(float multiplier)
        {
            damageTakenMultiplier *= Mathf.Max(multiplier, 0.01f);
        }

        public void RemoveDefenseBuff(float multiplier)
        {
            if (multiplier <= 0f)
                return;

            damageTakenMultiplier /= multiplier;
        }
        

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
            if (isDead)
                return;

            // 狀態效果必須持續倒數
            TickEffects();

            if (targetPoint == null)
                return;

            Move();
        }

        // ⭐ 新增效果

        public void AddEffect(StatusEffect effect)
        {
            if (effect == null)
                return;

            // 同類型效果重新施放時，先移除舊效果
            StatusEffect existing =
                effects.Find(e =>
                    e != null &&
                    e.GetType() == effect.GetType()
                );

            if (existing != null)
            {
                existing.OnExpire();
                effects.Remove(existing);
            }

            effect.OnApply();
            effects.Add(effect);
        }

        private void TickEffects()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effects[i];

                if (effect == null)
                {
                    effects.RemoveAt(i);
                    continue;
                }

                effect.Tick(Time.deltaTime);

                if (effect.IsExpired)
                {
                    effect.OnExpire();
                    effects.RemoveAt(i);
                }
            }
        }
        // ⭐ 減速
       public void ApplySlow(float percent)
{
    percent = Mathf.Clamp01(percent);
    slowMultiplier = 1f - percent;
}
        public void RemoveSlow()
        {
            slowMultiplier = 1f;
        }

        // ⭐ 暈眩
        public void AddStun()
        {
            stunCount++;
        }

        public void RemoveStun()
        {
            stunCount = Mathf.Max(0, stunCount - 1);
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
            if (isDead)
                return;

            float finalDamage =
                amount * damageTakenMultiplier;

            currentHealth -= finalDamage;

            if (currentHealth <= 0f)
            {
                KillByTower();
            }
        }

        public void KillByTower()
        {
            isDead = true;
            GameManager.Instance.AddGold(data.goldReward);
            HeroManager.Instance.OnEnemyKilled(data.xpReward);
            Destroy(gameObject);
        }

        public void ReachGoal()
        {
            int finalDamage = Mathf.RoundToInt(
                data.damage * damageBuffMultiplier
            );

            GameManager.Instance.TakeDamege(finalDamage);
            Destroy(gameObject);
        }
    }
}