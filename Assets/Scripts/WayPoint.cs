using UnityEngine;

namespace CHANG
{
    public class Waypoints : MonoBehaviour
    {
       public static Transform[] Points;
        void Awake()
        {
            InitializePoints();
        }

        public void InitializePoints()
        {
            // 如果已經抓過了就跳過
            if (Points != null && Points.Length > 0) return;

            int childCount = transform.childCount;
            Points = new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                Points[i] = transform.GetChild(i);
            }

            Debug.Log($"路徑點初始化成功，抓到 {Points.Length} 個點");
        }
    }
}