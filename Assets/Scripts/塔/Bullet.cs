using UnityEngine;

namespace CHANG
{
    public class Bullet : MonoBehaviour
    {
        [Header("子彈設定")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float reachDistance = 0.2f; // 判定撞擊的距離
        [SerializeField] private GameObject hitEffectPrefab; // 撞擊特效

        private Enemy target;
        private float damage;

        // 由 Tower 在 Instantiate 後呼叫
        public void SetTarget(Enemy target, float damage)
        {
            this.target = target;
            this.damage = damage;
        }

        private void Update()
        {
            if (target == null)
            {
                Destroy(gameObject); // 目標消失就自毀
                return;
            }

            // 朝目標移動
            Vector3 direction = target.transform.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            // 檢查是否到達目標
            if (direction.magnitude <= reachDistance)
            {
                HitTarget();
                return;
            }

            // 移動並轉向目標
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
            transform.LookAt(target.transform);
        }

        private void HitTarget()
        {
            // 產生特效
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // 造成傷害
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}