using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    // ============================================================
    // 資料結構區
    // ============================================================

    [System.Serializable]
    public struct HeroLevelStats
    {
        public int xpToNextLevel;   // 升到下一級需要的累積經驗值
        public float damage;
        public float attackSpeed;
        public float range;
        public string unlockDescription; // 這一級解鎖了什麼（給UI顯示）
    }

    [System.Serializable]
    public struct ActiveSkillData
    {
        public string skillName;
        public string description;
        public float cooldown;
        public float radius;
        public float value; // 傷害量 / 治療量 / 增益數值，依技能類型使用
        public Sprite icon; // 給UI按鈕用
        public GameObject vfxPrefab; // 技能特效（帶ParticleSystem的prefab）
    }

    [System.Serializable]
    public struct PassiveSkillData
    {
        public string skillName;
        public string description;
        public float auraRadius;      // 光環範圍（影響周圍塔）
        public float buffMultiplier;  // 例如 1.15 = 傷害+15%
        public PassiveBuffType buffType;

        [Tooltip("只有這些屬性的塔會被光環影響，例如只選 Burn 和 Poison。留空 = 影響全部塔。")]
        public TowerEffectType[] targetEffectTypes;
    }

    public enum PassiveBuffType { AttackSpeed, Damage, Range }

    // ============================================================
    // ScriptableObject 本體
    // ============================================================

    [CreateAssetMenu(fileName = "NewHero", menuName = "CHANG/Hero Data")]
    public class HeroData : ScriptableObject
    {
        public string heroName;
        public Sprite icon;
        public GameObject prefab;

        [Header("普通攻擊")]
        public GameObject bulletPrefab;

        [Header("購買花費（遊戲內金幣）")]
        public int purchaseCost;

        [Header("每級數值（index 0 = 1級）")]
        public HeroLevelStats[] levelStats;

        [Header("主動技能 × 2")]
        public ActiveSkillData skill1;
        public ActiveSkillData skill2;

        [Header("被動技能 × 1（光環）")]
        public PassiveSkillData passive;
    }
}