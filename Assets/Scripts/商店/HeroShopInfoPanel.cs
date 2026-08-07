using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 英雄商店資訊面板。
    /// 負責顯示英雄資料、技能資訊、被動光環，
    /// 並處理解鎖與選擇操作。
    /// </summary>
    public class HeroShopInfoPanel : MonoBehaviour
    {
        #region Singleton

        public static HeroShopInfoPanel Instance
        {
            get;
            private set;
        }

        #endregion

        #region Inspector 設定

        [Header("面板")]
        [SerializeField]
        private CanvasGroup infoGroup;

        [Header("英雄介紹 UI")]
        [SerializeField]
        private Image heroIcon;

        [SerializeField]
        private TMP_Text heroNameText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private TMP_Text costText;

        [Header("主動技能 1")]
        [SerializeField]
        private Image skillIcon;

        [SerializeField]
        private TMP_Text skillText;

        [Header("主動技能 2")]
        [SerializeField]
        private Image skill2Icon;

        [SerializeField]
        private TMP_Text skill2Text;

        [Header("被動技能")]
        [SerializeField]
        private Image passiveIcon;

        [SerializeField]
        private TMP_Text passiveText;

        [Header("狀態按鈕")]
        [SerializeField]
        private Button actionButton;

        [SerializeField]
        private TMP_Text buttonText;

        [SerializeField]
        private Button panelExitButton;

        #endregion

        #region 執行期間資料

        private HeroShopData currentShopData;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (infoGroup == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 Info Group",
                    this
                );

                return;
            }

            HidePanel();
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(
                    OnClickAction
                );
            }

            if (panelExitButton != null)
            {
                panelExitButton.onClick.AddListener(
                    OnClickExit
                );
            }
        }

        private void UnregisterEvents()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(
                    OnClickAction
                );
            }

            if (panelExitButton != null)
            {
                panelExitButton.onClick.RemoveListener(
                    OnClickExit
                );
            }
        }

        #endregion

        #region 面板顯示

        public void ShowHero(
            HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return;
            }

            currentShopData =
                shopData;

            Refresh();
            ShowPanel();
        }

        public void OnClickExit()
        {
            HidePanel();
        }

        private void ShowPanel()
        {
            if (infoGroup == null)
                return;

            infoGroup.alpha = 1f;
            infoGroup.interactable = true;
            infoGroup.blocksRaycasts = true;
        }

        private void HidePanel()
        {
            if (infoGroup == null)
                return;

            infoGroup.alpha = 0f;
            infoGroup.interactable = false;
            infoGroup.blocksRaycasts = false;
        }

        #endregion

        #region 英雄資訊更新

        public void Refresh()
        {
            if (currentShopData == null ||
                currentShopData.heroData == null ||
                HeroShopManager.Instance == null)
            {
                return;
            }

            HeroData hero =
                currentShopData.heroData;

            RefreshHeroInfo(hero);
            RefreshSkillInfo(hero);
            RefreshButtonState();
        }

        private void RefreshHeroInfo(
            HeroData hero)
        {
            if (hero == null)
                return;

            SetIcon(
                heroIcon,
                hero.icon
            );

            if (heroNameText != null)
            {
                heroNameText.text =
                    hero.heroName;
            }

            if (descriptionText != null)
            {
                descriptionText.text =
                    hero.description;
            }
        }

        #endregion

        #region 技能資訊更新

        private void RefreshSkillInfo(
            HeroData hero)
        {
            if (hero == null)
                return;

            SetIcon(
                skillIcon,
                hero.skill1.icon
            );

            SetIcon(
                skill2Icon,
                hero.skill2.icon
            );

            SetIcon(
                passiveIcon,
                hero.passive.icon
            );

            if (skillText != null)
            {
                skillText.text =
                    $"【{hero.skill1.skillName}】\n" +
                    $"{hero.skill1.description}";
            }

            if (skill2Text != null)
            {
                skill2Text.text =
                    $"【{hero.skill2.skillName}】\n" +
                    $"{hero.skill2.description}";
            }

            if (passiveText != null)
            {
                passiveText.text =
                    $"【被動光環：{hero.passive.skillName}】\n" +
                    $"{hero.passive.description}";
            }
        }

        private void SetIcon(
            Image image,
            Sprite sprite)
        {
            if (image == null)
                return;

            image.sprite = sprite;
            image.enabled =
                sprite != null;

            image.color =
                Color.white;

            image.preserveAspect =
                true;
        }

        #endregion

        #region 購買與選擇

        private void OnClickAction()
        {
            if (currentShopData == null ||
                HeroShopManager.Instance == null)
            {
                return;
            }

            bool unlocked =
                HeroShopManager.Instance.IsUnlocked(
                    currentShopData
                );

            if (!unlocked)
            {
                bool success =
                    HeroShopManager.Instance.BuyHero(
                        currentShopData
                    );

                if (!success)
                    return;
            }

            HeroShopManager.Instance.SelectHero(
                currentShopData
            );

            Refresh();

            ShopManager.Instance
                ?.RefreshCrystalUI();
        }

        #endregion

        #region 按鈕狀態

        private void RefreshButtonState()
        {
            bool unlocked =
                HeroShopManager.Instance.IsUnlocked(
                    currentShopData
                );

            bool selected =
                HeroShopManager.Instance.IsSelected(
                    currentShopData
                );

            if (!unlocked)
            {
                SetLockedState();
                return;
            }

            if (selected)
            {
                SetSelectedState();
                return;
            }

            SetOwnedState();
        }

        private void SetLockedState()
        {
            if (costText != null)
            {
                costText.text =
                    $"{currentShopData.crystalCost} 水晶";
            }

            if (buttonText != null)
            {
                buttonText.text =
                    "解鎖";
            }

            if (actionButton != null)
            {
                actionButton.interactable =
                    true;

                actionButton.image.color =
                    Color.gray;
            }
        }

        private void SetSelectedState()
        {
            if (costText != null)
            {
                costText.text =
                    "已擁有";
            }

            if (buttonText != null)
            {
                buttonText.text =
                    "使用中";
            }

            if (actionButton != null)
            {
                actionButton.interactable =
                    false;

                actionButton.image.color =
                    new Color(
                        1f,
                        0.8f,
                        0f
                    );
            }
        }

        private void SetOwnedState()
        {
            if (costText != null)
            {
                costText.text =
                    "已擁有";
            }

            if (buttonText != null)
            {
                buttonText.text =
                    "選擇";
            }

            if (actionButton != null)
            {
                actionButton.interactable =
                    true;

                actionButton.image.color =
                    Color.green;
            }
        }

        #endregion
    }
}