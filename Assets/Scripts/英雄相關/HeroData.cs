using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    #region 英雄類型定義

    public enum HeroAttackType
    {
        Ranged, // 遠程子彈
        Melee   // 近戰揮砍
    }

    public enum HeroSkillType
    {
        None,

        // 自然英雄
        AreaStun,
        SummonCreature,

        // 火焰劍士
        FireWall,
        SunOfNoon
    }

    public enum PassiveBuffType
    {
        AttackSpeed,
        Damage,
        Range
    }

    #endregion

    #region 英雄等級資料

    [System.Serializable]
    public struct HeroLevelStats
    {
        [Tooltip("升到下一級所需經驗值，最高等級填 0")]
        public int xpToNextLevel;

        [Header("普通攻擊數值")]
        public float damage;

        [Tooltip("每秒攻擊次數")]
        public float attackSpeed;

        public float range;

        [TextArea]
        public string unlockDescription;
    }

    #endregion

    #region 技能等級資料

    [System.Serializable]
    public struct SkillLevelStats
    {
        [Header("主要數值")]
        [Tooltip("技能主要數值，例如傷害或禁錮秒數")]
        public float value;

        [Header("範圍")]
        [Tooltip("圓形技能半徑")]
        public float radius;

        [Tooltip("長方形技能長度，例如火焰之牆")]
        public float length;

        [Tooltip("長方形技能寬度，例如火焰之牆")]
        public float width;

        [Header("時間")]
        [Tooltip("技能冷卻時間")]
        public float cooldown;

        [Tooltip("技能持續時間")]
        public float duration;

        [Header("持續傷害")]
        [Tooltip("每秒傷害，例如燃燒 DPS 或太陽 DPS")]
        public float damagePerSecond;

        [Header("其他")]
        [Tooltip("召喚物攻擊速度等額外數值")]
        public float attackSpeed;

        [Tooltip("額外倍率，例如正午太陽的燃燒倍率")]
        public float multiplier;
    }

    #endregion

    #region 主動技能資料

    [System.Serializable]
    public struct ActiveSkillData
    {
        [Header("基本資料")]
        public string skillName;

        [TextArea(2, 4)]
        public string description;

        [Header("技能設定")]
        public HeroSkillType skillType;

        [Tooltip("英雄達到此等級後解鎖技能")]
        public int unlockLevel;

        [Header("UI 與特效")]
        public Sprite icon;
        public GameObject vfxPrefab;

        [Header("召喚物")]
        public GameObject summonPrefab;

        [Header("每級數值")]
        public SkillLevelStats[] levelStats;
    }

    #endregion

    #region 被動技能資料

    [System.Serializable]
    public struct PassiveSkillData
    {
        [Header("基本資料")]
        public string skillName;

        public Sprite icon;

        [TextArea]
        public string description;

        [Header("光環設定")]
        public float auraRadius;

        [Tooltip("例如 1.15 代表增加 15%")]
        public float buffMultiplier;

        public PassiveBuffType buffType;

        [Tooltip(
            "只有指定屬性的塔會受到光環影響。" +
            "例如選 Burn 和 Poison；陣列留空代表全部塔。"
        )]
        public TowerEffectType[] targetEffectTypes;
    }

    #endregion

    /// <summary>
    /// 英雄靜態資料。
    /// 包含基本資訊、普通攻擊、等級、主動技能與被動技能設定。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewHero",
        menuName = "CHANG/Hero Data"
    )]
    public class HeroData : ScriptableObject
    {
        #region 英雄基本資料

        [Header("英雄基本資料")]
        public string heroName;

        [Tooltip("英雄建造按鈕與英雄 UI 使用的圖片")]
        public Sprite icon;

        [Tooltip("放置到場上的英雄 Prefab")]
        public GameObject prefab;

        [TextArea(2, 4)]
        public string description;

        [Header("購買花費")]
        public int purchaseCost;

        #endregion

        #region 普通攻擊設定

        [Header("普通攻擊")]
        public HeroAttackType attackType =
            HeroAttackType.Ranged;

        public GameObject bulletPrefab;

        [Header("近戰攻擊")]
        [Tooltip("近戰命中範圍半徑")]
        public float meleeHitRadius = 1.5f;

        [Tooltip("近戰判定中心向前偏移")]
        public float meleeHitOffset = 1.2f;

        [Header("普通攻擊燃燒")]
        public bool normalAttackBurn;

        [Tooltip("普通攻擊燃燒時間")]
        public float burnDuration = 3f;

        [Tooltip("普通攻擊每秒燃燒傷害")]
        public float burnDamagePerSecond = 5f;

        [Header("普通攻擊特效")]
        public GameObject normalAttackVFX;

        [Header("攻擊音效")]
        public AudioClip attackSFX;

        #endregion

        #region 英雄等級設定

        [Header("英雄每級數值（Element 0 = Lv.1）")]
        public HeroLevelStats[] levelStats;

        #endregion

        #region 主動技能設定

        [Header("主動技能 1")]
        public ActiveSkillData skill1;

        [Header("主動技能 2")]
        public ActiveSkillData skill2;

        #endregion

        #region 被動技能設定

        [Header("被動技能")]
        public PassiveSkillData passive;

        #endregion
    }
}