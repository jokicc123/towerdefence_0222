using UnityEngine;

namespace CHANG
{
    public enum ShopUpgradeType
    {
        CastleHP,
        UnlockHero,
        TowerDamage,
        HeroDamage,
    }

    [CreateAssetMenu(menuName = "CHANG/Shop Upgrade")]
    public class ShopUpgradeData : ScriptableObject
    {
        public string upgradeName;

        public Sprite icon;

        public ShopUpgradeType type;

        public int maxLevel = 10;

        [Tooltip("每次升級需要的水晶。Element 0 = Lv0 → Lv1")]
        public int[] crystalCosts;

        public float[] values;
    }
}