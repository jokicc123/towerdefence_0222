using UnityEngine;

namespace CHANG
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        public Hero activeHero;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // 玩家點擊放置英雄時呼叫這個，取代原本的「賽前解鎖」流程
        public bool TryPurchaseHero(
          HeroData heroData,
           Vector3 position)
        {
            if (heroData == null ||
                heroData.prefab == null)
            {
                Debug.LogError("HeroData 或英雄 Prefab 沒有設定");
                return false;
            }

            if (activeHero != null)
            {
                Debug.Log("場上已經有英雄");
                return false;
            }

            if (!GameManager.Instance.SpendGold(
                heroData.purchaseCost))
            {
                Debug.Log("金幣不足");
                return false;
            }
            Debug.Log(
                $"準備生成英雄：" +
                $"HeroData={heroData.name}，" +
                $"HeroName={heroData.heroName}，" +
                $"Prefab={heroData.prefab.name}"
);
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
                    $"{heroData.prefab.name} 沒有 Hero 腳本"
                );

                Destroy(heroObject);
                return false;
            }

            hero.data = heroData;
            activeHero = hero;

            return true;
        }

        public void RegisterHero(Hero hero) => activeHero = hero;
        public void OnEnemyKilled(int xpValue) => activeHero?.GainXP(xpValue);
        public void ResetForNewGame() => activeHero = null;
    }
}