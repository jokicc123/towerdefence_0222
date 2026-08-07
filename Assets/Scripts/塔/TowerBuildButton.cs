using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 防禦塔建造按鈕。
    /// 顯示建造價格，並依照金幣與遊戲狀態控制按鈕是否可用。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TowerBuildButton : MonoBehaviour
    {
        #region Inspector 設定

        [Header("設定這顆按鈕代表哪座塔")]
        [SerializeField]
        private TowerData towerData;

        [Header("UI 元件連結")]
        [SerializeField]
        private TextMeshProUGUI priceText;

        #endregion

        #region 執行期間資料

        private Button button;
        private int buildCost;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            button =
                GetComponent<Button>();
        }

        private void Start()
        {
            InitializeButton();
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region UI 初始化

        private void InitializeButton()
        {
            if (towerData == null ||
                towerData.levels == null ||
                towerData.levels.Length == 0)
            {
                Debug.LogWarning(
                    $"{gameObject.name} 沒有設定有效的 TowerData",
                    this
                );

                DisableButton();
                return;
            }

            buildCost =
                towerData.levels[0].cost;

            if (priceText != null)
            {
                priceText.text =
                    $"${buildCost}";
            }

            RefreshButtonState();
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnGoldChanged +=
                CheckMoney;
        }

        private void UnregisterEvents()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnGoldChanged -=
                CheckMoney;
        }

        #endregion

        #region 按鈕狀態

        private void CheckMoney(
            int currentGold)
        {
            RefreshButtonState(
                currentGold
            );
        }

        private void RefreshButtonState()
        {
            int currentGold =
                GameManager.Instance != null
                    ? GameManager.Instance.Gold
                    : 0;

            RefreshButtonState(
                currentGold
            );
        }

        private void RefreshButtonState(
            int currentGold)
        {
            if (button == null)
                return;

            bool canBuild =
                GameManager.Instance != null &&
                GameManager.Instance.CanBuildTower();

            bool hasEnoughGold =
                currentGold >= buildCost;

            button.interactable =
                canBuild &&
                hasEnoughGold;
        }

        private void DisableButton()
        {
            if (button != null)
            {
                button.interactable =
                    false;
            }

            if (priceText != null)
            {
                priceText.text =
                    "--";
            }
        }

        #endregion
    }
}