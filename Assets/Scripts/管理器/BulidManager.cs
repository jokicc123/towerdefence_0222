using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace CHANG
{
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance;

        [Header("設定")]
        [SerializeField] private LayerMask groundLayer; // 可放置塔的地面 Layer
        [SerializeField] private Color canBuildColor = new Color(0, 1, 0, 0.5f); // 可建造顏色（綠）
        [SerializeField] private Color cantBuildColor = new Color(1, 0, 0, 0.5f); // 不可建造顏色（紅）
        [Header("建造判定用 LayerMask")]
        [SerializeField] private LayerMask buildCheckLayers; // Inspector 裡勾選：TowerBody、道路、環境裝飾
        [SerializeField] private LayerMask selectableLayerMask; // 只勾選 TowerBody（建造判定用的那個 Layer）
        [SerializeField] private Vector3 heroFootprint = new Vector3(1, 2, 1); // ⭐ 修正1：預覽跟實際放置統一用這個值

        private TowerData selectedTowerData;   // 目前選擇的塔資料
        private GameObject previewInstance;     // 預覽塔物件
        private HeroData selectedHeroData;
        private bool placingHero;
        private MeshRenderer previewRenderer;   // 預覽塔材質
        private RangeCircle previewRange;       // 攻擊範圍顯示

        private RaycastHit currentHit;          // 記錄滑鼠射線打到的資訊（用來判斷道路）

        void Awake()
        {
            Instance = this; // 單例模式，方便其他地方呼叫
        }

        void Update()
        {
            HandleTowerClick(); // 處理點擊塔/英雄的事件（升級UI / 英雄資訊UI）

            // ================================
            // 如果有預覽物件，就更新位置
            // ================================
            if (previewInstance != null)
            {
                UpdatePreview();

                bool mouseClicked =
                    Pointer.current != null ?
                    Pointer.current.press.wasPressedThisFrame :
                    Input.GetMouseButtonDown(0);

                if (mouseClicked &&
                   !EventSystem.current.IsPointerOverGameObject())
                {
                    if (placingHero)
                        TryPlaceHero();
                    else
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
        // 選擇英雄（從 UI 呼叫）
        // =========================================
        public void SelectHero(HeroData data)
        {
            if (!GameManager.Instance.CanBuildTower())
                return;

            CancelSelection();

            selectedHeroData = data;
            placingHero = true;

            if (data.prefab == null)
            {
                Debug.LogError("HeroData 缺少 Prefab");
                return;
            }

            previewInstance = Instantiate(data.prefab);

            previewRenderer =
                previewInstance.GetComponentInChildren<MeshRenderer>();

            // ⭐ 英雄目前沒有RangeCircle可顯示，previewRange維持null，UpdatePreview內有null檢查不會出錯

            // 關閉英雄邏輯
            Hero h = previewInstance.GetComponentInChildren<Hero>();
            if (h != null)
                h.enabled = false;

            // 關閉碰撞
            foreach (Collider col in previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        // =========================================
        // 更新預覽位置 + 顏色判斷
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

                // 將預覽物件「吸附」到格點（簡單網格對齊）
                Vector3 snapPos = new Vector3(
                    Mathf.Round(hit.point.x),
                    hit.point.y,
                    Mathf.Round(hit.point.z)
                );

                previewInstance.transform.position = snapPos;

                // ⭐ 修正1：預覽跟實際放置統一呼叫同一個CanBuild，用同一份footprint
                Vector3 footprint = placingHero ? heroFootprint : selectedTowerData.buildFootprint;
                bool canBuild = CanBuild(snapPos, footprint);

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
                // 沒打到地面 → 隱藏預覽物件
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
            if (CanBuild(pos, selectedTowerData.buildFootprint))
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
        // 嘗試放置英雄
        // =========================================
        private void TryPlaceHero()
        {
            // ⭐ 修正3：補上跟TryPlaceTower()一致的閘門檢查
            if (!GameManager.Instance.CanBuildTower()) return;
            if (previewInstance == null || selectedHeroData == null)
                return;

            Vector3 pos = previewInstance.transform.position;

            // ⭐ 修正1：跟預覽用同一個CanBuild + heroFootprint，不再用獨立的CanBuildHero
            if (!CanBuild(pos, heroFootprint))
            {
                Debug.Log("英雄不能放這裡");
                return;
            }

            bool success =
                HeroManager.Instance.TryPurchaseHero(
                    selectedHeroData,
                    pos
                );

            if (success)
            {
                Debug.Log($"成功放置英雄 {selectedHeroData.heroName}");
                CancelSelection();
            }
        }

        // =========================================
        // 建造規則判斷（核心邏輯，塔跟英雄共用）
        // =========================================
        public bool CanBuild(Vector3 buildPosition, Vector3 footprint)
        {
            Vector3 halfExtents = footprint / 2f;

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

                // ⭐ 合併自舊版CanBuildHero()：塔跟英雄都不能疊在已有的英雄上面
                if (col.GetComponentInParent<Hero>() != null)
                {
                    Debug.Log($"   ❌ 擋住：已有英雄 {col.name}");
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
            selectedHeroData = null;
            placingHero = false;

            if (previewInstance != null)
                Destroy(previewInstance);
        }

        void HandleTowerClick()
        {
            if (!GameManager.Instance.CanBuildTower()) return;
            if (previewInstance != null)
            {
                Debug.Log("❌ 有預覽物件，跳出");
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
                selectableLayerMask, QueryTriggerInteraction.Collide))
            {
                // 點塔
                Tower tower = hit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    UiManager.Instance.ShowUpgradeUI(tower);
                    return;
                }

                // 點英雄
                Hero hero = hit.collider.GetComponentInParent<Hero>();
                if (hero != null)
                {
                    UiManager.Instance.ShowHeroUI(hero);
                    return;
                }

                Debug.Log("❌ 打到物件但不是塔或英雄");
            }
        }
    }
}