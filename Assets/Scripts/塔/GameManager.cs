using UnityEngine;

namespace CHANG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int playerHP = 10;
        public int money = 200;

        private void Awake()
        {
            Instance = this;
        }

        public void PlayerDamage(int damage)
        {
            playerHP -= damage;

            UIManager.Instance.UpdateUI();

            if (playerHP <= 0)
            {
                GameOver();
            }
        }

        public void AddMoney(int amount)
        {
            money += amount;
            UIManager.Instance.UpdateUI();
        }

        void GameOver()
        {
            Debug.Log("遊戲結束");
        }
    }
}