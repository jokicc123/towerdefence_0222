using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 關卡資料，用於選關與載入關卡。
    /// </summary>
    [CreateAssetMenu(
        fileName = "LevelData",
        menuName = "TowerDefense/LevelData"
    )]
    public class LevelData : ScriptableObject
    {
        #region Inspector 設定

        [Header("基本資訊")]
        [SerializeField] private string levelName;

        [Header("場景設定")]
        [SerializeField] private string sceneName;

        [Header("關卡數值")]
        [SerializeField] private int totalWaves;

        #endregion

        #region 屬性

        public string LevelName => levelName;

        public string SceneName => sceneName;

        public int TotalWaves => totalWaves;

        #endregion
    }
}