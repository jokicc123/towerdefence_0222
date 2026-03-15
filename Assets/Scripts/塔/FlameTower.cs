using UnityEngine;

namespace CHANG
{
    public class FlameTower : Tower
    {
        // 覆寫 Fire()
        public override void Fire(Enemy target)
        {
            Debug.Log("Fire() 被呼叫"); // ✅ 確認是否執行

            if (target == null) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
            Debug.Log("OverlapSphere 偵測到敵人數量: " + hits.Length); // ✅ 確認範圍內敵人

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Enemy enemy))
                {
                    Debug.Log("對敵人造成傷害: " + enemy.name); // ✅ 確認有找到 Enemy
                    enemy.TakeDamage(damage * Time.deltaTime);
                }
            }
            if (data.bulletPrefab != null && firePoint != null)
            {
                GameObject effect = Instantiate(data.bulletPrefab, firePoint.position, firePoint.rotation);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play(); // 確保粒子播放
                Destroy(effect, 1f);       // 1秒後刪除
            }

        }
    }
}