using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// Hero 的等級與經驗值系統。
    /// </summary>
    public partial class Hero
    {
        #region 經驗值與升級

        /// <summary>
        /// 增加英雄經驗值，並在達到需求時自動升級。
        /// </summary>
        public void GainXP(int amount)
        {
            if (amount <= 0 ||
                data == null ||
                data.levelStats == null ||
                data.levelStats.Length == 0)
            {
                return;
            }

            int maxLevel =
                data.levelStats.Length;

            // 已經滿等，不再取得經驗。
            if (currentLevel >= maxLevel)
            {
                SetMaximumLevel(maxLevel);
                return;
            }

            currentXP += amount;

            while (currentLevel < maxLevel)
            {
                int requiredXP =
                    data.levelStats[currentLevel - 1]
                        .xpToNextLevel;

                if (requiredXP <= 0 ||
                    currentXP < requiredXP)
                {
                    break;
                }

                currentXP -= requiredXP;
                currentLevel++;

                OnLevelUp();

                if (currentLevel >= maxLevel)
                {
                    SetMaximumLevel(maxLevel);
                    return;
                }
            }

            NotifyHeroDataChanged();
        }

        /// <summary>
        /// 將英雄設定為最高等級。
        /// </summary>
        private void SetMaximumLevel(
            int maxLevel)
        {
            currentLevel =
                Mathf.Clamp(
                    maxLevel,
                    1,
                    data.levelStats.Length
                );

            currentXP = 0;

            NotifyHeroDataChanged();
        }

        /// <summary>
        /// 英雄升級時執行。
        /// </summary>
        private void OnLevelUp()
        {
            RefreshRangeCircle();

#if UNITY_EDITOR
            HeroLevelStats stats =
                CurrentStats;

            Debug.Log(
                $"{data.heroName} 升到 Lv.{currentLevel}：" +
                $"{stats.unlockDescription}",
                this
            );
#endif
        }
        

        #endregion

        #region 資料更新通知

        private void NotifyHeroDataChanged()
        {
            OnHeroDataChanged?.Invoke();
        }

        #endregion
    }
}