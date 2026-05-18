using CHANG;
using UnityEngine;
using UnityEngine.UI;
public class SettingsPanel: MonoBehaviour
{
    private Button btn, btnSettings, btnBack;
    private CanvasGroup groupOption;
    private Slider sliderMaster, sliderBgm, sliderSfx;

    private void Awake()
    {
        btnSettings = GameObject.Find("按鈕_設定").GetComponent<Button>();
        btnBack = GameObject.Find("按鈕_設定_返回").GetComponent<Button>();
        groupOption = GameObject.Find("群組_設定").GetComponent<CanvasGroup>();
        sliderMaster = GameObject.Find("滑桿_主音量").GetComponent<Slider>();
        sliderBgm = GameObject.Find("滑桿_音樂").GetComponent<Slider>();
        sliderSfx = GameObject.Find("滑桿_音效").GetComponent<Slider>();

        btnSettings.onClick.AddListener(() =>
        {
            StartCoroutine(FadeSystem.Fade(groupOption));
        });
        btnBack.onClick.AddListener(() =>
        {
            StartCoroutine(FadeSystem.Fade(groupOption, false));
        });
        sliderMaster.onValueChanged.AddListener(x => SoundManager.Instance.UpdateMasterVolume(x));
        sliderBgm.onValueChanged.AddListener(x => SoundManager.Instance.UpdateBGMVolume(x));
        sliderSfx.onValueChanged.AddListener(x => SoundManager.Instance.UpdateSFXVolume(x));

       
    }
    private void Start()
    {
        sliderMaster.value = SoundManager.Instance.VolumeMaster;
        sliderBgm.value = SoundManager.Instance.VolumeBGM;
        sliderSfx.value = SoundManager.Instance.VolumeSFX;
    }
   
}
