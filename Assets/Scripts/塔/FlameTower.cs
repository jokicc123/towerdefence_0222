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
                    enemy.TakeDamage(damage);
                }
            }
            if (data.levels[currentLevel].bulletPrefab != null)
            {
                GameObject effect = Instantiate(
                data.levels[currentLevel].bulletPrefab,
                target.transform.position + Vector3.up * 1f,
                Quaternion.identity,
                target.transform
            );

                ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();

                if (ps != null)
                {
                    ps.Play();
                    Destroy(effect, 2f); // 不要太長，避免堆積
                }
            }
        }
    }
}