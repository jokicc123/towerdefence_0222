using System.Collections.Generic;
using UnityEngine;
using static CHANG.EnemyData;

namespace CHANG
{
    /// <summary>
    /// 敵人光環系統。
    /// 定期搜尋範圍內的敵人，套用或移除移動速度、傷害或防禦 Buff。
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyBuffAura : MonoBehaviour
    {
        #region 執行期間資料

        private Enemy owner;

        private readonly HashSet<Enemy> buffedEnemies =
            new();

        private readonly HashSet<Enemy> enemiesCurrentlyInAura =
            new();

        private readonly List<Enemy> removeList =
            new();

        private float updateTimer;

        #endregion

        #region 屬性

        private EnemyData Data =>
            owner != null
                ? owner.Data
                : null;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            owner = GetComponent<Enemy>();

            if (owner == null)
            {
                Debug.LogError(
                    $"{name} 找不到 Enemy 元件",
                    this
                );

                enabled = false;
            }
        }

        private void Start()
        {
            if (Data == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 EnemyData",
                    this
                );

                enabled = false;
                return;
            }

            if (Data.buffType == EnemyBuffType.None)
            {
                enabled = false;
                return;
            }

            RefreshAura();
        }

        private void Update()
        {
            if (Data == null)
                return;

            updateTimer -= Time.deltaTime;

            if (updateTimer > 0f)
                return;

            updateTimer = Mathf.Max(
                Data.buffUpdateInterval,
                0.1f
            );

            RefreshAura();
        }

        private void OnDisable()
        {
            RemoveAllBuffs();
        }

        private void OnDestroy()
        {
            RemoveAllBuffs();
        }

        private void OnDrawGizmosSelected()
        {
            Enemy enemy =
                GetComponent<Enemy>();

            if (enemy == null ||
                enemy.Data == null ||
                enemy.Data.buffType ==
                EnemyBuffType.None)
            {
                return;
            }

            Gizmos.color =
                Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                enemy.Data.buffRadius
            );
        }

        #endregion

        #region 光環更新

        private void RefreshAura()
        {
            enemiesCurrentlyInAura.Clear();
            removeList.Clear();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    Data.buffRadius
                );

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead)
                {
                    continue;
                }

                if (!Data.buffSelf &&
                    enemy == owner)
                {
                    continue;
                }

                enemiesCurrentlyInAura.Add(
                    enemy
                );

                if (buffedEnemies.Contains(enemy))
                    continue;

                ApplyBuff(enemy);
                buffedEnemies.Add(enemy);
            }

            foreach (Enemy enemy in buffedEnemies)
            {
                if (enemy == null)
                {
                    removeList.Add(enemy);
                    continue;
                }

                if (enemiesCurrentlyInAura.Contains(enemy))
                    continue;

                RemoveBuff(enemy);
                removeList.Add(enemy);
            }

            foreach (Enemy enemy in removeList)
            {
                buffedEnemies.Remove(enemy);
            }
        }

        #endregion

        #region Buff 套用與移除

        private void ApplyBuff(
            Enemy enemy)
        {
            if (enemy == null ||
                Data == null)
            {
                return;
            }

            switch (Data.buffType)
            {
                case EnemyBuffType.MoveSpeed:
                    enemy.ApplySpeedBuff(
                        Data.buffMultiplier
                    );
                    break;

                case EnemyBuffType.Damage:
                    enemy.ApplyDamageBuff(
                        Data.buffMultiplier
                    );
                    break;

                case EnemyBuffType.Defense:
                    enemy.ApplyDefenseBuff(
                        Data.buffMultiplier
                    );
                    break;
            }

            SpawnBuffVFX(enemy);

#if UNITY_EDITOR
            Debug.Log(
                $"{owner.name} 對 {enemy.name} 套用 " +
                $"{Data.buffType} × {Data.buffMultiplier}",
                this
            );
#endif
        }

        private void RemoveBuff(
            Enemy enemy)
        {
            if (enemy == null ||
                Data == null)
            {
                return;
            }

            switch (Data.buffType)
            {
                case EnemyBuffType.MoveSpeed:
                    enemy.RemoveSpeedBuff(
                        Data.buffMultiplier
                    );
                    break;

                case EnemyBuffType.Damage:
                    enemy.RemoveDamageBuff(
                        Data.buffMultiplier
                    );
                    break;

                case EnemyBuffType.Defense:
                    enemy.RemoveDefenseBuff(
                        Data.buffMultiplier
                    );
                    break;
            }
        }

        private void RemoveAllBuffs()
        {
            if (Data == null)
                return;

            foreach (Enemy enemy in buffedEnemies)
            {
                if (enemy != null)
                {
                    RemoveBuff(enemy);
                }
            }

            buffedEnemies.Clear();
            enemiesCurrentlyInAura.Clear();
            removeList.Clear();
        }

        #endregion

        #region 特效

        private void SpawnBuffVFX(
            Enemy enemy)
        {
            if (Data == null ||
                Data.buffVFX == null ||
                enemy == null)
            {
                return;
            }

            Instantiate(
                Data.buffVFX,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform
            );
        }

        #endregion
    }
}