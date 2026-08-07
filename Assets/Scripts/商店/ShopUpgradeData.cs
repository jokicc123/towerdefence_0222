using UnityEngine;

namespace CHANG
{
    #region 商店升級類型

    public enum ShopUpgradeType
    {
        CastleHP,
        UnlockHero,
        TowerDamage,
        HeroDamage
    }

    #endregion

    /// <summary>
    /// 商店永久升級資料。
    /// 記錄升級名稱、圖示、類型、最高等級、價格與每級效果。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewShopUpgradeData",
        menuName = "CHANG/Shop Upgrade"
    )]
    public class ShopUpgradeData : ScriptableObject
    {
        #region 基本資料

        [Header("基本資料")]

        public string upgradeName;

        public Sprite icon;

        public ShopUpgradeType type;

        #endregion

        #region 升級設定

        [Header("升級設定")]

        [Min(1)]
        public int maxLevel = 10;

        [Tooltip(
            "每次升級需要消耗的水晶。" +
            "Element 0 = Lv.0 → Lv.1"
        )]
        public int[] crystalCosts;

        [Tooltip(
            "每個等級對應的實際效果值。" +
            "通常 Element 0 = Lv.0 的效果。"
        )]
        public float[] values;

        #endregion
    }
}