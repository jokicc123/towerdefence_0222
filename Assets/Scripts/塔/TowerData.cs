
using UnityEngine;
namespace CHANG 
{
    [CreateAssetMenu(fileName = "NewTowerData", menuName = "Game/TowerData")]
    public class TowerData : ScriptableObject
    {
        public float attackRange = 5f;
        public float damage = 10f;
        public float attackSpeed = 2f;
        public  GameObject bulletPrefab;
        public int cost = 100;
        public GameObject towerModelPrefab; // 用於預覽和實際放置的模型   
        public string towerName;
    }
}
