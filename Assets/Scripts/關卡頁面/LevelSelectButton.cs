using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CHANG
{
    [RequireComponent(typeof(Button))]
    public class LevelSelectButton : MonoBehaviour
    {
        [Header("這顆按鈕代表哪一關")]
        [SerializeField] private LevelData levelData;

        [Header("UI 元件連結")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private Image thumbnailImage;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            if (levelData == null)
            {
                Debug.LogWarning($"{gameObject.name} 沒有設定 LevelData！");
                return;
            }

            if (levelNameText != null)
                levelNameText.text = levelData.levelName;

            if (thumbnailImage != null && levelData.thumbnail != null)
                thumbnailImage.sprite = levelData.thumbnail;

            button.onClick.AddListener(OnClickSelectLevel);
        }

        private void OnClickSelectLevel()
        {
            Debug.Log($"①點擊了關卡: {levelData?.levelName}");
            LevelSession.SelectedLevel = levelData;
            Debug.Log($"②LoadingManager.Instance = {LoadingManager.Instance}");
            LoadingManager.Instance.StartLoad(levelData.sceneName);
            Debug.Log("③StartLoad已呼叫");
        }
    }
}