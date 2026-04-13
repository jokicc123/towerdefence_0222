using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace CHANG
{
    public class TowerManager : MonoBehaviour
    {
        public static TowerManager Instance;

        [Header("設定")]
        [SerializeField] private LayerMask groundLayer; // 可放置塔的地面 Layer
        [SerializeField] private Color canBuildColor = new Color(0, 1, 0, 0.5f); // 可建造顏色（綠）
        [SerializeField] private Color cantBuildColor = new Color(1, 0, 0, 0.5f); // 不可建造顏色（紅）

        private TowerData selectedTowerData;   // 目前選擇的塔資料
        private GameObject previewInstance;     // 預覽塔物件
        private MeshRenderer previewRenderer;   // 預覽塔材質
        private RangeCircle previewRange;       // 攻擊範圍顯示

        private RaycastHit currentHit;          // 記錄滑鼠射線打到的資訊（用來判斷道路）

        void Awake()
        {
            Instance = this; // 單例模式，方便其他地方呼叫
        }

        void Update()
        {
            // ================================
            // 如果有預覽塔，就更新位置
            // ================================
            if (previewInstance != null)
            {
                UpdatePreview();

                // 偵測左鍵（支援新 Input System + 舊 Input）
                bool mouseClicked = Pointer.current != null
                    ? Pointer.current.press.wasPressedThisFrame
                    : Input.GetMouseButtonDown(0);

                // 點擊左鍵且沒有點在 UI 上 → 嘗試建造
                if (mouseClicked && !EventSystem.current.IsPointerOverGameObject())
                {
                    TryPlaceTower();
                }
            }

            // ================================
            // 右鍵取消建造（隨時可用）
            // ================================
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelSelection();
            }
        }

        // =========================================
        // 選擇塔（從 UI 呼叫）
        // =========================================
        public void SelectTower(TowerData data)
        {
            CancelSelection(); // 先清掉舊預覽

            selectedTowerData = data;

            if (data.towerModelPrefab == null)
            {
                Debug.LogError("TowerData 缺少 Prefab！");
                return;
            }

            // 建立預覽塔
            previewInstance = Instantiate(data.towerModelPrefab);

            // 顯示攻擊範圍
            previewRange = previewInstance.GetComponentInChildren<RangeCircle>();
            if (previewRange != null)
            {
                previewRange.DrawCircle(data.attackRange);
            }

            // 取得渲染器（用來改顏色）
            previewRenderer = previewInstance.GetComponentInChildren<MeshRenderer>();

            // 關閉塔的邏輯（避免預覽塔攻擊或運作）
            if (previewInstance.TryGetComponent(out Tower t))
                t.enabled = false;

            // 關閉所有碰撞（避免影響射線）
            foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        // =========================================
        // 更新預覽塔位置 + 顏色判斷
        // =========================================
        private void UpdatePreview()
        {
            // 取得滑鼠位置
            Vector2 mousePosition = Pointer.current != null
                ? Pointer.current.position.ReadValue()
                : (Vector2)Input.mousePosition;

            // 從相機射線打到地面
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 20000f, groundLayer))
            {
                // 記錄目前打到的物件（用來判斷是否道路）
                currentHit = hit;

                previewInstance.SetActive(true);

                // 將塔「吸附」到格點（簡單網格對齊）
                Vector3 snapPos = new Vector3(
                    Mathf.Round(hit.point.x),
                    hit.point.y,
                    Mathf.Round(hit.point.z)
                );

                previewInstance.transform.position = snapPos;

                // 判斷是否可以建造
                bool canBuild = CanBuild(snapPos);

                // 決定顏色（綠 or 紅）
                Color stateColor = canBuild ? canBuildColor : cantBuildColor;

                // 改變塔顏色
                if (previewRenderer != null)
                {
                    previewRenderer.material.color = stateColor;
                }

                // 改變範圍顏色
                if (previewRange != null)
                {
                    previewRange.SetColor(stateColor);
                }
            }
            else
            {
                // 沒打到地面 → 隱藏預覽塔
                previewInstance.SetActive(false);
            }
        }

        // =========================================
        // 嘗試放置塔
        // =========================================
        private void TryPlaceTower()
        {
            if (previewInstance == null || selectedTowerData == null)
                return;

            Vector3 pos = previewInstance.transform.position;

            // 再次確認能不能建造
            if (CanBuild(pos))
            {
                // 檢查金幣是否足夠
                if (!GameManager.Instance.SpendGold(selectedTowerData.cost))
                {
                    Debug.LogWarning("金幣不足！");
                    return;
                }

                // 建立正式塔
                GameObject newTower = Instantiate(
                    selectedTowerData.towerModelPrefab,
                    pos,
                    Quaternion.identity
                );

                // 初始化塔
                if (newTower.TryGetComponent(out Tower towerScript))
                {
                    towerScript.Initialize(selectedTowerData);
                }

                Debug.Log($"成功建造: {selectedTowerData.towerName}");

                // 清除預覽
                CancelSelection();
            }
        }

        // =========================================
        // 建造規則判斷（核心邏輯）
        // =========================================
        private bool CanBuild(Vector3 pos)
        {
            // 如果射線打到的是道路 → 不能建造
            if (currentHit.collider != null &&
                currentHit.collider.CompareTag("道路"))
                return false;

            // 如果範圍內已有塔 → 不能建造
            if (Physics.CheckSphere(pos, 0.4f, LayerMask.GetMask("Tower")))
                return false;

            // 其他情況都可以建造
            return true;
        }

        // =========================================
        // 取消建造 / 清除預覽
        // =========================================
        private void CancelSelection()
        {
            selectedTowerData = null;
            previewRange = null;

            if (previewInstance != null)
                Destroy(previewInstance);
        }
    }
}