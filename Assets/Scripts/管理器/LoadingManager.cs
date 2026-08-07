using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 管理場景非同步載入、
    /// 載入進度 UI 與淡入淡出效果。
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        #region Singleton

        public static LoadingManager Instance
        {
            get;
            private set;
        }

        #endregion

        #region 執行期間資料

        private CanvasGroup group;
        private TMP_Text textProgress;
        private Image imageProgress;

        private bool isLoading;

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

            DontDestroyOnLoad(gameObject);

            CacheComponents();
        }

        #endregion

        #region 初始化

        private void CacheComponents()
        {
            group =
                GetComponent<CanvasGroup>();

            Transform textTransform =
                transform.Find(
                    "文字_載入進度"
                );

            Transform imageTransform =
                transform.Find(
                    "圖片_載入進度"
                );

            if (textTransform != null)
            {
                textProgress =
                    textTransform
                        .GetComponent<TMP_Text>();
            }

            if (imageTransform != null)
            {
                imageProgress =
                    imageTransform
                        .GetComponent<Image>();
            }

            if (group == null)
            {
                Debug.LogError(
                    "LoadingManager 缺少 CanvasGroup",
                    this
                );

                return;
            }

            group.blocksRaycasts = true;
            group.interactable = true;
        }

        #endregion

        #region 場景載入

        /// <summary>
        /// 開始非同步載入指定場景。
        /// </summary>
        public void StartLoad(
            string sceneName)
        {
            if (isLoading)
                return;

            if (string.IsNullOrWhiteSpace(
                    sceneName))
            {
                Debug.LogError(
                    "載入場景名稱不能為空",
                    this
                );

                return;
            }

            StartCoroutine(
                Loading(sceneName)
            );
        }

        #endregion

        #region 載入流程

        private IEnumerator Loading(
            string sceneName)
        {
            isLoading = true;

            if (group != null)
            {
                yield return StartCoroutine(
                    FadeSystem.Fade(
                        group,
                        true
                    )
                );
            }

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(
                    sceneName
                );

            if (operation == null)
            {
                Debug.LogError(
                    $"無法載入場景：{sceneName}",
                    this
                );

                isLoading = false;
                yield break;
            }

            operation.allowSceneActivation =
                false;

            while (operation.progress < 0.9f)
            {
                float progress =
                    Mathf.Clamp01(
                        operation.progress /
                        0.9f
                    );

                UpdateProgressUI(
                    progress
                );

                yield return null;
            }

            // 載入完成
            UpdateProgressUI(1f);

            operation.allowSceneActivation =
                true;

            // 等待新場景真正完成切換
            while (!operation.isDone)
            {
                yield return null;
            }

            if (group != null)
            {
                yield return StartCoroutine(
                    FadeSystem.Fade(
                        group,
                        false
                    )
                );
            }

            isLoading = false;
        }

        private void UpdateProgressUI(
            float progress)
        {
            progress =
                Mathf.Clamp01(progress);

            if (textProgress != null)
            {
                textProgress.text =
                    $"{progress * 100f:0}%";
            }

            if (imageProgress != null)
            {
                imageProgress.fillAmount =
                    progress;
            }
        }

        #endregion
    }
}