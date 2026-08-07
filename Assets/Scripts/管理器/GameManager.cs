using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 管理單一關卡的遊戲狀態、城堡生命、金幣、波數與勝敗流程。
    /// 永久水晶資料由 PlayerData 負責保存。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region  Singleton
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                }

                return instance;
            }
        }
        #endregion

        #region   列舉
        public enum GameState
        {
            Build,
            Playing,
            Paused,
            GameOver,
            Win
        } 
        #endregion

        #region 設定

        [SerializeField]
        private GameState currentState = GameState.Build;

        [SerializeField]
        private int castleHp;

        [SerializeField]
        private int gold = 100;

        [SerializeField]
        private int currentWave;

        [SerializeField]
        private int totalWaves;

       

        [Header("水晶獎勵")]
        [SerializeField, Min(0)] private int winCrystalReward = 100;
        [SerializeField, Min(0)] private int loseCrystalReward = 25;

        [Header("返回主選單")]
        [SerializeField] private Button btnBackToMenuWin;
        [SerializeField] private Button btnBackToMenuGameOver;

        #endregion

        #region   事件

        public event Action<int> OnHpChanged;
        public event Action<int> OnGoldChanged;
        public event Action<int> OnWaveChanged;
        public event Action<int> OnCrystalChanged;

        // 保留舊名稱，避免 UiManager 原本的訂閱失效。
        public event Action OnWin;
        public event Action OnGameOver;

        /// <summary>
        /// 正確命名的勝利事件別名；新程式建議使用這個名稱。
        /// </summary>
        public event Action Onwin
        {
            add => OnWin += value;
            remove => OnWin -= value;
        }

        #endregion

        #region 公開屬性

        public GameState CurrentState => currentState;

        public int CastleHp => castleHp;

        public int Gold => gold;

        public int CurrentWave => currentWave;

        public int TotalWaves => totalWaves;

        public int Crystal => PlayerData.Crystal;
        public int WinCrystalReward => winCrystalReward;
        public int LoseCrystalReward => loseCrystalReward;

        private const int DefaultGold = 100;
        private const string MainMenuSceneName = "主選單";

        private bool crystalRewardGiven;
        #endregion

        #region Unity 生命週期 

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            InitializeRuntimeValues();
        }

        private void Start()
        {
            if (LevelSession.SelectedLevel != null)
            {
                totalWaves = LevelSession.SelectedLevel.TotalWaves;
            }

            BindButtons();
            NotifyAllValues();
        }

        private void OnDestroy()
        {
            UnbindButtons();

            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region 遊戲狀態

        /// <summary>
        /// 目前狀態是否允許建造塔或英雄。
        /// </summary>
        private void ChangeState(GameState state)
        {
            // 狀態沒變就不用重複設定
            if (currentState == state)
                return;

            currentState = state;

#if UNITY_EDITOR
            Debug.Log(
                $"Game State → {state}",
                this
            );
#endif
        }
        public bool CanBuildTower()
        {
            
            return currentState == GameState.Build ||
                   currentState == GameState.Playing;
        }

        public bool IsGameRunning()
        {
            return currentState == GameState.Playing;
        }

        public void StartGame()
        {
            if (currentState != GameState.Build)
                return;

            crystalRewardGiven = false;
            castleHp = ShopBonus.CastleMaxHP;
            ChangeState(GameState.Playing);
            ResumeTime();

          NotifyHp();

#if UNITY_EDITOR
            Debug.Log(
                $"遊戲開始。城堡等級：" +
                $"{PlayerPrefs.GetInt(ShopUpgradeType.CastleHP.ToString(), 0)}，" +
                $"本場生命：{castleHp}",
                this
            );
#endif
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing)
                return;

            ChangeState(GameState.Paused);
           PauseTime(); 
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused)
                return;

            ChangeState(GameState.Playing);
            ResumeTime();
        }

        public void ResetGameState()
        {
            ResumeTime();

            castleHp = ShopBonus.CastleMaxHP;
            gold = DefaultGold;
            currentWave = 0;
            ChangeState(GameState.Build);
            crystalRewardGiven = false;

            NotifyAllValues();
        }

        #endregion

        #region  城堡系統

        /// <summary>
        /// 對城堡造成傷害。
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (currentState != GameState.Playing || damage <= 0)
                return;

            castleHp = Mathf.Max(0, castleHp - damage);
            NotifyHp();

            if (castleHp == 0)
            {
                GameOver();
            }
        }
        #endregion

        #region  金幣系統

        public void AddGold(int amount)
        {
            if (!CanBuildTower() || amount <= 0)
                return;

            gold = Mathf.Max(0, gold + amount);
            NotifyGold();
        }

        public bool SpendGold(int amount)
        {
            if (!CanBuildTower() ||
                amount <= 0 ||
                gold < amount)
            {
                return false;
            }

            gold -= amount;
            NotifyGold();
            return true;
        }

        #endregion

        #region 波數系統
        public void SetTotalWaves(int amount)
        {
            totalWaves = Mathf.Max(0, amount);
        }

        public void StartNextWave()
        {
            if (currentState != GameState.Playing)
                return;

            currentWave++;
            NotifyWave();

#if UNITY_EDITOR
            Debug.Log($"第 {currentWave} 波開始", this);
#endif
        }

        public void EndWave()
        {
            if (currentState != GameState.Playing)
                return;

            CheckWin();

            if (currentState == GameState.Playing)
            {
                StartNextWave();
            }
        }

        public void CheckWin()
        {
            if (currentState != GameState.Playing)
                return;

            if (totalWaves <= 0)
            {
                Debug.LogWarning("Total Waves 尚未設定，無法判斷勝利。", this);
                return;
            }

            if (currentWave >= totalWaves && castleHp > 0)
            {
                Win();
            }
        }

        #endregion
        #region  勝敗系統

        public void Win()
        {
            if (currentState != GameState.Playing)
                return;

            ChangeState(GameState.Win);
            GiveCrystalReward(winCrystalReward);
            PauseTime();

            OnWin?.Invoke();

#if UNITY_EDITOR
            Debug.Log(
                $"遊戲勝利，獲得 {winCrystalReward} 水晶；" +
                $"目前共有 {PlayerData.Crystal} 水晶。",
                this
            );
#endif
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver ||
                currentState == GameState.Win)
            {
                return;
            }

            ChangeState(GameState.GameOver);
            GiveCrystalReward(loseCrystalReward);
            PauseTime();

            OnGameOver?.Invoke();

#if UNITY_EDITOR
            Debug.Log(
                $"遊戲失敗，獲得 {loseCrystalReward} 水晶；" +
                $"目前共有 {PlayerData.Crystal} 水晶。",
                this
            );
#endif
        }

        private void GiveCrystalReward(int amount)
        {
            if (crystalRewardGiven || amount <= 0)
                return;

            crystalRewardGiven = true;
            AddCrystal(amount);
        }

        #endregion

        #region  水晶系統

        /// <summary>
        /// 消耗永久水晶。
        /// </summary>
        public bool SpendCrystal(int amount)
        {
            if (amount <= 0)
                return false;

            if (!PlayerData.SpendCrystal(amount))
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"水晶不足，目前水晶：{PlayerData.Crystal}，需要：{amount}",
                    this
                );
#endif
                return false;
            }

            NotifyCrystal();
            return true;
        }

        /// <summary>
        /// 增加永久水晶。
        /// </summary>
        public void AddCrystal(int amount)
        {
            if (amount <= 0)
                return;

            PlayerData.Crystal += amount;
            NotifyCrystal();       
        }

        #endregion

        #region 場景切換
        private void BackToMenu()
        {
            ResumeTime();

            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.StartLoad(MainMenuSceneName);
                return;
            }

            Debug.LogWarning(
                "找不到 LoadingManager，改用 SceneManager 直接切換場景。",
                this
            );

            SceneManager.LoadScene(MainMenuSceneName);
        }

        #endregion

        #region   UI更新
        private void NotifyHp()
        {
            OnHpChanged?.Invoke(castleHp);
        }

        private void NotifyGold()
        {
            OnGoldChanged?.Invoke(gold);
        }

        private void NotifyWave()
        {
            OnWaveChanged?.Invoke(currentWave);
        }
        private void NotifyCrystal()
        {
            OnCrystalChanged?.Invoke(PlayerData.Crystal);
        } 
        #endregion
        #region  初始化

        private void InitializeRuntimeValues()
        {
            castleHp = ShopBonus.CastleMaxHP;
            gold = Mathf.Max(0, gold);
            currentWave = Mathf.Max(0, currentWave);
            crystalRewardGiven = false;
            ResumeTime();
        }

        private void NotifyAllValues()
        {
            NotifyHp();
            NotifyGold();
            NotifyWave();
            OnCrystalChanged?.Invoke(PlayerData.Crystal);
        }

        private void BindButtons()
        {
            if (btnBackToMenuWin != null)
            {
                btnBackToMenuWin.onClick.AddListener(BackToMenu);
            }

            if (btnBackToMenuGameOver != null)
            {
                btnBackToMenuGameOver.onClick.AddListener(BackToMenu);
            }
        }

        private void UnbindButtons()
        {
            if (btnBackToMenuWin != null)
            {
                btnBackToMenuWin.onClick.RemoveListener(BackToMenu);
            }

            if (btnBackToMenuGameOver != null)
            {
                btnBackToMenuGameOver.onClick.RemoveListener(BackToMenu);
            }
        }

        #endregion
        #region  工具方法
        private void PauseTime()
        {
            Time.timeScale = 0f;
        }

        private void ResumeTime()
        {
            Time.timeScale = 1f;
        } 
        #endregion
    }
}