using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 英雄資料庫。
    /// 集中管理所有可使用的 HeroData。
    /// </summary>
    [CreateAssetMenu(
        fileName = "HeroDatabase",
        menuName = "CHANG/Hero Database"
    )]
    public class HeroDatabase : ScriptableObject
    {
        #region Inspector 設定

        [Header("英雄資料")]
        [SerializeField]
        private HeroData[] heroes;

        #endregion

        #region 屬性

        public HeroData[] Heroes => heroes;

        #endregion
    }
}