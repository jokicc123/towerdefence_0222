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

        public TowerAttackType attackType;
        public TowerEffectType effectType;

        [Header("等級資料")]
        public TowerLevel[] levels; // ⭐ 核心
        [Header("模型（每級外觀）")]
        public GameObject[] levelPrefabs;
        [Header("視覺模型（每級換外觀用）")]
        public GameObject[] levelModelPrefabs; // 純模型 Prefab（換外觀用）
        public string towerName;

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
    }

}