using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 商店管理器。
    /// 負責永久升級、水晶顯示與商店場景操作。
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        #region Singleton

        public static ShopManager Instance
        {
            get;
            private set;
        }

        #endregion

        #region Inspector 設定

        [Header("商店 UI")]
        [SerializeField]
        private Button shopExitButton;

        [SerializeField]
        private TMP_Text crystalText;

        [SerializeField]
        private Image crystalIcon;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            RegisterEvents();
            RefreshCrystalUI();

#if UNITY_EDITOR
            Debug.Log(
                $"目前水晶：{PlayerData.Crystal}",
                this
            );
#endif
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (shopExitButton != null)
            {
                shopExitButton.onClick.AddListener(
                    OnClickShopExit
                );
            }
        }

        private void UnregisterEvents()
        {
            if (shopExitButton != null)
            {
                shopExitButton.onClick.RemoveListener(
                    OnClickShopExit
                );
            }
        }

        #endregion

        #region 商店升級

        public bool Upgrade(
            ShopUpgradeData data)
        {
            if (data == null)
            {
                Debug.LogError(
                    "ShopUpgradeData 沒有設定",
                    this
                );

                return false;
            }

            int level =
                GetLevel(data.type);

            if (level >= data.maxLevel)
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"{data.upgradeName} 已達最高等級",
                    this
                );
#endif
                return false;
            }

            if (data.crystalCosts == null ||
                level >= data.crystalCosts.Length)
            {
                Debug.LogError(
                    $"{data.upgradeName} 的 Crystal Costs 數量不足",
                    this
                );

                return false;
            }

            int cost =
                data.crystalCosts[level];

            if (!PlayerData.SpendCrystal(cost))
            {
#if UNITY_EDITOR
                Debug.Log(
                    "水晶不足",
                    this
                );
#endif
                return false;
            }

            int newLevel =
                level + 1;

            PlayerPrefs.SetInt(
                data.type.ToString(),
                newLevel
            );

            PlayerPrefs.Save();

            RefreshCrystalUI();

#if UNITY_EDITOR
            Debug.Log(
                $"{data.upgradeName} 升到 Lv.{newLevel}",
                this
            );
#endif

            return true;
        }

        #endregion

        #region 商店資料查詢

        public int GetLevel(
            ShopUpgradeType type)
        {
            return PlayerPrefs.GetInt(
                type.ToString(),
                0
            );
        }

        public float GetCurrentValue(
            ShopUpgradeData data)
        {
            if (data == null ||
                data.values == null ||
                data.values.Length == 0)
            {
                return 1f;
            }

            int level =
                GetLevel(data.type);

            int index =
                Mathf.Clamp(
                    level,
                    0,
                    data.values.Length - 1
                );

            return data.values[index];
        }

        #endregion

        #region 水晶 UI

        public void RefreshCrystalUI()
        {
            if (crystalText != null)
            {
                crystalText.text =
                    $"{PlayerData.Crystal}";
            }
        }

        #endregion

        #region 場景切換

        private void OnClickShopExit()
        {
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.StartLoad(
                    "主選單"
                );

                return;
            }

            SceneManager.LoadScene(
                "主選單"
            );
        }

        #endregion

        #region 測試工具

        [ContextMenu("重置商店升級資料")]
        private void ResetShopData()
        {
            foreach (ShopUpgradeType type in
                     System.Enum.GetValues(
                         typeof(ShopUpgradeType)))
            {
                PlayerPrefs.DeleteKey(
                    type.ToString()
                );
            }

            PlayerPrefs.Save();

            RefreshCrystalUI();

#if UNITY_EDITOR
            Debug.Log(
                "已重置所有商店升級資料",
                this
            );
#endif
        }

        #endregion
    }
}