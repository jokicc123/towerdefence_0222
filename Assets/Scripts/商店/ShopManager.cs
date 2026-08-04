using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace CHANG
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;

        [SerializeField] private Button shopExitButton;
        [SerializeField] private TMP_Text crystalText;
        [SerializeField] private Image crystalIcon;   // 可選，如果需要控制圖示

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 測試用：直接設定 1000 水晶
            PlayerData.Crystal = 1000;

            Debug.Log(
                $"目前水晶：{PlayerData.Crystal}"
            );

            if (shopExitButton != null)
            {
                shopExitButton.onClick.AddListener(
                    OnClickShopExit
                );
            }
            RefreshCrystalUI();
        }

        // 取得目前升級等級
        public int GetLevel(ShopUpgradeType type)
        {
            return PlayerPrefs.GetInt(type.ToString(), 0);
        }

        // 升級
        public bool Upgrade(ShopUpgradeData data)
        {
            if (data == null)
            {
                Debug.LogError("ShopUpgradeData 沒有設定");
                return false;
            }

            int level = GetLevel(data.type);

            if (level >= data.maxLevel)
            {
                Debug.Log("已達最高等級");
                return false;
            }

            if (data.crystalCosts == null ||
                level >= data.crystalCosts.Length)
            {
                Debug.LogError(
                    $"{data.upgradeName} 的 Crystal Costs 數量不足"
                );

                return false;
            }

            int cost = data.crystalCosts[level];

            if (!PlayerData.SpendCrystal(cost))
            {
                Debug.Log("水晶不足");
                return false;
            }

            PlayerPrefs.SetInt(
                data.type.ToString(),
                level + 1
            );

            PlayerPrefs.Save();
            Debug.Log(
            $"{data.type} = {PlayerPrefs.GetInt(data.type.ToString())}"
             );
            RefreshCrystalUI();

            Debug.Log(
                $"{data.upgradeName} 升到 Lv.{level + 1}"
            );

            return true;
        }

       
        private void OnDestroy()
        {
            if (shopExitButton != null)
            {
                shopExitButton.onClick.RemoveListener(
                    OnClickShopExit
                );
            }
        }
        public void RefreshCrystalUI()
        {
            if (crystalText != null)
            {
                crystalText.text = $" {PlayerData.Crystal}";
            }
        }
        public void OnClickShopExit()
        {
            SceneManager.LoadScene("主選單");
        }
        [ContextMenu("重置所有商店資料")]
        public void ResetShopData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("已重置所有 PlayerPrefs");
        }
        public float GetCurrentValue(ShopUpgradeData data)

        {
            if (data == null ||
                data.values == null ||
                data.values.Length == 0)
            {
                return 1f;
            }

            int level = GetLevel(data.type);

            int index = Mathf.Clamp(
                level,
                0,
                data.values.Length - 1
            );

            return data.values[index];
        }
      
        

    }
}