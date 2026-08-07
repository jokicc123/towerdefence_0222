using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    #region 路線資料結構

    /// <summary>
    /// 敵人生成路線資料。
    /// 綁定生成傳送門與專屬 Waypoints。
    /// </summary>
    [System.Serializable]
    public class PortalRoute
    {
        public string routeName;

        public Transform spawnPortal;

        public Transform[] waypoints;
    }

    #endregion


    /// <summary>
    /// 管理關卡敵人波數、生成與多路線設定。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        #region Inspector 設定

        [Header("波數設定")]
        [SerializeField]
        private List<Wave> waves = new();

        [Header("多路線設定")]
        [Tooltip(
            "Element 0、1... 對應 EnemySpawnData 的 portalIndex。"
        )]
        [SerializeField]
        private PortalRoute[] routes;

        [Header("波次設定")]
        [SerializeField, Min(0f)]
        private float waveInterval = 10f;

        #endregion

        #region 執行期間資料

        private int currentWaveIndex;

        #endregion

        #region Unity 生命週期

        private IEnumerator Start()
        {
            yield return new WaitUntil(
                () =>
                    GameManager.Instance != null &&
                    GameManager.Instance.CurrentState ==
                    GameManager.GameState.Playing
            );

            GameManager.Instance.SetTotalWaves(
                waves.Count
            );

            yield return StartCoroutine(
                SpawnWaves()
            );
        }

        #endregion

        #region 波數流程

        private IEnumerator SpawnWaves()
        {
            while (currentWaveIndex < waves.Count)
            {
                GameManager.Instance.StartNextWave();

                yield return StartCoroutine(
                    SpawnWave()
                );

                yield return new WaitForSeconds(
                    waveInterval
                );

                yield return new WaitUntil(
                    () =>
                        FindObjectsByType<Enemy>(
                            FindObjectsSortMode.None
                        ).Length == 0
                );

                currentWaveIndex++;
            }

#if UNITY_EDITOR
            Debug.Log(
                "所有波數生成完成",
                this
            );
#endif

            GameManager.Instance.CheckWin();
        }

        #endregion

        #region 單波生成

        private IEnumerator SpawnWave()
        {
            Wave wave =
                waves[currentWaveIndex];

            foreach (var enemyData in wave.enemies)
            {
                for (int i = 0;
                     i < enemyData.count;
                     i++)
                {
                    SpawnEnemy(enemyData);

                    yield return new WaitForSeconds(
                        enemyData.spawnDelay
                    );
                }
            }
        }

        #endregion

        #region 敵人生成

        private void SpawnEnemy(
            EnemyWaveData enemyData)
        {
            if (enemyData.enemyPrefab == null)
            {
                Debug.LogError(
                    "EnemyWaveData 沒有設定 Enemy Prefab",
                    this
                );

                return;
            }

            int portalIndex =
                enemyData.portalIndex;

            if (TryGetRoute(
                    portalIndex,
                    out PortalRoute route))
            {
                GameObject enemyObject =
                    Instantiate(
                        enemyData.enemyPrefab,
                        route.spawnPortal.position,
                        Quaternion.identity
                    );

                if (enemyObject.TryGetComponent(
                        out Enemy enemy))
                {
                    enemy.InitializePath(
                        route.waypoints
                    );
                }

                return;
            }

            // 找不到路線時使用 Spawner 位置作為備援。
            Instantiate(
                enemyData.enemyPrefab,
                transform.position,
                Quaternion.identity
            );

#if UNITY_EDITOR
            Debug.LogWarning(
                $"找不到 Portal Index {portalIndex}，" +
                $"已改用 {name} 的位置生成。",
                this
            );
#endif
        }

        private bool TryGetRoute(
            int index,
            out PortalRoute route)
        {
            route = null;

            if (routes == null ||
                index < 0 ||
                index >= routes.Length)
            {
                return false;
            }

            PortalRoute selectedRoute =
                routes[index];

            if (selectedRoute == null ||
                selectedRoute.spawnPortal == null)
            {
                return false;
            }

            route = selectedRoute;
            return true;
        }

        #endregion
    }
}