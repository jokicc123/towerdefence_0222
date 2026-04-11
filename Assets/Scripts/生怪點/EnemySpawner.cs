using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    public class EnemySpawner : MonoBehaviour
    {
        public List<Wave> waves;

        private int currentWaveIndex = 0;

        void Start()
        {
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            while (currentWaveIndex < waves.Count)
            {
                yield return StartCoroutine(SpawnWave());

                // 每波間隔
                yield return new WaitForSeconds(2f);
            }

            Debug.Log("所有波數生成完成");

            GameManager.Instance.CheckWin();
        }

        private IEnumerator SpawnWave()
        {
            // 🔥 更新波數 UI
            GameManager.Instance.StartNextWave();

            Wave wave = waves[currentWaveIndex];

            foreach (var enemyData in wave.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);

                    yield return new WaitForSeconds(enemyData.spawnDelay);
                }
            }

            currentWaveIndex++;
        }
    }
}