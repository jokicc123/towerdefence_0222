using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 火焰塔。
    /// 攻擊時對範圍內所有敵人造成傷害，
    /// 並在主要目標位置生成火焰特效。
    /// </summary>
    public class FlameTower : Tower
    {
        #region 執行期間資料

        private readonly HashSet<Enemy> damagedEnemies =
            new();

        #endregion

        #region 攻擊系統

        public override void Fire(
            Enemy target)
        {
            if (target == null ||
                target.IsDead)
            {
                return;
            }

            damagedEnemies.Clear();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    AttackRange
                );

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead ||
                    !damagedEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.TakeDamage(
                    Damage
                );
            }

            SpawnAttackEffect(
                target
            );
        }

        #endregion

        #region 攻擊特效

        private void SpawnAttackEffect(
            Enemy target)
        {
            if (target == null ||
                Data == null)
            {
                return;
            }

            GameObject effectPrefab =
                Data.levels[CurrentLevel]
                    .bulletPrefab;

            if (effectPrefab == null)
                return;

            Vector3 spawnPosition =
                target.transform.position +
                Vector3.up;

            GameObject effect =
                Instantiate(
                    effectPrefab,
                    spawnPosition,
                    Quaternion.identity,
                    target.transform
                );

            ParticleSystem particleSystem =
                effect.GetComponentInChildren<
                    ParticleSystem>();

            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            Destroy(
                effect,
                2f
            );
        }

        #endregion
    }
}