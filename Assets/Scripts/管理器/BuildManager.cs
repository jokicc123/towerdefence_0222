using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace CHANG
{
    /// <summary>
    /// 建造管理器，負責處理防禦塔與英雄的建造、預覽、放置與取消。
    /// </summary>
    public class BuildManager : MonoBehaviour
    {
        #region Singleton

        public static BuildManager Instance { get; private set; }

        #endregion


        #region Inspector 設定

        [Header("設定")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField]
        private Color canBuildColor =
            new Color(0, 1, 0, 0.5f);

        [SerializeField]
        private Color cantBuildColor =
            new Color(1, 0, 0, 0.5f);

        [Header("建造判定用 LayerMask")]
        [SerializeField] private LayerMask buildCheckLayers;
        [SerializeField] private LayerMask selectableLayerMask;
        [SerializeField]
        private Vector3 heroFootprint =
            new Vector3(1, 2, 1);

        #endregion


        #region 執行期間資料

        private TowerData selectedTowerData;
        private HeroData selectedHeroData;

        private GameObject previewInstance;
        private MeshRenderer previewRenderer;
        private RangeCircle previewRange;

        private bool placingHero;

        private Tower movingTower;
        private bool isMovingTower;

        private RaycastHit currentHit;

        #endregion


        #region Unity 生命週期

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this; // 單例模式，方便其他地方呼叫

        }

        private void Update()
        {
            HandleSelectionClick();
            HandlePlacementInput();
            HandleCancelInput();
        }

        #endregion


        #region 輸入處理

        private void HandlePlacementInput()
        {
            if (previewInstance == null)
                return;

            UpdatePreview();

            bool mouseClicked =
                Pointer.current != null
                    ? Pointer.current.press.wasPressedThisFrame
                    : Input.GetMouseButtonDown(0);

            if (!mouseClicked)
                return;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (placingHero)
            {
                TryPlaceHero();
            }
            else
            {
                TryPlaceTower();
            }
        }

        private void HandleCancelInput()
        {
            if (Mouse.current == null)
                return;

            if (!Mouse.current.rightButton.wasPressedThisFrame)
                return;

            CancelSelection();
        }

        #endregion


        #region 建造選擇

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
            // ⭐ 關閉所有粒子
            foreach (ParticleSystem ps in
                     previewInstance.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                ps.gameObject.SetActive(false);
            }

            // ⭐ 關閉所有 Trail
            foreach (TrailRenderer trail in
                     previewInstance.GetComponentsInChildren<TrailRenderer>(true))
            {
                trail.Clear();
                trail.enabled = false;
            }
            foreach (RangeCircle range in
                     previewInstance.GetComponentsInChildren<RangeCircle>(true))
            {
                range.gameObject.SetActive(false);
            }
        }

        #endregion


        #region 預覽系統

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
                    footprint = movingTower.Data.buildFootprint;
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

        #endregion


        #region 放置系統

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
                footprint = movingTower.Data.buildFootprint;
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

            // 先判斷位置能不能放
            if (!CanBuild(
                    placePosition,
                    heroFootprint))
            {
                Debug.Log("這裡不能放置英雄");
                return;
            }

            // 確認可以放之後，才扣錢並生成英雄
            bool purchased =
                HeroManager.Instance != null &&
                HeroManager.Instance.TryPurchaseHero(
                    selectedHeroData,
                    placePosition
                );

            if (!purchased)
            {
                Debug.Log("英雄購買或生成失敗");
                return;
            }

            // 正式英雄由 HeroManager 生成
            // 預覽物件刪除
            CancelSelection();
        }

        #endregion


        #region 建造判定

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



        #endregion


        #region 物件選擇

        private void HandleSelectionClick()
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

        #endregion


        #region 防禦塔移動
        public void StartMoveTower(Tower tower)
        {
            if (tower == null)
                return;

            CancelSelection();

            movingTower = tower;
            isMovingTower = true;
            placingHero = false;

            // 記錄塔資料，讓 UpdatePreview 能取得 footprint
            selectedTowerData = tower.Data;

            // 隱藏原本的塔
            movingTower.gameObject.SetActive(false);

            // 只建立模型作為預覽，不要建立完整塔 Prefab
            GameObject modelPrefab =
                tower.Data.levelModelPrefabs[tower.CurrentLevel];

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
                previewRange.DrawCircle(movingTower.AttackRange);
            }

            Debug.Log($"開始移動塔：{movingTower.name}");

        }
        #endregion


        #region 取消放置

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

        #endregion
    }
}