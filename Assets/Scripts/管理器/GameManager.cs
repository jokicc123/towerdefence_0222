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
        public int castleHp;
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

        [Header("水晶")]
        // 這裡的水晶數量是遊戲內的初始值，實際的水晶數量應該從 PlayerData 或其他持久化系統中讀取
        [SerializeField] private int crystal = 100;

        public int Crystal => crystal;

        public event System.Action<int> OnCrystalChanged;
        [SerializeField] private int winCrystalReward = 100;
        [SerializeField] private int loseCrystalReward = 25;
        public int WinCrystalReward => winCrystalReward;
        public int LoseCrystalReward => loseCrystalReward;
        private bool crystalRewardGiven;

        public bool CanBuildTower()
        {
            return currentState == GameState.Build || currentState == GameState.Playing;
        }

        public void StartGame()
        {
            crystalRewardGiven = false;

            castleHp =
               ShopBonus.CastleMaxHP;

            OnHpChanged?.Invoke(castleHp);

            Debug.Log(
                $"商店城堡等級：" +
                $"{PlayerPrefs.GetInt(ShopUpgradeType.CastleHP.ToString(), 0)}，" +
                $"本場城堡生命：{castleHp}"
            );
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
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            castleHp = ShopBonus.CastleMaxHP;
            crystalRewardGiven = false;

            Debug.Log($"城堡初始生命：{castleHp}");
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
            GiveCrystalReward(winCrystalReward);
            Time.timeScale = 0f;   // ⭐ 補上
            Debug.Log(
                $"你贏了，獲得 {winCrystalReward} 水晶，" +
                $"目前共有 {PlayerData.Crystal} 水晶"
            );
            Onwin?.Invoke();
        }

        public void GameOver()
        {
            if (currentState == GameState.GameOver)
                return;

            currentState = GameState.GameOver;

            GiveCrystalReward(loseCrystalReward);

            Time.timeScale = 0f;

            OnGameOver?.Invoke();

            Debug.Log(
                $"遊戲失敗，獲得 {loseCrystalReward} 水晶，" +
                $"目前共有 {PlayerData.Crystal} 水晶"
            );
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
            castleHp = ShopBonus.CastleMaxHP;
            gold = 100;
            currentWave = 0;
            currentState = GameState.Build;

            // 重置後記得更新 UI 顯示
            OnHpChanged?.Invoke(castleHp);
            OnGoldChanged?.Invoke(gold);
            OnWaveChanged?.Invoke(currentWave);

        }
        private void GiveCrystalReward(int amount)
        {
            PlayerData.Crystal += amount;
        }
        public bool SpendCrystal(int amount)
        {
            if (amount <= 0)
                return false;

            if (crystal < amount)
            {
                Debug.Log($"水晶不足，目前水晶：{crystal}，需要：{amount}");
                return false;
            }

            crystal -= amount;
            OnCrystalChanged?.Invoke(crystal);

            Debug.Log($"消耗 {amount} 水晶，剩餘 {crystal}");
            return true;
        }

        public void AddCrystal(int amount)
        {
            if (amount <= 0)
                return;

            crystal += amount;
            OnCrystalChanged?.Invoke(crystal);
        }
    }
}
