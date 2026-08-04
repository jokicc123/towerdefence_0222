using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    public class HeroShopItemUI : MonoBehaviour
    {
        [Header("英雄商品資料")]
        [SerializeField] private HeroShopData shopData;

        [Header("列表 UI")]
        [SerializeField] private Image heroIcon;
        [SerializeField] private Button selectButton;

        private void Start()
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 HeroShopData 或 HeroData",
                    this
                );

                return;
            }

            if (heroIcon != null)
            {
                heroIcon.sprite =
                    shopData.heroData.icon;
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(
                    OnClickShowHero
                );
            }
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(
                    OnClickShowHero
                );
            }
        }

        public void OnClickShowHero()
        {
            if (HeroShopInfoPanel.Instance == null)
            {
                Debug.LogError(
                    "場景中找不到 HeroShopInfoPanel",
                    this
                );

                return;
            }

            HeroShopInfoPanel.Instance.ShowHero(
                shopData
            );
        }
    }
}