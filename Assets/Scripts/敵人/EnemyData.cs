using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 敵人靜態資料。
    /// 包含基本能力、擊殺獎勵與 Buff 光環設定。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewEnemyData",
        menuName = "CHANG/Enemy Data"
    )]
    public class EnemyData : ScriptableObject
    {
        #region 敵人基本資料

        [Header("基本資料")]

        public string enemyName;

        [Tooltip("敵人最大生命值")]
        public float maxHealth = 50f;

        [Tooltip("敵人基礎移動速度")]
        public float moveSpeed = 3f;

        [Tooltip("抵達終點後對城堡造成的傷害")]
        public int damage = 10;

        [Tooltip("擊殺後獲得的金幣")]
        public int goldReward = 10;

        [Tooltip("擊殺後英雄獲得的經驗值")]
        public int xpReward = 10;

        [Tooltip("敵人的模型 Prefab")]
        public GameObject modelPrefab;

        #endregion

        #region Buff 類型

        public enum EnemyBuffType
        {
            None,
            MoveSpeed,
            Damage,
            Defense
        }

        #endregion

        #region Buff 光環設定

        [Header("怪物 Buff 光環")]

        [Tooltip("此敵人提供的 Buff 類型")]
        public EnemyBuffType buffType =
            EnemyBuffType.None;

        [Tooltip(
            "Buff 倍率。" +
            "例如 1.2 = 增加 20%；" +
            "Defense 填 0.8 = 只受到 80% 傷害。"
        )]
        public float buffMultiplier = 1.2f;

        [Tooltip("Buff 光環半徑")]
        public float buffRadius = 6f;

        [Tooltip("光環重新檢查範圍的間隔，單位為秒")]
        public float buffUpdateInterval = 0.5f;

        [Tooltip("是否讓光環效果套用到自己")]
        public bool buffSelf;

        [Tooltip("敵人受到 Buff 時顯示的特效")]
        public GameObject buffVFX;

        #endregion
    }
}