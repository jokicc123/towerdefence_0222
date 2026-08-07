using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理預設路徑點。
    /// 若 Enemy 沒有指定專屬路線，
    /// 就會使用這組 Waypoints。
    /// </summary>
    public class Waypoints : MonoBehaviour
    {
        #region 公開屬性

        public static Transform[] Points
        {
            get;
            private set;
        }

        public static readonly Vector3[] RotationPoints =
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 90, 0)
        };

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            InitializePoints();
        }

        #endregion

        #region 初始化

        public void InitializePoints()
        {
            // 已初始化就不重複抓
            if (Points != null &&
                Points.Length > 0)
            {
                return;
            }

            int childCount =
                transform.childCount;

            Points =
                new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Points[i] =
                    transform.GetChild(i);
            }

#if UNITY_EDITOR
            Debug.Log(
                $"路徑點初始化成功，共 {Points.Length} 個。",
                this
            );
#endif
        }

        #endregion
    }
}