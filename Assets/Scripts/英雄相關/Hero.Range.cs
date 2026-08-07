using UnityEngine;
namespace CHANG
{
    public partial class Hero
    {
        #region 攻擊範圍顯示

        public void ShowRangeCircle()
        {
            Debug.Log(
                $"ShowRangeCircle 被呼叫：{name}",
                this
            );

            if (rangeCircle == null)
            {
                Debug.LogWarning(
                    $"{name} 沒有設定 RangeCircle",
                    this
                );

                return;
            }

            rangeCircle.gameObject.SetActive(true);

            rangeCircle.DrawCircle(
                CurrentStats.range
            );

            rangeCircle.SetColor(
                new Color(
                    0f,
                    1f,
                    0f,
                    0.8f
                )
            );
        }

        public void HideRangeCircle()
        {
            if (rangeCircle == null)
                return;

            rangeCircle.gameObject.SetActive(
                false
            );
        }

        private void RefreshRangeCircle()
        {
            if (rangeCircle == null)
                return;

            rangeCircle.DrawCircle(
                CurrentStats.range
            );
        }

        #endregion
    }
}