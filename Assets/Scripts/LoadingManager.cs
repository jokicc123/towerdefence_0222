using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
namespace CHANG
{
    /// <summary>
    /// 載入管理器
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        //單例模式
        public static LoadingManager Instance => Singleton<LoadingManager>.Instance;
        private CanvasGroup group;
        private TMP_Text textprogress;
        private Image imageprogress;

        private void Awake()
        {
            if (Instance != this) Destroy(gameObject);
            else DontDestroyOnLoad(gameObject);

            group = GetComponent<CanvasGroup>();
            textprogress = transform.Find("文字_載入進度").GetComponent<TMP_Text>();
            imageprogress = transform.Find("圖片_載入進度").GetComponent<Image>();
        }


        public void StartLoad(string scneName)
        {
            StartCoroutine(Loading(scneName));
        }
        /// <summary>
        /// 載入
        /// </summary>
        /// <param name="scneName"></param>
        /// <returns></returns>
        private IEnumerator Loading(string scneName)
        {
            yield return StartCoroutine(FadeSystem.Fade(group));
            AsyncOperation option = SceneManager.LoadSceneAsync(scneName);
            option.allowSceneActivation = false; //不自動載入
            while (!option.isDone)
            {
                textprogress.text = $"{option.progress / 0.9f * 100}%";
                imageprogress.fillAmount = option.progress / 0.9f;
                yield return null;

                if (option.progress == 0.9f) option.allowSceneActivation = true;//允許載入
            }

            option.allowSceneActivation = true;
            yield return new WaitForSeconds(1);
            yield return StartCoroutine(FadeSystem.Fade(group, false));

        }


    }
}
