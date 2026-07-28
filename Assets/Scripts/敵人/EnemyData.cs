using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "CHANG/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public float maxHealth = 50f;
        public float moveSpeed = 3f;
        public int damage=10; // 對城堡造成的傷害
        public int goldReward = 10;// 擊殺獎勵
        public GameObject modelPrefab; // 敵人的模型
        public int xpReward = 10; // 擊殺獎勵經驗值
    }
}