using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace CHANG
{
    /// <summary>
    ///主選單管理器
    /// </summary>
    public class MenuManager : MonoBehaviour

    {
        private Button btnContinue, btnNewGame, btnOption, btnCredit, btnQuit, btnBackOption, btnBackCredit;

        private CanvasGroup groupMain, groupOption, groupCredit;
        private Slider sliderMaster, sliderBgm, sliderSfx;

        private void Awake()
        {
            #region  尋找介面物件
            btnNewGame = GameObject.Find("按鈕_開始遊戲").GetComponent<Button>();
            btnOption = GameObject.Find("按鈕_設定").GetComponent<Button>();
            btnCredit = GameObject.Find("按鈕_製作團隊").GetComponent<Button>();
            btnQuit = GameObject.Find("按鈕_退出遊戲").GetComponent<Button>();
            btnBackOption = GameObject.Find("按鈕_設定_返回").GetComponent<Button>();
            btnBackCredit = GameObject.Find("按鈕_製作團隊_返回").GetComponent<Button>();
            groupOption = GameObject.Find("群組_設定").GetComponent<CanvasGroup>();
            groupCredit = GameObject.Find("群組_製作團隊").GetComponent<CanvasGroup>();
            sliderMaster = GameObject.Find("滑桿_主音量").GetComponent<Slider>();
            sliderBgm = GameObject.Find("滑桿_音樂").GetComponent<Slider>();
            sliderSfx = GameObject.Find("滑桿_音效").GetComponent<Slider>();


            #endregion
            btnOption.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupOption));

            });
            btnCredit.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupCredit));

            });
            btnBackOption.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupOption, false));

            });
            btnBackCredit.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupCredit, false));

            });
            btnNewGame.onClick.AddListener(NewGame);
            btnQuit.onClick.AddListener(Quit);

            sliderMaster.onValueChanged.AddListener(x => SoundManager.Instance.UpdateMasterVolume(x));
            sliderBgm.onValueChanged.AddListener(x => SoundManager.Instance.UpdateBGMVolume(x));
            sliderSfx.onValueChanged.AddListener(x => SoundManager.Instance.UpdateSFXVolume(x));

            sliderMaster.value = SoundManager.Instance.VolumeMaster;
            sliderBgm.value = SoundManager.Instance.VolumeBGM;
            sliderSfx.value = SoundManager.Instance.VolumeSFX;



        }
        private void Continue()
        {

        }
        private void NewGame()
        {
            LoadingManager.Instance.StartLoad("遊戲場景");
        }
        private void Quit()
        {
            Application.Quit();
            Debug.Log("遊戲已退出");
        }
    }
}