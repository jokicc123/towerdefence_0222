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

    }
    [System.Serializable]
    public class Wave
    {
       
        public List<EnemyWaveData> enemies;
    }
   
}
