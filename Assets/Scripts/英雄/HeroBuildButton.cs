using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CHANG
{
    [RequireComponent(typeof(Button))]
    public class HeroBuildButton : MonoBehaviour
    {
        [Header("設定這顆按鈕代表哪個英雄")]
        [SerializeField] private HeroData heroData;
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

        // ? 初始化按鈕：讀取HeroData的購買價格並顯示
        private void InitButtonUI()
        {
            if (heroData == null)
            {
                Debug.LogWarning($"{gameObject.name} 沒有設定 HeroData！");
                return;
            }

            // ? 跟塔不同，英雄的價格不是分等級的，直接讀 purchaseCost
            buildCost = heroData.purchaseCost;

            if (priceText != null)
            {
                priceText.text = $"${buildCost}"; // 畫面上會顯示成如 $100
            }

            // 訂閱 GameManager 的金幣改變事件，只要錢一變動就檢查按鈕能不能按
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += CheckMoney;
                CheckMoney(GameManager.Instance.gold);
            }
        }

        // ? 核心邏輯：根據玩家目前的金幣，決定按鈕能不能點擊
        private void CheckMoney(int currentGold)
        {
            if (button == null) return;

            bool isPlaying = GameManager.Instance != null && GameManager.Instance.CanBuildTower();

            // 錢夠 且 遊戲進行中，才能點
            button.interactable = isPlaying && currentGold >= buildCost;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= CheckMoney;
            }
        }
    }
}