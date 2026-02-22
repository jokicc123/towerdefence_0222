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
                if (instance == null) instance = FindAnyObjectByType<SoundManager>();
                return instance;
            }
        }


        [SerializeField]
        private AudioMixer audioMixer;

        private AudioSource aud;

        private string parMater = "VolumeMaster", ParBGM = "VolumeBGM", parSFX = "VolumeSFX";
        //取得主音量 背景音量 音效音量 (如果沒有儲存過預設為0)
        public float VolumeMaster => PlayerPrefs.GetFloat(parMater, 0f);
        public float VolumeBGM => PlayerPrefs.GetFloat(ParBGM, 0f);
        public float VolumeSFX => PlayerPrefs.GetFloat(parSFX, 0f);

        private void Awake()
        {
            aud = GetComponent<AudioSource>();

        }
        /// <summary>
        /// 更新主音量
        /// </summary>
        /// <param name="volume"></param>
        public void UpdateMasterVolume(float volume)
        {
            audioMixer.SetFloat("VolumeMaster", volume);
            //儲存音量設定到電腦內，名稱為 VolumeMaster
            PlayerPrefs.SetFloat("VolumeMaster", volume);

        }
        /// <summary>
        /// 更新背景音樂音量
        /// </summary>
        /// <param name="volume"></param>
        public void UpdateBGMVolume(float volume)
        {
            audioMixer.SetFloat("VolumeBGM", volume);
            //儲存音量設定到電腦內，名稱為 VolumeBGM
            PlayerPrefs.SetFloat("VolumeBGM", volume);
        }

        /// <summary>
        /// 更新音效音量  
        /// </summary>
        /// <param name="volume"></param>
        public void UpdateSFXVolume(float volume)
        {
            audioMixer.SetFloat("VolumeSFX", volume);
            //儲存音量設定到電腦內，名稱為 VolumeSFX
            PlayerPrefs.SetFloat("VolumeSFX", volume);
        }

    }
}