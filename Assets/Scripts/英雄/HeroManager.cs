using UnityEngine;

namespace CHANG
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        private Hero activeHero;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // 玩家點擊放置英雄時呼叫這個，取代原本的「賽前解鎖」流程
        public bool TryPurchaseHero(HeroData data, Vector3 position)
        {
            if (activeHero != null)
            {
                Debug.LogWarning("場上已有英雄，同時只能放一位");
                return false;
            }

            if (GameManager.Instance.gold < data.purchaseCost)
            {
                Debug.LogWarning("金幣不足，無法購買英雄");
                return false;
            }

            GameManager.Instance.SpendGold(data.purchaseCost);
            var heroObj = Instantiate(data.prefab, position, Quaternion.identity);
            var hero = heroObj.GetComponent<Hero>();
            hero.data = data;
            return true;
        }

        public void RegisterHero(Hero hero) => activeHero = hero;
        public void OnEnemyKilled(int xpValue) => activeHero?.GainXP(xpValue);
        public void ResetForNewGame() => activeHero = null;
    }
}