using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    public class Bullet : MonoBehaviour
    {
        [Header("子彈設定")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float reachDistance = 0.2f; // 判定撞擊的距離
        [SerializeField] private GameObject hitEffectPrefab; // 撞擊特效
        [SerializeField] private float slowPercent = 0.5f; // 毒藥減速百分比

        private Enemy target;
        private float damage;
        private TowerEffectType effectType;
        private float effectDuration;
        private float effectDps;
        private float blastRadius;
        private bool isAoE;
        // 由 Tower 在 Instantiate 後呼叫
        public void SetTarget(
     Enemy newTarget,
     float dmg,
     TowerEffectType type,
     float duration,
     float dps,
     float blastRadius
 )
        {
            target = newTarget;
            damage = dmg;

            effectType = type;
            effectDuration = duration;
            effectDps = dps;

            this.blastRadius = blastRadius;
            isAoE = blastRadius > 0f;
        }
        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance <= reachDistance)
            {
                HitTarget();
                return;
            }

            Vector3 dir = (target.transform.position - transform.position).normalized;

            transform.position += dir * speed * Time.deltaTime;

            transform.LookAt(target.transform);
        }

        private void HitTarget()
        {
            if (isAoE)
            {
                Collider[] hits = Physics.OverlapSphere(
                    transform.position,
                    blastRadius
                );

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
    }
}