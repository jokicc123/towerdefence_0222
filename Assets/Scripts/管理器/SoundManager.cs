using UnityEngine;
using UnityEngine.Audio;

namespace CHANG
{
    /// <summary>
    /// 音效管理器。
    /// 負責 AudioMixer 音量設定與音效播放。
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        #region Singleton

        private static SoundManager instance;

        public static SoundManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance =
                        FindAnyObjectByType<SoundManager>();
                }

                return instance;
            }
        }

        #endregion

        #region Inspector 設定

        [Header("Audio Mixer")]
        [SerializeField]
        private AudioMixer audioMixer;

        [Header("音效播放來源")]
        [SerializeField]
        private AudioSource sfxSource;

        #endregion

        #region 常數

        private const string VolumeMasterParameter =
            "VolumeMaster";

        private const string VolumeBGMParameter =
            "VolumeBGM";

        private const string VolumeSFXParameter =
            "VolumeSFX";

        #endregion

        #region 公開屬性

        public float VolumeMaster =>
            PlayerPrefs.GetFloat(
                VolumeMasterParameter,
                0f
            );

        public float VolumeBGM =>
            PlayerPrefs.GetFloat(
                VolumeBGMParameter,
                0f
            );

        public float VolumeSFX =>
            PlayerPrefs.GetFloat(
                VolumeSFXParameter,
                0f
            );

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            if (instance != null &&
                instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            CacheAudioSource();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region 初始化

        private void CacheAudioSource()
        {
            if (sfxSource == null)
            {
                sfxSource =
                    GetComponent<AudioSource>();
            }

            if (sfxSource == null)
            {
                Debug.LogError(
                    "SoundManager 沒有設定 AudioSource",
                    this
                );
            }
        }

        #endregion

        #region 音量設定

        public void UpdateMasterVolume(
            float volume)
        {
            SetMixerVolume(
                VolumeMasterParameter,
                volume
            );
        }

        public void UpdateBGMVolume(
            float volume)
        {
            SetMixerVolume(
                VolumeBGMParameter,
                volume
            );
        }

        public void UpdateSFXVolume(
            float volume)
        {
            SetMixerVolume(
                VolumeSFXParameter,
                volume
            );
        }

        private void SetMixerVolume(
            string parameterName,
            float volume)
        {
            if (audioMixer == null)
                return;

            bool success =
                audioMixer.SetFloat(
                    parameterName,
                    volume
                );

            if (!success)
            {
                Debug.LogError(
                    $"AudioMixer 找不到參數：{parameterName}",
                    this
                );

                return;
            }

            PlayerPrefs.SetFloat(
                parameterName,
                volume
            );

            PlayerPrefs.Save();
        }

        #endregion

        #region 音效播放

        public void PlaySFX(
            AudioClip clip)
        {
            PlaySFX(
                clip,
                1f
            );
        }

        public void PlaySFX(
            AudioClip clip,
            float volumeScale)
        {
            if (clip == null)
                return;

            if (sfxSource == null)
            {
                Debug.LogWarning(
                    "SoundManager 沒有 AudioSource",
                    this
                );

                return;
            }

            sfxSource.PlayOneShot(
                clip,
                Mathf.Clamp01(volumeScale)
            );
        }

        #endregion
    }
}