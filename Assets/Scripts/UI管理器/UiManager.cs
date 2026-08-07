using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// UI 管理器核心類別。
    /// 負責 UI 系統初始化、事件註冊與目前選擇物件管理。
    /// 其他 UI 功能由 partial 檔案分開處理。
    /// </summary>
    public partial class UiManager : MonoBehaviour
    {
        #region Singleton

        public static UiManager Instance
        {
            get;
            private set;
        }

        #endregion

        #region 執行期間資料

        private Tower currentTower;
        private Hero currentHero;
        private Hero activeHero;

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
        }

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError(
                    "找不到 GameManager",
                    this
                );

                return;
            }

            RegisterGameEvents();
            RegisterHeroSelectionEvents();

            InitializeGeneralUI();
            InitializeSkillUI();
        }

        private void Update()
        {
            if (activeHero == null)
                return;

            UpdateHeroSkillCooldownUI();
        }

        private void OnDestroy()
        {
            UnregisterGameEvents();
            UnregisterHeroSelectionEvents();
            UnsubscribeCurrentHero();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 遊戲事件

        private void RegisterGameEvents()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            gameManager.OnHpChanged +=
                UpdateHp;

            gameManager.OnGoldChanged +=
                UpdateGold;

            gameManager.OnWaveChanged +=
                UpdateWave;

            gameManager.OnGameOver +=
                ShowGameOver;

            gameManager.OnWin +=
                ShowWin;

            gameManager.OnGameOver +=
                OnGameEnd;

            gameManager.OnWin +=
                OnGameEnd;
        }

        private void UnregisterGameEvents()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            gameManager.OnHpChanged -=
                UpdateHp;

            gameManager.OnGoldChanged -=
                UpdateGold;

            gameManager.OnWaveChanged -=
                UpdateWave;

            gameManager.OnGameOver -=
                ShowGameOver;

            gameManager.OnWin -=
                ShowWin;

            gameManager.OnGameOver -=
                OnGameEnd;

            gameManager.OnWin -=
                OnGameEnd;
        }

        #endregion

        #region 英雄選擇事件

        private void RegisterHeroSelectionEvents()
        {
            HeroSelectionManager manager =
                HeroSelectionManager.Instance;

            if (manager == null)
                return;

            manager.OnSelectedHeroChanged +=
                RefreshSelectedHeroSkillIcons;
        }

        private void UnregisterHeroSelectionEvents()
        {
            HeroSelectionManager manager =
                HeroSelectionManager.Instance;

            if (manager == null)
                return;

            manager.OnSelectedHeroChanged -=
                RefreshSelectedHeroSkillIcons;
        }

        #endregion

        #region 英雄事件

        private void UnsubscribeCurrentHero()
        {
            if (currentHero == null)
                return;

            currentHero.OnHeroDataChanged -=
                RefreshHeroUI;

            currentHero = null;
        }

        #endregion
    }
}