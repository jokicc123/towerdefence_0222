using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理英雄商店的解鎖、購買與選擇狀態。
    /// </summary>
    public class HeroShopManager : MonoBehaviour
    {
        #region Singleton

        public static HeroShopManager Instance
        {
            get;
            private set;
        }

        #endregion

        #region 常數

        private const string SelectedHeroKey =
            "SelectedHero";

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
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 英雄解鎖

        public bool IsUnlocked(
            HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return false;
            }

            if (shopData.unlockedByDefault)
            {
                return true;
            }

            return PlayerPrefs.GetInt(
                shopData.SaveKey,
                0
            ) == 1;
        }

        #endregion

        #region 英雄購買

        public bool BuyHero(
            HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return false;
            }

            if (IsUnlocked(shopData))
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"{shopData.heroData.heroName} 已經解鎖",
                    this
                );
#endif

                return false;
            }

            if (!PlayerData.SpendCrystal(
                    shopData.crystalCost))
            {
#if UNITY_EDITOR
                Debug.Log(
                    "水晶不足",
                    this
                );
#endif

                return false;
            }

            PlayerPrefs.SetInt(
                shopData.SaveKey,
                1
            );

            PlayerPrefs.Save();

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RefreshCrystalUI();
            }

#if UNITY_EDITOR
            Debug.Log(
                $"成功解鎖英雄：" +
                $"{shopData.heroData.heroName}",
                this
            );
#endif

            return true;
        }

        #endregion

        #region 英雄選擇

        public void SelectHero(
            HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return;
            }

            if (!IsUnlocked(shopData))
            {
#if UNITY_EDITOR
                Debug.Log(
                    "尚未解鎖此英雄",
                    this
                );
#endif

                return;
            }

            PlayerPrefs.SetString(
                SelectedHeroKey,
                shopData.heroData.name
            );

            PlayerPrefs.Save();

#if UNITY_EDITOR
            Debug.Log(
                $"已選擇英雄：" +
                $"{shopData.heroData.heroName}",
                this
            );
#endif
        }

        #endregion

        #region 選擇狀態查詢

        public string GetSelectedHeroName()
        {
            return PlayerPrefs.GetString(
                SelectedHeroKey,
                string.Empty
            );
        }

        public bool IsSelected(
            HeroShopData shopData)
        {
            if (shopData == null ||
                shopData.heroData == null)
            {
                return false;
            }

            return GetSelectedHeroName() ==
                   shopData.heroData.name;
        }

        #endregion
    }
}