using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace CHANG
{
    public class UiManager : MonoBehaviour
    {
        private Tower currentTower; //目前選擇的塔
        public static UiManager Instance;

        private void Awake()
        {
            //確保場上只有一個 UIManager
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #region  UI文字
        //UI文字
        [Header("UI")]
        [SerializeField]
        private TMP_Text hpText;
        [SerializeField]
        private TMP_Text goldText;
        [SerializeField]
        private TMP_Text waveText;
        [SerializeField]
        private CanvasGroup gameOverUI;
        [SerializeField]
        private CanvasGroup winUI;

        #endregion
        #region  升級UI
        [Header("升級UI")]
        [SerializeField]
        private GameObject upgradePanel;
        [SerializeField]
        private TMP_Text towerNameText;
        [SerializeField]
        private TMP_Text levelText;
        [SerializeField]
        private TMP_Text costText;
        [SerializeField]
        private Button upgradeButton;
        #endregion
        private void Start()
        {
            //更新UI
            GameManager.Instance.OnHpChanged += UpdateHp;
            GameManager.Instance.OnGoldChanged += UpdateGold;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.Onwin += ShowWin;
            UpdateHp(GameManager.Instance.castleHp);
            UpdateGold(GameManager.Instance.gold);
            UpdateWave(GameManager.Instance.currentWave);

        }
        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            //取消訂閱事件
            GameManager.Instance.OnHpChanged -= UpdateHp;
            GameManager.Instance.OnGoldChanged -= UpdateGold;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.Onwin -= ShowWin;
        }

        public void ShowUpgradeUI(Tower tower)
        {
            currentTower = tower;
            upgradePanel.SetActive(true);
            RefreshUpgradeUI();
        }

        //更新血量 UI
        private void UpdateHp(int hp)
        {
            //顯示數值
            hpText.text = hp.ToString();
        }
        //更新金錢 UI
        private void UpdateGold(int gold)
        {

            goldText.text = gold.ToString();
            // ⭐ 如果升級UI開著 → 更新
            if (upgradePanel.activeSelf)
            {
                RefreshUpgradeUI();
            }
        }
        //更新波數 UI
        private void UpdateWave(int wave)
        {
            waveText.text = $"{wave}";
        }
        //顯示遊戲結束 UI
        private void ShowGameOver()
        {
            StopAllCoroutines();//停止所有協程，確保不會同時顯示遊戲結束和勝利 UI
            StartCoroutine(FadeSystem.Fade(gameOverUI, true));
        }
        //顯示勝利 UI
        private void ShowWin()
        {
            StopAllCoroutines();
            StartCoroutine(FadeSystem.Fade(winUI, true));
        }
        void RefreshUpgradeUI()
        {
            if (currentTower == null) return;

            var data = currentTower.data;

            towerNameText.text = data.towerName;
            levelText.text = "Next Lv." + (currentTower.currentLevel + 1);

            if (currentTower.CanUpgrade())
            {
                int cost = currentTower.GetUpgradeCost();
                costText.text = "Cost: " + cost;

                // 判斷錢夠不夠
                if (GameManager.Instance.gold >= cost)
                {
                    upgradeButton.interactable = true;
                }
                else
                {
                    upgradeButton.interactable = false;
                }
            }
            else
            {
                costText.text = "MAX";
                upgradeButton.interactable = false;
            }
        }
        public void HideUpgradeUI()
        {
            upgradePanel.SetActive(false);
        }
        public void OnClickUpgrade()
        {
            if (currentTower == null) return;

            currentTower.Upgrade();
            RefreshUpgradeUI();
        }

    }
}