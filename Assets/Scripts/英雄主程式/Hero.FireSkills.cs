using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// Hero 的火焰系主動技能。
    /// 包含火焰之牆與正午的太陽。
    /// </summary>
    public partial class Hero
    {
        #region 執行期間資料

        private readonly HashSet<Enemy> fireSkillEnemies =
            new();

        #endregion

        #region 主動技能一：火焰之牆

        private void UseFireWall(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            float length =
                Mathf.Max(
                    0.1f,
                    stats.length
                );

            float width =
                Mathf.Max(
                    0.1f,
                    stats.width
                );

            Vector3 center =
                transform.position +
                transform.forward *
                (length * 0.5f);

            Vector3 halfExtents =
                new Vector3(
                    width * 0.5f,
                    1.5f,
                    length * 0.5f
                );

            Collider[] hits =
                Physics.OverlapBox(
                    center,
                    halfExtents,
                    transform.rotation
                );

            fireSkillEnemies.Clear();

            float skillDamage =
                stats.value *
                ShopBonus.HeroDamageMultiplier;

            float burnDps =
                stats.damagePerSecond *
                ShopBonus.HeroDamageMultiplier;

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead ||
                    !fireSkillEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.TakeDamage(
                    skillDamage
                );

                if (stats.duration > 0f &&
                    burnDps > 0f)
                {
                    enemy.AddEffect(
                        new BurnEffect(
                            enemy,
                            stats.duration,
                            burnDps
                        )
                    );
                }

                SpawnFireWallVFX(
                    skill,
                    enemy
                );
            }

#if UNITY_EDITOR
            Debug.Log(
                $"火焰之牆：" +
                $"命中 {fireSkillEnemies.Count} 隻敵人，" +
                $"傷害 {skillDamage:0.0}，" +
                $"長度 {length:0.0}，" +
                $"寬度 {width:0.0}",
                this
            );
#endif
        }

        private void SpawnFireWallVFX(
            ActiveSkillData skill,
            Enemy enemy)
        {
            if (skill.vfxPrefab == null ||
                enemy == null)
            {
                return;
            }

            Vector3 vfxPosition =
                enemy.transform.position +
                Vector3.up;

            GameObject vfxObject =
                Instantiate(
                    skill.vfxPrefab,
                    vfxPosition,
                    Quaternion.identity
                );

            Destroy(
                vfxObject,
                3f
            );
        }

        #endregion

        #region 主動技能二：正午的太陽

        private IEnumerator UseSunOfNoonRoutine(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            float duration =
                Mathf.Max(
                    0.1f,
                    stats.duration
                );

            float radius =
                Mathf.Max(
                    0.1f,
                    stats.radius
                );

            float damagePerSecond =
                Mathf.Max(
                    0f,
                    stats.damagePerSecond
                ) *
                ShopBonus.HeroDamageMultiplier;

            float burnMultiplier =
                Mathf.Max(
                    1f,
                    stats.multiplier
                );

            Vector3 sunCenter =
                transform.position +
                transform.forward * 3f;

            SpawnSunVFX(
                skill,
                sunCenter
            );

            BurnDamageSystem.AddSun(
                burnMultiplier
            );

            sunBurnBuffActive = true;

#if UNITY_EDITOR
            Debug.Log(
                $"正午的太陽啟動：" +
                $"持續 {duration:0.0} 秒，" +
                $"每秒傷害 {damagePerSecond:0.0}，" +
                $"燃燒倍率 ×{burnMultiplier:0.##}",
                this
            );
#endif

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.deltaTime;

                DamageEnemiesInSun(
                    sunCenter,
                    radius,
                    damagePerSecond
                );

                yield return null;
            }

            FinishSunOfNoon();
        }

        private void DamageEnemiesInSun(
            Vector3 center,
            float radius,
            float damagePerSecond)
        {
            Collider[] hits =
                Physics.OverlapSphere(
                    center,
                    radius
                );

            fireSkillEnemies.Clear();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead ||
                    !fireSkillEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.TakeDamage(
                    damagePerSecond *
                    Time.deltaTime
                );
            }
        }

        private void SpawnSunVFX(
            ActiveSkillData skill,
            Vector3 center)
        {
            if (skill.vfxPrefab == null)
                return;

            Vector3 vfxPosition =
                center +
                Vector3.up * 4f;

            currentSunVFX =
                Instantiate(
                    skill.vfxPrefab,
                    vfxPosition,
                    Quaternion.identity
                );
        }

        private void StopSunOfNoon()
        {
            if (sunCoroutine != null)
            {
                StopCoroutine(
                    sunCoroutine
                );

                sunCoroutine = null;
            }

            FinishSunOfNoon();
        }

        private void FinishSunOfNoon()
        {
            if (sunBurnBuffActive)
            {
                BurnDamageSystem.RemoveSun();

                sunBurnBuffActive =
                    false;
            }

            if (currentSunVFX != null)
            {
                Destroy(
                    currentSunVFX
                );

                currentSunVFX =
                    null;
            }

            sunCoroutine =
                null;
        }

        #endregion
    }
}