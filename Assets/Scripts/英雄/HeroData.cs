using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    // ============================================================
    // 英雄每級普通攻擊數值
    // ============================================================

    [System.Serializable]
    public struct HeroLevelStats
    {
        [Tooltip("升到下一級所需經驗值，最高等級填 0")]
        public int xpToNextLevel;

        public float damage;
        public float attackSpeed;
        public float range;

        [TextArea]
        public string unlockDescription;
    }

    // ============================================================
    // 技能每級數值
    // ============================================================

    [System.Serializable]
    public struct SkillLevelStats
    {
        [Tooltip("技能效果數值，例如定身秒數或樹人傷害")]
        public float value;

        [Tooltip("技能範圍或召喚物搜尋範圍")]
        public float radius;

        [Tooltip("技能冷卻時間")]
        public float cooldown;

        [Tooltip("技能或召喚物持續時間")]
        public float duration;

        [Tooltip("召喚物每秒攻擊次數")]
        public float attackSpeed;
    }

    // ============================================================
    // 主動技能資料
    // ============================================================

    [System.Serializable]
    public struct ActiveSkillData
    {
        [Header("基本資料")]
        public string skillName;

        [TextArea]
        public string description;

        [Tooltip("英雄達到此等級後才可使用")]
        public int unlockLevel;

        [Header("圖片與特效")]
        public Sprite icon;
        public GameObject vfxPrefab;

        [Header("召喚技能設定")]
        public GameObject summonPrefab;

        [Header("每級技能數值")]
        public SkillLevelStats[] levelStats;
    }

    // ============================================================
    // 被動技能資料
    // ============================================================

    [System.Serializable]
    public struct PassiveSkillData
    {
        public string skillName;

        [TextArea]
        public string description;

        public float auraRadius;

        [Tooltip("例如 1.15 代表增加 15%")]
        public float buffMultiplier;

        public PassiveBuffType buffType;

        [Tooltip(
            "只有這些屬性的塔會受到光環影響。" +
            "例如選 Burn 和 Poison；留空代表全部塔。"
        )]
        public TowerEffectType[] targetEffectTypes;
    }

    public enum PassiveBuffType
    {
        AttackSpeed,
        Damage,
        Range
    }

    // ============================================================
    // HeroData ScriptableObject
    // ============================================================

    [CreateAssetMenu(
        fileName = "NewHero",
        menuName = "CHANG/Hero Data"
    )]
    public class HeroData : ScriptableObject
    {
        [Header("英雄基本資料")]
        public string heroName;
        public Sprite icon;
        public GameObject prefab;

        [Header("普通攻擊")]
        public GameObject bulletPrefab;
        public AudioClip attackSFX;

        [Header("購買花費")]
        public int purchaseCost;

        [Header("英雄每級數值（Element 0 = Lv.1）")]
        public HeroLevelStats[] levelStats;

        [Header("主動技能")]
        public ActiveSkillData skill1;
        public ActiveSkillData skill2;

        [Header("被動技能")]
        public PassiveSkillData passive;
    }
}