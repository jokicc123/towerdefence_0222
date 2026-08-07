using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理場上唯一英雄的生成、註冊與經驗值取得。
    /// </summary>
    public class HeroManager : MonoBehaviour
    {
        #region Singleton

        public static HeroManager Instance { get; private set; }

        #endregion

        #region 執行期間資料

        [SerializeField]
        private Hero activeHero;

        public Hero ActiveHero => activeHero;

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
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region 英雄生成

        /// <summary>
        /// 消耗關卡金幣並生成英雄。
        /// 場上同一時間只能存在一名英雄。
        /// </summary>
        public bool TryPurchaseHero(
            HeroData heroData,
            Vector3 position)
        {
            if (heroData == null ||
                heroData.prefab == null)
            {
                Debug.LogError(
                    "HeroData 或英雄 Prefab 沒有設定",
                    this
                );

                return false;
            }

            if (activeHero != null)
            {
#if UNITY_EDITOR
                Debug.Log(
                    "場上已經有英雄",
                    this
                );
#endif

                return false;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError(
                    "找不到 GameManager",
                    this
                );

                return false;
            }

            if (!GameManager.Instance.SpendGold(
                    heroData.purchaseCost))
            {
#if UNITY_EDITOR
                Debug.Log(
                    "金幣不足",
                    this
                );
#endif

                return false;
            }

#if UNITY_EDITOR
            Debug.Log(
                $"準備生成英雄：" +
                $"HeroData={heroData.name}，" +
                $"HeroName={heroData.heroName}，" +
                $"Prefab={heroData.prefab.name}",
                this
            );
#endif

            GameObject heroObject = Instantiate(
                heroData.prefab,
                position,
                Quaternion.identity
            );

            Hero hero =
                heroObject.GetComponent<Hero>();

            if (hero == null)
            {
                Debug.LogError(
                    $"{heroData.prefab.name} 沒有 Hero 腳本",
                    heroObject
                );

                Destroy(heroObject);

                // 生成失敗，把金幣退回
                GameManager.Instance.AddGold(
                    heroData.purchaseCost
                );

                return false;
            }

            hero.data = heroData;
            activeHero = hero;

            return true;
        }

        #endregion

        #region 英雄註冊

        /// <summary>
        /// 將英雄登記為目前場上的主英雄。
        /// </summary>
        public void RegisterHero(Hero hero)
        {
            if (hero == null)
                return;

            activeHero = hero;
        }

        /// <summary>
        /// 清除目前場上的英雄資料。
        /// </summary>
        public void ResetForNewGame()
        {
            activeHero = null;
        }

        #endregion

        #region 經驗值

        /// <summary>
        /// 敵人死亡時，將經驗值給目前場上的英雄。
        /// </summary>
        public void OnEnemyKilled(int xpValue)
        {
            if (xpValue <= 0)
                return;

            activeHero?.GainXP(xpValue);
        }

        #endregion
    }
}