using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// Hero 的共用屬性與技能數值取得。
    /// </summary>
    public partial class Hero
    {
        #region 英雄屬性

        public HeroLevelStats CurrentStats
        {
            get
            {
                if (data == null ||
                    data.levelStats == null ||
                    data.levelStats.Length == 0)
                {
                    return default;
                }

                int index = Mathf.Clamp(
                    currentLevel - 1,
                    0,
                    data.levelStats.Length - 1
                );

                return data.levelStats[index];
            }
        }

        public float FinalDamage =>
            CurrentStats.damage *
            ShopBonus.HeroDamageMultiplier;

        private float AttackInterval
        {
            get
            {
                float attackSpeed = Mathf.Max(
                    0.01f,
                    CurrentStats.attackSpeed
                );

                return 1f / attackSpeed;
            }
        }

        #endregion

        #region 技能屬性

        private SkillLevelStats CurrentSkill1Stats =>
            GetSkillStats(data.skill1);

        private SkillLevelStats CurrentSkill2Stats =>
            GetSkillStats(data.skill2);

        public float Skill1CooldownRatio =>
            GetCooldownRatio(
                data.skill1,
                skill1Timer
            );

        public float Skill2CooldownRatio =>
            GetCooldownRatio(
                data.skill2,
                skill2Timer
            );

        #endregion

        #region 技能數值取得

        private SkillLevelStats GetSkillStats(
            ActiveSkillData skill)
        {
            if (skill.levelStats == null ||
                skill.levelStats.Length == 0)
            {
                return default;
            }

            int index = Mathf.Clamp(
                currentLevel - skill.unlockLevel,
                0,
                skill.levelStats.Length - 1
            );

            return skill.levelStats[index];
        }

        private float GetCooldownRatio(
            ActiveSkillData skill,
            float timer)
        {
            if (!HasSkillStats(skill))
                return 0f;

            float cooldown =
                GetSkillStats(skill).cooldown;

            if (cooldown <= 0f)
                return 0f;

            return Mathf.Clamp01(
                timer / cooldown
            );
        }

        #endregion
    }
}