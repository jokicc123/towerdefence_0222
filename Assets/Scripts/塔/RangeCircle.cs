using UnityEngine;
namespace CHANG
{
    //強制這個腳本所在的物件一定要有某個元件
    [RequireComponent(typeof(LineRenderer))]
    public class RangeCircle : MonoBehaviour
    {
        [Header("圓形細緻度")]
        [SerializeField]
        private int segments = 50;
        private LineRenderer line;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            line.loop = true; // 讓線條形成閉合的圓
            line.useWorldSpace = false; // 使用物件的本地座標系統
            line.widthMultiplier = 0.1f; // 線條寬度
        }
        public void DrawCircle(float radius)
        {
            line.positionCount = segments; // 設定線條的點數
            float anglestep = 360f / segments; // 每段的角度

            for (int i = 0; i < segments; i++)
            {
                float angle = anglestep * i;

                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius; // 計算x座標
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius; // 計算z座標

                line.SetPosition(i, new Vector3(x, 0.05f, z)); // 設定線條的點位置
            }

        }
        public void SetColor(Color color)
        {
            
            line.startColor = color;
            line.endColor = color;
        }
    }
}
