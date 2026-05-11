using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(fileName = "NewTowerData", menuName = "Game/TowerData")]
    public class TowerData : ScriptableObject
    {
        public enum TowerAttackType
        {
            Bullet,
            Flame
        }

        public TowerAttackType attackType;

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
    }
}