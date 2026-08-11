using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 商店升級項目 UI。
    /// 顯示目前等級、升級花費與升級效果，
    /// 並負責處理升級按鈕。
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        #region Inspector 設定

        [Header("商品資料")]
        [SerializeField]
        private ShopUpgradeData data;

        [Header("UI")]
        [SerializeField]
        private TMP_Text levelText;

        [SerializeField]
        private TMP_Text crystalCostsText;

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private Button upgradeButton;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            RegisterEvents();
            Refresh();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(
                    OnClickUpgrade
                );
            }
        }

        private void UnregisterEvents()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(
                    OnClickUpgrade
                );
            }
        }

        #endregion

        #region 按鈕事件

        private void OnClickUpgrade()
        {
            if (data == null)
            {
                Debug.LogWarning(
                    $"{name} 沒有設定 ShopUpgradeData"
                );
                return;
            }

            if (ShopManager.Instance == null)
            {
                Debug.LogError(
                    "場景中找不到 ShopManager"
                );
                return;
            }

            if (ShopManager.Instance.Upgrade(data))
            {
                Refresh();
            }
        }

        #endregion

        #region UI 更新

        public void Refresh()
        {
            if (data == null ||
                ShopManager.Instance == null)
            {
                return;
            }

            int level =
                ShopManager.Instance.GetLevel(
                    data.type
                );
            UpdateName();
            UpdateLevel(level);

            if (level >= data.maxLevel)
            {
                UpdateMaxLevel(level);
                return;
            }

            UpdateCost(level);
            UpdateValue(level);
            UpdateButton(level);
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text =
                    $"Lv.{level}";
            }
        }

        private void UpdateMaxLevel(int level)
        {
            if (crystalCostsText != null)
            {
                crystalCostsText.text = "MAX";
            }

            if (valueText != null &&
                data.values != null &&
                data.values.Length > 0)
            {
                int index = Mathf.Clamp(
                    level,
                    0,
                    data.values.Length - 1
                );

                valueText.text =
                    FormatValue(
                        data.values[index]
                    );
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }
        }

        private void UpdateCost(int level)
        {
            if (crystalCostsText == null)
                return;

            if (data.crystalCosts != null &&
                level < data.crystalCosts.Length)
            {
                crystalCostsText.text =
                    $"{data.crystalCosts[level]} 水晶";
            }
            else
            {
                crystalCostsText.text =
                    "價格未設定";
            }
        }

        private void UpdateValue(int level)
        {
            if (valueText == null ||
                data.values == null ||
                data.values.Length == 0)
            {
                return;
            }

            int current =
                Mathf.Clamp(
                    level,
                    0,
                    data.values.Length - 1
                );

            int next =
                Mathf.Clamp(
                    level + 1,
                    0,
                    data.values.Length - 1
                );

            valueText.text =
                $"{FormatValue(data.values[current])}" +
                $" → " +
                $"{FormatValue(data.values[next])}";
        }

        private void UpdateButton(int level)
        {
            if (upgradeButton == null)
                return;

            bool hasCost =
                data.crystalCosts != null &&
                level < data.crystalCosts.Length;

            upgradeButton.interactable =
                hasCost;
        }
        private void UpdateName()
        {
            if (nameText == null ||
                data == null)
            {
                return;
            }

            switch (data.type)
            {
                case ShopUpgradeType.CastleHP:
                    nameText.text = "城堡生命";
                    break;

                case ShopUpgradeType.TowerDamage:
                    nameText.text = "防禦塔傷害";
                    break;

                case ShopUpgradeType.HeroDamage:
                    nameText.text = "英雄傷害";
                    break;
            }
        }

        #endregion

            #region 顯示格式

        private string FormatValue(
            float value)
        {
            switch (data.type)
            {
                case ShopUpgradeType.CastleHP:
                    return $"{value:0} HP";

                case ShopUpgradeType.TowerDamage:
                case ShopUpgradeType.HeroDamage:
                    return
                        $"+{(value - 1f) * 100f:0}%";

                default:
                    return value.ToString("0.##");
            }
        }

        #endregion
    }
}