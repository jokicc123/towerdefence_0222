using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 返回主選單按鈕。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BackMenu : MonoBehaviour
    {
        #region Inspector 設定

        [SerializeField]
        private Button btnBackToMenu;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (btnBackToMenu == null)
            {
                btnBackToMenu =
                    GetComponent<Button>();
            }

            Debug.Assert(
                btnBackToMenu != null,
                "BackMenu 缺少 Button",
                this
            );

            btnBackToMenu.onClick.AddListener(
                BackToMenu
            );
        }

        private void OnDestroy()
        {
            if (btnBackToMenu != null)
            {
                btnBackToMenu.onClick.RemoveListener(
                    BackToMenu
                );
            }
        }

        #endregion

        #region 返回主選單

        private void BackToMenu()
        {
            Time.timeScale = 1f;

            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.StartLoad(
                    "主選單"
                );

                return;
            }

            Debug.LogWarning(
                "沒有 LoadingManager，直接切換場景（開發測試用）"
            );

            SceneManager.LoadScene(
                "主選單"
            );
        }

        #endregion
    }
}