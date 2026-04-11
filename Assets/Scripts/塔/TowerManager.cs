using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace CHANG
{
    public class TowerManager : MonoBehaviour
    {
        public static TowerManager Instance;

        [Header("設定")]
        [SerializeField] private LayerMask groundLayer; // 請在 Inspector 選擇 "地面" 層
        [SerializeField] private Color canBuildColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color cantBuildColor = new Color(1, 0, 0, 0.5f);

        private TowerData selectedTowerData;
        private GameObject previewInstance;
        private MeshRenderer previewRenderer;
        private RangeCircle previewRange;

        void Awake()
        {
            Instance = this;
           
        }

        void Update()
        {
            // 只要有預覽物件，就執行更新位置的邏輯
            if (previewInstance != null)
            {
                UpdatePreview();

                // 點擊左鍵放置塔 (且滑鼠不能在 UI 上)
                bool mouseClicked = Pointer.current != null ? Pointer.current.press.wasPressedThisFrame : Input.GetMouseButtonDown(0);

                // EventSystem.current.IsPointerOverGameObject() 確保點按鈕時不會同時在按鈕下蓋塔
                if (mouseClicked && !EventSystem.current.IsPointerOverGameObject())
                {
                    TryPlaceTower();
                }
            }
        }

        public void SelectTower(TowerData data)
        {
            //避免多個預覽
            CancelSelection();
            //設定當前防禦塔資料
            selectedTowerData = data;

            if (data.towerModelPrefab == null)
            {
                Debug.LogError("TowerData 裡缺少 Prefab！");
                return;
            }
            //實例化預覽物件
            previewInstance = Instantiate(data.towerModelPrefab);
            // 嘗試取得 RangeCircle 組件以顯示攻擊範圍
            previewRange = previewInstance.GetComponentInChildren<RangeCircle>();
            if (previewRange != null)
            {
                previewRange.DrawCircle(data.attackRange);
            } 

            // 取得渲染器以更改顏色 (包含子物件)
            previewRenderer = previewInstance.GetComponentInChildren<MeshRenderer>();

            // 禁用碰撞與腳本，防止預覽塔干擾射線或自行運作
            if (previewInstance.TryGetComponent(out Tower t)) t.enabled = false;

            // 重要：預覽塔如果帶有 Collider，必須禁用或設為 Trigger，否則射線會射到自己
            foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        private void UpdatePreview()
        {
            // 取得滑鼠座標
            Vector2 mousePosition = Pointer.current != null ? Pointer.current.position.ReadValue() : (Vector2)Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // 【核心修正】使用 groundLayer 變數而非硬編碼字串
            if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundLayer))
                
            {
                //Debug.Log($"<color=green>射中地板了：{hit.collider.name}，座標：{hit.point}</color>");
                previewInstance.SetActive(true);

                // 網格對齊邏輯
                Vector3 snapPos = new Vector3(
                    Mathf.Round(hit.point.x),
                    hit.point.y, // 貼合地面高度
                    Mathf.Round(hit.point.z)
                );
                previewInstance.transform.position = snapPos;

                // 檢查是否可建造並更新顏色
                bool canBuild = CheckBuildLocation(snapPos);

                // 決定顏色
                Color color = canBuild ? canBuildColor : cantBuildColor;

                // 改塔顏色
                if (previewRenderer != null)
                {
                    previewRenderer.material.color = color;
                }

                // ⭐ 同步範圍顏色
                if (previewRange != null)
                {
                    previewRange.SetColor(color);
                }
            }
            else
            {
                // 沒射中地面時隱藏，避免塔飄在地圖外
                previewInstance.SetActive(false);
            }
        }

        private void TryPlaceTower()
        {
            // 確保預覽物件存在且有選擇的塔資料
            if (previewInstance == null || selectedTowerData == null) return;

            Vector3 pos = previewInstance.transform.position;

            // 最終確認建造位置是否合法
            if (CheckBuildLocation(pos))
            {
                if(!GameManager.Instance.SpendGold(selectedTowerData.cost))
                { 
                    Debug.LogWarning("金幣不足，無法建造！");
                    return;
                }

                //在合法位置實例化真正的塔
                GameObject newTower = Instantiate(selectedTowerData.towerModelPrefab, pos, Quaternion.identity);

                if (newTower.TryGetComponent(out Tower towerScript))
                {
                    towerScript.Initialize(selectedTowerData);
                }

                Debug.Log($"成功建造: {selectedTowerData.towerName}");
                CancelSelection();
            }
        }

        //建塔限制：檢查半徑 0.4 內是否有其它塔
        private bool CheckBuildLocation(Vector3 pos)
        {
            // 檢查半徑 0.4 內是否有其它塔
            return !Physics.CheckSphere(pos, 0.4f, LayerMask.GetMask("Tower"));
        }

        // 取消選擇並清理預覽
        private void CancelSelection()
        {
            previewRange = null;
            selectedTowerData = null;
            if (previewInstance != null) Destroy(previewInstance);
        }
    }
}