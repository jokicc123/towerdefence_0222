using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 怪物狀態圖示 UI。
    /// 顯示燃燒、中毒等狀態，
    /// 並讓 UI 永遠面向攝影機。
    /// </summary>
    public class EnemyStatusUI : MonoBehaviour
    {
        #region Inspector 設定

        [SerializeField]
        private GameObject burnIcon;

        [SerializeField]
        private GameObject poisonIcon;

        #endregion

        #region 執行期間資料

        private Transform cameraTransform;

        #endregion

        #region Unity 生命週期

        private void Start()
        {
            if (Camera.main != null)
            {
                cameraTransform =
                    Camera.main.transform;
            }

            SetBurn(false);
            SetPoison(false);
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
                return;

            transform.rotation =
                Quaternion.LookRotation(
                    transform.position -
                    cameraTransform.position
                );
        }

        #endregion

        #region 狀態圖示

        public void SetBurn(bool active)
        {
            if (burnIcon != null)
            {
                burnIcon.SetActive(active);
            }
        }

        public void SetPoison(bool active)
        {
            if (poisonIcon != null)
            {
                poisonIcon.SetActive(active);
            }
        }

        #endregion
    }
}