using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    public class UiManager : MonoBehaviour
    {
        public static UiManager Instance;

        [Header("UI 總開關")]
        [SerializeField] private Canvas mainCanvas;

        // currentTower：目前打開升級面板的塔
        private Tower currentTower;

        // currentHero：目前打開英雄資訊面板的英雄
        private Hero currentHero;

        // activeHero：場上真正負責施放技能的英雄
        private Hero activeHero;

      
        #region 一般 UI

        [Header("一般 UI")]
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private CanvasGroup gameOverUI;
        [SerializeField] private CanvasGroup winUI;
        [SerializeField] private TMP_Text winCrystalText;
        [SerializeField] private TMP_Text loseCrystalText;

        #endregion

        #region 防禦塔資訊UI

        [Header("升級 UI")]
        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
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

        #region 英雄資訊 UI

        [Header("英雄資訊 UI")]
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text heroLevelText;
        [SerializeField] private TMP_Text heroAttackText;
        [SerializeField] private TMP_Text heroRangeText;
        [SerializeField] private TMP_Text heroAttackSpeedText;
        [SerializeField] private TMP_Text heroExpText;
        [SerializeField] private TMP_Text heroUnlockDescriptionText;
        [SerializeField] private GameObject heroPanel;
        [SerializeField] private Slider heroExpSlider;
        [SerializeField] private Image heroIcon;
        #endregion

        #region 英雄技能 UI

        [Header("英雄技能 UI")]
        [SerializeField] private Button skill1Button;
        [SerializeField] private Button skill2Button;

        // 技能圖示本身
        [SerializeField] private Image skill1Icon;
        [SerializeField] private Image skill2Icon;

        // 疊在技能圖示上方的黑色冷卻遮罩
        [SerializeField] private Image skill1CooldownImage;
        [SerializeField] private Image skill2CooldownImage;

        #endregion

        #region 遊戲控制

        [Header("遊戲控制")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private Sprite pauseSprite;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("找不到 GameManager");
                return;
            }

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

            // 遊戲開始尚未放置英雄時，技能不能使用
            SetSkillButtonsEnabled(false);

            if (skill1CooldownImage != null)
                skill1CooldownImage.fillAmount = 0f;

            if (skill2CooldownImage != null)
                skill2CooldownImage.fillAmount = 0f;

            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance.OnSelectedHeroChanged +=
                    RefreshSelectedHeroSkillIcons;

                RefreshSelectedHeroSkillIcons();
            }
        }

        private void Update()
        {
            // 每幀更新技能冷卻遮罩及按鈕狀態
            UpdateHeroSkillCooldownUI();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHpChanged -= UpdateHp;
                GameManager.Instance.OnGoldChanged -= UpdateGold;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
                GameManager.Instance.OnGameOver -= ShowGameOver;
                GameManager.Instance.Onwin -= ShowWin;
                GameManager.Instance.OnGameOver -= OnGameEnd;
                GameManager.Instance.Onwin -= OnGameEnd;
            }

            if (currentHero != null)
            {
                currentHero.OnHeroDataChanged -= RefreshHeroUI;
            }

            if (Instance == this)
            {
                Instance = null;
            }
            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance.OnSelectedHeroChanged -=
                    RefreshSelectedHeroSkillIcons;
            }
        }

        #region 一般 UI 更新

        private void UpdateHp(int hp)
        {
            if (hpText != null)
                hpText.text = hp.ToString();
        }

        private void UpdateGold(int gold)
        {
            if (goldText != null)
                goldText.text = gold.ToString();

            if (upgradePanel != null && upgradePanel.activeSelf)
            {
                RefreshUpgradeUI();
            }
        }

        private void UpdateWave(int wave)
        {
            if (waveText != null)
                waveText.text = wave.ToString();
        }

        private void ShowGameOver()
        {
            StopAllCoroutines();

            if (gameOverUI != null)
            {
                StartCoroutine(
                    FadeSystem.Fade(gameOverUI, true)
                );
            }
            if (loseCrystalText != null)
            {
                loseCrystalText.text =
                    $"獲得 {GameManager.Instance.LoseCrystalReward} 水晶";
            }

            StopAllCoroutines();
            StartCoroutine(
                FadeSystem.Fade(gameOverUI, true)
            );
        }

        private void ShowWin()
        {
            Debug.Log("ShowWin");

            StopAllCoroutines();

            if (winUI != null)
            {
                StartCoroutine(
                    FadeSystem.Fade(winUI, true)
                );
            }
            if (winCrystalText != null)
            {
                winCrystalText.text =
                    $"獲得 {GameManager.Instance.WinCrystalReward} 水晶";
            }

            StopAllCoroutines();
            StartCoroutine(
                FadeSystem.Fade(winUI, true)
            );
        }

        #endregion

        #region 防禦塔資訊 UI

        public void ShowUpgradeUI(Tower tower)
        {
            if (tower == null)
                return;

            currentTower = tower;

            if (upgradePanel != null)
                upgradePanel.SetActive(true);

            RefreshUpgradeUI();
        }

        private void RefreshUpgradeUI()
        {
            if (currentTower == null)
                return;

            var data = currentTower.data;

            if (data == null)
                return;

            if (towerNameText != null)
                towerNameText.text = data.towerName;

            if (levelText != null)
                levelText.text =
                    "等級:" + (currentTower.currentLevel + 1);

            if (towerIcon != null)
                towerIcon.sprite = data.icon;

            if (rangeText != null)
                rangeText.text =
                    "射程: " + currentTower.attackRange.ToString("0.0");

            if (damageText != null)
                damageText.text =
                    "傷害: " + currentTower.damage.ToString("0");

            if (attackSpeedText != null)
            {
                attackSpeedText.text =
                    $"攻速: {currentTower.attackSpeed:0.0}/秒";
            }

            if (currentTower.CanUpgrade())
            {
                int cost = currentTower.GetUpgradeCost();

                if (upgradeText != null)
                    upgradeText.text = "升級";

                if (costText != null)
                    costText.text = $"{cost} 金幣";

                if (upgradeButton != null)
                {
                    upgradeButton.interactable =
                        GameManager.Instance.gold >= cost;
                }
            }
            else
            {
                if (costText != null)
                    costText.text = "已滿級";

                if (upgradeButton != null)
                    upgradeButton.interactable = false;
            }

            {
                if (sellText != null)
                    sellText.text = "出售";

                if (moveText != null)
                    moveText.text = "移動";
            }
        }

        public void OnClickUpgrade()
        {
            if (currentTower == null)
                return;

            currentTower.Upgrade();
            RefreshUpgradeUI();
        }

        public void HideUpgradeUI()
        {
            currentTower = null;

            if (upgradePanel != null)
                upgradePanel.SetActive(false);
        }

        public void OnClickColseUpgradePanel()
        {
            HideUpgradeUI();
        }
        public void OnClickMoveTower()
        {
            if (currentTower == null)
                return;


            BuildManager.Instance.StartMoveTower(currentTower);

            HideUpgradeUI();
        }
        public void OnClickSell()
        {
            if (currentTower == null)
                return;

            int sellPrice =
                currentTower.GetSellPrice();

            GameManager.Instance.AddGold(sellPrice);

            Destroy(currentTower.gameObject);

            HideUpgradeUI();
        }

        #endregion

        #region 英雄資訊面板

        public void ShowHeroUI(Hero hero)
        {
            if (hero == null)
                return;

            // 解除上一個資訊面板英雄的事件
            if (currentHero != null)
            {
                currentHero.OnHeroDataChanged -= RefreshHeroUI;
            }

            currentHero = hero;

            if (heroPanel != null)
                heroPanel.SetActive(true);

            currentHero.OnHeroDataChanged += RefreshHeroUI;

            RefreshHeroUI();
        }

        private void RefreshHeroUI()
        {
            if (currentHero == null || currentHero.data == null)
                return;

            if (heroNameText != null)
                heroNameText.text = currentHero.data.heroName;

            if (heroIcon != null)
                heroIcon.sprite = currentHero.data.icon;

            if (heroLevelText != null)
                heroLevelText.text = $"Lv.{currentHero.currentLevel}";

            if (heroAttackText != null)
            {
                heroAttackText.text =
                    "傷害:" +
                    currentHero.CurrentStats.damage.ToString("0");
            }

            if (heroRangeText != null)
            {
                heroRangeText.text =
                    "射程:" +
                    currentHero.CurrentStats.range.ToString("0.0");
            }

            if (heroAttackSpeedText != null)
            {
                heroAttackSpeedText.text =
                    "攻速:" +
                    currentHero.CurrentStats.attackSpeed.ToString("0.0");
            }
            if (heroUnlockDescriptionText != null)
            {
                string unlockDescription =
                    currentHero.CurrentStats.unlockDescription;

                if (string.IsNullOrWhiteSpace(unlockDescription))
                {
                    heroUnlockDescriptionText.text =
                        "本級沒有新的解鎖效果";
                }
                else
                {
                    heroUnlockDescriptionText.text =
                        $"等級效果：\n{unlockDescription}";
                }
            }


            int maxLevel = currentHero.data.levelStats.Length;

            bool isMaxLevel =
                currentHero.currentLevel >= maxLevel;

            if (heroLevelText != null)
            {
                heroLevelText.text = isMaxLevel
                    ? $"Lv.{currentHero.currentLevel} MAX"
                    : $"Lv.{currentHero.currentLevel}";
            }

            if (heroExpSlider != null)
            {
                if (isMaxLevel)
                {
                    heroExpSlider.minValue = 0;
                    heroExpSlider.maxValue = 1;
                    heroExpSlider.value = 1;
                }
                else
                {
                    heroExpSlider.minValue = 0;
                    heroExpSlider.maxValue =
                        currentHero.CurrentStats.xpToNextLevel;

                    heroExpSlider.value =
                        currentHero.currentXP;
                }
            }

            if (heroExpText != null)
            {
                heroExpText.text = isMaxLevel
                    ? "MAX"
                    : $"{currentHero.currentXP}/{currentHero.CurrentStats.xpToNextLevel}";
            }
        }

        public void HideHeroUI()
        {
            if (currentHero != null)
            {
                currentHero.OnHeroDataChanged -= RefreshHeroUI;
            }

            // 只清除資訊面板選擇
            // 不要清除 activeHero
            currentHero = null;

            if (heroPanel != null)
                heroPanel.SetActive(false);
        }

        #endregion

        #region 場上英雄與技能

        /// <summary>
        /// 英雄生成後呼叫。
        /// 將英雄設定成技能按鈕要控制的英雄。
        /// </summary>
        public void SetActiveHero(Hero hero)
        {
            activeHero = hero;

            if (activeHero == null || activeHero.data == null)
            {
                SetSkillButtonsEnabled(false);
                return;
            }

            if (skill1Icon != null)
            {
                skill1Icon.sprite =
                    activeHero.data.skill1.icon;
            }

            if (skill2Icon != null)
            {
                skill2Icon.sprite =
                    activeHero.data.skill2.icon;
            }

            UpdateHeroSkillCooldownUI();

            Debug.Log(
                $"已設定技能英雄：{activeHero.data.heroName}"
            );
        }

        /// <summary>
        /// 英雄被移除或銷毀時呼叫。
        /// </summary>
        public void ClearActiveHero(Hero hero)
        {
            // 避免其他英雄誤清除目前英雄
            if (activeHero != hero)
                return;

            activeHero = null;

            SetSkillButtonsEnabled(false);

            if (skill1CooldownImage != null)
                skill1CooldownImage.fillAmount = 0f;

            if (skill2CooldownImage != null)
                skill2CooldownImage.fillAmount = 0f;
        }

        public void OnClickSkill1()
        {
            if (activeHero == null)
            {
                Debug.LogWarning("場上沒有可使用技能的英雄");
                return;
            }

            if (!activeHero.CanUseSkill1())
            {
                Debug.Log("技能1仍在冷卻中");
                return;
            }

            activeHero.UseSkill1();
            UpdateHeroSkillCooldownUI();
        }

        public void OnClickSkill2()
        {
            if (activeHero == null)
            {
                Debug.LogWarning("場上沒有可使用技能的英雄");
                return;
            }

            if (!activeHero.CanUseSkill2())
            {
                Debug.Log("技能2仍在冷卻中");
                return;
            }

            activeHero.UseSkill2();
            UpdateHeroSkillCooldownUI();
        }

        private void UpdateHeroSkillCooldownUI()
        {
            if (activeHero == null)
            {
                SetSkillButtonsEnabled(false);
                return;
            }

            // Ratio：
            // 剛施放時為 1，冷卻完成時為 0
            if (skill1CooldownImage != null)
            {
                skill1CooldownImage.fillAmount =
                    activeHero.Skill1CooldownRatio;
            }

            if (skill2CooldownImage != null)
            {
                skill2CooldownImage.fillAmount =
                    activeHero.Skill2CooldownRatio;
            }

            if (skill1Button != null)
            {
                skill1Button.interactable =
                    activeHero.CanUseSkill1();
            }

            if (skill2Button != null)
            {
                skill2Button.interactable =
                    activeHero.CanUseSkill2();
            }
        }

        private void SetSkillButtonsEnabled(bool enabled)
        {
            if (skill1Button != null)
                skill1Button.interactable = enabled;

            if (skill2Button != null)
                skill2Button.interactable = enabled;
        }

        #endregion

        #region 遊戲控制

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

        private void UpdatePlayPauseUI()
        {
            if (playPauseButton == null)
                return;

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

        private void OnGameEnd()
        {
            HideUpgradeUI();
            HideHeroUI();

            currentTower = null;
            currentHero = null;
            activeHero = null;

            SetSkillButtonsEnabled(false);

            if (skill1CooldownImage != null)
                skill1CooldownImage.fillAmount = 0f;

            if (skill2CooldownImage != null)
                skill2CooldownImage.fillAmount = 0f;
        }
        private void RefreshSelectedHeroSkillIcons()
        {
            if (HeroSelectionManager.Instance == null)
                return;

            HeroData selectedData =
                HeroSelectionManager.Instance.CurrentHeroData;

            if (selectedData == null)
                return;

            if (skill1Icon != null)
            {
                skill1Icon.sprite = selectedData.skill1.icon;
                skill1Icon.enabled = selectedData.skill1.icon != null;
                skill1Icon.color = Color.white;
            }

            if (skill2Icon != null)
            {
                skill2Icon.sprite = selectedData.skill2.icon;
                skill2Icon.enabled = selectedData.skill2.icon != null;
                skill2Icon.color = Color.white;
            }

            Debug.Log(
                $"技能圖片已切換：" +
                $"{selectedData.heroName} | " +
                $"技能1={selectedData.skill1.icon?.name} | " +
                $"技能2={selectedData.skill2.icon?.name}",
                this
            );
        }

        #endregion
    }
}