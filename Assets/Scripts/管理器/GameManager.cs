using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    /// <summary>
    /// 遊戲狀態
    /// </summary>
    public enum GameState
    {
        Playing,
        GameOver,
        Win
    }
    /// <summary>
    /// 遊戲狀態初始化
    /// </summary>
    public GameState currentState = GameState.Playing;
    [Header("數值")]
    #region  數值
    public int castleHp = 100;
    public int gold = 100;
    public int currentWave = 1;
    public int totalWaves = 5;
    #endregion
    #region 事件
    public event Action<int> OnHpChanged;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnWaveChanged;
    public event Action Onwin;
    public event Action OnGameOver;
    #endregion

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
        // 初始化事件(顯示當前數值)
        OnHpChanged?.Invoke(castleHp);
        OnGoldChanged?.Invoke(gold);
        OnWaveChanged?.Invoke(currentWave);
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
        if (currentState!=GameState.Playing)return;
        //波數 + 1
        currentWave++;
        //更新UI
        OnWaveChanged?.Invoke(currentWave);
        Debug.Log($"第{currentWave}波開始");
    }
    public void CheckWin()
    {
        if (currentWave >= totalWaves)
        {
            win();
        }
    }
    public void win()
    {
        
        if (currentState != GameState.Playing) return;
        //設定狀態
        currentState = GameState.Win;
        Debug.Log("你贏了!");
        Onwin?.Invoke();
    }
    public void GameOver()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.GameOver;

        Debug.Log("遊戲結束!");

    }
} 
