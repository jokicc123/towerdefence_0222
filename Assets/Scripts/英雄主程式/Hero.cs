using System.Collections.Generic;
using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 英雄核心類別。
    /// 負責英雄初始化、生命週期與共用執行期間資料。
    /// 攻擊、技能、等級與被動效果由其他 partial 檔案負責。
    /// </summary>
    public partial class Hero : MonoBehaviour
    {
        #region Inspector 設定

        [Header("英雄資料")]
        public HeroData data;

        [Header("英雄等級")]
        [SerializeField]
        private int currentLevel = 1;

        [SerializeField]
        private int currentXP;

        [Header("攻擊元件")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform head;
        [SerializeField] private Animator animator;

        [Header("特效生成位置")]
        [SerializeField] private Transform normalAttackVFXPoint;
        [SerializeField] private Transform deathFlameVFXPoint;

        [Header("被動光環更新")]
        [SerializeField, Min(0.1f)]
        private float auraUpdateInterval = 0.5f;

        [Header("攻擊範圍顯示")]
        [SerializeField]
        private RangeCircle rangeCircle;
        #endregion

        #region 屬性

        public int CurrentLevel =>
            currentLevel;

        public int CurrentXP =>
            currentXP;

        #endregion

        #region 執行期間資料

        private readonly List<Enemy> enemiesInRange =
            new();

        private readonly List<Tower> buffedTowers =
            new();

        private float attackTimer;
        private Enemy pendingFireTarget;

        private float skill1Timer;
        private float skill2Timer;

        private Coroutine auraCoroutine;
        private Coroutine sunCoroutine;

        private bool sunBurnBuffActive;
        private GameObject currentSunVFX;

        #endregion

        #region 事件

        public event System.Action OnHeroDataChanged;

        #endregion

        #region Unity 生命週期

        private void Awake()
        {
            CacheAnimator();
        }

        private void Start()
        {
            if (!ValidateHeroData())
                return;

            HeroManager.Instance?.RegisterHero(this);
            UiManager.Instance?.SetActiveHero(this);

            auraCoroutine = StartCoroutine(
                UpdatePassiveAuraRoutine()
            );
            HideRangeCircle();
            OnHeroDataChanged?.Invoke();
        }

        private void Update()
        {
            UpdateSkillTimers();
            UpdateEnemiesInRange();
            HandleAttack();
        }

        private void OnDestroy()
        {
            StopSunOfNoon();
            StopAuraCoroutine();
            RemoveAllPassiveBuffs();

            UiManager.Instance?.ClearActiveHero(this);
        }

        #endregion

        #region 初始化

        private void CacheAnimator()
        {
            if (animator == null)
            {
                animator =
                    GetComponentInChildren<Animator>(true);
            }

            if (animator == null)
            {
                Debug.LogError(
                    $"{name} 找不到 Animator",
                    this
                );
            }
        }

        private bool ValidateHeroData()
        {
            if (data != null)
                return true;

            Debug.LogError(
                $"{name} 沒有設定 HeroData",
                this
            );

            enabled = false;
            return false;
        }

        private void StopAuraCoroutine()
        {
            if (auraCoroutine == null)
                return;

            StopCoroutine(auraCoroutine);
            auraCoroutine = null;
        }

        #endregion

        #region 共用更新

        private void UpdateSkillTimers()
        {
            skill1Timer =
                Mathf.Max(
                    0f,
                    skill1Timer - Time.deltaTime
                );

            skill2Timer =
                Mathf.Max(
                    0f,
                    skill2Timer - Time.deltaTime
                );
        }

        #endregion
    }
}