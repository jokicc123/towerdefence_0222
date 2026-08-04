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
        private Button btnNewGame,  btnCredit,btnShop, btnQuit,btnBackCredit;
        private CanvasGroup groupCredit;
        private void Awake()
        {
            #region  尋找介面物件
            btnNewGame = GameObject.Find("按鈕_開始遊戲").GetComponent<Button>();
            btnCredit = GameObject.Find("按鈕_製作團隊").GetComponent<Button>();
            btnShop = GameObject.Find("按鈕_商店").GetComponent<Button>();
            btnBackCredit = GameObject.Find("按鈕_製作團隊_返回").GetComponent<Button>();
            btnQuit = GameObject.Find("按鈕_退出遊戲").GetComponent<Button>();
            groupCredit = GameObject.Find("群組_製作團隊").GetComponent<CanvasGroup>();
            #endregion
       
            btnCredit.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupCredit));
            });
           
            btnBackCredit.onClick.AddListener(() =>
            {
                StartCoroutine(FadeSystem.Fade(groupCredit, false));
            });
            btnShop.onClick.AddListener(() =>
            {
                LoadingManager.Instance.StartLoad("商店");
            });


            btnNewGame.onClick.AddListener(NewGame);
            btnQuit.onClick.AddListener(Quit);
        }
        private void NewGame()
        {
            LoadingManager.Instance.StartLoad("選關頁面");
        }
        private void Quit()
        {
            Application.Quit();
            Debug.Log("遊戲已退出");
        }
    }
}