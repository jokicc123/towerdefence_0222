using System.Collections.Generic;
using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    /// <summary>
    /// Hero 的普通攻擊系統。
    /// 負責搜尋目標、播放攻擊動畫、遠程與近戰攻擊。
    /// </summary>
    public partial class Hero
    {
        #region Animator 參數

        private static readonly int AttackTrigger =
            Animator.StringToHash("Attack");

        #endregion

        #region 攻擊搜尋暫存

        private readonly HashSet<Enemy> foundEnemies =
            new();

        private readonly HashSet<Enemy> damagedEnemies =
            new();

        #endregion

        #region 目標搜尋

        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();
            foundEnemies.Clear();

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                CurrentStats.range
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

                if (foundEnemies.Add(enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }

            enemiesInRange.Sort(
                CompareEnemyDistance
            );
        }

        private int CompareEnemyDistance(
            Enemy first,
            Enemy second)
        {
            float firstDistance =
                (first.transform.position -
                 transform.position)
                .sqrMagnitude;

            float secondDistance =
                (second.transform.position -
                 transform.position)
                .sqrMagnitude;

            return firstDistance.CompareTo(
                secondDistance
            );
        }

        #endregion

        #region 攻擊流程

        private void HandleAttack()
        {
            attackTimer -= Time.deltaTime;

            enemiesInRange.RemoveAll(
                enemy =>
                    enemy == null ||
                    enemy.IsDead
            );

            if (enemiesInRange.Count == 0)
                return;

            Enemy target =
                enemiesInRange[0];

            RotateBodyToTarget(target);
            RotateHeadToTarget(target);

            if (attackTimer > 0f)
                return;

            pendingFireTarget = target;

            if (animator != null)
            {
                animator.SetTrigger(
                    AttackTrigger
                );
            }
            else
            {
                ExecutePendingAttack();
            }

            attackTimer =
                AttackInterval;
        }

        /// <summary>
        /// 由攻擊動畫的 Animation Event 呼叫。
        /// </summary>
        public void FireFromAnimationEvent()
        {
            ExecutePendingAttack();
        }

        private void ExecutePendingAttack()
        {
            Enemy target =
                pendingFireTarget;

            pendingFireTarget = null;

            if (target == null ||
                target.IsDead ||
                data == null)
            {
                return;
            }

            if (SoundManager.Instance != null &&
                data.attackSFX != null)
            {
                SoundManager.Instance.PlaySFX(
                    data.attackSFX
                );
            }

            PlayNormalAttackVFX();
            Fire(target);
        }

        private void Fire(
            Enemy target)
        {
            if (target == null ||
                target.IsDead ||
                data == null)
            {
                return;
            }

            switch (data.attackType)
            {
                case HeroAttackType.Ranged:
                    FireRanged(target);
                    break;

                case HeroAttackType.Melee:
                    FireMelee(target);
                    break;
            }
        }

        #endregion

        #region 遠程攻擊

        private void FireRanged(
            Enemy target)
        {
            if (data.bulletPrefab == null ||
                firePoint == null)
            {
                Debug.LogWarning(
                    $"{data.heroName} 缺少子彈或 FirePoint",
                    this
                );

                return;
            }

            Vector3 direction =
                target.transform.position -
                firePoint.position;

            if (direction.sqrMagnitude >
                0.001f)
            {
                firePoint.rotation =
                    Quaternion.LookRotation(
                        direction.normalized
                    );
            }

            GameObject bulletObject =
                Instantiate(
                    data.bulletPrefab,
                    firePoint.position,
                    firePoint.rotation
                );

            if (!bulletObject.TryGetComponent(
                    out Bullet bullet))
            {
                Debug.LogWarning(
                    $"{bulletObject.name} 沒有 Bullet 腳本",
                    bulletObject
                );

                Destroy(bulletObject);
                return;
            }

            bullet.SetTarget(
                target,
                FinalDamage,
                TowerEffectType.None,
                0f,
                0f,
                0f
            );
        }

        #endregion

        #region 近戰攻擊

        private void FireMelee(
            Enemy target)
        {
            if (target == null ||
                target.IsDead)
            {
                return;
            }

            Vector3 hitCenter =
                transform.position +
                transform.forward *
                data.meleeHitOffset;

            Collider[] hits =
                Physics.OverlapSphere(
                    hitCenter,
                    data.meleeHitRadius
                );

            damagedEnemies.Clear();

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
                    FinalDamage
                );

                if (!data.normalAttackBurn)
                    continue;

                enemy.AddEffect(
                    new BurnEffect(
                        enemy,
                        data.burnDuration,
                        data.burnDamagePerSecond
                    )
                );
            }
        }

        #endregion

        #region 普通攻擊特效

        private void PlayNormalAttackVFX()
        {
            if (data == null ||
                data.normalAttackVFX == null)
            {
                return;
            }

            Transform spawnPoint =
                normalAttackVFXPoint != null
                    ? normalAttackVFXPoint
                    : firePoint;

            if (spawnPoint == null)
                return;

            GameObject vfx =
                Instantiate(
                    data.normalAttackVFX,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    spawnPoint
                );

            Destroy(
                vfx,
                3f
            );
        }

        #endregion

        #region 旋轉

        private void RotateBodyToTarget(
            Enemy target)
        {
            RotateTransformToTarget(
                transform,
                target,
                10f
            );
        }

        private void RotateHeadToTarget(
            Enemy target)
        {
            if (head == null)
                return;

            RotateTransformToTarget(
                head,
                target,
                10f
            );
        }

        private static void RotateTransformToTarget(
            Transform targetTransform,
            Enemy enemy,
            float speed)
        {
            if (targetTransform == null ||
                enemy == null ||
                enemy.IsDead)
            {
                return;
            }

            Vector3 direction =
                enemy.transform.position -
                targetTransform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <=
                0.001f)
            {
                return;
            }

            Quaternion rotation =
                Quaternion.LookRotation(
                    direction
                );

            targetTransform.rotation =
                Quaternion.Slerp(
                    targetTransform.rotation,
                    rotation,
                    speed * Time.deltaTime
                );
        }

        #endregion
    }
}