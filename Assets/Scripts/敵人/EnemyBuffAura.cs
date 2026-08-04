using System.Collections.Generic;
using UnityEngine;
using static CHANG.EnemyData;

namespace CHANG
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyBuffAura : MonoBehaviour
    {
        private Enemy owner;
        private EnemyData Data =>
            owner != null ? owner.Data : null;

        private readonly HashSet<Enemy> buffedEnemies =
            new HashSet<Enemy>();

        private float updateTimer;

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

            // 沒有光環能力就關閉腳本
            if (Data.buffType == EnemyBuffType.None)
            {
                enabled = false;
                return;
            }

            // 出生後立即檢查一次
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

        private void RefreshAura()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                Data.buffRadius
            );

            HashSet<Enemy> enemiesCurrentlyInAura =
                new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                // 預設不強化自己
                if (!Data.buffSelf && enemy == owner)
                    continue;

                enemiesCurrentlyInAura.Add(enemy);

                if (buffedEnemies.Contains(enemy))
                    continue;

                ApplyBuff(enemy);
                buffedEnemies.Add(enemy);
            }

            List<Enemy> removeList =
                new List<Enemy>();

            foreach (Enemy enemy in buffedEnemies)
            {
                if (enemy == null)
                {
                    removeList.Add(enemy);
                    continue;
                }

                if (!enemiesCurrentlyInAura.Contains(enemy))
                {
                    RemoveBuff(enemy);
                    removeList.Add(enemy);
                }
            }

            foreach (Enemy enemy in removeList)
            {
                buffedEnemies.Remove(enemy);
            }
        }

        private void ApplyBuff(Enemy enemy)
        {
            if (enemy == null || Data == null)
                return;

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

            if (Data.buffVFX != null)
            {
                Instantiate(
                    Data.buffVFX,
                    enemy.transform.position,
                    Quaternion.identity,
                    enemy.transform
                );
            }

            Debug.Log(
                $"{owner.name} 對 {enemy.name} 套用 " +
                $"{Data.buffType} × {Data.buffMultiplier}"
            );
        }

        private void RemoveBuff(Enemy enemy)
        {
            if (enemy == null || Data == null)
                return;

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
            Enemy enemy = GetComponent<Enemy>();

            if (enemy == null ||
                enemy.Data == null ||
                enemy.Data.buffType ==
                EnemyBuffType.None)
            {
                return;
            }

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                enemy.Data.buffRadius
            );
        }
    }
}