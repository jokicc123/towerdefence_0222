using UnityEngine;

namespace CHANG
{
    public class HeroSelectionManager : MonoBehaviour
    {
        public static HeroSelectionManager Instance
        {
            get;
            private set;
        }

        [Header("英雄資料庫")]
        [SerializeField] private HeroDatabase heroDatabase;

        public HeroData CurrentHeroData
        {
            get;
            private set;
        }

        private const string SelectedHeroKey =
            "SelectedHero";

        public event System.Action OnSelectedHeroChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadSelectedHero();
        }

        public void LoadSelectedHero()
        {
            if (heroDatabase == null ||
                heroDatabase.heroes == null ||
                heroDatabase.heroes.Length == 0)
            {
                Debug.LogError(
                    "HeroSelectionManager 沒有設定 HeroDatabase",
                    this
                );

                CurrentHeroData = null;
                return;
            }

            string selectedHeroName =
                PlayerPrefs.GetString(
                    SelectedHeroKey,
                    string.Empty
                );

            CurrentHeroData = null;

            foreach (HeroData hero in heroDatabase.heroes)
            {
                if (hero == null)
                    continue;

                if (hero.name == selectedHeroName)
                {
                    CurrentHeroData = hero;
                    break;
                }
            }

            // 沒有存檔或找不到英雄時，使用第一位英雄
            if (CurrentHeroData == null)
            {
                CurrentHeroData =
                    heroDatabase.heroes[0];

                PlayerPrefs.SetString(
                    SelectedHeroKey,
                    CurrentHeroData.name
                );

                PlayerPrefs.Save();
            }

            Debug.Log(
                $"目前選擇英雄：" +
                $"{CurrentHeroData.heroName}，" +
                $"Prefab：" +
                $"{CurrentHeroData.prefab?.name}",
                this
            );
        }

        public void SelectHero(HeroData heroData)
        {
            if (heroData == null)
            {
                Debug.LogError("傳入的 HeroData 是空的", this);
                return;
            }

            CurrentHeroData = heroData;

            PlayerPrefs.SetString(
                SelectedHeroKey,
                heroData.name
            );

            PlayerPrefs.Save();

            // 通知建造按鈕更新
            OnSelectedHeroChanged?.Invoke();

            Debug.Log(
                $"已選擇英雄：{heroData.heroName} | " +
                $"圖片：{heroData.icon?.name} | " +
                $"Prefab：{heroData.prefab?.name}",
                this
            );
        }
    }
}