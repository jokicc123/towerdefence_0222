using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(
        fileName = "NewHeroShopData",
        menuName = "CHANG/Hero Shop Data"
    )]
    public class HeroShopData : ScriptableObject
    {
        [Header("英雄資料")]
        public HeroData heroData;

        [Header("購買設定")]
        public int crystalCost = 500;

        [Tooltip("預設英雄可勾選，代表一開始就已解鎖")]
        public bool unlockedByDefault;

        public string SaveKey
        {
            get
            {
                if (heroData == null)
                    return "HeroUnlocked_Invalid";

                return $"HeroUnlocked_{heroData.name}";
            }
        }
    }
}