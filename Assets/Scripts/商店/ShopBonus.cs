using UnityEngine;

namespace CHANG
{
    public static class ShopBonus
    {
        private static int GetLevel(
            ShopUpgradeType type)
        {
            return PlayerPrefs.GetInt(
                type.ToString(),
                0
            );
        }

        public static float TowerDamageMultiplier
        {
            get
            {
                int level = PlayerPrefs.GetInt(
                    ShopUpgradeType.TowerDamage.ToString(),
                    0
                );

                return 1f + level * 0.05f;
            }
        }

        public static float HeroDamageMultiplier
        {
            get
            {
                int level =
                    GetLevel(
                        ShopUpgradeType.HeroDamage
                    );

                return 1f + level * 0.03f;
            }
        }

        public static int CastleMaxHP
        {
            get
            {
                int level =
                    GetLevel(
                        ShopUpgradeType.CastleHP
                    );

                return 100 + level * 10;
            }
        }

    }
}