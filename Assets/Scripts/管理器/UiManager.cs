using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace CHANG
{
    public class UiManager : MonoBehaviour
    {
        [Header("UI 總開關")]
        [SerializeField] private Canvas mainCanvas;   // 在 Inspector 手動拖曳指定
        private Tower currentTower; //目前選擇的塔
        private Hero currentHero; //目前選擇的英雄
        public static UiManager Instance;

        private void Awake()
        {
            //確保場上只有一個 UIManager
            if (Instance == null)
            {
                Instance = this;

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
        private Image towerIcon;
        [SerializeField]
        private TMP_Text towerNameText;
        [SerializeField]
        private TMP_Text levelText;
        [SerializeField]
        private TMP_Text costText;
        [SerializeField]
        private TMP_Text rangeText;
        [SerializeField]
        private TMP_Text damageText;
        [SerializeField]
        private TMP_Text attackSpeedText;
        [SerializeField]
        private Button upgradeButton;
        [SerializeField]
        private Button upgradecloseButton;
        [SerializeField]
        private TMP_Text upgradeText;
        #endregion
        #region 英雄UI
        [Header("英雄UI")]
        [SerializeField] private GameObject heroPanel;
        [SerializeField] private Image heroIcon;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text heroLevelText;
        [SerializeField] private TMP_Text heroAttackText;
        [SerializeField] private TMP_Text heroRangeText;
        [SerializeField] private TMP_Text heroAttackSpeedText;
        [SerializeField] private Slider heroExpSlider;
        [SerializeField] private TMP_Text heroExpText;

        #endregion
        #region 遊戲控制
        [Header("遊戲控制")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        #endregion
        private void Start()
        {
            //更新UI
            GameManager.Instance.OnHpChanged += UpdateHp;
            GameManager.Instance.OnGoldChanged += UpdateGold;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.Onwin += ShowWin;
            GameManager.Instance.OnGameOver += OnGameEnd;
            GameManager.Instance.Onwin += OnGameEnd;
            UpdateHp(GameManager.Instance.castleHp);
            UpdateGold(GameManager.Instance.gold);
            UpdateWave(GameManager.Instance.currentWave);
            UpdatePlayPauseUI();
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
            GameManager.Instance.OnGameOver -= OnGameEnd;
            GameManager.Instance.Onwin -= OnGameEnd;
            if (currentHero != null)
            {
                currentHero.OnHeroDataChanged -= RefreshHeroUI;
            }
        }

        public void ShowUpgradeUI(Tower tower)
        {
            currentTower = tower;
            upgradePanel.SetActive(true);
            RefreshUpgradeUI();
        }
        public void ShowHeroUI(Hero hero)
        {
            if (currentHero != null)
                currentHero.OnHeroDataChanged -= RefreshHeroUI;

            currentHero = hero;

            heroPanel.SetActive(true);

            currentHero.OnHeroDataChanged += RefreshHeroUI;

            RefreshHeroUI();
        }
        #region  UI更新與顯示
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
            Debug.Log("ShowWin");

            StopAllCoroutines();
            StartCoroutine(FadeSystem.Fade(winUI, true));
        }
        #endregion

        void RefreshUpgradeUI()
        {
            if (currentTower == null) return;

            var data = currentTower.data;

            towerNameText.text = data.towerName;
            levelText.text = "等級:" + (currentTower.currentLevel + 1);
            towerIcon.sprite = data.icon;
            rangeText.text = "射程: " + currentTower.attackRange.ToString("0.0");
            damageText.text = "傷害: " + currentTower.damage.ToString("0");
            attackSpeedText.text = "攻速" + $"{currentTower.attackSpeed:0.0}/秒";
            if (currentTower.CanUpgrade())
            {
                int cost = currentTower.GetUpgradeCost();
                upgradeText.text = "升級";
                costText.text = $"{cost} 金幣";

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
                costText.text = "已滿級";
                upgradeButton.interactable = false;
            }
        }
        private void RefreshHeroUI()
        {
            if (currentHero == null) return;

            heroNameText.text = currentHero.data.heroName;

            heroIcon.sprite = currentHero.data.icon; // ⭐ 修正：之前漏了這行，Icon欄位填了也不會顯示

            heroLevelText.text = $"Lv.{currentHero.currentLevel}";

            heroAttackText.text = "傷害:" + currentHero.CurrentStats.damage.ToString();

            heroRangeText.text = "射程:" + currentHero.CurrentStats.range.ToString("0.0");

            heroAttackSpeedText.text = "攻速:" + currentHero.CurrentStats.attackSpeed.ToString("0.0");

            heroExpSlider.maxValue = currentHero.CurrentStats.xpToNextLevel;

            heroExpSlider.value = currentHero.currentXP;

            heroExpText.text =
                $"{currentHero.currentXP}/{currentHero.CurrentStats.xpToNextLevel}";
        }
        public void HideUpgradeUI()
        {
            currentTower = null;
            upgradePanel.SetActive(false);
        }
        public void HideHeroUI()
        {
            if (currentHero != null)
            {
                currentHero.OnHeroDataChanged -= RefreshHeroUI;
            }

            currentHero = null;
            heroPanel.SetActive(false);
        }
        public void OnClickUpgrade()
        {
            if (currentTower == null) return;

            currentTower.Upgrade();
            RefreshUpgradeUI();
        }
        private void OnGameEnd()
        {
            HideUpgradeUI();
            HideHeroUI();

            currentTower = null;
            currentHero = null;
        }
        public void OnClickPlayPause()
        {
            switch (GameManager.Instance.currentState)
            {
                case GameManager.GameState.Build:
                    GameManager.Instance.StartGame();
                    break;

                case GameManager.GameState.Playing:
                    GameManager.Instance.PauseGame();
                    break;

                case GameManager.GameState.Paused:
                    GameManager.Instance.ResumeGame();
                    break;
            }

            UpdatePlayPauseUI();
        }
        public void OnClickColseUpgradePanel()
        {
            HideUpgradeUI();
        }
        private void UpdatePlayPauseUI()
        {
            switch (GameManager.Instance.currentState)
            {
                case GameManager.GameState.Build:
                    playPauseButton.image.sprite = playSprite;
                    break;

                case GameManager.GameState.Playing:
                    playPauseButton.image.sprite = pauseSprite;
                    break;

                case GameManager.GameState.Paused:
                    playPauseButton.image.sprite = playSprite;
                    break;
            }
        }
    }
}