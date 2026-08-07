using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理正午太陽造成的全域燃燒倍率。
    /// </summary>
    public static class BurnDamageSystem
    {
        #region  執行資料
        private static int activeSunCount;
        private static float currentMultiplier = 1f;
        #endregion 
        #region   屬性

        public static float CurrentMultiplier =>
            activeSunCount > 0
                ? currentMultiplier
                : 1f; 
        #endregion

        #region  公開方法
        public static void AddSun(float multiplier)
        {
            activeSunCount++;

            currentMultiplier =
                Mathf.Max(
                    currentMultiplier,
                    multiplier,
                    1f
                );
        }

        public static void RemoveSun()
        {
            activeSunCount =
                Mathf.Max(0, activeSunCount - 1);

            if (activeSunCount == 0)
            {
                currentMultiplier = 1f;
            }
        }

        public static void Reset()
        {
            activeSunCount = 0;
            currentMultiplier = 1f;
        } 
        #endregion
    }
}