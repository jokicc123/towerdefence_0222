using TMPro;
using UnityEngine;

namespace CHANG
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public TextMeshProUGUI hpText;
        public TextMeshProUGUI moneyText;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateUI()
        {
            hpText.text = "HP: " + GameManager.Instance.playerHP;
            moneyText.text = "Money: " + GameManager.Instance.money;
        }
    }
}