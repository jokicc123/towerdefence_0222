using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    /// <summary>
    /// 防禦塔子彈。
    /// 負責追蹤目標、造成單體或範圍傷害，
    /// 並套用燃燒或中毒等狀態效果。
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        #region Inspector 設定

        [Header("子彈設定")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float reachDistance = 0.2f;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private float slowPercent = 0.5f;

        [Header("物理優化")]
        [SerializeField] private LayerMask enemyLayer;

        #endregion

        #region 執行期間資料

        private Enemy target;

        private float damage;
        private TowerEffectType effectType;
        private float effectDuration;
        private float effectDps;

        private float blastRadius;
        private bool isAoE;

        private float sqrReachDistance;

        #endregion

        #region Unity 生命週期

        private void Update()
        {
            if (target == null ||
                target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            MoveToTarget();
        }

        private void OnDrawGizmosSelected()
        {
            if (blastRadius <= 0f)
                return;

            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                transform.position,
                blastRadius
            );
        }

        #endregion

        #region 目標設定

        public void SetTarget(
            Enemy newTarget,
            float damage,
            TowerEffectType effectType,
            float effectDuration,
            float effectDps,
            float blastRadius)
        {
            target = newTarget;

            this.damage =
                Mathf.Max(0f, damage);

            this.effectType =
                effectType;

            this.effectDuration =
                Mathf.Max(0f, effectDuration);

            this.effectDps =
                Mathf.Max(0f, effectDps);

            this.blastRadius =
                Mathf.Max(0f, blastRadius);

            isAoE =
                this.blastRadius > 0f;

            sqrReachDistance =
                reachDistance *
                reachDistance;
        }

        #endregion

        #region 移動系統

        private void MoveToTarget()
        {
            Vector3 offset =
                target.transform.position -
                transform.position;

            if (offset.sqrMagnitude <=
                sqrReachDistance)
            {
                HitTarget();
                return;
            }

            Vector3 direction =
                offset.normalized;

            transform.position +=
                direction *
                speed *
                Time.deltaTime;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction
                    );
            }
        }

        #endregion

        #region 命中處理

        private void HitTarget()
        {
            SpawnHitEffect();

            if (isAoE)
            {
                DamageArea();
            }
            else
            {
                DamageEnemy(target);
            }

            Destroy(gameObject);
        }

        private void SpawnHitEffect()
        {
            if (hitEffectPrefab == null)
                return;

            GameObject effect =
                Instantiate(
                    hitEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

            Destroy(
                effect,
                2f
            );
        }

        private void DamageArea()
        {
            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    blastRadius,
                    enemyLayer,
                    QueryTriggerInteraction.Collide
                );

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead)
                {
                    continue;
                }

                DamageEnemy(enemy);
            }
        }

        #endregion

        #region 傷害與狀態效果

        private void DamageEnemy(
            Enemy enemy)
        {
            if (enemy == null ||
                enemy.IsDead)
            {
                return;
            }

            enemy.TakeDamage(
                damage
            );

            ApplyStatusEffect(
                enemy
            );

#if UNITY_EDITOR
            Debug.Log(
                $"子彈命中：{enemy.name}，傷害：{damage}",
                enemy
            );
#endif
        }

        private void ApplyStatusEffect(
            Enemy enemy)
        {
            switch (effectType)
            {
                case TowerEffectType.Burn:
                    enemy.AddEffect(
                        new BurnEffect(
                            enemy,
                            effectDuration,
                            effectDps
                        )
                    );
                    break;

                case TowerEffectType.Poison:
                    enemy.AddEffect(
                        new PoisonEffect(
                            enemy,
                            effectDuration,
                            effectDps,
                            slowPercent
                        )
                    );
                    break;
            }
        }

        #endregion
    }
}