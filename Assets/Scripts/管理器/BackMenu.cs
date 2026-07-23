using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace CHANG
{
    public class BackMenu : MonoBehaviour
    {
        [SerializeField] private Button btnBackToMenu;

        private void Awake()
        {
            btnBackToMenu.onClick.AddListener(BackToMenu);
        }

        private void BackToMenu()
        {
          
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
    }
}