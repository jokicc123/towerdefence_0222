using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("商品資料")]
        [SerializeField] private ShopUpgradeData data;

        [Header("UI")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text crystalCostsText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Button upgradeButton;
        

        private void Start()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(
                    OnClickUpgrade
                );
            }
            
           

            Refresh();
        }

     
        private void OnDestroy()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(
                    OnClickUpgrade
                );
            }
        }

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

        public void Refresh()
        {
            if (data == null ||
                ShopManager.Instance == null)
            {
                return;
            }

            int level =
                ShopManager.Instance.GetLevel(data.type);

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }

            // 已滿級
            if (level >= data.maxLevel)
            {
                if (crystalCostsText != null)
                {
                    crystalCostsText.text = "MAX";
                }

                if (valueText != null &&
                    data.values != null &&
                    data.values.Length > 0)
                {
                    int maxIndex = Mathf.Clamp(
                        level,
                        0,
                        data.values.Length - 1
                    );

                    valueText.text =
                        FormatValue(data.values[maxIndex]);
                }

                if (upgradeButton != null)
                {
                    upgradeButton.interactable = false;
                }

                return;
            }

            // 顯示升級價格
            if (crystalCostsText != null)
            {
                if (data.crystalCosts != null &&
                    level < data.crystalCosts.Length)
                {
                    crystalCostsText.text =
                        $"{data.crystalCosts[level]} 水晶";
                }
                else
                {
                    crystalCostsText.text = "價格未設定";
                }
            }

            // 顯示目前效果 → 下一級效果
            if (valueText != null &&
                data.values != null &&
                data.values.Length > 0)
            {
                int currentIndex = Mathf.Clamp(
                    level,
                    0,
                    data.values.Length - 1
                );

                int nextIndex = Mathf.Clamp(
                    level + 1,
                    0,
                    data.values.Length - 1
                );

                valueText.text =
                    $"{FormatValue(data.values[currentIndex])}" +
                    $" → {FormatValue(data.values[nextIndex])}";
            }

            if (upgradeButton != null)
            {
                bool hasCost =
                    data.crystalCosts != null &&
                    level < data.crystalCosts.Length;

                upgradeButton.interactable = hasCost;
            }
        }

        private string FormatValue(float value)
        {
            switch (data.type)
            {
                case ShopUpgradeType.CastleHP:
                    return $"{value:0} HP";

                case ShopUpgradeType.TowerDamage:
                case ShopUpgradeType.HeroDamage:
                    return $"+{(value - 1f) * 100f:0}%";

                default:
                    return value.ToString("0.##");
            }
        }
    }
}