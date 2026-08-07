using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// Hero 的主動技能共用流程。
    /// 負責技能可用判斷、冷卻與技能類型分派。
    /// </summary>
    public partial class Hero
    {
        #region 技能狀態判斷

        private bool HasSkillStats(
            ActiveSkillData skill)
        {
            return skill.levelStats != null &&
                   skill.levelStats.Length > 0;
        }

        private bool CanUseSkill(
            ActiveSkillData skill,
            float timer)
        {
            return data != null &&
                   HasSkillStats(skill) &&
                   currentLevel >= skill.unlockLevel &&
                   timer <= 0f;
        }

        public bool CanUseSkill1()
        {
            if (data == null)
                return false;

            return CanUseSkill(
                data.skill1,
                skill1Timer
            );
        }

        public bool CanUseSkill2()
        {
            if (data == null)
                return false;

            return CanUseSkill(
                data.skill2,
                skill2Timer
            );
        }

        #endregion

        #region 技能施放

        public void UseSkill1()
        {
            if (!CanUseSkill1())
                return;

            SkillLevelStats stats =
                CurrentSkill1Stats;

            if (!ExecuteSkill(
                    data.skill1,
                    stats))
            {
                return;
            }

            skill1Timer =
                Mathf.Max(
                    0f,
                    stats.cooldown
                );
        }

        public void UseSkill2()
        {
            if (!CanUseSkill2())
                return;

            SkillLevelStats stats =
                CurrentSkill2Stats;

            if (!ExecuteSkill(
                    data.skill2,
                    stats))
            {
                return;
            }

            skill2Timer =
                Mathf.Max(
                    0f,
                    stats.cooldown
                );
        }

        #endregion

        #region 技能分派

        private bool ExecuteSkill(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            switch (skill.skillType)
            {
                case HeroSkillType.AreaStun:
                    UseAreaStun(
                        skill,
                        stats
                    );
                    return true;

                case HeroSkillType.SummonCreature:
                    return UseSummonSkill(
                        skill,
                        stats
                    );

                case HeroSkillType.FireWall:
                    UseFireWall(
                        skill,
                        stats
                    );
                    return true;

                case HeroSkillType.SunOfNoon:
                    StopSunOfNoon();

                    sunCoroutine =
                        StartCoroutine(
                            UseSunOfNoonRoutine(
                                skill,
                                stats
                            )
                        );

                    return true;

                case HeroSkillType.None:
                default:
                    return false;
            }
        }

        #endregion

        #region 共用技能特效

        private void PlayVFX(
            GameObject vfxPrefab,
            Vector3 position)
        {
            if (vfxPrefab == null)
                return;

            GameObject vfxObject =
                Instantiate(
                    vfxPrefab,
                    position,
                    Quaternion.identity
                );

            ParticleSystem[] particles =
                vfxObject.GetComponentsInChildren<
                    ParticleSystem>();

            float lifetime = 3f;

            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule main =
                    particle.main;

                lifetime =
                    Mathf.Max(
                        lifetime,
                        main.duration +
                        main.startLifetime.constantMax
                    );
            }

            Destroy(
                vfxObject,
                lifetime
            );
        }

        #endregion
    }
}