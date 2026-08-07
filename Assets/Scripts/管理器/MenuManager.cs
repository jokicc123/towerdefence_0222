using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 主選單管理器。
    /// 負責開始遊戲、商店、製作團隊與離開遊戲。
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        #region Inspector 設定

        [Header("主選單按鈕")]
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnCredit;
        [SerializeField] private Button btnShop;
        [SerializeField] private Button btnQuit;

        [Header("製作團隊")]
        [SerializeField] private Button btnBackCredit;
        [SerializeField] private CanvasGroup groupCredit;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            RegisterButtons();
        }

        private void OnDestroy()
        {
            UnregisterButtons();
        }

        #endregion

        #region UI 初始化

        private void RegisterButtons()
        {
            if (btnNewGame != null)
            {
                btnNewGame.onClick.AddListener(
                    NewGame
                );
            }

            if (btnCredit != null)
            {
                btnCredit.onClick.AddListener(
                    ShowCredit
                );
            }

            if (btnBackCredit != null)
            {
                btnBackCredit.onClick.AddListener(
                    HideCredit
                );
            }

            if (btnShop != null)
            {
                btnShop.onClick.AddListener(
                    OpenShop
                );
            }

            if (btnQuit != null)
            {
                btnQuit.onClick.AddListener(
                    Quit
                );
            }
        }

        private void UnregisterButtons()
        {
            if (btnNewGame != null)
            {
                btnNewGame.onClick.RemoveListener(
                    NewGame
                );
            }

            if (btnCredit != null)
            {
                btnCredit.onClick.RemoveListener(
                    ShowCredit
                );
            }

            if (btnBackCredit != null)
            {
                btnBackCredit.onClick.RemoveListener(
                    HideCredit
                );
            }

            if (btnShop != null)
            {
                btnShop.onClick.RemoveListener(
                    OpenShop
                );
            }

            if (btnQuit != null)
            {
                btnQuit.onClick.RemoveListener(
                    Quit
                );
            }
        }

        #endregion

        #region 按鈕事件

        private void ShowCredit()
        {
            if (groupCredit == null)
                return;

            StartCoroutine(
                FadeSystem.Fade(
                    groupCredit,
                    true
                )
            );
        }

        private void HideCredit()
        {
            if (groupCredit == null)
                return;

            StartCoroutine(
                FadeSystem.Fade(
                    groupCredit,
                    false
                )
            );
        }

        private void OpenShop()
        {
            LoadScene("商店");
        }

        private void NewGame()
        {
            LoadScene("選關頁面");
        }

        private void Quit()
        {
#if UNITY_EDITOR
            Debug.Log(
                "遊戲已退出",
                this
            );
#endif

            Application.Quit();
        }

        #endregion

        #region 場景切換

        private void LoadScene(
            string sceneName)
        {
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.StartLoad(
                    sceneName
                );

                return;
            }

            Debug.LogError(
                $"找不到 LoadingManager，無法載入場景：{sceneName}",
                this
            );
        }

        #endregion
    }
}