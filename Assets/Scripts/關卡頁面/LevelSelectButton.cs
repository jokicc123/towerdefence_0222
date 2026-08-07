using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CHANG
{
    [RequireComponent(typeof(Button))]
    public class LevelSelectButton : MonoBehaviour
    {
        #region Inspector 設定

        [Header("這顆按鈕代表哪一關")]
        [SerializeField] private LevelData levelData;

        [Header("UI 元件連結")]
        [SerializeField] private TextMeshProUGUI levelNameText;

        #endregion

        #region 執行期間資料

        private Button button;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            button = GetComponent<Button>();

            Debug.Assert(
                button != null,
                "LevelSelectButton 缺少 Button"
            );
        }

        private void Start()
        {
            InitializeUI();
            RegisterButton();
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(
                    LoadSelectedLevel
                );
            }
        }

        #endregion

        #region 初始化

        private void InitializeUI()
        {
            if (levelData == null)
            {
                Debug.LogWarning(
                    $"{gameObject.name} 沒有設定 LevelData！"
                );

                return;
            }

            if (levelNameText != null)
            {
                levelNameText.text =
                    levelData.LevelName;
            }
        }

        private void RegisterButton()
        {
            if (LoadingManager.Instance == null)
            {
                Debug.LogError(
                    "LoadingManager 尚未初始化，請確認場景載入順序"
                );

                return;
            }

            button.onClick.AddListener(
                LoadSelectedLevel
            );
        }

        #endregion

        #region 按鈕事件

        private void LoadSelectedLevel()
        {
            Debug.Log($"點擊了關卡：{levelData?.LevelName}");

            LevelSession.SelectLevel(levelData);

            LoadingManager.Instance.StartLoad(
                levelData.SceneName
            );
        }

        #endregion
    }
}