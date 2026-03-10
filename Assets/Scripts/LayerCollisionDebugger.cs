using UnityEngine;

namespace CHANG
{
    public class LayerCollisionDebugger : MonoBehaviour
    {
        [Header("要檢查的塔和敵人")]
        public GameObject tower;
        public GameObject enemy;

        void Start()
        {
            CheckCollision();
        }

        public void CheckCollision()
        {
            if (tower == null || enemy == null)
            {
                Debug.LogWarning("請先指定 Tower 和 Enemy 物件");
                return;
            }

            int towerLayer = tower.layer;
            int enemyLayer = enemy.layer;

            bool canCollide = Physics.GetIgnoreLayerCollision(towerLayer, enemyLayer) == false;

            Debug.Log($"Tower Layer: {LayerMask.LayerToName(towerLayer)} ({towerLayer})\n" +
                      $"Enemy Layer: {LayerMask.LayerToName(enemyLayer)} ({enemyLayer})\n" +
                      $"Layer Collision 設定允許碰撞? {canCollide}");
        }
    }
}