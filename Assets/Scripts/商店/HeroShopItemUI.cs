using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 英雄商店列表項目。
    /// 顯示英雄圖示，並在點擊後開啟英雄詳細資訊面板。
    /// </summary>
    public class HeroShopItemUI : MonoBehaviour
    {
        #region Inspector 設定

        [Header("英雄商品資料")]
        [SerializeField]
        private HeroShopData shopData;

        [Header("列表 UI")]
        [SerializeField]
        private Image heroIcon;

        [SerializeField]
        private Button selectButton;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            if (!ValidateData())
                return;

            InitializeUI();
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region UI 初始化

        private bool ValidateData()
        {
            if (shopData != null &&
                shopData.heroData != null)
            {
                return true;
            }

            Debug.LogError(
                $"{name} 沒有設定 HeroShopData 或 HeroData",
                this
            );

            return false;
        }

        private void InitializeUI()
        {
            if (heroIcon == null)
                return;

            Sprite icon =
                shopData.heroData.icon;

            heroIcon.sprite =
                icon;

            heroIcon.enabled =
                icon != null;

            heroIcon.color =
                Color.white;

            heroIcon.preserveAspect =
                true;
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(
                    OnClickShowHero
                );
            }
        }

        private void UnregisterEvents()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(
                    OnClickShowHero
                );
            }
        }

        #endregion

        #region 按鈕事件

        private void OnClickShowHero()
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

        #endregion
    }
}