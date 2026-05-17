using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    public class EnemyStatusUI : MonoBehaviour
    {
        [SerializeField] private GameObject burnIcon;
        [SerializeField] private GameObject poisonIcon;

        private Transform cam;

        private void Start()
        {
            cam = Camera.main.transform;
            burnIcon.SetActive(false);
            poisonIcon.SetActive(false);
        }

        private void LateUpdate()
        {
            // 永遠面向鏡頭
            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.position
            );
        }

        public void SetBurn(bool active)
        {
            burnIcon.SetActive(active);
        }

        public void SetPoison(bool active)
        {
            poisonIcon.SetActive(active);
        }
    }
}