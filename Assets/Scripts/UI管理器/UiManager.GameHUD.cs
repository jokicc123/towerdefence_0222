using TMPro;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// UiManager 的一般遊戲 HUD。
    /// 負責生命、金幣、波數，以及勝敗畫面顯示。
    /// </summary>
    public partial class UiManager
    {
        #region Inspector 設定

        [Header("一般 UI")]
        [SerializeField]
        private TMP_Text hpText;

        [SerializeField]
        private TMP_Text goldText;

        [SerializeField]
        private TMP_Text waveText;

        [Header("勝敗畫面")]
        [SerializeField]
        private CanvasGroup gameOverUI;

        [SerializeField]
        private CanvasGroup winUI;

        [SerializeField]
        private TMP_Text winCrystalText;

        [SerializeField]
        private TMP_Text loseCrystalText;

        #endregion

        #region UI 初始化

        private void InitializeGeneralUI()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            UpdateHp(
                gameManager.CastleHp
            );

            UpdateGold(
                gameManager.Gold
            );

            UpdateWave(
                gameManager.CurrentWave
            );
        }

        #endregion

        #region HUD 更新

        private void UpdateHp(
            int hp)
        {
            if (hpText == null)
                return;

            hpText.text =
                hp.ToString();
        }

        private void UpdateGold(
            int gold)
        {
            if (goldText != null)
            {
                goldText.text =
                    gold.ToString();
            }

            if (upgradePanel != null &&
                upgradePanel.activeSelf)
            {
                RefreshUpgradeUI();
            }
        }

        private void UpdateWave(
            int wave)
        {
            if (waveText == null)
                return;

            waveText.text =
                wave.ToString();
        }

        #endregion

        #region 勝敗畫面

        private void ShowGameOver()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            if (loseCrystalText != null)
            {
                loseCrystalText.text =
                    $"獲得 {gameManager.LoseCrystalReward} 水晶";
            }

            ShowCanvasGroup(
                gameOverUI
            );
        }

        private void ShowWin()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            if (winCrystalText != null)
            {
                winCrystalText.text =
                    $"獲得 {gameManager.WinCrystalReward} 水晶";
            }

            ShowCanvasGroup(
                winUI
            );
        }

        #endregion

        #region 共用 UI 工具

        private void ShowCanvasGroup(
            CanvasGroup group)
        {
            if (group == null)
                return;

            StartCoroutine(
                FadeSystem.Fade(
                    group,
                    true
                )
            );
        }

        #endregion
    }
}