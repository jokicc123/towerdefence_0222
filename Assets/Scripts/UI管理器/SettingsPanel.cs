using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 設定面板。
    /// 負責開關設定介面與同步音量滑桿。
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        #region Inspector 設定

        [Header("設定面板")]
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnBack;
        [SerializeField] private CanvasGroup groupOption;

        [Header("音量滑桿")]
        [SerializeField] private Slider sliderMaster;
        [SerializeField] private Slider sliderBgm;
        [SerializeField] private Slider sliderSfx;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            RegisterEvents();
        }

        private void Start()
        {
            RefreshVolumeUI();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (btnSettings != null)
            {
                btnSettings.onClick.AddListener(
                    ShowSettings
                );
            }

            if (btnBack != null)
            {
                btnBack.onClick.AddListener(
                    HideSettings
                );
            }

            if (sliderMaster != null)
            {
                sliderMaster.onValueChanged.AddListener(
                    OnMasterVolumeChanged
                );
            }

            if (sliderBgm != null)
            {
                sliderBgm.onValueChanged.AddListener(
                    OnBgmVolumeChanged
                );
            }

            if (sliderSfx != null)
            {
                sliderSfx.onValueChanged.AddListener(
                    OnSfxVolumeChanged
                );
            }
        }

        private void UnregisterEvents()
        {
            if (btnSettings != null)
            {
                btnSettings.onClick.RemoveListener(
                    ShowSettings
                );
            }

            if (btnBack != null)
            {
                btnBack.onClick.RemoveListener(
                    HideSettings
                );
            }

            if (sliderMaster != null)
            {
                sliderMaster.onValueChanged.RemoveListener(
                    OnMasterVolumeChanged
                );
            }

            if (sliderBgm != null)
            {
                sliderBgm.onValueChanged.RemoveListener(
                    OnBgmVolumeChanged
                );
            }

            if (sliderSfx != null)
            {
                sliderSfx.onValueChanged.RemoveListener(
                    OnSfxVolumeChanged
                );
            }
        }

        #endregion

        #region 設定面板

        private void ShowSettings()
        {
            if (groupOption == null)
                return;

            StartCoroutine(
                FadeSystem.Fade(
                    groupOption,
                    true
                )
            );
        }

        private void HideSettings()
        {
            if (groupOption == null)
                return;

            StartCoroutine(
                FadeSystem.Fade(
                    groupOption,
                    false
                )
            );
        }

        #endregion

        #region 音量更新

        private void RefreshVolumeUI()
        {
            if (SoundManager.Instance == null)
                return;

            if (sliderMaster != null)
            {
                sliderMaster.SetValueWithoutNotify(
                    SoundManager.Instance.VolumeMaster
                );
            }

            if (sliderBgm != null)
            {
                sliderBgm.SetValueWithoutNotify(
                    SoundManager.Instance.VolumeBGM
                );
            }

            if (sliderSfx != null)
            {
                sliderSfx.SetValueWithoutNotify(
                    SoundManager.Instance.VolumeSFX
                );
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            SoundManager.Instance?.UpdateMasterVolume(
                value
            );
        }

        private void OnBgmVolumeChanged(float value)
        {
            SoundManager.Instance?.UpdateBGMVolume(
                value
            );
        }

        private void OnSfxVolumeChanged(float value)
        {
            SoundManager.Instance?.UpdateSFXVolume(
                value
            );
        }

        #endregion
    }
}