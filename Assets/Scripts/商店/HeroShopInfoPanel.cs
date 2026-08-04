using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    public class HeroShopInfoPanel : MonoBehaviour
    {
        public static HeroShopInfoPanel Instance;
        [SerializeField] private CanvasGroup infoGroup;
        [Header("英雄介紹 UI")]
        [SerializeField] private Image heroIcon;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Image skillIcon;
        [SerializeField] private Image skill2Icon;
        [SerializeField] private TMP_Text skillText;
        [SerializeField] private TMP_Text skill2Text;
        [SerializeField] private Image passiveIcon;
        [SerializeField] private TMP_Text passiveText;

        [Header("狀態按鈕")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] public Button panelExitButton;
        private HeroShopData currentShopData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (infoGroup == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 Info Group",
                    this
                );

                return;
            }

            // 開場隱藏介紹介面
            HidePanel();

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

        private void OnDestroy()
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

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ShowHero(HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return;
            }

            currentShopData = shopData;

            Refresh();
            ShowPanel();
        }

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

                // 購買成功後直接選擇英雄
                HeroShopManager.Instance.SelectHero(
                    currentShopData
                );
            }
            else
            {
                HeroShopManager.Instance.SelectHero(
                    currentShopData
                );
            }

            Refresh();

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RefreshCrystalUI();
            }
        }

        public void OnClickExit()
        {
            HidePanel();
        }
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

            if (heroIcon != null)
            {
                heroIcon.sprite = hero.icon;
                heroIcon.enabled = hero.icon != null;
                heroIcon.color = Color.white;
                heroIcon.preserveAspect = true;
            }

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

            if (skillIcon != null)
            {
                skillIcon.sprite =
                    hero.skill1.icon;

                skillIcon.enabled =
                    hero.skill1.icon != null;

                skillIcon.color = Color.white;
                skillIcon.preserveAspect = true;
            }

            if (skill2Icon != null)
            {
                skill2Icon.sprite =
                    hero.skill2.icon;

                skill2Icon.enabled =
                    hero.skill2.icon != null;

                skill2Icon.color = Color.white;
                skill2Icon.preserveAspect = true;
            }

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
                    $"{hero.skill2.description}\n\n";
            }
            if (passiveIcon != null)
            {
                passiveIcon.sprite = hero.passive.icon;
                passiveIcon.enabled = hero.passive.icon != null;
                passiveIcon.color = Color.white;
                passiveIcon.preserveAspect = true;
            }

            // 被動光環名稱與說明
            if (passiveText != null)
            {
                passiveText.text =
                    $"【被動光環：{hero.passive.skillName}】\n" +
                    $"{hero.passive.description}";
            }

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
                if (costText != null)
                {
                    costText.text =
                        $"{currentShopData.crystalCost} 水晶";
                }

                if (buttonText != null)
                {
                    buttonText.text = "解鎖";
                }

                if (actionButton != null)
                {
                    actionButton.interactable = true;
                    actionButton.image.color =
                        Color.gray;
                }
            }
            else if (selected)
            {
                if (costText != null)
                {
                    costText.text = "已擁有";
                }

                if (buttonText != null)
                {
                    buttonText.text = "使用中";
                }

                if (actionButton != null)
                {
                    actionButton.interactable = false;
                    actionButton.image.color =
                        new Color(
                            1f,
                            0.8f,
                            0f
                        );
                }
            }
            else
            {
                if (costText != null)
                {
                    costText.text = "已擁有";
                }

                if (buttonText != null)
                {
                    buttonText.text = "選擇";
                }

                if (actionButton != null)
                {
                    actionButton.interactable = true;
                    actionButton.image.color =
                        Color.green;
                }

            }

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

    }
}