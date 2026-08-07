using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 防禦塔模型參考點。
    /// 提供 Tower 快速取得模型上的重要節點。
    /// </summary>
    public class TowerModelRef : MonoBehaviour
    {
        #region 模型節點

        [Header("子彈發射點")]
        [SerializeField]
        private Transform firePoint;

        [Header("塔頭旋轉節點")]
        [SerializeField]
        private Transform head;

        public Transform FirePoint => firePoint;

        public Transform Head => head;

        #endregion
    }
}