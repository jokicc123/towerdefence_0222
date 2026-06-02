using UnityEngine;
using System.Collections.Generic;
namespace CHANG
{

    [System.Serializable]
    public class EnemyWaveData
    {
        public GameObject enemyPrefab;
        public int count;
        public float spawnDelay;
        public int portalIndex = 0; // 預設 0 代表陸路，1 代表水路
    }
    [System.Serializable]
    public class Wave
    {
       
        public List<EnemyWaveData> enemies;
    }
   
}
