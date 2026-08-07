using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// UiManager 的防禦塔升級面板。
    /// 負責顯示塔資訊、升級、出售與移動功能。
    /// </summary>
    public partial class UiManager
    {
        #region Inspector 設定

        [Header("防禦塔升級 UI")]

        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text rangeText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text attackSpeedText;
        [SerializeField] private TMP_Text upgradeText;
        [SerializeField] private TMP_Text sellText;
        [SerializeField] private TMP_Text moveText;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button upgradecloseButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button moveTowerButton;

        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private Image towerIcon;

        #endregion

        #region 升級面板

        public void ShowUpgradeUI(Tower tower)
        {
            if (tower == null)
                return;

            currentTower = tower;

            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
            }

            RefreshUpgradeUI();
        }

        public void HideUpgradeUI()
        {
            currentTower = null;

            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
        }

        private void RefreshUpgradeUI()
        {
            if (currentTower == null ||
                currentTower.Data == null)
            {
                return;
            }

            UpdateTowerBasicInfo();
            UpdateTowerStats();
            UpdateTowerUpgradeState();
        }

        #endregion

        #region 塔基本資訊

        private void UpdateTowerBasicInfo()
        {
            TowerData data =
                currentTower.Data;

            if (towerNameText != null)
            {
                towerNameText.text =
                    data.towerName;
            }

            if (levelText != null)
            {
                levelText.text =
                    $"等級：{currentTower.CurrentLevel + 1}";
            }

            if (towerIcon != null)
            {
                towerIcon.sprite =
                    data.icon;

                towerIcon.enabled =
                    data.icon != null;

                towerIcon.preserveAspect =
                    true;
            }
        }

        #endregion

        #region 塔能力數值

        private void UpdateTowerStats()
        {
            if (rangeText != null)
            {
                rangeText.text =
                    $"射程：{currentTower.AttackRange:0.0}";
            }

            if (damageText != null)
            {
                damageText.text =
                    $"傷害：{currentTower.Damage:0}";
            }

            if (attackSpeedText != null)
            {
                attackSpeedText.text =
                    $"攻速：{currentTower.AttackSpeed:0.0}/秒";
            }
        }

        #endregion

        #region 升級狀態

        private void UpdateTowerUpgradeState()
        {
            bool canUpgrade =
                currentTower.CanUpgrade();

            if (sellText != null)
            {
                sellText.text = "出售";
            }

            if (moveText != null)
            {
                moveText.text = "移動";
            }

            if (!canUpgrade)
            {
                if (upgradeText != null)
                {
                    upgradeText.text =
                        "已滿級";
                }

                if (upgradeButton != null)
                {
                    upgradeButton.interactable =
                        false;
                }

                return;
            }

            int upgradeCost =
                currentTower.GetUpgradeCost();

            if (upgradeText != null)
            {
                upgradeText.text =
                    $"升級:{upgradeCost} 金幣";
            }

            if (upgradeButton != null &&
                GameManager.Instance != null)
            {
                upgradeButton.interactable =
                    GameManager.Instance.Gold >=
                    upgradeCost;
            }
        }

        #endregion

        #region 按鈕事件

        public void OnClickUpgrade()
        {
            if (currentTower == null)
                return;

            currentTower.Upgrade();

            RefreshUpgradeUI();
        }

        public void OnClickMoveTower()
        {
            if (currentTower == null ||
                BuildManager.Instance == null)
            {
                return;
            }

            BuildManager.Instance
                .StartMoveTower(currentTower);

            HideUpgradeUI();
        }

        public void OnClickSell()
        {
            if (currentTower == null ||
                GameManager.Instance == null)
            {
                return;
            }

            int sellPrice =
                currentTower.GetSellPrice();

            GameManager.Instance.AddGold(
                sellPrice
            );

            Destroy(currentTower.gameObject);

            HideUpgradeUI();
        }

        public void OnClickCloseUpgradePanel()
        {
            HideUpgradeUI();
        }

        #endregion
    }
}