using System.Collections.Generic;
using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    /// <summary>
    /// 防禦塔核心類別。
    /// 負責等級、攻擊、目標搜尋、英雄光環、模型與範圍顯示。
    /// </summary>
    public class Tower : Character
    {
        #region 資料

        [Header("資料")]
        [SerializeField]
        private TowerData data;

        [SerializeField]
        private int currentLevel;

        [Header("模型")]
        [SerializeField]
        private Transform modelRoot;

        [Header("範圍顯示")]
        [SerializeField]
        private RangeCircle rangeCircle;

        public TowerData Data =>
            data;

        public int CurrentLevel =>
            currentLevel;

        private TowerLevel CurrentData =>
            data.levels[currentLevel];

        public float AttackRange =>
            CurrentData.attackRange *
            rangeMultiplier;

        public float AttackSpeed =>
            CurrentData.attackSpeed *
            attackSpeedMultiplier;

        public float Cost =>
            CurrentData.cost;

        public TowerEffectType EffectType =>
            CurrentData.effectType;

        public float Damage =>
            CurrentData.damage *
            damageMultiplier *
            ShopBonus.TowerDamageMultiplier;

        private GameObject BulletPrefab =>
            CurrentData.bulletPrefab;

        public Transform FirePoint
        {
            get;
            private set;
        }

        public Transform Head
        {
            get;
            private set;
        }

        #endregion

        #region 執行期間資料

        private float damageMultiplier = 1f;
        private float attackSpeedMultiplier = 1f;
        private float rangeMultiplier = 1f;

        private float attackTimer;

        private float AttackInterval =>
            1f / Mathf.Max(
                AttackSpeed,
                0.01f
            );

        private GameObject currentModel;

        private SphereCollider rangeCollider;

        private readonly List<Enemy> enemiesInRange =
            new();

        private readonly HashSet<Enemy> foundEnemies =
            new();

        #endregion

        #region 狀態機

        public TowerIdle Idle
        {
            get;
            private set;
        }

        public TowerAttack Attack
        {
            get;
            private set;
        }

       
        #endregion

        #region Unity 生命週期

        protected override void Awake()
        {
            base.Awake();

            rangeCollider =
                GetComponent<SphereCollider>();

            if (rangeCollider != null)
            {
                rangeCollider.isTrigger = true;
            }

            InitializeStates();
        }

        protected override void Update()
        {
            if (!CanUpdateTower())
                return;

            UpdateEnemiesInRange();

            base.Update();
        }

        #endregion

        #region 初始化

        private void InitializeStates()
        {
            Idle =
                new TowerIdle(
                    "待機",
                    stateMachine,
                    this
                );

            Attack =
                new TowerAttack(
                    "攻擊",
                    stateMachine,
                    this
                );

          

            stateMachine.Initialize(
                Idle
            );
        }

        public void Initialize(
            TowerData towerData)
        {
            if (towerData == null)
            {
                Debug.LogError(
                    $"{name} 初始化失敗：TowerData 為空",
                    this
                );

                enabled = false;
                return;
            }

            if (towerData.levels == null ||
                towerData.levels.Length == 0)
            {
                Debug.LogError(
                    $"{towerData.name} 沒有設定任何塔等級資料",
                    this
                );

                enabled = false;
                return;
            }

            data = towerData;
            currentLevel = 0;

            ResetBuffMultipliers();

            ApplyLevel();
            HideRangeCircle();

#if UNITY_EDITOR
            Debug.Log(
                $"Tower Initialize：{name}",
                this
            );
#endif
        }

        private void ApplyLevel()
        {
            RefreshRangeCollider();

            if (rangeCircle != null)
            {
                rangeCircle.DrawCircle(
                    AttackRange
                );

                rangeCircle.SetColor(
                    new Color(
                        0f,
                        1f,
                        0f,
                        0.4f
                    )
                );
            }

            attackTimer = 0f;

            UpdateModel();

#if UNITY_EDITOR
            Debug.Log(
                $"套用塔等級：{currentLevel + 1}",
                this
            );
#endif
        }

        private void ResetBuffMultipliers()
        {
            damageMultiplier = 1f;
            attackSpeedMultiplier = 1f;
            rangeMultiplier = 1f;
        }

        #endregion

        #region 更新流程

        private bool CanUpdateTower()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameRunning())
            {
                return false;
            }

            return data != null &&
                   data.levels != null &&
                   data.levels.Length > 0;
        }

        private void UpdateTowerState()
        {
            if (HasTarget() &&
                stateMachine.CurrentState== Idle)
            {
                stateMachine.ChangeState(
                    Attack
                );

                return;
            }

            if (!HasTarget() &&
                stateMachine.CurrentState == Attack)
            {
                stateMachine.ChangeState(
                    Idle
                );
            }
        }

        #endregion

        #region 升級系統

        public bool CanUpgrade()
        {
            return data != null &&
                   data.levels != null &&
                   currentLevel <
                   data.levels.Length - 1;
        }

        public int GetUpgradeCost()
        {
            if (!CanUpgrade())
                return 0;

            return data.levels[
                currentLevel + 1
            ].cost;
        }

        public void Upgrade()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsGameRunning())
            {
                return;
            }

            if (!CanUpgrade())
                return;

            int upgradeCost =
                GetUpgradeCost();

            if (GameManager.Instance == null ||
                !GameManager.Instance.SpendGold(
                    upgradeCost))
            {
#if UNITY_EDITOR
                Debug.Log(
                    "金幣不足，無法升級",
                    this
                );
#endif
                return;
            }

            currentLevel++;

            ApplyLevel();

#if UNITY_EDITOR
            Debug.Log(
                $"升級完成 → Lv.{currentLevel + 1}",
                this
            );
#endif
        }

        #endregion

        #region 攻擊系統

        public bool HasTarget()
        {
            enemiesInRange.RemoveAll(
                enemy =>
                    enemy == null ||
                    enemy.IsDead
            );

            return enemiesInRange.Count > 0;
        }

        public Enemy GetTarget()
        {
            return HasTarget()
                ? enemiesInRange[0]
                : null;
        }

        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();
            foundEnemies.Clear();

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    AttackRange
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

                if (foundEnemies.Add(enemy))
                {
                    enemiesInRange.Add(
                        enemy
                    );
                }
            }

            enemiesInRange.Sort(
                CompareEnemyDistance
            );
        }

        private int CompareEnemyDistance(
            Enemy first,
            Enemy second)
        {
            float firstDistance =
                (first.transform.position -
                 transform.position)
                .sqrMagnitude;

            float secondDistance =
                (second.transform.position -
                 transform.position)
                .sqrMagnitude;

            return firstDistance.CompareTo(
                secondDistance
            );
        }

        public void ResetAttackTimer()
        {
            attackTimer =
                AttackInterval;
        }

        public void TickAttackTimer()
        {
            attackTimer =
                Mathf.Max(
                    0f,
                    attackTimer -
                    Time.deltaTime
                );
        }

        public bool IsAttackReady()
        {
            return attackTimer <= 0f;
        }

        public virtual void Fire(
            Enemy target)
        {
            if (target == null ||
                target.IsDead)
            {
                return;
            }

            if (BulletPrefab == null)
            {
                Debug.LogWarning(
                    $"{name} 沒有 Bullet Prefab",
                    this
                );

                return;
            }

            if (FirePoint == null)
            {
                Debug.LogWarning(
                    $"{name} 沒有 FirePoint",
                    this
                );

                return;
            }

            PlayAttackSound();

            Vector3 direction =
                target.transform.position -
                FirePoint.position;

            if (direction.sqrMagnitude >
                0.001f)
            {
                FirePoint.rotation =
                    Quaternion.LookRotation(
                        direction.normalized
                    );
            }

            GameObject bulletObject =
                Instantiate(
                    BulletPrefab,
                    FirePoint.position,
                    FirePoint.rotation
                );

            if (!bulletObject.TryGetComponent(
                    out Bullet bullet))
            {
                Debug.LogWarning(
                    $"{bulletObject.name} 沒有 Bullet 腳本",
                    bulletObject
                );

                Destroy(
                    bulletObject
                );

                return;
            }

            bullet.SetTarget(
                target,
                Damage,
                CurrentData.effectType,
                CurrentData.effectDuration,
                CurrentData.effectDamagePerSecond,
                CurrentData.blastRadius
            );
        }

        private void PlayAttackSound()
        {
            if (SoundManager.Instance == null ||
                data.attackSFX == null)
            {
                return;
            }

            SoundManager.Instance.PlaySFX(
                data.attackSFX
            );
        }

        protected void ApplyEffect(
            Enemy enemy)
        {
            if (enemy == null ||
                enemy.IsDead)
            {
                return;
            }

            switch (CurrentData.effectType)
            {
                case TowerEffectType.Burn:
                    enemy.AddEffect(
                        new BurnEffect(
                            enemy,
                            CurrentData.effectDuration,
                            CurrentData.effectDamagePerSecond
                        )
                    );
                    break;

                case TowerEffectType.Poison:
                    enemy.AddEffect(
                        new PoisonEffect(
                            enemy,
                            CurrentData.effectDuration,
                            CurrentData.effectDamagePerSecond,
                            CurrentData.slowPercent
                        )
                    );
                    break;
            }
        }

        #endregion

        #region 被動光環

        public void ApplyBuff(
            PassiveBuffType buffType,
            float multiplier)
        {
            if (multiplier <= 0f)
                return;

            switch (buffType)
            {
                case PassiveBuffType.Damage:
                    damageMultiplier *=
                        multiplier;
                    break;

                case PassiveBuffType.AttackSpeed:
                    attackSpeedMultiplier *=
                        multiplier;
                    break;

                case PassiveBuffType.Range:
                    rangeMultiplier *=
                        multiplier;

                    RefreshRangeCollider();
                    break;
            }

#if UNITY_EDITOR
            Debug.Log(
                $"{name} 套用 {buffType} Buff，倍率：{multiplier}",
                this
            );
#endif
        }

        public void RemoveBuff(
            PassiveBuffType buffType,
            float multiplier)
        {
            if (multiplier <= 0f)
                return;

            switch (buffType)
            {
                case PassiveBuffType.Damage:
                    damageMultiplier /=
                        multiplier;
                    break;

                case PassiveBuffType.AttackSpeed:
                    attackSpeedMultiplier /=
                        multiplier;
                    break;

                case PassiveBuffType.Range:
                    rangeMultiplier /=
                        multiplier;

                    RefreshRangeCollider();
                    break;
            }

#if UNITY_EDITOR
            Debug.Log(
                $"{name} 移除 {buffType} Buff",
                this
            );
#endif
        }

        private void RefreshRangeCollider()
        {
            if (rangeCollider != null)
            {
                rangeCollider.isTrigger = true;
                rangeCollider.radius =
                    AttackRange;
            }

            if (rangeCircle != null)
            {
                rangeCircle.DrawCircle(
                    AttackRange
                );
            }
        }

        #endregion

        #region 外觀系統

        private void UpdateModel()
        {
            if (modelRoot == null)
            {
                Debug.LogWarning(
                    $"{name} 沒有設定 Model Root",
                    this
                );

                return;
            }

            if (data == null ||
                data.levelModelPrefabs == null ||
                data.levelModelPrefabs.Length == 0)
            {
                Debug.LogWarning(
                    $"{name} 沒有設定等級模型 Prefab",
                    this
                );

                return;
            }

            if (currentLevel < 0 ||
                currentLevel >=
                data.levelModelPrefabs.Length)
            {
                Debug.LogError(
                    $"{name} 找不到第 {currentLevel + 1} 級模型",
                    this
                );

                return;
            }

            ClearCurrentModel();

            GameObject prefab =
                data.levelModelPrefabs[
                    currentLevel
                ];

            if (prefab == null)
                return;

            currentModel =
                Instantiate(
                    prefab,
                    modelRoot
                );

            currentModel.transform.localPosition =
                Vector3.zero;

            currentModel.transform.localRotation =
                Quaternion.identity;

            currentModel.transform.localScale =
                Vector3.one;

            CacheModelReferences();
        }

        private void ClearCurrentModel()
        {
            for (int i =
                     modelRoot.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    modelRoot.GetChild(i)
                        .gameObject
                );
            }

            currentModel = null;
            FirePoint = null;
            Head = null;
        }

        private void CacheModelReferences()
        {
            if (currentModel == null)
                return;

            TowerModelRef modelRef =
                currentModel
                    .GetComponentInChildren<
                        TowerModelRef>();

            if (modelRef == null)
            {
                Debug.LogWarning(
                    $"{currentModel.name} 找不到 TowerModelRef",
                    currentModel
                );

                return;
            }

            FirePoint =
                modelRef.FirePoint;

            Head =
                modelRef.Head;
        }

        #endregion

        #region 範圍顯示

        public void ShowRangeCircle()
        {
            SetRangeCircleVisible(
                true
            );
        }

        public void HideRangeCircle()
        {
            SetRangeCircleVisible(
                false
            );
        }

        private void SetRangeCircleVisible(
            bool visible)
        {
            if (rangeCircle != null)
            {
                rangeCircle.gameObject
                    .SetActive(
                        visible
                    );
            }
        }

        #endregion

        #region 回收系統

        public int GetSellPrice()
        {
            if (data == null ||
                data.levels == null)
            {
                return 0;
            }

            int totalCost = 0;

            for (int i = 0;
                 i <= currentLevel &&
                 i < data.levels.Length;
                 i++)
            {
                totalCost +=
                    data.levels[i].cost;
            }

            return Mathf.RoundToInt(
                totalCost * 0.7f
            );
        }

        #endregion

        #region 測試工具

        [ContextMenu("測試塔傷害加成")]
        private void TestTowerDamageBonus()
        {
#if UNITY_EDITOR
            if (data == null)
                return;

            Debug.Log(
                $"塔基礎傷害：{CurrentData.damage}\n" +
                $"英雄光環倍率：{damageMultiplier}\n" +
                $"商店倍率：{ShopBonus.TowerDamageMultiplier}\n" +
                $"最終傷害：{Damage}",
                this
            );
#endif
        }

        #endregion
    }
}