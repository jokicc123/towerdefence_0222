using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    /// <summary>
    /// Hero 的被動光環系統。
    /// 定期搜尋範圍內符合條件的防禦塔，
    /// 並套用或移除被動 Buff。
    /// </summary>
    public partial class Hero
    {
        #region 執行期間資料

        private readonly HashSet<Tower> towersCurrentlyInAura =
            new();

        #endregion

        #region 被動光環更新

        private IEnumerator UpdatePassiveAuraRoutine()
        {
            while (true)
            {
                UpdatePassiveAura();

                yield return new WaitForSeconds(
                    auraUpdateInterval
                );
            }
        }

        private void UpdatePassiveAura()
        {
            if (data == null ||
                data.passive.auraRadius <= 0f)
            {
                return;
            }

            towersCurrentlyInAura.Clear();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    data.passive.auraRadius
                );

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                Tower tower =
                    hit.GetComponentInParent<Tower>();

                if (tower == null ||
                    !IsBuffableTower(tower))
                {
                    continue;
                }

                towersCurrentlyInAura.Add(
                    tower
                );

                if (buffedTowers.Contains(tower))
                    continue;

                ApplyPassiveBuff(tower);
            }

            RemoveTowersOutsideAura();
        }

        #endregion

        #region Buff 套用與移除

        private void ApplyPassiveBuff(
            Tower tower)
        {
            if (tower == null ||
                data == null)
            {
                return;
            }

            tower.ApplyBuff(
                data.passive.buffType,
                data.passive.buffMultiplier
            );

            buffedTowers.Add(
                tower
            );

#if UNITY_EDITOR
            Debug.Log(
                $"光環加入：{data.heroName} → {tower.name}，" +
                $"類型：{data.passive.buffType}，" +
                $"倍率：{data.passive.buffMultiplier}",
                tower
            );
#endif
        }

        private void RemoveTowersOutsideAura()
        {
            for (int i =
                     buffedTowers.Count - 1;
                 i >= 0;
                 i--)
            {
                Tower tower =
                    buffedTowers[i];

                if (tower == null)
                {
                    buffedTowers.RemoveAt(i);
                    continue;
                }

                if (towersCurrentlyInAura.Contains(tower))
                    continue;

                RemovePassiveBuff(tower);
                buffedTowers.RemoveAt(i);
            }
        }

        private void RemovePassiveBuff(
            Tower tower)
        {
            if (tower == null ||
                data == null)
            {
                return;
            }

            tower.RemoveBuff(
                data.passive.buffType,
                data.passive.buffMultiplier
            );

#if UNITY_EDITOR
            Debug.Log(
                $"光環移除：{data.heroName} → {tower.name}",
                tower
            );
#endif
        }

        private void RemoveAllPassiveBuffs()
        {
            if (data == null)
                return;

            foreach (Tower tower in buffedTowers)
            {
                if (tower == null)
                    continue;

                tower.RemoveBuff(
                    data.passive.buffType,
                    data.passive.buffMultiplier
                );
            }

            buffedTowers.Clear();
            towersCurrentlyInAura.Clear();
        }

        #endregion

        #region 光環目標判斷

        private bool IsBuffableTower(
            Tower tower)
        {
            if (tower == null ||
                data == null)
            {
                return false;
            }

            TowerEffectType[] targetTypes =
                data.passive.targetEffectTypes;

            // 清單留空代表所有塔都能受到 Buff。
            if (targetTypes == null ||
                targetTypes.Length == 0)
            {
                return true;
            }

            foreach (TowerEffectType type in targetTypes)
            {
                if (tower.EffectType == type)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (data == null)
                return;

            Gizmos.color =
                Color.green;

            Gizmos.DrawWireSphere(
                transform.position,
                data.passive.auraRadius
            );

            if (data.attackType !=
                HeroAttackType.Melee)
            {
                return;
            }

            Vector3 hitCenter =
                transform.position +
                transform.forward *
                data.meleeHitOffset;

            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                hitCenter,
                data.meleeHitRadius
            );
        }

        #endregion
    }
}