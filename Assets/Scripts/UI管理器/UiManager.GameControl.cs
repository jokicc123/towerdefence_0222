using UnityEngine;
using UnityEngine.UI;

namespace CHANG
{
    /// <summary>
    /// UiManager 的遊戲控制功能。
    /// 負責開始、暫停、繼續，以及遊戲結束時清理 UI。
    /// </summary>
    public partial class UiManager
    {
        #region Inspector 設定

        [Header("遊戲控制")]
        [SerializeField]
        private Button playPauseButton;

        [SerializeField]
        private Sprite playSprite;

        [SerializeField]
        private Sprite pauseSprite;

        #endregion

        #region 遊戲控制

        public void OnClickPlayPause()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (gameManager == null)
                return;

            switch (gameManager.CurrentState)
            {
                case GameManager.GameState.Build:
                    gameManager.StartGame();
                    break;

                case GameManager.GameState.Playing:
                    gameManager.PauseGame();
                    break;

                case GameManager.GameState.Paused:
                    gameManager.ResumeGame();
                    break;
            }

            UpdatePlayPauseUI();
        }

        #endregion

        #region 遊戲控制 UI

        private void UpdatePlayPauseUI()
        {
            GameManager gameManager =
                GameManager.Instance;

            if (playPauseButton == null ||
                gameManager == null)
            {
                return;
            }

            Sprite targetSprite =
                gameManager.CurrentState ==
                GameManager.GameState.Playing
                    ? pauseSprite
                    : playSprite;

            if (playPauseButton.image != null)
            {
                playPauseButton.image.sprite =
                    targetSprite;
            }
        }

        #endregion

        #region 遊戲結束處理

        private void OnGameEnd()
        {
            HideUpgradeUI();
            HideHeroUI();

            currentTower = null;
            currentHero = null;
            activeHero = null;

            SetSkillButtonsEnabled(
                false
            );

            ResetSkillCooldownImages();

            UpdatePlayPauseUI();
        }

        #endregion
    }
}