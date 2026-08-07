using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 暫存目前玩家選擇的關卡資料，
    /// 用於場景切換時傳遞資訊。
    /// </summary>
    public static class LevelSession
    {
        #region 關卡資料

        public static LevelData SelectedLevel { get; private set; }

        #endregion

        #region 公開方法

        /// <summary>
        /// 設定目前選擇的關卡。
        /// </summary>
        public static void SelectLevel(LevelData level)
        {
            SelectedLevel = level;
        }

        /// <summary>
        /// 清除目前選擇的關卡。
        /// </summary>
        public static void Clear()
        {
            SelectedLevel = null;
        }

        #endregion
    }
}