using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 玩家存檔資料。
    /// 目前負責管理水晶數量。
    /// </summary>
    public static class PlayerData
    {
        #region PlayerPrefs Key

        private const string CrystalKey =
            "水晶";

        #endregion

        #region 玩家貨幣

        /// <summary>
        /// 玩家目前擁有的水晶數量。
        /// </summary>
        public static int Crystal
        {
            get =>
                PlayerPrefs.GetInt(
                    CrystalKey,
                    0
                );

            set =>
                PlayerPrefs.SetInt(
                    CrystalKey,
                    value
                );
        }

        #endregion

        #region 水晶操作

        /// <summary>
        /// 增加水晶。
        /// </summary>
        public static void AddCrystal(
            int amount)
        {
            Crystal += amount;
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 消耗水晶。
        /// 成功回傳 true，失敗回傳 false。
        /// </summary>
        public static bool SpendCrystal(
            int amount)
        {
            if (Crystal < amount)
            {
                return false;
            }

            Crystal -= amount;

            PlayerPrefs.Save();

            return true;
        }

        #endregion
    }
}