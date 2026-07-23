using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace CHANG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        /// <summary>
        /// 遊戲狀態
        /// </summary>
        public enum GameState
        {
            Build,
            Playing,
            Paused,
            GameOver,
            Win
        }
        /// <summary>
        /// 遊戲狀態初始化
        /// </summary>
        public GameState currentState = GameState.Build;
        [Header("數值")]
        #region  數值
        public int castleHp = 100;
        public int gold = 100;
        public int currentWave = 0;
        public int totalWaves = 0;
        #endregion
        #region 事件
        public event Action<int> OnHpChanged;
        public event Action<int> OnGoldChanged;
        public event Action<int> OnWaveChanged;
        public event Action Onwin;
        public event Action OnGameOver;
        #endregion
        [Header("返回主選單")]
        [SerializeField] private Button btnBackToMenuWin;
        [SerializeField] private Button btnBackToMenuGameOver;


        public bool CanBuildTower()
        {
            return currentState == GameState.Build || currentState == GameState.Playing;
        }

        public void StartGame()
        {
            if (currentState != GameState.Build) return;

            currentState = GameState.Playing;
            Time.timeScale = 1f; // 確保遊戲時間恢復正常
            Debug.Log("遊戲開始!");

        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Paused;
            Time.timeScale = 0f; // 暫停遊戲時間
            Debug.Log("遊戲暫停!");
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = GameState.Playing;
            Time.timeScale = 1f; // 恢復遊戲時間
            Debug.Log("遊戲繼續!");
        }
        private void Awake()
        {
            // 單例模式
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
        private void Start()
        {
            if (LevelSession.SelectedLevel != null)
            {
                totalWaves = LevelSession.SelectedLevel.totalWaves;
            }
            // 初始化事件(顯示當前數值)
            OnHpChanged?.Invoke(castleHp);
            OnGoldChanged?.Invoke(gold);
            OnWaveChanged?.Invoke(currentWave);
            btnBackToMenuWin.onClick.AddListener(BackToMenu);
            btnBackToMenuGameOver.onClick.AddListener(BackToMenu);
        }
        public void TakeDamege(int damage)
        {
            // 只有在遊戲進行中才會受到傷害
            if (currentState != GameState.Playing) return;
            castleHp -= damage;
            // 更新UI
            OnHpChanged?.Invoke(castleHp);
            if (castleHp <= 0)
            {
                castleHp = 0;
                GameOver();
            }
        }
        public void AddGold(int amount)
        {
            if (currentState != GameState.Playing) return;

            gold += amount;
            if (gold < 0) gold = 0;

            OnGoldChanged?.Invoke(gold);
        }
        public bool SpendGold(int amout)
        {
            if (gold >= amout)
            {
                gold -= amout;
                OnGoldChanged?.Invoke(gold);
                return true;
            }
            return false;
        }
        public void StartNextWave()
        {
            //防止重複觸發
            if (currentState != GameState.Playing) return;
            //波數 + 1
            currentWave++;
            //更新UI
            OnWaveChanged?.Invoke(currentWave);
            Debug.Log($"第{currentWave}波開始");
        }
        public void EndWave()
        {
            if (currentState != GameState.Playing) return;

            CheckWin();

            if (currentState == GameState.Playing)
            {
                StartNextWave();
            }
        }
        public void CheckWin()
        {
            Debug.Log($"CheckWin currentWave={currentWave}, totalWaves={totalWaves}, HP={castleHp}");

            if (currentWave >= totalWaves && castleHp > 0)
            {
                Debug.Log("Win!");
                Win();
            }
        }
        public void Win()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Win;
            Time.timeScale = 0f;   // ⭐ 補上
            Debug.Log("你贏了!");
            Onwin?.Invoke();
        }

        public void GameOver()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.GameOver;
            Time.timeScale = 0f;   // ⭐ 補上
            Debug.Log("遊戲結束!");
            OnGameOver?.Invoke();
        }
        public bool IsGameRunning()
        {
            return currentState == GameState.Playing;
        }
        private void BackToMenu()
        {
            Time.timeScale = 1f;
            ResetGameState();   // 如果這個方法就寫在 GameManager 裡，直接呼叫自己就好，不用 GameManager.Instance.

            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.StartLoad("主選單");
            }
            else
            {
                Debug.LogWarning("沒有 LoadingManager，直接切換場景（開發測試用）");
                SceneManager.LoadScene("主選單");
            }
        }
        public void ResetGameState()
        {
            castleHp = 100;
            gold = 100;
            currentWave = 0;
            currentState = GameState.Build;

            // 重置後記得更新 UI 顯示
            OnHpChanged?.Invoke(castleHp);
            OnGoldChanged?.Invoke(gold);
            OnWaveChanged?.Invoke(currentWave);

        }
    }
}
