using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    #region 敵人波數資料

    /// <summary>
    /// 單一敵人群組的生成設定。
    /// </summary>
    [System.Serializable]
    public class EnemyWaveData
    {
        [Tooltip("要生成的敵人 Prefab")]
        public GameObject enemyPrefab;

        [Tooltip("生成數量")]
        public int count = 1;

        [Tooltip("每隻敵人之間的生成間隔")]
        public float spawnDelay = 1f;

        [Tooltip("對應 EnemySpawner.routes 的索引。0 = 陸路，1 = 水路。")]
        public int portalIndex;
    }

    #endregion

    #region 波數資料

    /// <summary>
    /// 單一波次的敵人生成資料。
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        [Header("敵人群組")]
        public List<EnemyWaveData> enemies =
            new();
    }

    #endregion
}