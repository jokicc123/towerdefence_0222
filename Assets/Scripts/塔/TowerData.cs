using UnityEngine;

namespace CHANG
{
    #region 防禦塔類型

    public enum TowerAttackType
    {
        Bullet
    }

    public enum TowerEffectType
    {
        None,
        Burn,
        Poison
    }

    #endregion


    /// <summary>
    /// 防禦塔資料。
    /// 包含基本資訊、建造設定、每級數值、模型、圖示與音效。
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewTowerData",
        menuName = "Game/TowerData"
    )]
    public class TowerData : ScriptableObject
    {
        #region 建造設定

        [Header("建造設定")]
        [Tooltip("防禦塔在地圖上的佔地大小")]
        public Vector3 buildFootprint =
            new Vector3(1f, 1f, 1f);

        #endregion

        #region 攻擊設定

        [Header("攻擊設定")]
        public TowerAttackType attackType;

        #endregion

        #region 文字說明

        [Header("文字說明")]
        public string towerName;

        [TextArea(3, 5)]
        public string description;

        #endregion

        #region 等級資料

        [Header("等級資料")]
        public TowerLevel[] levels;

        #endregion

        #region 模型設定

        [Header("完整 Prefab（每級建造用）")]
        public GameObject[] levelPrefabs;

        [Header("視覺模型（每級換外觀用）")]
        public GameObject[] levelModelPrefabs;

        #endregion

        #region UI 設定

        [Header("圖示")]
        public Sprite icon;

        #endregion

        #region 音效設定

        [Header("音效")]
        public AudioClip attackSFX;

        #endregion
    }


    #region 防禦塔每級資料

    /// <summary>
    /// 防禦塔單一等級的能力數值。
    /// </summary>
    [System.Serializable]
    public class TowerLevel
    {
        #region 基礎數值

        [Header("基礎數值")]

        [Tooltip("攻擊範圍")]
        public float attackRange;

        [Tooltip("攻擊傷害")]
        public float damage;

        [Tooltip("每秒攻擊次數")]
        public float attackSpeed;

        [Tooltip("建造或升級花費")]
        public int cost;

        #endregion

        #region 子彈設定

        [Header("子彈設定")]

        [Tooltip("此等級使用的子彈 Prefab")]
        public GameObject bulletPrefab;

        #endregion

        #region 狀態效果

        [Header("狀態效果")]

        public TowerEffectType effectType;

        [Tooltip("狀態效果持續時間")]
        public float effectDuration;

        [Tooltip("持續傷害 DPS")]
        public float effectDamagePerSecond;

        [Range(0f, 1f)]
        [Tooltip("減速百分比，例如 0.5 = 減速 50%")]
        public float slowPercent;

        #endregion

        #region 範圍傷害

        [Header("範圍傷害")]

        [Tooltip("爆炸半徑，0 代表單體攻擊")]
        public float blastRadius;

        [Range(0f, 1f)]
        [Tooltip("爆炸邊緣最低傷害倍率")]
        public float minDamageRatio = 0.4f;

        #endregion
    }

    #endregion
}