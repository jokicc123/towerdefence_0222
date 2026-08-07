using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// 管理關卡內唯一的英雄建造按鈕。
    /// 根據目前選擇的英雄更新圖片、價格與按鈕狀態。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class HeroBuildButton : MonoBehaviour
    {
        #region Inspector 設定

        [Header("UI 元件")]
        [SerializeField] private Image heroIcon;
        [SerializeField] private TMP_Text priceText;

        #endregion

        #region 執行期間資料

        private Button button;
        private HeroData selectedHero;
        private int buildCost;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private IEnumerator Start()
        {
            // 等待一幀，確保其他 Manager 已完成 Awake。
            yield return null;

            RegisterEvents();
            LoadSelectedHero();
            RefreshButtonState();
        }

        private void OnEnable()
        {
            // Start 尚未執行完成時也可以安全呼叫。
            if (HeroSelectionManager.Instance != null)
            {
                LoadSelectedHero();
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region 事件註冊

        private void RegisterEvents()
        {
            if (button != null)
            {
                button.onClick.AddListener(
                    OnClickBuildHero
                );
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged +=
                    CheckMoney;
            }

            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance
                    .OnSelectedHeroChanged +=
                    LoadSelectedHero;
            }
            else
            {
                Debug.LogError(
                    "找不到 HeroSelectionManager",
                    this
                );
            }
        }

        private void UnregisterEvents()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(
                    OnClickBuildHero
                );
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -=
                    CheckMoney;
            }

            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance
                    .OnSelectedHeroChanged -=
                    LoadSelectedHero;
            }
        }

        #endregion

        #region 英雄資料更新

        /// <summary>
        /// 讀取目前選擇的英雄，
        /// 並更新建造按鈕圖片與價格。
        /// </summary>
        public void LoadSelectedHero()
        {
            if (HeroSelectionManager.Instance == null)
            {
                selectedHero = null;
                DisableButton();

                Debug.LogError(
                    "場景中找不到 HeroSelectionManager",
                    this
                );

                return;
            }

            selectedHero =
                HeroSelectionManager.Instance.CurrentHeroData;

            if (selectedHero == null)
            {
                DisableButton();

                Debug.LogError(
                    "目前選擇的英雄資料是空的",
                    this
                );

                return;
            }

            buildCost =
                selectedHero.purchaseCost;

            RefreshHeroIcon();
            RefreshPrice();
            RefreshButtonState();

#if UNITY_EDITOR
            Debug.Log(
                $"建造按鈕已刷新：" +
                $"英雄={selectedHero.heroName} | " +
                $"圖片={selectedHero.icon?.name}",
                this
            );
#endif
        }

        private void RefreshHeroIcon()
        {
            if (heroIcon == null)
                return;

            heroIcon.sprite =
                selectedHero.icon;

            heroIcon.enabled =
                selectedHero.icon != null;

            heroIcon.color =
                Color.white;

            heroIcon.preserveAspect =
                true;
        }

        private void RefreshPrice()
        {
            if (priceText != null)
            {
                priceText.text =
                    $"${buildCost}";
            }
        }

        #endregion

        #region 建造按鈕事件

        /// <summary>
        /// 開始放置目前選擇的英雄。
        /// </summary>
        private void OnClickBuildHero()
        {
            if (selectedHero == null)
                return;

            if (BuildManager.Instance == null)
            {
                Debug.LogError(
                    "場景中找不到 BuildManager",
                    this
                );

                return;
            }

            if (HeroManager.Instance != null &&
                HeroManager.Instance.ActiveHero != null)
            {
#if UNITY_EDITOR
                Debug.Log(
                    "場上已經有英雄，不能重複放置",
                    this
                );
#endif

                RefreshButtonState();
                return;
            }

            BuildManager.Instance.SelectHero(
                selectedHero
            );
        }

        #endregion

        #region 按鈕狀態更新

        private void CheckMoney(int currentGold)
        {
            RefreshButtonState(currentGold);
        }

        private void RefreshButtonState()
        {
            int currentGold =
                GameManager.Instance != null
                    ? GameManager.Instance.Gold
                    : 0;

            RefreshButtonState(currentGold);
        }

        private void RefreshButtonState(
            int currentGold)
        {
            if (button == null)
                return;

            bool gameAllowsBuild =
                GameManager.Instance != null &&
                GameManager.Instance.CanBuildTower();

            bool hasEnoughGold =
                selectedHero != null &&
                currentGold >= buildCost;

            bool heroAlreadyExists =
                HeroManager.Instance != null &&
                HeroManager.Instance.ActiveHero != null;

            button.interactable =
                gameAllowsBuild &&
                hasEnoughGold &&
                !heroAlreadyExists;
        }

        private void DisableButton()
        {
            if (button != null)
            {
                button.interactable = false;
            }

            if (priceText != null)
            {
                priceText.text = "--";
            }

            if (heroIcon != null)
            {
                heroIcon.enabled = false;
            }
        }

        #endregion
    }
}