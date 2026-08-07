using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 英雄商店資料。
    /// 記錄商店販售的英雄、價格與解鎖資訊。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewHeroShopData",
        menuName = "CHANG/Hero Shop Data"
    )]
    public class HeroShopData : ScriptableObject
    {
        #region 英雄資料

        [Header("英雄資料")]
        public HeroData heroData;

        #endregion

        #region 購買設定

        [Header("購買設定")]

        [Tooltip("解鎖此英雄需要消耗的水晶")]
        public int crystalCost = 500;

        [Tooltip("勾選後代表遊戲開始時就已解鎖")]
        public bool unlockedByDefault;

        #endregion

        #region 存檔資訊

        /// <summary>
        /// 此英雄對應的 PlayerPrefs Key。
        /// </summary>
        public string SaveKey
        {
            get
            {
                if (heroData == null)
                {
                    return "HeroUnlocked_Invalid";
                }

                return
                    $"HeroUnlocked_{heroData.name}";
            }
        }

        #endregion
    }
}