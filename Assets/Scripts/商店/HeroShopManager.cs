using UnityEngine;

namespace CHANG
{
    public class HeroShopManager : MonoBehaviour
    {
        public static HeroShopManager Instance { get; private set; }

        private const string SelectedHeroKey = "SelectedHero";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsUnlocked(HeroShopData shopData)
        {
            if (shopData == null || shopData.heroData == null)
                return false;

            if (shopData.unlockedByDefault)
                return true;

            return PlayerPrefs.GetInt(
                shopData.SaveKey,
                0
            ) == 1;
        }

        public bool BuyHero(HeroShopData shopData)
        {
            if (shopData == null || shopData.heroData == null)
                return false;

            if (IsUnlocked(shopData))
            {
                Debug.Log($"{shopData.heroData.heroName} 已經解鎖");
                return false;
            }

            if (!PlayerData.SpendCrystal(shopData.crystalCost))
            {
                Debug.Log("水晶不足");
                return false;
            }

            PlayerPrefs.SetInt(shopData.SaveKey, 1);
            PlayerPrefs.Save();
            ShopManager.Instance.RefreshCrystalUI();
            Debug.Log(
                $"成功解鎖英雄：{shopData.heroData.heroName}"
            );

            return true;
        }

        public void SelectHero(HeroShopData shopData)
        {
            if (shopData == null || shopData.heroData == null)
                return;

            if (!IsUnlocked(shopData))
            {
                Debug.Log("尚未解鎖此英雄");
                return;
            }

            PlayerPrefs.SetString(
                SelectedHeroKey,
                shopData.heroData.name
            );

            PlayerPrefs.Save();

            Debug.Log(
                $"已選擇英雄：{shopData.heroData.heroName}"
            );

        }

        public string GetSelectedHeroName()
        {
            return PlayerPrefs.GetString(
                SelectedHeroKey,
                string.Empty
            );
        }

        public bool IsSelected(HeroShopData shopData)
        {
            if (shopData == null || shopData.heroData == null)
                return false;

            return GetSelectedHeroName() ==
                   shopData.heroData.name;
        }

    }
}