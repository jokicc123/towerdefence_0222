using System.Collections;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 管理召喚生物的生成、追蹤敵人、攻擊與存在時間。
    /// </summary>
    public class SummonedCreature : MonoBehaviour
    {
        #region Inspector 設定

        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float stopDistance = 1.8f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("生成動畫設定")]
        [Tooltip("召喚生物降臨動畫長度，單位為秒")]
        [SerializeField] private float spawnAnimationDuration = 1.5f;

        [Header("攻擊動畫設定")]
        [Tooltip("攻擊動畫完整長度，單位為秒")]
        [SerializeField] private float attackAnimationDuration = 1.2f;

        [Header("音效")]
        [SerializeField] private AudioClip attackSFX;

        #endregion

        #region 執行期間資料

        private Animator animator;
        private Coroutine attackCoroutine;

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

        #endregion

        #region Animator 參數

        private static readonly int MovingParameter =
            Animator.StringToHash("移動");

        private static readonly int AttackTrigger =
            Animator.StringToHash("攻擊");

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!initialized)
                return;

            UpdateLifetime();

            if (lifeTimer <= 0f)
                return;

            if (spawning)
                return;

            attackTimer -= Time.deltaTime;

            if (isAttacking)
            {
                FaceTarget();
                return;
            }

            UpdateTarget();

            if (currentTarget == null)
            {
                SetMoving(false);
                return;
            }

            MoveOrAttack();
        }

        private void OnDestroy()
        {
            CancelInvoke();

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(
                transform.position,
                searchRadius
            );
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化召喚物的技能數值。
        /// </summary>
        public void Initialize(
            float newDamage,
            float duration,
            float newAttackSpeed,
            float newSearchRadius)
        {
            damage = Mathf.Max(0f, newDamage);
            lifeTimer = Mathf.Max(0f, duration);

            attackSpeed =
                Mathf.Max(newAttackSpeed, 0.01f);

            searchRadius =
                Mathf.Max(0f, newSearchRadius);

            attackTimer = 0f;
            initialized = true;
            spawning = true;
            isAttacking = false;
            damageDealt = false;

            SetMoving(false);

            Invoke(
                nameof(FinishSpawn),
                spawnAnimationDuration
            );
        }

        #endregion

        #region 存在時間

        private void UpdateLifetime()
        {
            lifeTimer -= Time.deltaTime;

            if (lifeTimer <= 0f)
            {
                Disappear();
            }
        }

        private void Disappear()
        {
            Destroy(gameObject);
        }

        #endregion

        #region 敵人搜尋

        private void UpdateTarget()
        {
            if (currentTarget != null &&
                IsTargetValid(currentTarget))
            {
                return;
            }

            currentTarget = null;
            FindNearestEnemy();
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
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null ||
                    enemy.IsDead)
                {
                    continue;
                }

                float distanceSqr =
                    (enemy.transform.position -
                     transform.position)
                    .sqrMagnitude;

                if (distanceSqr >= nearestDistanceSqr)
                    continue;

                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }

            currentTarget = nearestEnemy;
        }

        private bool IsTargetValid(
            Enemy enemy)
        {
            if (enemy == null ||
                enemy.IsDead)
            {
                return false;
            }

            float distanceSqr =
                (enemy.transform.position -
                 transform.position)
                .sqrMagnitude;

            return distanceSqr <=
                   searchRadius * searchRadius;
        }

        #endregion

        #region 移動

        private void MoveOrAttack()
        {
            if (currentTarget == null)
                return;

            Vector3 direction =
                currentTarget.transform.position -
                transform.position;

            direction.y = 0f;

            float distance =
                direction.magnitude;

            FaceTarget();

            if (distance > stopDistance)
            {
                SetMoving(true);

                transform.position +=
                    direction.normalized *
                    moveSpeed *
                    Time.deltaTime;

                return;
            }

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
                currentTarget.transform.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }

        private void SetMoving(bool moving)
        {
            if (animator == null)
                return;

            animator.SetBool(
                MovingParameter,
                moving
            );
        }

        #endregion

        #region 攻擊

        private void StartAttack()
        {
            if (currentTarget == null ||
                isAttacking)
            {
                return;
            }

            isAttacking = true;
            damageDealt = false;

            SetMoving(false);

            if (animator != null)
            {
                animator.ResetTrigger(
                    AttackTrigger
                );

                animator.SetTrigger(
                    AttackTrigger
                );
            }
            else
            {
                DealDamage();
            }

            if (attackCoroutine != null)
            {
                StopCoroutine(
                    attackCoroutine
                );
            }

            attackCoroutine =
                StartCoroutine(
                    FinishAttackFallback()
                );

            attackTimer =
                1f /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                );
        }

        private IEnumerator FinishAttackFallback()
        {
            yield return new WaitForSeconds(
                attackAnimationDuration
            );

            attackCoroutine = null;

            if (!damageDealt)
            {
                DealDamage();
            }

            if (isAttacking)
            {
                FinishAttack();
            }
        }

        /// <summary>
        /// Animation Event：
        /// 放在攻擊命中的動畫幀。
        /// </summary>
        public void DealDamage()
        {
            if (damageDealt ||
                currentTarget == null ||
                currentTarget.IsDead)
            {
                return;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    currentTarget.transform.position
                );

            if (distance >
                stopDistance + 1f)
            {
                return;
            }

            damageDealt = true;

            currentTarget.TakeDamage(
                damage
            );

            if (SoundManager.Instance != null &&
                attackSFX != null)
            {
                SoundManager.Instance.PlaySFX(
                    attackSFX
                );
            }
        }

        /// <summary>
        /// Animation Event：
        /// 放在攻擊動畫最後一幀。
        /// </summary>
        public void FinishAttack()
        {
            if (!isAttacking)
                return;

            isAttacking = false;

            if (attackCoroutine != null)
            {
                StopCoroutine(
                    attackCoroutine
                );

                attackCoroutine = null;
            }

            if (currentTarget == null ||
                !IsTargetValid(currentTarget))
            {
                currentTarget = null;
            }
        }

        #endregion

        #region 生成動畫

        /// <summary>
        /// Animation Event：
        /// 放在生成動畫最後一幀。
        /// </summary>
        public void FinishSpawn()
        {
            if (!spawning)
                return;

            spawning = false;
            SetMoving(false);
        }

        #endregion
    }
}