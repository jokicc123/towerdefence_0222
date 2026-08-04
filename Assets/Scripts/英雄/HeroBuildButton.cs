using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    [RequireComponent(typeof(Button))]
    public class HeroBuildButton : MonoBehaviour
    {
        [Header("UI 元件")]
        [SerializeField] private Image heroIcon;
        [SerializeField] private TMP_Text priceText;

        private Button button;
        private HeroData selectedHero;
        private int buildCost;

        private void Awake()
        {
            button = GetComponent<Button>();
        }
        private IEnumerator Start()
        {
            yield return null;

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance.OnSelectedHeroChanged +=
                    LoadSelectedHero;
            }
            else
            {
                Debug.LogError(
                    "找不到 HeroSelectionManager",
                    this
                );
            }

            LoadSelectedHero();

            if (button != null)
            {
                button.onClick.AddListener(OnClickBuildHero);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += CheckMoney;
                CheckMoney(GameManager.Instance.gold);
            }
        }

        /// <summary>
        /// 讀取商店目前選擇的英雄，
        /// 並更新建造按鈕的圖片與價格。
        /// </summary>
        public void LoadSelectedHero()
        {
            if (HeroSelectionManager.Instance == null)
            {
                Debug.LogError(
                    "場景中找不到 HeroSelectionManager",
                    this
                );

                selectedHero = null;
                DisableButton();
                return;
            }

            selectedHero =
                HeroSelectionManager.Instance.CurrentHeroData;

            if (selectedHero == null)
            {
                Debug.LogError(
                    "目前選擇的英雄資料是空的",
                    this
                );

                DisableButton();
                return;
            }

            buildCost = selectedHero.purchaseCost;

            if (heroIcon != null)
            {
                heroIcon.sprite = selectedHero.icon;
                heroIcon.enabled = selectedHero.icon != null;

                // 避免圖片顏色透明
                heroIcon.color = Color.white;

                // 保持圖片比例
                heroIcon.preserveAspect = true;
            }
            else
            {
                Debug.LogError(
                    "HeroBuildButton 沒有指定 Hero Icon",
                    this
                );
            }

            if (priceText != null)
            {
                priceText.text = $"${buildCost}";
            }

            if (GameManager.Instance != null)
            {
                CheckMoney(GameManager.Instance.gold);
            }

            Debug.Log(
                $"建造按鈕已刷新：" +
                $"英雄={selectedHero.heroName} | " +
                $"圖片={selectedHero.icon?.name}",
                this
            );
        }

        /// <summary>
        /// 點擊唯一的英雄建造按鈕。
        /// </summary>
        private void OnClickBuildHero()
        {
            if (selectedHero == null)
            {
                Debug.LogWarning(
                    "目前沒有可建造的英雄",
                    this
                );

                return;
            }

            if (BuildManager.Instance == null)
            {
                Debug.LogError(
                    "場景中找不到 BuildManager",
                    this
                );

                return;
            }

            if (HeroManager.Instance != null &&
                HeroManager.Instance.activeHero != null)
            {
                Debug.Log(
                    "場上已經有英雄，不能重複放置",
                    this
                );

                CheckMoney(
                    GameManager.Instance != null
                        ? GameManager.Instance.gold
                        : 0
                );

                return;
            }

            BuildManager.Instance.SelectHero(selectedHero);
        }

        /// <summary>
        /// 根據金幣、遊戲狀態與英雄是否已存在，
        /// 控制建造按鈕能否點擊。
        /// </summary>
        private void CheckMoney(int currentGold)
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
                HeroManager.Instance.activeHero != null;

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


        private void OnEnable()
        {
            // 物件再次啟用時重新讀取目前選擇的英雄
            if (HeroSelectionManager.Instance != null)
            {
                LoadSelectedHero();
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClickBuildHero);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= CheckMoney;
            }

            if (HeroSelectionManager.Instance != null)
            {
                HeroSelectionManager.Instance.OnSelectedHeroChanged -=
                    LoadSelectedHero;
            }
        }
    }
}