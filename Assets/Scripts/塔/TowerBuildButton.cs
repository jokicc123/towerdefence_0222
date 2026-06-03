using UnityEngine;
using TMPro;      // ?? 記得引入 TMPro
using UnityEngine.UI;

namespace CHANG
{
    [RequireComponent(typeof(Button))]
    public class TowerBuildButton : MonoBehaviour
    {
        [Header("設定這顆按鈕代表哪座塔")]
        [SerializeField] private TowerData towerData;

        [Header("UI 元件連結")]
        [SerializeField] private TextMeshProUGUI priceText;

        private Button button;
        private int buildCost;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            InitButtonUI();
        }

        // ? 初始化按鈕：讀取 ScriptableObject 的初始價格並顯示
        private void InitButtonUI()
        {
            if (towerData == null || towerData.levels.Length == 0)
            {
                Debug.LogWarning($"{gameObject.name} 沒有設定 TowerData！");
                return;
            }

            // ?? 抓取這座塔第 0 等級（也就是建造初始級）的花費
            buildCost = towerData.levels[0].cost;

            if (priceText != null)
            {
                priceText.text = $"${buildCost}"; // 畫面上會顯示成如 $100
            }

            // ??? 訂閱 GameManager 的金幣改變事件，只要錢一變動就檢查按鈕能不能按
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += CheckMoney;
                // 開局先檢查一次
                CheckMoney(GameManager.Instance.gold);
            }
        }

        // ? 核心邏輯：根據玩家目前的金幣，決定按鈕能不能點擊
        private void CheckMoney(int currentGold)
        {
            if (button == null) return;

            // 錢夠就可以點（true），錢不夠就反灰（false）
            button.interactable = currentGold >= buildCost;
        }

        private void OnDestroy()
        {
            // ?? 養成好習慣，物件被銷毀時取消訂閱事件，防止記憶體漏失
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= CheckMoney;
            }
        }
    }
}