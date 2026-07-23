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
        [Header("建造判定用 LayerMask")]
        [SerializeField] private LayerMask buildCheckLayers; // Inspector 裡勾選：TowerBody、道路、環境裝飾

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

            HandleTowerClick(); // 處理點擊塔的事件（升級 UI）
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
            if (!GameManager.Instance.CanBuildTower()) return;
            CancelSelection(); // 先清掉舊預覽

            selectedTowerData = data;

            if (data.levelPrefabs[0] == null)
            {
                Debug.LogError("TowerData 缺少 Prefab！");
                return;
            }

            // 建立預覽塔
            previewInstance = Instantiate(data.levelPrefabs[0]);

            // 顯示攻擊範圍
            previewRange = previewInstance.GetComponentInChildren<RangeCircle>();
            if (previewRange != null)
            {
                previewRange.DrawCircle(data.levels[0].attackRange);
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
            if (!GameManager.Instance.CanBuildTower()) return;
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
                bool canBuild = CanBuild(snapPos,selectedTowerData);

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
            if (!GameManager.Instance.CanBuildTower()) return;
            if (previewInstance == null || selectedTowerData == null)
                return;

            Vector3 pos = previewInstance.transform.position;

            // 再次確認能不能建造
            if (CanBuild(pos,selectedTowerData))
            {
                // 檢查金幣是否足夠
                if (!GameManager.Instance.SpendGold(selectedTowerData.levels[0].cost))
                {
                    Debug.LogWarning("金幣不足！");
                    return;
                }

                // 建立正式塔
                GameObject newTower = Instantiate(
                    selectedTowerData.levelPrefabs[0],
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
        // =========================================
        // 建造規則判斷（核心邏輯 - 完美修正版）
        // =========================================
        public bool CanBuild(Vector3 buildPosition, TowerData towerData)
        {
            Vector3 halfExtents = towerData.buildFootprint / 2f;

            Collider[] colliders = Physics.OverlapBox(
                buildPosition,
                halfExtents,
                Quaternion.identity,
                buildCheckLayers,
                QueryTriggerInteraction.Collide
            );

            Debug.Log($"🔍 OverlapBox 抓到 {colliders.Length} 個 Collider");
            foreach (var c in colliders)
            {
                Debug.Log($"   - {c.name} | Layer: {LayerMask.LayerToName(c.gameObject.layer)} | Tag: {c.tag} | IsTrigger: {c.isTrigger}");
            }

            foreach (var col in colliders)
            {
                if (((1 << col.gameObject.layer) & groundLayer) != 0)
                {
                    Debug.Log($"   ⏭️ 跳過地面: {col.name}");
                    continue;
                }

                if (col.CompareTag("道路") || col.CompareTag("環境裝飾"))
                {
                    Debug.Log($"   ❌ 擋住：{col.name} (Tag: {col.tag})");
                    return false;
                }

                if (col.GetComponentInParent<Tower>() != null)
                {
                    Debug.Log($"   ❌ 擋住：已有塔 {col.name}");
                    return false;
                }
            }

            Debug.Log("   ✅ 可以建造");
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
        void HandleTowerClick()
        {
            if (!GameManager.Instance.CanBuildTower()) return;
            if (previewInstance != null)
            {
                Debug.Log("❌ 有預覽塔，跳出");
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("❌ 點到UI，跳出");
                return;
            }

            Debug.Log("✅ 開始射線");

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                Debug.Log($"✅ 射線打到: {hit.collider.name} | Tag: {hit.collider.tag}");

                Tower tower = hit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    Debug.Log("✅ 找到 Tower，開啟UI");
                    UiManager.Instance.ShowUpgradeUI(tower);
                    return;
                }
                else
                {
                    Debug.Log("❌ 打到物件但沒有 Tower 腳本");
                }
            }
            else
            {
                Debug.Log("❌ 射線沒有打到任何東西");
            }

            UiManager.Instance.HideUpgradeUI();
        }
    }
}