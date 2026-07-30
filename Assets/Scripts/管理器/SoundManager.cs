using UnityEngine;
using UnityEngine.Audio;

namespace CHANG
{
    /// <summary>
    /// 音效管理器
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
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

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("音效播放來源")]
        [SerializeField] private AudioSource sfxSource;

        private const string VolumeMasterParameter =
            "VolumeMaster";

        private const string VolumeBGMParameter =
            "VolumeBGM";

        private const string VolumeSFXParameter =
            "VolumeSFX";

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

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

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

        public void UpdateMasterVolume(float volume)
        {
            if (audioMixer == null)
                return;

            audioMixer.SetFloat(
                VolumeMasterParameter,
                volume
            );

            PlayerPrefs.SetFloat(
                VolumeMasterParameter,
                volume
            );

            PlayerPrefs.Save();
        }

        public void UpdateBGMVolume(float volume)
        {
            if (audioMixer == null)
                return;

            audioMixer.SetFloat(
                VolumeBGMParameter,
                volume
            );

            PlayerPrefs.SetFloat(
                VolumeBGMParameter,
                volume
            );

            PlayerPrefs.Save();
        }

        public void UpdateSFXVolume(float volume)
        {
            if (audioMixer == null)
                return;

            bool success = audioMixer.SetFloat(
                VolumeSFXParameter,
                volume
            );

            if (!success)
            {
                Debug.LogError(
                    $"AudioMixer 找不到參數：{VolumeSFXParameter}"
                );
            }

            PlayerPrefs.SetFloat(
                VolumeSFXParameter,
                volume
            );

            PlayerPrefs.Save();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
                return;

            if (sfxSource == null)
            {
                Debug.LogWarning(
                    "SoundManager 沒有 AudioSource"
                );

                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        public void PlaySFX(
            AudioClip clip,
            float volumeScale)
        {
            if (clip == null || sfxSource == null)
                return;

            sfxSource.PlayOneShot(
                clip,
                Mathf.Clamp01(volumeScale)
            );
        }
    }
}