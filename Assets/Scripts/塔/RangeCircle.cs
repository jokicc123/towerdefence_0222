using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 使用 LineRenderer 繪製塔的攻擊範圍。
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RangeCircle : MonoBehaviour
    {
        #region Inspector 設定

        [Header("圓形細緻度")]
        [SerializeField, Min(3)]
        private int segments = 50;

        [Header("線條寬度")]
        [SerializeField]
        private float lineWidth = 0.1f;

        [Header("高度偏移")]
        [SerializeField]
        private float yOffset = 0.05f;

        #endregion

        #region 執行期間資料

        private LineRenderer line;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            line.loop = true;
            line.useWorldSpace = false;
            line.widthMultiplier = lineWidth;
        }

        #endregion

        #region 繪製範圍

        public void DrawCircle(float radius)
        {
            if (line == null)
                return;

            radius = Mathf.Max(0f, radius);

            line.positionCount = segments;

            float angleStep =
                360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle =
                    angleStep * i * Mathf.Deg2Rad;

                Vector3 point =
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        yOffset,
                        Mathf.Sin(angle) * radius
                    );

                line.SetPosition(
                    i,
                    point
                );
            }
        }

        #endregion

        #region 外觀設定

        public void SetColor(Color color)
        {
            if (line == null)
                return;

            line.startColor = color;
            line.endColor = color;
        }

        #endregion
    }
}