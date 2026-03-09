using UnityEngine;
using System.Collections;
namespace CHANG 
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("怪獸設定")]
        public GameObject enemyPrefab;  // 怪物的預製物件
        public float spawnInterval = 2f; // 每幾秒生一隻

        [Header("波次設定")]
        public int enemiesPerWave = 10;
        private int spawnedCount = 0;

        void Start()
        {
            // 開始生怪循環
            StartCoroutine(SpawnRoutine());
        }

        IEnumerator SpawnRoutine()
        {
            while (spawnedCount < enemiesPerWave)
            {
                SpawnEnemy();
                spawnedCount++;

                // 等待間隔時間
                yield return new WaitForSeconds(spawnInterval);
            }
            Debug.Log("本波怪物已生完！");
        }

        void SpawnEnemy()
        {
            if (enemyPrefab != null)
            {
                // 在生怪點的位置生成怪物
                GameObject enemy = Instantiate(enemyPrefab, transform.position,Quaternion.Euler(0,0,0));

                // 如果你的地圖座標很大，建議在這裡強制檢查一次座標
                // Debug.Log($"怪物生成於：{enemy.transform.position}");
            }
        }
    }
}
