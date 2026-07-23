using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    public class Bullet : MonoBehaviour
    {
        [Header("子彈設定")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float reachDistance = 0.2f;
        [SerializeField] private GameObject hitEffectPrefab; // 撞擊/爆炸特效
        [SerializeField] private float slowPercent = 0.5f;

        [Header(" 物理優化")]
        [SerializeField] private LayerMask enemyLayer;      // 只偵測怪物的圖層

        private Enemy target;
        private float damage;
        private TowerEffectType effectType;
        private float effectDuration;
        private float effectDps;
        private float blastRadius;
        private bool isAoE;

        private float sqrReachDistance; // 快取判定距離的平方

        public void SetTarget(Enemy newTarget, float dmg, TowerEffectType type, float duration, float dps, float blastRadius)
        {
            target = newTarget;
            damage = dmg;
            effectType = type;
            effectDuration = duration;
            effectDps = dps;
            this.blastRadius = blastRadius;
            isAoE = blastRadius > 0f;

            //  在初始化時先算好平方，避免 Update 裡一直做開根號運算
            sqrReachDistance = reachDistance * reachDistance;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            // 用 sqrMagnitude 代替 Vector3.Distance
            Vector3 offset = target.transform.position - transform.position;
            if (offset.sqrMagnitude <= sqrReachDistance)
            {
                HitTarget();
                return;
            }

            Vector3 dir = offset.normalized;
            transform.position += dir * speed * Time.deltaTime;
            transform.LookAt(target.transform);
        }

        private void HitTarget()
        {
            // 1.生成擊中/爆炸特效
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f); // 2秒後自動銷毀特效，防止 Hierarchy 爆炸
            }

            // 2. 處理傷害邏輯
            if (isAoE)
            {
                // 加入 enemyLayer，物理引擎直接過濾掉地面、雜物，只抓小怪
                Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius, enemyLayer);

                foreach (Collider hit in hits)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        DamageEnemy(enemy);
                    }
                }
            }
            else
            {
                DamageEnemy(target);
            }

            Destroy(gameObject);
        }

        private void DamageEnemy(Enemy enemy)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"💥 打到敵人：{enemy.name}");

            switch (effectType)
            {
                case TowerEffectType.Burn:
                    enemy.AddEffect(new BurnEffect(enemy, effectDuration, effectDps));
                    break;

                case TowerEffectType.Poison:
                    enemy.AddEffect(new PoisonEffect(enemy, effectDuration, effectDps, slowPercent));
                    break;
            }
        }

        //在 Scene 視窗畫出爆炸範圍，方便你調數值
        private void OnDrawGizmosSelected()
        {
            if (blastRadius > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, blastRadius);
            }
        }
    }
}