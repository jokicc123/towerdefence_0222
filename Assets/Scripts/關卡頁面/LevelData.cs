using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "TowerDefense/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("基本資訊")]
        public string levelName;        // 關卡顯示名稱，例如 "第一關"
        public Sprite thumbnail;        // 關卡縮圖（選關頁面顯示用）

        [Header("場景設定")]
        public string sceneName;        // 要載入的實際 Scene 名稱

        [Header("關卡數值")]
        public int totalWaves;          // 這關總波數，對應 GameManager.totalWaves
    }
}