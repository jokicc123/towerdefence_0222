using UnityEngine;
using UnityEngine.UIElements;

namespace CHANG
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        private float currentHealth;
        private bool isDead = false;

        // --- 路徑相關 ---
        private Transform targetPoint;
        private int wavePointIndex = 0;
        Rigidbody rb;

        public float MoveSpeed => data != null ? data.moveSpeed : 3f;

        private void Awake()
        {
            if (data != null) currentHealth = data.maxHealth;
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // 如果怪物出生時路徑點還沒抓好，強迫路徑腳本動起來
            if (Waypoints.Points == null || Waypoints.Points.Length == 0)
            {
                Waypoints wp = Object.FindFirstObjectByType<Waypoints>();
                if (wp != null) wp.InitializePoints();
            }

            // 重新檢查一次
            if (Waypoints.Points != null && Waypoints.Points.Length > 0)
            {
                targetPoint = Waypoints.Points[0];
                Debug.Log("怪物已鎖定第一個目標點");
            }
            else
            {
                Debug.LogError("錯誤：場景中找不到任何路徑點！請檢查 Path 物件下是否有子物件。");
            }
        }

        private void Update()
        {
            if (targetPoint == null) return;
            Move();
        }

        private void Move()
        {
            if (targetPoint == null) return;

            // 取得目標方向
            Vector3 dir = targetPoint.position - transform.position;

            // 移動
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, MoveSpeed * Time.deltaTime);

            // 【核心修正】忽略 Y 軸高度差來判斷是否到達
            Vector3 flatPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 flatTarget = new Vector3(targetPoint.position.x, 0, targetPoint.position.z);

            if (Vector3.Distance(flatPosition, flatTarget) <= 0.2f)
            {
                GetNextWaypoint();
            }

        }

        private void GetNextWaypoint()
        {
            // 檢查是否還有下一個點
            if (wavePointIndex >= Waypoints.Points.Length - 1)
            {
                GameManager.Instance.TakeDamege(data.damage);
                Die();

                return;
            }

            wavePointIndex++;
            targetPoint = Waypoints.Points[wavePointIndex];
            // 讓敵人旋轉
            transform.eulerAngles = Waypoints.RotationPoints[wavePointIndex];
        }


        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0) Die();
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            // 確保只有敵人會被摧毀，避免誤刪其他物件
            if (!CompareTag("敵人"))
            {
                Destroy(gameObject);
                return;
            }

            GameManager.Instance.AddGold(data.goldReward);

            Debug.Log($"{data.enemyName} 死亡");
            Destroy(gameObject);
        }
    }
}