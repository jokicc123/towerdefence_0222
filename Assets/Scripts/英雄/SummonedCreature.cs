using UnityEngine;
using System.Collections;
namespace CHANG
{
    public class SummonedCreature : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float stopDistance = 1.8f;
        [SerializeField] private float rotationSpeed = 10f;

        [Tooltip("樹人降臨動畫的長度，單位為秒")]
        [SerializeField] private float spawnAnimationDuration = 1.5f;

        [Header("攻擊動畫設定")]
        [Tooltip("攻擊動畫完整長度，單位為秒")]
        [SerializeField] private float attackAnimationDuration = 1.2f;

        private Coroutine attackCoroutine;
        [Header("音效")]
        [SerializeField] private AudioClip attackSFX;

        

        private Animator ani;

        // Hero 傳入的技能數值
        private float damage;
        private float lifeTimer;
        private float attackSpeed;
        private float searchRadius;

        private float attackTimer;

        private Enemy currentTarget;

        private bool initialized;
        private bool spawning = true;
        private bool isAttacking;
        private bool damageDealt;

        public void Initialize(
            float newDamage,
            float duration,
            float newAttackSpeed,
            float newSearchRadius)
        {
            damage = newDamage;
            lifeTimer = duration;
            attackSpeed = Mathf.Max(newAttackSpeed, 0.01f);
            searchRadius = newSearchRadius;

            attackTimer = 0f;
            initialized = true;
            spawning = true;
            isAttacking = false;

            if (ani != null)
            {
                ani.SetBool("移動", false);
            }

            // 如果不想使用 Animation Event，
            // 可以使用時間自動結束降臨狀態。
            Invoke(nameof(FinishSpawn), spawnAnimationDuration);
        }

        private void Awake()
        {
            ani = GetComponent<Animator>();
        }
        private void Update()
        {
            if (!initialized)
                return;

            // 存在時間倒數
            lifeTimer -= Time.deltaTime;

            if (lifeTimer <= 0f)
            {
                Disappear();
                return;
            }

            // 降臨動畫期間不能戰鬥
            if (spawning)
                return;

            attackTimer -= Time.deltaTime;

            // 攻擊動畫期間只轉向，不移動
            if (isAttacking)
            {
                FaceTarget();
                return;
            }

            // 目標死亡、被銷毀或離開搜尋範圍
            if (currentTarget == null || !IsTargetValid(currentTarget))
            {
                currentTarget = null;
                FindNearestEnemy();
            }

            // 沒找到敵人就待機
            if (currentTarget == null)
            {
                SetMoving(false);
                return;
            }

            MoveOrAttack();
        }



        private void FindNearestEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                searchRadius
            );

            Enemy nearestEnemy = null;
            float nearestDistanceSqr = Mathf.Infinity;

            foreach (Collider hit in hits)
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                float distanceSqr =
                    (enemy.transform.position - transform.position)
                    .sqrMagnitude;

                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestEnemy = enemy;
                }
            }

            currentTarget = nearestEnemy;
        }

        private bool IsTargetValid(Enemy enemy)
        {
            if (enemy == null)
                return false;

            float distanceSqr =
                (enemy.transform.position - transform.position)
                .sqrMagnitude;

            return distanceSqr <= searchRadius * searchRadius;
        }

        private void MoveOrAttack()
        {
            if (currentTarget == null)
                return;

            Vector3 direction =
                currentTarget.transform.position - transform.position;

            direction.y = 0f;

            float distance = direction.magnitude;

            FaceTarget();

            // 距離超過攻擊範圍，繼續追擊
            if (distance > stopDistance)
            {
                SetMoving(true);

                transform.position +=
                    direction.normalized *
                    moveSpeed *
                    Time.deltaTime;

                return;
            }

            // 抵達敵人旁邊
            SetMoving(false);

            if (attackTimer <= 0f)
            {
                StartAttack();
            }
        }

        private void FaceTarget()
        {
            if (currentTarget == null)
                return;

            Vector3 direction =
                currentTarget.transform.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        private void StartAttack()
        {
            if (currentTarget == null || isAttacking)
                return;

            isAttacking = true;
            damageDealt = false;

            SetMoving(false);

            if (ani != null)
            {
                ani.ResetTrigger("攻擊");
                ani.SetTrigger("攻擊");

                Debug.Log($"樹人開始攻擊：{currentTarget.name}");
            }
            else
            {
                DealDamage();
            }

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }

            attackCoroutine = StartCoroutine(
                FinishAttackFallback()
            );

            attackTimer =
                1f / Mathf.Max(attackSpeed, 0.01f);
        }
        private IEnumerator FinishAttackFallback()
        {
            yield return new WaitForSeconds(
                attackAnimationDuration
            );

            attackCoroutine = null;

            // 動畫事件沒有造成傷害時，才保底扣血
            if (!damageDealt)
            {
                DealDamage();
            }

            // 動畫事件沒有結束攻擊時，才保底結束
            if (isAttacking)
            {
                FinishAttack();
            }
        }
        /// <summary>
        /// 放在攻擊動畫拳頭碰到敵人的那一幀。
        /// Animation Event 呼叫。
        /// </summary>
        public void DealDamage()
        {
            if (damageDealt)
                return;

            if (currentTarget == null)
                return;

            float distance = Vector3.Distance(
                transform.position,
                currentTarget.transform.position
            );

            if (distance > stopDistance + 1f)
            {
                Debug.Log("敵人已離開樹人的攻擊距離");
                return;
            }

            damageDealt = true;

            currentTarget.TakeDamage(damage);
            if (SoundManager.Instance != null &&
                attackSFX != null)
            {
                SoundManager.Instance.PlaySFX(
                attackSFX
                );

                Debug.Log(
                $"樹人攻擊 {currentTarget.name}，造成 {damage} 傷害"
            );
            }
        }
        /// <summary>
        /// 放在攻擊動畫最後一幀。
        /// </summary>
        public void FinishAttack()
        {
            if (!isAttacking)
                return;

            isAttacking = false;

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            if (currentTarget == null ||
                !IsTargetValid(currentTarget))
            {
                currentTarget = null;
            }

            Debug.Log("樹人攻擊結束，繼續追擊");
        }

        /// <summary>
        /// 放在 Spawn 動畫最後一幀。
        /// </summary>
        public void FinishSpawn()
        {
            if (!spawning)
                return;

            spawning = false;

            if (ani != null)
            {
                ani.SetBool("移動", false);
            }
        }

        private void SetMoving(bool moving)
        {
            if (ani != null)
            {
                ani.SetBool("移動", moving);
            }
        }

        private void Disappear()
        {
            

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            CancelInvoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                searchRadius
            );
        }
    }
}