using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    // ✨ 新增：用來在 Inspector 綁定「傳送門」與「專屬路徑點」的資料結構
    [System.Serializable]
    public class PortalRoute
    {
        public string routeName;       // 路線標籤（例如：陸路、水路）
        public Transform spawnPortal;  // 該路線的傳送門起點
        public Transform[] waypoints;  // 該路線所有的 Waypoints 路點
     
    }

    public class EnemySpawner : MonoBehaviour
    {
        public List<Wave> waves;

        [Header("多路線設定")]
        [Tooltip("Element 0 放原本的陸路設定，Element 1 放水路設定。這裡的順序要對應 enemyData.portalIndex 喔！")]
        public PortalRoute[] routes;

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
                    // 💡 檢查是否有設定路線，且資料裡的 portalIndex 有在範圍內
                    // 備註：請確保你定義 enemyData 的類別（例如 EnemySpawnData）裡面有 public int portalIndex; 這個變數
                    int pIndex = enemyData.portalIndex;

                    // 這裡利用反射或直接讀取來安全取得 portalIndex（假設你已經在你的 Wave/EnemyData 結構裡加了這個欄位）
                    // 如果你還沒加，可以去定義 wave.enemies 的那個 class 補上 public int portalIndex = 0;
                    // pIndex = enemyData.portalIndex; 

                    if (routes != null && pIndex < routes.Length)
                    {
                        PortalRoute selectedRoute = routes[pIndex];

                        // ✨ 修改：不在 spawner 本身位置生成，改在「指定傳送門」的位置生成
                        GameObject enemyObj = Instantiate(
                            enemyData.enemyPrefab,
                            selectedRoute.spawnPortal.position,
                            Quaternion.identity
                        );

                        // ✨ 修改：生成後，立刻把這條路徑的 waypoints 陣列塞給怪物
                        if (enemyObj.TryGetComponent(out Enemy enemyScript))
                        {
                            enemyScript.InitializePath(selectedRoute.waypoints);
                        }
                    }
                    else
                    {
                        // 備援方案：如果 Index 超出範圍，就用原本 Spawner 的位置防呆
                        Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);
                        Debug.LogWarning($"Portal Index 找不到對應路線，已使用預設位置生成！");
                    }

                    yield return new WaitForSeconds(enemyData.spawnDelay);
                }
            }

            currentWaveIndex++;
        }
    }
}