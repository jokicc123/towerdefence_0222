using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 商店永久加成。
    /// 依照商店升級等級，提供遊戲中的各項倍率。
    /// </summary>
    public static class ShopBonus
    {
        #region 讀取升級等級

        /// <summary>
        /// 取得指定商店升級目前等級。
        /// </summary>
        private static int GetLevel(
            ShopUpgradeType type)
        {
            return PlayerPrefs.GetInt(
                type.ToString(),
                0
            );
        }

        #endregion

        #region 防禦塔加成

        /// <summary>
        /// 防禦塔傷害倍率。
        /// 每級增加 5%。
        /// </summary>
        public static float TowerDamageMultiplier
        {
            get
            {
                int level =
                    GetLevel(
                        ShopUpgradeType.TowerDamage
                    );

                return 1f + level * 0.05f;
            }
        }

        #endregion

        #region 英雄加成

        /// <summary>
        /// 英雄傷害倍率。
        /// 每級增加 3%。
        /// </summary>
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

        #endregion

        #region 城堡加成

        /// <summary>
        /// 城堡最大生命值。
        /// 每級增加 10 點生命。
        /// </summary>
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

        #endregion
    }
}