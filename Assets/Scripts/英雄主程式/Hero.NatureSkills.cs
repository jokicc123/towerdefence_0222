using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// Hero 的自然系主動技能。
    /// 包含範圍暈眩與召喚生物。
    /// </summary>
    public partial class Hero
    {
        #region 執行期間資料

        private readonly HashSet<Enemy> affectedEnemies =
            new();

        #endregion

        #region 荊棘蔓延(範圍暈眩)

        private void UseAreaStun(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            affectedEnemies.Clear();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    stats.radius
                );

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead ||
                    !affectedEnemies.Add(enemy))
                {
                    continue;
                }

                enemy.AddEffect(
                    new StunEffect(
                        enemy,
                        stats.value
                    )
                );

                PlayVFX(
                    skill.vfxPrefab,
                    enemy.transform.position
                );
            }
        }

        #endregion

        #region 樹人降臨(召喚技能)

        private bool UseSummonSkill(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            if (skill.summonPrefab == null)
            {
                Debug.LogWarning(
                    $"{data.heroName} 的召喚技能沒有設定 Summon Prefab",
                    this
                );

                return false;
            }

            Vector3 spawnPosition =
                transform.position +
                transform.forward * 2.5f;

            GameObject summonedObject =
                Instantiate(
                    skill.summonPrefab,
                    spawnPosition,
                    transform.rotation
                );

            SummonedCreature creature =
                summonedObject
                    .GetComponentInChildren<
                        SummonedCreature>();

            if (creature == null)
            {
                Debug.LogError(
                    $"{summonedObject.name} 找不到 SummonedCreature",
                    summonedObject
                );

                Destroy(summonedObject);
                return false;
            }

            creature.Initialize(
                stats.value,
                stats.duration,
                stats.attackSpeed,
                stats.radius
            );

            PlayVFX(
                skill.vfxPrefab,
                spawnPosition
            );

            return true;
        }

        #endregion
    }
}