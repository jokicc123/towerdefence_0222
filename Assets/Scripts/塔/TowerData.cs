using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(fileName = "NewTowerData", menuName = "Game/TowerData")]
    public class TowerData : ScriptableObject
    {
        public enum TowerAttackType
        {
            Bullet
        }
        public enum TowerEffectType
        {
            None,
            Burn, // 燒傷（持續傷害）
            Poison  , // 毒（持續傷害 + 減速）
        }
        public Vector3 buildFootprint = new Vector3(1f, 1f, 1f); // 手動在 Inspector 設定塔的佔地大小
        public TowerAttackType attackType;
        public TowerEffectType effectType;
        [Header("文字說明")]
        public string towerName;      // 防禦塔名稱（例如：火焰祭壇）
        [TextArea(3, 5)]
        public string description;    // 防禦塔詳細介紹（例如：噴射出熊熊烈火，造成持續性範圍傷害。）
        [Header("等級資料")]
        public TowerLevel[] levels; // ⭐ 核心
        [Header("模型（每級外觀）")]
        public GameObject[] levelPrefabs;
        [Header("視覺模型（每級換外觀用）")]
        public GameObject[] levelModelPrefabs; // 純模型 Prefab（換外觀用）
        [Header("圖示")]
        public Sprite icon; // 防禦塔圖示
        [Header("音效")]
        public AudioClip attackSFX;
    }

    [System.Serializable]
    public class TowerLevel
    {
        public float attackRange;
        public float damage;
        public float attackSpeed;
        public int cost;
        public GameObject bulletPrefab; // ⭐ 每級可不同子彈
        public TowerData.TowerEffectType effectType; // 每級特效類型（可選擇覆蓋或繼承）
        public float effectDuration; // 特效持續時間（如果有）
        public float effectDamagePerSecond; // 特效每秒傷害（如果有）
        public float slowPercent; // 毒藥減速百分比（如果有）
        public float blastRadius;
        public float minDamageRatio = 0.4f; // 邊緣傷害 = maxDamage × 這個值
    }

}