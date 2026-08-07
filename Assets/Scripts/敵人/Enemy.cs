using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理敵人的生命、移動、路徑、狀態效果與 Buff。
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        #region Inspector 設定

        [SerializeField] private EnemyData data;

        #endregion

        #region 屬性

        public EnemyData Data => data;

        public float CurrentHealth =>
            currentHealth;

        public bool IsDead =>
            isDead;

        public bool IsStunned =>
            stunCount > 0;

        public float MoveSpeed
        {
            get
            {
                if (IsStunned)
                    return 0f;

                if (data == null)
                    return 3f;

                return data.moveSpeed *
                       slowMultiplier *
                       speedBuffMultiplier;
            }
        }

        #endregion

        #region 執行期間資料

        private float currentHealth;
        private bool isDead;

        private Renderer[] renderers;
        private Color[] originalColors;

        private readonly List<StatusEffect> effects =
            new();

        private float slowMultiplier = 1f;
        private float speedBuffMultiplier = 1f;
        private float damageBuffMultiplier = 1f;
        private float damageTakenMultiplier = 1f;

        private int stunCount;

        #endregion

        #region 路徑資料

        private Transform[] myPath;
        private Transform targetPoint;
        private int wavePointIndex;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (data != null)
            {
                currentHealth =
                    data.maxHealth;
            }

            CacheRendererColors();
        }

        private void Start()
        {
            InitializeDefaultPath();
        }

        private void Update()
        {
            if (isDead)
                return;

            TickEffects();

            if (targetPoint == null)
                return;

            Move();
        }

        #endregion

        #region 路徑系統

        public void InitializePath(
            Transform[] specificPath)
        {
            myPath = specificPath;
            wavePointIndex = 0;

            if (myPath != null &&
                myPath.Length > 0)
            {
                targetPoint = myPath[0];
            }
        }

        private void InitializeDefaultPath()
        {
            if (myPath == null ||
                myPath.Length == 0)
            {
                if (Waypoints.Points == null ||
                    Waypoints.Points.Length == 0)
                {
                    Waypoints waypoints =
                        FindFirstObjectByType<Waypoints>();

                    waypoints?.InitializePoints();
                }

                if (Waypoints.Points != null &&
                    Waypoints.Points.Length > 0)
                {
                    myPath =
                        Waypoints.Points;
                }
            }

            if (myPath != null &&
                myPath.Length > 0)
            {
                targetPoint =
                    myPath[0];
            }
            else
            {
                Debug.LogError(
                    $"{name} 找不到任何可以前進的路徑點！",
                    this
                );
            }
        }

        private void GetNextWaypoint()
        {
            if (myPath == null ||
                myPath.Length == 0)
            {
                return;
            }

            if (wavePointIndex >=
                myPath.Length - 1)
            {
                ReachGoal();
                return;
            }

            wavePointIndex++;
            targetPoint =
                myPath[wavePointIndex];
        }

        #endregion

        #region 移動系統

        private void Move()
        {
            if (targetPoint == null)
                return;

            Vector3 direction =
                targetPoint.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction.normalized
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        10f * Time.deltaTime
                    );
            }

            Vector3 flatTargetPosition =
                new Vector3(
                    targetPoint.position.x,
                    transform.position.y,
                    targetPoint.position.z
                );

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    flatTargetPosition,
                    MoveSpeed * Time.deltaTime
                );

            Vector3 flatPosition =
                new Vector3(
                    transform.position.x,
                    0f,
                    transform.position.z
                );

            Vector3 flatTarget =
                new Vector3(
                    targetPoint.position.x,
                    0f,
                    targetPoint.position.z
                );

            if ((flatPosition - flatTarget)
                .sqrMagnitude <= 0.04f)
            {
                GetNextWaypoint();
            }
        }

        #endregion

        #region 狀態效果

        public void AddEffect(
            StatusEffect effect)
        {
            if (effect == null)
                return;

            StatusEffect existing =
                effects.Find(e =>
                    e != null &&
                    e.GetType() ==
                    effect.GetType()
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
            for (int i =
                     effects.Count - 1;
                 i >= 0;
                 i--)
            {
                StatusEffect effect =
                    effects[i];

                if (effect == null)
                {
                    effects.RemoveAt(i);
                    continue;
                }

                effect.Tick(
                    Time.deltaTime
                );

                if (!effect.IsExpired)
                    continue;

                effect.OnExpire();
                effects.RemoveAt(i);
            }
        }

        public void ApplySlow(float percent)
        {
            percent =
                Mathf.Clamp01(percent);

            slowMultiplier =
                1f - percent;
        }

        public void RemoveSlow()
        {
            slowMultiplier = 1f;
        }

        public void AddStun()
        {
            stunCount++;
        }

        public void RemoveStun()
        {
            stunCount =
                Mathf.Max(
                    0,
                    stunCount - 1
                );
        }

        #endregion

        #region Buff 系統

        public void ApplySpeedBuff(
            float multiplier)
        {
            speedBuffMultiplier *=
                Mathf.Max(
                    multiplier,
                    0.01f
                );
        }

        public void RemoveSpeedBuff(
            float multiplier)
        {
            if (multiplier <= 0f)
                return;

            speedBuffMultiplier =
                Mathf.Max(
                    0.01f,
                    speedBuffMultiplier /
                    multiplier
                );
        }

        public void ApplyDamageBuff(
            float multiplier)
        {
            damageBuffMultiplier *=
                Mathf.Max(
                    multiplier,
                    0.01f
                );
        }

        public void RemoveDamageBuff(
            float multiplier)
        {
            if (multiplier <= 0f)
                return;

            damageBuffMultiplier =
                Mathf.Max(
                    0.01f,
                    damageBuffMultiplier /
                    multiplier
                );
        }

        public void ApplyDefenseBuff(
            float multiplier)
        {
            damageTakenMultiplier *=
                Mathf.Max(
                    multiplier,
                    0.01f
                );
        }

        public void RemoveDefenseBuff(
            float multiplier)
        {
            if (multiplier <= 0f)
                return;

            damageTakenMultiplier =
                Mathf.Max(
                    0.01f,
                    damageTakenMultiplier /
                    multiplier
                );
        }

        #endregion

        #region 視覺效果

        private void CacheRendererColors()
        {
            renderers =
                GetComponentsInChildren<Renderer>();

            originalColors =
                new Color[renderers.Length];

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                originalColors[i] =
                    renderers[i]
                        .material.color;
            }
        }

        public void SetEffectColor(
            Color color)
        {
            foreach (Renderer renderer in
                     renderers)
            {
                if (renderer != null)
                {
                    renderer.material.color =
                        color;
                }
            }
        }

        public void ResetColor()
        {
            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i]
                    .material.color =
                    originalColors[i];
            }
        }

        #endregion

        #region 傷害與死亡

        public void TakeDamage(
            float amount)
        {
            if (isDead ||
                amount <= 0f)
            {
                return;
            }

            float finalDamage =
                amount *
                damageTakenMultiplier;

            currentHealth =
                Mathf.Max(
                    0f,
                    currentHealth -
                    finalDamage
                );

            if (currentHealth <= 0f)
            {
                KillByTower();
            }
        }

        public void KillByTower()
        {
            if (isDead)
                return;

            isDead = true;

            if (data != null)
            {
                GameManager.Instance?.AddGold(
                    data.goldReward
                );

                HeroManager.Instance
                    ?.OnEnemyKilled(
                        data.xpReward
                    );
            }

            Destroy(gameObject);
        }

        public void ReachGoal()
        {
            if (isDead ||
                data == null)
            {
                return;
            }

            isDead = true;

            int finalDamage =
                Mathf.RoundToInt(
                    data.damage *
                    damageBuffMultiplier
                );

            GameManager.Instance
                ?.TakeDamage(
                    finalDamage
                );

            Destroy(gameObject);
        }

        #endregion
    }
}