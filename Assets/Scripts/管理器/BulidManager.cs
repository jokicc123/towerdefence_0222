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
        private Tower movingTower;// 目前正在移動的塔（如果有的話）
        private bool isMovingTower;

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

            if (data == null || data.prefab == null)
            {
                Debug.LogError("HeroData 或英雄 Prefab 沒有設定");
                return;
            }

            selectedHeroData = data;
            placingHero = true;

            previewInstance = Instantiate(data.prefab);

            // 關閉預覽英雄邏輯，避免攻擊、光環和註冊英雄
            Hero previewHero =
                previewInstance.GetComponentInChildren<Hero>();

            if (previewHero != null)
            {
                previewHero.enabled = false;
            }
            else
            {
                Debug.LogWarning("英雄預覽物件找不到 Hero 腳本");
            }
            foreach (Animator previewAnimator in
             previewInstance.GetComponentsInChildren<Animator>(true))
            {
                previewAnimator.SetBool("Jump", false);
                previewAnimator.ResetTrigger("Attack");
                previewAnimator.Play("Idle", 0, 0f);
                previewAnimator.Update(0f);
                previewAnimator.speed = 0f;
            }
            previewRenderer =
                previewInstance.GetComponentInChildren<MeshRenderer>();

            // 關閉碰撞，避免預覽物件擋住射線與建造判斷
            foreach (Collider col in
                     previewInstance.GetComponentsInChildren<Collider>())
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

                Vector3 footprint;

                if (placingHero)
                {
                    footprint = heroFootprint;
                }
                else if (isMovingTower && movingTower != null)
                {
                    footprint = movingTower.data.buildFootprint;
                }
                else
                {
                    if (selectedTowerData == null)
                        return;

                    footprint = selectedTowerData.buildFootprint;
                }
                
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
            if (!GameManager.Instance.CanBuildTower())
                return;

            if (previewInstance == null)
                return;

            Vector3 pos =
                previewInstance.transform.position;

            Vector3 footprint;

            if (isMovingTower && movingTower != null)
            {
                footprint = movingTower.data.buildFootprint;
            }
            else
            {
                if (selectedTowerData == null)
                    return;

                footprint = selectedTowerData.buildFootprint;
            }

            if (!CanBuild(pos, footprint))
            {
                Debug.Log("目前位置不能放置");
                return;
            }

            // ==========================
            // 正在重新放置舊塔
            // ==========================
            if (isMovingTower && movingTower != null)
            {
                movingTower.transform.position = pos;
                movingTower.gameObject.SetActive(true);

                Debug.Log($"塔已移動到：{pos}");

                Destroy(previewInstance);

                previewInstance = null;
                previewRenderer = null;
                previewRange = null;

                movingTower = null;
                isMovingTower = false;
                selectedTowerData = null;

                return;
            }

            // ==========================
            // 原本的新建塔邏輯
            // ==========================
            if (!GameManager.Instance.SpendGold(
                    selectedTowerData.levels[0].cost))
            {
                Debug.LogWarning("金幣不足！");
                return;
            }

            GameObject newTower = Instantiate(
                selectedTowerData.levelPrefabs[0],
                pos,
                Quaternion.identity
            );

            if (newTower.TryGetComponent(out Tower towerScript))
            {
                towerScript.Initialize(selectedTowerData);
            }

            Debug.Log($"成功建造：{selectedTowerData.towerName}");

            CancelSelection();
        }



        // =========================================
        // 嘗試放置英雄
        // =========================================
        private void TryPlaceHero()
        {
            if (!placingHero)
                return;

            if (selectedHeroData == null)
                return;

            if (previewInstance == null)
                return;

            Vector3 placePosition =
                previewInstance.transform.position;

            if (!CanBuildHero(placePosition))
            {
                Debug.Log("這裡不能放置英雄");
                return;
            }

            bool purchased =
                HeroManager.Instance.TryPurchaseHero(
                    selectedHeroData,
                    placePosition
                );

            if (!purchased)
            {
                Debug.Log("英雄購買或生成失敗");
                return;
            }

            // 正式英雄由 HeroManager 重新 Instantiate
            // 預覽物件直接刪除
            CancelSelection();
        }
        private bool CanBuildHero(Vector3 position)
        {
            Vector3 halfExtents =
                new Vector3(0.5f, 1f, 0.5f);

            Collider[] hits = Physics.OverlapBox(
                position,
                halfExtents,
                Quaternion.identity,
                buildCheckLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                // 地面不算阻擋物
                if (((1 << hit.gameObject.layer) & groundLayer.value) != 0)
                    continue;

                // 道路、環境、其他塔或英雄禁止放置
                if (hit.CompareTag("道路") ||
                    hit.CompareTag("環境裝飾") ||
                    hit.GetComponentInParent<Tower>() != null ||
                    hit.GetComponentInParent<Hero>() != null)
                {
                    return false;
                }
            }

            return true;
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
            if (isMovingTower && movingTower != null)
            {
                movingTower.gameObject.SetActive(true);
            }

            if (previewInstance != null)
            {
                Destroy(previewInstance);
            }

            previewInstance = null;
            previewRenderer = null;
            previewRange = null;

            selectedTowerData = null;
            selectedHeroData = null;
            placingHero = false;

            movingTower = null;
            isMovingTower = false;
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

        // =========================================
        // 移動防禦塔
        // =========================================
        public void StartMoveTower(Tower tower)
        {
            if (tower == null)
                return;

            CancelSelection();

            movingTower = tower;
            isMovingTower = true;
            placingHero = false;

            // 記錄塔資料，讓 UpdatePreview 能取得 footprint
            selectedTowerData = tower.data;

            // 隱藏原本的塔
            movingTower.gameObject.SetActive(false);

            // 只建立模型作為預覽，不要建立完整塔 Prefab
            GameObject modelPrefab =
                tower.data.levelModelPrefabs[tower.currentLevel];

            if (modelPrefab == null)
            {
                Debug.LogError("移動塔失敗：沒有設定 Level Model Prefab");

                movingTower.gameObject.SetActive(true);
                movingTower = null;
                isMovingTower = false;
                selectedTowerData = null;
                return;
            }

            previewInstance = Instantiate(modelPrefab);

            previewRenderer =
                previewInstance.GetComponentInChildren<MeshRenderer>();

            // 保險：關閉預覽內所有 Collider
            foreach (Collider col in
                     previewInstance.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }

            // 如果模型裡意外有 Tower 腳本，也一起停用
            foreach (Tower towerScript in
                     previewInstance.GetComponentsInChildren<Tower>(true))
            {
                towerScript.enabled = false;
            }

            previewRange =
                previewInstance.GetComponentInChildren<RangeCircle>();

            if (previewRange != null)
            {
                previewRange.DrawCircle(movingTower.attackRange);
            }

            Debug.Log($"開始移動塔：{movingTower.name}");
        }

    }
}