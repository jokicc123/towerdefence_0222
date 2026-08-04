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

        public enum EnemyBuffType
        {
            None,
            MoveSpeed,
            Damage,
            Defense
        }
        [Header("怪物 Buff 光環")]
        public EnemyBuffType buffType = EnemyBuffType.None;

        [Tooltip("例如 1.2 = 增加20%；防禦填0.8 = 只受到80%傷害")]
        public float buffMultiplier = 1.2f;

        [Tooltip("Buff 光環範圍")]
        public float buffRadius = 6f;

        [Tooltip("光環檢查間隔")]
        public float buffUpdateInterval = 0.5f;

        [Tooltip("是否強化自己")]
        public bool buffSelf = false;

        [Tooltip("受到 Buff 時顯示的特效")]
        public GameObject buffVFX;
    }
}