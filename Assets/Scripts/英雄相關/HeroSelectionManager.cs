using System;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理目前玩家選擇的英雄，
    /// 並負責從 PlayerPrefs 載入與儲存英雄選擇。
    /// </summary>
    public class HeroSelectionManager : MonoBehaviour
    {
        #region Singleton

        public static HeroSelectionManager Instance
        {
            get;
            private set;
        }

        #endregion

        #region Inspector 設定

        [Header("英雄資料庫")]
        [SerializeField] private HeroDatabase heroDatabase;

        #endregion

        #region 屬性

        public HeroData CurrentHeroData
        {
            get;
            private set;
        }

        #endregion

        #region 常數

        private const string SelectedHeroKey =
            "SelectedHero";

        #endregion

        #region 事件

        public event Action OnSelectedHeroChanged;

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

            LoadSelectedHero();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 英雄選擇

        /// <summary>
        /// 載入玩家目前選擇的英雄。
        /// 如果沒有有效存檔，則使用資料庫中的第一位英雄。
        /// </summary>
        public void LoadSelectedHero()
        {
            if (!HasValidDatabase())
            {
                CurrentHeroData = null;

                Debug.LogError(
                    "HeroSelectionManager 沒有設定有效的 HeroDatabase",
                    this
                );

                return;
            }

            string selectedHeroName =
                PlayerPrefs.GetString(
                    SelectedHeroKey,
                    string.Empty
                );

            CurrentHeroData =
                FindHeroByAssetName(
                    selectedHeroName
                );

            // 沒有存檔或原英雄不存在時，
            // 使用資料庫中的第一位英雄。
            if (CurrentHeroData == null)
            {
                CurrentHeroData =
                    heroDatabase.Heroes[0];

                SaveSelectedHero(
                    CurrentHeroData
                );
            }

#if UNITY_EDITOR
            Debug.Log(
                $"目前選擇英雄：" +
                $"{CurrentHeroData.heroName}，" +
                $"Prefab：" +
                $"{CurrentHeroData.prefab?.name}",
                this
            );
#endif
        }

        /// <summary>
        /// 設定玩家目前選擇的英雄。
        /// </summary>
        public void SelectHero(
            HeroData heroData)
        {
            if (heroData == null)
            {
                Debug.LogError(
                    "傳入的 HeroData 是空的",
                    this
                );

                return;
            }

            if (CurrentHeroData == heroData)
            {
                return;
            }

            CurrentHeroData = heroData;

            SaveSelectedHero(
                heroData
            );

            OnSelectedHeroChanged?.Invoke();

#if UNITY_EDITOR
            Debug.Log(
                $"已選擇英雄：" +
                $"{heroData.heroName} | " +
                $"圖片：{heroData.icon?.name} | " +
                $"Prefab：{heroData.prefab?.name}",
                this
            );
#endif
        }

        #endregion

        #region 資料查詢

        private bool HasValidDatabase()
        {
            return heroDatabase != null &&
                   heroDatabase.Heroes != null &&
                   heroDatabase.Heroes.Length > 0;
        }

        private HeroData FindHeroByAssetName(
            string heroAssetName)
        {
            if (string.IsNullOrEmpty(heroAssetName))
                return null;

            foreach (HeroData hero in heroDatabase.Heroes)
            {
                if (hero == null)
                    continue;

                if (hero.name == heroAssetName)
                {
                    return hero;
                }
            }

            return null;
        }

        #endregion

        #region 存檔

        private static void SaveSelectedHero(
            HeroData heroData)
        {
            if (heroData == null)
                return;

            PlayerPrefs.SetString(
                SelectedHeroKey,
                heroData.name
            );

            PlayerPrefs.Save();
        }

        #endregion
    }
}