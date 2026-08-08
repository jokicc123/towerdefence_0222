using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// UiManager 的英雄資訊面板。
    /// 負責顯示英雄基本資料、能力值與經驗值。
    /// </summary>
    public partial class UiManager
    {
        #region Inspector 設定

        [Header("英雄資訊 UI")]
        [SerializeField]
        private TMP_Text heroNameText;

        [SerializeField]
        private TMP_Text heroLevelText;

        [SerializeField]
        private TMP_Text heroAttackText;

        [SerializeField]
        private TMP_Text heroRangeText;

        [SerializeField]
        private TMP_Text heroAttackSpeedText;

        [SerializeField]
        private TMP_Text heroExpText;

        [SerializeField]
        private TMP_Text heroUnlockDescriptionText;

        [SerializeField]
        private CanvasGroup heroPanel;

        [SerializeField]
        private Slider heroExpSlider;

        [SerializeField]
        private Image heroIcon;

        #endregion

        #region 英雄面板顯示

        public void ShowHeroUI(
    Hero hero)
        {
            if (hero == null)
                return;

            if (currentHero != null &&
                currentHero != hero)
            {
                currentHero.HideRangeCircle();
            }

            UnsubscribeCurrentHero();

            currentHero = hero;

            currentHero.OnHeroDataChanged +=
                RefreshHeroUI;

            currentHero.ShowRangeCircle();

            StartCoroutine(
               FadeSystem.Fade(
               heroPanel,
               true
               )
               );

            RefreshHeroUI();
        }

        public void HideHeroUI()
        {
            if (currentHero != null)
            {
                currentHero.HideRangeCircle();
            }

            UnsubscribeCurrentHero();

            StartCoroutine(
               FadeSystem.Fade(
               heroPanel,
               false
               )
               );
        }

        #endregion

        #region 英雄資訊更新

        private void RefreshHeroUI()
        {
            if (!HasValidCurrentHero())
                return;

            RefreshHeroBasicInfo();
            RefreshHeroStats();
            RefreshHeroExperience();
        }

        private bool HasValidCurrentHero()
        {
            return currentHero != null &&
                   currentHero.data != null &&
                   currentHero.data.levelStats != null &&
                   currentHero.data.levelStats.Length > 0;
        }

        #endregion

        #region 英雄基本資訊

        private void RefreshHeroBasicInfo()
        {
            HeroData heroData =
                currentHero.data;

            if (heroNameText != null)
            {
                heroNameText.text =
                    heroData.heroName;
            }

            if (heroIcon != null)
            {
                Sprite icon =
                    heroData.icon;

                heroIcon.sprite =
                    icon;

                heroIcon.enabled =
                    icon != null;

                heroIcon.color =
                    Color.white;

                heroIcon.preserveAspect =
                    true;
            }

            if (heroUnlockDescriptionText != null)
            {
                string description =
                    currentHero.CurrentStats
                        .unlockDescription;

                heroUnlockDescriptionText.text =
                    string.IsNullOrWhiteSpace(
                        description
                    )
                        ? "本級沒有新的解鎖效果"
                        : $"等級效果：\n{description}";
            }
        }

        #endregion

        #region 英雄能力數值

        private void RefreshHeroStats()
        {
            HeroLevelStats stats =
                currentHero.CurrentStats;

            if (heroAttackText != null)
            {
                heroAttackText.text =
                    $"傷害：{currentHero.FinalDamage:0}";
            }

            if (heroRangeText != null)
            {
                heroRangeText.text =
                    $"射程：{stats.range:0.0}";
            }

            if (heroAttackSpeedText != null)
            {
                heroAttackSpeedText.text =
                    $"攻速：{stats.attackSpeed:0.0}";
            }
        }

        #endregion

        #region 英雄經驗值

        private void RefreshHeroExperience()
        {
            int maxLevel =
                currentHero.data.levelStats.Length;

            int currentLevel =
                currentHero.CurrentLevel;

            bool isMaxLevel =
                currentLevel >= maxLevel;

            if (heroLevelText != null)
            {
                heroLevelText.text =
                    isMaxLevel
                        ? $"Lv.{currentLevel} MAX"
                        : $"Lv.{currentLevel}";
            }

            RefreshHeroExperienceSlider(
                isMaxLevel
            );

            RefreshHeroExperienceText(
                isMaxLevel
            );
        }

        private void RefreshHeroExperienceSlider(
            bool isMaxLevel)
        {
            if (heroExpSlider == null)
                return;

            heroExpSlider.minValue = 0f;

            if (isMaxLevel)
            {
                heroExpSlider.maxValue = 1f;
                heroExpSlider.value = 1f;
                return;
            }

            int requiredXP =
                currentHero.CurrentStats
                    .xpToNextLevel;

            heroExpSlider.maxValue =
                Mathf.Max(
                    1,
                    requiredXP
                );

            heroExpSlider.value =
                currentHero.CurrentXP;
        }

        private void RefreshHeroExperienceText(
            bool isMaxLevel)
        {
            if (heroExpText == null)
                return;

            if (isMaxLevel)
            {
                heroExpText.text =
                    "MAX";

                return;
            }

            heroExpText.text =
                $"{currentHero.CurrentXP}/" +
                $"{currentHero.CurrentStats.xpToNextLevel}";
        }

        #endregion
    }
}