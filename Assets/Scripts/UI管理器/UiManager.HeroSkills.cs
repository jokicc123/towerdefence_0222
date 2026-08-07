using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// UiManager 的英雄技能 UI。
    /// 負責技能圖示、技能按鈕、冷卻顯示與場上英雄綁定。
    /// </summary>
    public partial class UiManager
    {
        #region Inspector 設定

        [Header("英雄技能 UI")]
        [SerializeField]
        private Button skill1Button;

        [SerializeField]
        private Button skill2Button;

        [SerializeField]
        private Image skill1Icon;

        [SerializeField]
        private Image skill2Icon;

        [SerializeField]
        private Image skill1CooldownImage;

        [SerializeField]
        private Image skill2CooldownImage;

        #endregion

        #region UI 初始化

        private void InitializeSkillUI()
        {
            SetSkillButtonsEnabled(
                false
            );

            ResetSkillCooldownImages();

            RefreshSelectedHeroSkillIcons();
        }

        #endregion

        #region 英雄綁定

        public void SetActiveHero(
            Hero hero)
        {
            activeHero = hero;

            if (activeHero == null ||
                activeHero.data == null)
            {
                SetSkillButtonsEnabled(
                    false
                );

                ResetSkillCooldownImages();

                return;
            }

            SetSkillIcon(
                skill1Icon,
                activeHero.data.skill1.icon
            );

            SetSkillIcon(
                skill2Icon,
                activeHero.data.skill2.icon
            );

            UpdateHeroSkillCooldownUI();
        }

        public void ClearActiveHero(
            Hero hero)
        {
            if (activeHero != hero)
                return;

            activeHero = null;

            SetSkillButtonsEnabled(
                false
            );

            ResetSkillCooldownImages();
        }

        #endregion

        #region 技能按鈕事件

        public void OnClickSkill1()
        {
            if (activeHero == null ||
                !activeHero.CanUseSkill1())
            {
                return;
            }

            activeHero.UseSkill1();

            UpdateHeroSkillCooldownUI();
        }

        public void OnClickSkill2()
        {
            if (activeHero == null ||
                !activeHero.CanUseSkill2())
            {
                return;
            }

            activeHero.UseSkill2();

            UpdateHeroSkillCooldownUI();
        }

        #endregion

        #region 技能冷卻 UI

        private void UpdateHeroSkillCooldownUI()
        {
            if (activeHero == null)
            {
                SetSkillButtonsEnabled(
                    false
                );

                ResetSkillCooldownImages();

                return;
            }

            RefreshSkillButton(
                skill1Button,
                skill1CooldownImage,
                activeHero.CanUseSkill1(),
                activeHero.Skill1CooldownRatio
            );

            RefreshSkillButton(
                skill2Button,
                skill2CooldownImage,
                activeHero.CanUseSkill2(),
                activeHero.Skill2CooldownRatio
            );
        }

        private static void RefreshSkillButton(
            Button button,
            Image cooldownImage,
            bool canUse,
            float cooldownRatio)
        {
            if (button != null)
            {
                button.interactable =
                    canUse;
            }

            if (cooldownImage != null)
            {
                cooldownImage.fillAmount =
                    Mathf.Clamp01(
                        cooldownRatio
                    );
            }
        }

        #endregion

        #region 技能按鈕狀態

        private void SetSkillButtonsEnabled(
            bool enabled)
        {
            if (skill1Button != null)
            {
                skill1Button.interactable =
                    enabled;
            }

            if (skill2Button != null)
            {
                skill2Button.interactable =
                    enabled;
            }
        }

        private void ResetSkillCooldownImages()
        {
            if (skill1CooldownImage != null)
            {
                skill1CooldownImage.fillAmount =
                    0f;
            }

            if (skill2CooldownImage != null)
            {
                skill2CooldownImage.fillAmount =
                    0f;
            }
        }

        #endregion

        #region 技能圖示

        private void RefreshSelectedHeroSkillIcons()
        {
            HeroSelectionManager selectionManager =
                HeroSelectionManager.Instance;

            if (selectionManager == null)
                return;

            HeroData selectedData =
                selectionManager.CurrentHeroData;

            if (selectedData == null)
                return;

            SetSkillIcon(
                skill1Icon,
                selectedData.skill1.icon
            );

            SetSkillIcon(
                skill2Icon,
                selectedData.skill2.icon
            );
        }

        private static void SetSkillIcon(
            Image image,
            Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite =
                sprite;

            image.enabled =
                sprite != null;

            image.color =
                Color.white;

            image.preserveAspect =
                true;
        }

        #endregion
    }
}