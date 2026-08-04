using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CHANG.TowerData;

namespace CHANG
{
    public class Hero : MonoBehaviour
    {
        public HeroData data;

        [Header("Runtime")]
        public int currentLevel = 1;
        public int currentXP = 0;

        #region 技能冷卻計時
        private float skill1Timer;
        private float skill2Timer;
        private Coroutine sunCoroutine;
        private bool sunBurnBuffActive;
        private GameObject currentSunVFX;
        #endregion

        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform head;
        [SerializeField] private Animator animator; // ⭐ 攻擊動畫
        [Header("特效生成位置")]
        [SerializeField] private Transform normalAttackVFXPoint;
        [SerializeField] private Transform deathFlameVFXPoint;


        // ⭐ 給UiManager訂閱：經驗值/等級變動時通知面板刷新
        public event System.Action OnHeroDataChanged;

        // 被光環影響到的塔，先簡單用 List 存
        private List<Tower> buffedTowers = new List<Tower>();
        [Header("被動光環更新")]
        [SerializeField] private float auraUpdateInterval = 0.5f;
        private Coroutine auraCoroutine;

        public float FinalDamage => CurrentStats.damage * ShopBonus.HeroDamageMultiplier;
        // ⭐ 普通攻擊
        private List<Enemy> enemiesInRange = new List<Enemy>();
        private float attackTimer;
        private Enemy pendingFireTarget; // ⭐ 等動畫事件觸發時要打誰
        private float AttackInterval
        {
            get
            {
                float attackSpeed =
                    Mathf.Max(
                        0.01f,
                        CurrentStats.attackSpeed
                    );

                return 1f / attackSpeed;
            }
        }

        public HeroLevelStats CurrentStats
        {
            get
            {
                if (data == null ||
                    data.levelStats == null ||
                    data.levelStats.Length == 0)
                {
                    return default;
                }

                int safeIndex = Mathf.Clamp(
                    currentLevel - 1,
                    0,
                    data.levelStats.Length - 1
                );

                return data.levelStats[safeIndex];
            }
        }
        #region Unity 生命週期

        private void Awake()
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

        private void Start()
        {
            if (data == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 HeroData",
                    this
                );

                enabled = false;
                return;
            }

            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.RegisterHero(this);
            }

            if (UiManager.Instance != null)
            {
                UiManager.Instance.SetActiveHero(this);
            }

            auraCoroutine = StartCoroutine(
                UpdatePassiveAuraRoutine()
            );

            OnHeroDataChanged?.Invoke();
        }

        private void Update()
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

            UpdateEnemiesInRange();
            HandleAttack();
        }

        private void OnDestroy()
        {
            StopSunOfNoon();

            if (auraCoroutine != null)
            {
                StopCoroutine(auraCoroutine);
                auraCoroutine = null;
            }

            RemoveAllPassiveBuffs();

            if (UiManager.Instance != null)
            {
                UiManager.Instance.ClearActiveHero(this);
            }
        }

        #endregion


        #region 普通攻擊
        private void UpdateEnemiesInRange()
        {
            enemiesInRange.Clear();

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                CurrentStats.range
            );

            HashSet<Enemy> foundEnemies =
                new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                if (foundEnemies.Add(enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }

            // 優先打最近的敵人
            enemiesInRange.Sort((a, b) =>
            {
                float distanceA =
                    (a.transform.position -
                     transform.position).sqrMagnitude;

                float distanceB =
                    (b.transform.position -
                     transform.position).sqrMagnitude;

                return distanceA.CompareTo(distanceB);
            });
        }
        #endregion

        private void HandleAttack()
        {
            attackTimer -= Time.deltaTime;

            enemiesInRange.RemoveAll(
                enemy => enemy == null
            );

            if (enemiesInRange.Count == 0)
                return;

            Enemy target = enemiesInRange[0];

            RotateBodyToTarget(target);
            RotateHeadToTarget(target);

            if (attackTimer > 0f)
                return;

            pendingFireTarget = target;

            if (animator != null)
            {
               
                animator.SetTrigger("Attack");
                StartCoroutine(CheckAnimatorAfterAttack());
            }
            else
            {
                Fire(pendingFireTarget);
                pendingFireTarget = null;
            }

            attackTimer = AttackInterval;
        }



        // Function選這個方法名稱：FireFromAnimationEvent
        public void FireFromAnimationEvent()
        {
            if (data == null)
                return;

            Enemy target = pendingFireTarget;

            // 清掉目標，避免同一個動畫內有兩個 Event 時重複傷害
            pendingFireTarget = null;

            if (target == null)
                return;

            if (SoundManager.Instance != null &&
                data.attackSFX != null)
            {
                SoundManager.Instance.PlaySFX(
                    data.attackSFX
                );
            }

            PlayNormalAttackVFX();

            Fire(target);
        }
        private void PlayNormalAttackVFX()
        {
            if (data == null ||
                data.normalAttackVFX == null)
            {
                return;
            }

            if (firePoint == null)
            {
                Debug.LogWarning(
                    $"{data.heroName} 沒有設定 FirePoint",
                    this
                );

                return;
            }
            GameObject vfx = Instantiate(
                data.normalAttackVFX,
                firePoint.position,
                firePoint.rotation,
                firePoint
            );

            Destroy(vfx, 3f);
        }

        // ⭐ 讓整個英雄身體朝向敵人平滑轉動
        private void RotateBodyToTarget(Enemy target)
        {
            if (target == null) return;

            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f; // 只在水平面轉動，避免英雄歪掉

            if (direction == Vector3.zero) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        // ⭐ 讓Head朝向敵人平滑轉動（比照Enemy.cs的Move()裡用Slerp轉向的寫法）
        private void RotateHeadToTarget(Enemy target)
        {
            if (head == null || target == null) return;

            Vector3 direction = target.transform.position - head.position;
            direction.y = 0f; // 只在水平面轉動，避免Head歪掉

            if (direction == Vector3.zero) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            head.rotation = Quaternion.Slerp(head.rotation, lookRotation, 10f * Time.deltaTime);
        }

        private void Fire(Enemy target)
        {
            if (target == null || data == null)
                return;

            switch (data.attackType)
            {
                case HeroAttackType.Ranged:
                    FireRanged(target);
                    break;

                case HeroAttackType.Melee:
                    FireMelee(target);
                    break;
            }


        }

        #region 攻擊方式
        // ⭐ 遠程攻擊：直接對敵人造成傷害
        private void FireRanged(Enemy target)
        {
            if (data.bulletPrefab == null ||
                firePoint == null)
            {
                Debug.LogWarning(
                    $"{data.heroName} 缺少 Bullet Prefab 或 Fire Point",
                    this
                );

                return;
            }

            Vector3 direction =
                target.transform.position - firePoint.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                firePoint.rotation =
                    Quaternion.LookRotation(direction.normalized);
            }

            GameObject bulletObject = Instantiate(
                data.bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.SetTarget(
                    target,
                    FinalDamage,
                    TowerEffectType.None,
                    0f,
                    0f,
                    0f
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{bulletObject.name} 沒有 Bullet 腳本",
                    bulletObject
                );

                Destroy(bulletObject);
            }
        }
        // ⭐ 近戰攻擊：在英雄前方生成一個球形範圍，對範圍內的敵人造成傷害
        private void FireMelee(Enemy target)
        {
            if (target == null)
                return;

            // 攻擊判定中心位於英雄前方
            Vector3 hitCenter =
                transform.position +
                transform.forward * data.meleeHitOffset;

            Collider[] hits = Physics.OverlapSphere(
                hitCenter,
                data.meleeHitRadius
            );

            HashSet<Enemy> damagedEnemies =
                new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                // 防止同一隻敵人的多個 Collider 重複受傷
                if (!damagedEnemies.Add(enemy))
                    continue;

                enemy.TakeDamage(FinalDamage);

                // 火焰劍士普通攻擊附加燃燒
                if (data.normalAttackBurn)
                {
                    enemy.AddEffect(
                        new BurnEffect(
                            enemy,
                            data.burnDuration,
                            data.burnDamagePerSecond
                        )
                    );
                }

                Debug.Log(
                    $"{data.heroName} 近戰命中 {enemy.name}，" +
                    $"造成 {FinalDamage:0.0} 傷害",
                    enemy
                );
            }
        }
        #endregion
        #region 特效播放共用方法
        // ⭐ 生成特效並依ParticleSystem的實際時長自動銷毀，避免場上堆積殘留物件
        private void PlayVFX(
            GameObject vfxPrefab,
            Vector3 position)
        {
            if (vfxPrefab == null)
                return;

            GameObject vfxObject = Instantiate(
                vfxPrefab,
                position,
                Quaternion.identity
            );

            ParticleSystem[] particleSystems =
                vfxObject.GetComponentsInChildren<ParticleSystem>();

            float lifetime = 3f;

            foreach (ParticleSystem particle in particleSystems)
            {
                ParticleSystem.MainModule main =
                    particle.main;

                float particleLifetime =
                    main.duration +
                    main.startLifetime.constantMax;

                lifetime = Mathf.Max(
                    lifetime,
                    particleLifetime
                );
            }

            Destroy(vfxObject, lifetime);
        }
        #endregion

        #region 經驗值 / 升級
        public void GainXP(int amount)
        {
            if (data == null ||
                data.levelStats == null ||
                data.levelStats.Length == 0)
            {
                return;
            }

            int maxLevel = data.levelStats.Length;

            // 已經滿級就不再增加經驗
            if (currentLevel >= maxLevel)
            {
                currentLevel = maxLevel;
                currentXP = 0;
                OnHeroDataChanged?.Invoke();
                return;
            }

            currentXP += amount;

            while (currentLevel < maxLevel)
            {
                int requiredXP =
                    data.levelStats[currentLevel - 1].xpToNextLevel;

                if (requiredXP <= 0)
                {
                    Debug.LogWarning(
                        $"Lv.{currentLevel} 的 XP To Next Level 必須大於 0"
                    );
                    break;
                }

                if (currentXP < requiredXP)
                    break;

                currentXP -= requiredXP;

                // 先升級
                currentLevel++;

                // currentLevel 現在一定不會超過 maxLevel
                OnLevelUp();

                // 升到最高級後停止
                if (currentLevel >= maxLevel)
                {
                    currentLevel = maxLevel;
                    currentXP = 0;
                    break;
                }
            }

            OnHeroDataChanged?.Invoke();
        }
        private void OnLevelUp()
        {
            if (data == null ||
                data.levelStats == null ||
                data.levelStats.Length == 0)
            {
                return;
            }

            int safeIndex = Mathf.Clamp(
                currentLevel - 1,
                0,
                data.levelStats.Length - 1
            );

            HeroLevelStats stats =
                data.levelStats[safeIndex];

            Debug.Log(
                $"{data.heroName} 升到 {currentLevel} 級：" +
                $"{stats.unlockDescription}"
            );
        }
        #endregion
        private SkillLevelStats CurrentSkill1Stats
        {
            get
            {
                if (data == null ||
                    data.skill1.levelStats == null ||
                    data.skill1.levelStats.Length == 0)
                {
                    Debug.LogError(
                        $"{name} 的技能1沒有設定 Level Stats",
                        this
                    );

                    return default;
                }

                int index = Mathf.Clamp(
                    currentLevel - 1,
                    0,
                    data.skill1.levelStats.Length - 1
                );

                return data.skill1.levelStats[index];
            }
        }

        private SkillLevelStats CurrentSkill2Stats
        {
            get
            {
                if (data == null ||
                    data.skill2.levelStats == null ||
                    data.skill2.levelStats.Length == 0)
                {
                    Debug.LogError(
                        $"{name} 的技能2沒有設定 Level Stats",
                        this
                    );

                    return default;
                }

                int index = Mathf.Clamp(
                    currentLevel - 1,
                    0,
                    data.skill2.levelStats.Length - 1
                );

                return data.skill2.levelStats[index];
            }
        }

        #region 技能共用

        private bool HasSkillStats(
            ActiveSkillData skill)
        {
            return skill.levelStats != null &&
                   skill.levelStats.Length > 0;
        }

        public bool CanUseSkill1()
        {
            return data != null &&
                   HasSkillStats(data.skill1) &&
                   currentLevel >= data.skill1.unlockLevel &&
                   skill1Timer <= 0f;
        }

        public bool CanUseSkill2()
        {
            return data != null &&
                   HasSkillStats(data.skill2) &&
                   currentLevel >= data.skill2.unlockLevel &&
                   skill2Timer <= 0f;
        }

        public void UseSkill1()
        {
            if (!CanUseSkill1())
                return;

            SkillLevelStats stats =
                CurrentSkill1Stats;

            if (ExecuteSkill(
                data.skill1,
                stats))
            {
                skill1Timer =
                    Mathf.Max(
                        0f,
                        stats.cooldown
                    );
            }
        }

        public void UseSkill2()
        {
            if (!CanUseSkill2())
                return;

            SkillLevelStats stats =
                CurrentSkill2Stats;

            if (ExecuteSkill(
                data.skill2,
                stats))
            {
                skill2Timer =
                    Mathf.Max(
                        0f,
                        stats.cooldown
                    );
            }
        }

        private bool ExecuteSkill(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            switch (skill.skillType)
            {
                case HeroSkillType.AreaStun:
                    UseAreaStun(skill, stats);
                    return true;

                case HeroSkillType.SummonCreature:
                    return UseSummonSkill(
                        skill,
                        stats
                    );

                case HeroSkillType.FireWall:
                    UseFireWall(skill, stats);
                    return true;

                case HeroSkillType.SunOfNoon:
                    StopSunOfNoon();

                    sunCoroutine = StartCoroutine(
                        UseSunOfNoonRoutine(
                            skill,
                            stats
                        )
                    );

                    return true;

                default:
                    Debug.LogWarning(
                        $"{data.heroName} 的技能類型尚未設定：" +
                        $"{skill.skillType}",
                        this
                    );

                    return false;
            }
        }

        public float Skill1CooldownRatio
        {
            get
            {
                if (data == null ||
                    !HasSkillStats(data.skill1))
                {
                    return 0f;
                }

                float cooldown =
                    CurrentSkill1Stats.cooldown;

                if (cooldown <= 0f)
                    return 0f;

                return Mathf.Clamp01(
                    skill1Timer / cooldown
                );
            }
        }

        public float Skill2CooldownRatio
        {
            get
            {
                if (data == null ||
                    !HasSkillStats(data.skill2))
                {
                    return 0f;
                }

                float cooldown =
                    CurrentSkill2Stats.cooldown;

                if (cooldown <= 0f)
                    return 0f;

                return Mathf.Clamp01(
                    skill2Timer / cooldown
                );
            }
        }

        #endregion
        // ⭐【自然系英雄】技能1：荊棘蔓延 —— 範圍暈眩（纏繞），CD短，用來控場拖延
        #region 主動技能1：荊棘蔓延（暈眩控場）


        private void UseAreaStun(
             ActiveSkillData skill,
             SkillLevelStats stats)
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                stats.radius
            );

            HashSet<Enemy> affectedEnemies =
                new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                if (!affectedEnemies.Add(enemy))
                    continue;

                enemy.AddEffect(
                    new StunEffect(
                        enemy,
                        stats.value
                    )
                );

                PlayVFX(
                    skill.vfxPrefab,
                    enemy.transform.position
                );
            }
        }
        #endregion
        // ⭐【自然系英雄】技能2：樹人降臨 —— 召喚樹人，CD長，用來清場
        #region 主動技能2：樹人降臨

      

        private bool UseSummonSkill(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            if (skill.summonPrefab == null)
            {
                Debug.LogWarning(
                    $"{skill.skillName} 沒有設定 Summon Prefab"
                );

                return false;
            }

            Vector3 spawnPosition =
                transform.position +
                transform.forward * 2.5f;

            GameObject summonedObject = Instantiate(
                skill.summonPrefab,
                spawnPosition,
                transform.rotation
            );

            SummonedCreature creature =
                summonedObject.GetComponentInChildren<SummonedCreature>();

            if (creature == null)
            {
                Debug.LogError(
                    $"{skill.summonPrefab.name} 沒有 SummonedCreature",
                    summonedObject
                );

                Destroy(summonedObject);
                return false;
            }

            creature.Initialize(
                stats.value,
                stats.duration,
                stats.attackSpeed,
                stats.radius
            );

            PlayVFX(
                skill.vfxPrefab,
                spawnPosition
            );

            return true;
        }

        #endregion
        //*火焰劍士技能1：火焰之牆 召喚火焰牆對經過的敵人造成傷害
        #region  主動技能1 火焰之牆
        private void UseFireWall(
          ActiveSkillData skill,
          SkillLevelStats stats)
        {
            float length =
                Mathf.Max(0.1f, stats.length);

            float width =
                Mathf.Max(0.1f, stats.width);

            Vector3 center =
                transform.position +
                transform.forward *
                (length * 0.5f);

            Vector3 halfExtents =
                new Vector3(
                    width * 0.5f,
                    1.5f,
                    length * 0.5f
                );

            Collider[] hits =
                Physics.OverlapBox(
                    center,
                    halfExtents,
                    transform.rotation
                );

            HashSet<Enemy> damagedEnemies =
                new HashSet<Enemy>();

            float skillDamage =
                stats.value *
                ShopBonus.HeroDamageMultiplier;

            float burnDps =
                stats.damagePerSecond *
                ShopBonus.HeroDamageMultiplier;

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                // 防止同一隻怪有多個 Collider 時重複傷害
                if (!damagedEnemies.Add(enemy))
                    continue;

                enemy.TakeDamage(skillDamage);

                if (stats.duration > 0f &&
                    burnDps > 0f)
                {
                    enemy.AddEffect(
                        new BurnEffect(
                            enemy,
                            stats.duration,
                            burnDps
                        )
                    );
                }

                // 在被命中的怪物身上生成特效
                if (skill.vfxPrefab != null)
                {
                    Vector3 vfxPosition =
                        enemy.transform.position +
                        Vector3.up * 1f;

                    GameObject vfxObject =
                        Instantiate(
                            skill.vfxPrefab,
                            vfxPosition,
                            Quaternion.identity
                        );

                    Destroy(vfxObject, 3f);
                }
            }

            Debug.Log(
                $"死亡火焰：" +
                $"命中 {damagedEnemies.Count} 隻敵人，" +
                $"傷害 {skillDamage:0.0}，" +
                $"長度 {length:0.0}，" +
                $"寬度 {width:0.0}"
            );
        }
        #endregion
        //*火焰劍士技能2：正午的太陽 —— 召喚太陽，持續傷害，造成灼傷傷害加倍
        #region 主動技能2:正午的太陽
        private IEnumerator UseSunOfNoonRoutine(
            ActiveSkillData skill,
            SkillLevelStats stats)
        {
            float duration =
                Mathf.Max(0.1f, stats.duration);

            float radius =
                Mathf.Max(0.1f, stats.radius);

            float damagePerSecond =
                Mathf.Max(
                    0f,
                    stats.damagePerSecond
                ) *
                ShopBonus.HeroDamageMultiplier;

            float burnMultiplier =
                Mathf.Max(1f, stats.multiplier);

            Vector3 sunCenter =
                transform.position +
                transform.forward * 3f;

            Vector3 vfxPosition =
                sunCenter +
                Vector3.up * 4f;

            if (skill.vfxPrefab != null)
            {
                currentSunVFX = Instantiate(
                    skill.vfxPrefab,
                    vfxPosition,
                    Quaternion.identity
                );
            }

            BurnDamageSystem.AddSun(
                burnMultiplier
            );

            sunBurnBuffActive = true;

            Debug.Log(
                $"正午的太陽啟動：" +
                $"持續 {duration:0.0} 秒，" +
                $"每秒傷害 {damagePerSecond:0.0}，" +
                $"燃燒倍率 ×{burnMultiplier:0.##}"
            );

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                Collider[] hits =
                    Physics.OverlapSphere(
                        sunCenter,
                        radius
                    );

                HashSet<Enemy> damagedEnemies =
                    new HashSet<Enemy>();

                foreach (Collider hit in hits)
                {
                    Enemy enemy =
                        hit.GetComponentInParent<Enemy>();

                    if (enemy == null)
                        continue;

                    if (!damagedEnemies.Add(enemy))
                        continue;

                    enemy.TakeDamage(
                        damagePerSecond *
                        Time.deltaTime
                    );
                }

                yield return null;
            }

            FinishSunOfNoon();
        }



        private void StopSunOfNoon()
        {
            if (sunCoroutine != null)
            {
                StopCoroutine(sunCoroutine);
                sunCoroutine = null;
            }

            FinishSunOfNoon();
        }

        private void FinishSunOfNoon()
        {
            if (sunBurnBuffActive)
            {
                BurnDamageSystem.RemoveSun();
                sunBurnBuffActive = false;
            }

            if (currentSunVFX != null)
            {
                Destroy(currentSunVFX);
                currentSunVFX = null;
            }

            sunCoroutine = null;
        }
        
        #endregion


        // ⭐【自然系英雄】被動：森林之子 —— 增加防禦塔
        #region 被動技能：森林之子

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
            if (data == null)
                return;

            if (data.passive.auraRadius <= 0f)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                data.passive.auraRadius
            );

            // 本次更新真正位於光環內的塔
            HashSet<Tower> towersCurrentlyInAura =
                new HashSet<Tower>();

            foreach (Collider hit in hits)
            {
                if (hit == null)
                    continue;

                Tower tower = hit.GetComponentInParent<Tower>();

                if (tower == null)
                    continue;

                if (!IsBuffableTower(tower))
                    continue;

                towersCurrentlyInAura.Add(tower);

                // 尚未獲得 Buff 才套用，避免每 0.5 秒重複相乘
                if (!buffedTowers.Contains(tower))
                {
                    tower.ApplyBuff(
                        data.passive.buffType,
                        data.passive.buffMultiplier
                    );

                    buffedTowers.Add(tower);

                    Debug.Log(
                        $"光環加入：{data.heroName} → {tower.name}，" +
                        $"類型：{data.passive.buffType}，" +
                        $"倍率：{data.passive.buffMultiplier}",
                        tower
                    );
                }
            }

            // 從後往前檢查已加成的塔
            for (int i = buffedTowers.Count - 1; i >= 0; i--)
            {
                Tower tower = buffedTowers[i];

                // 塔已被銷毀
                if (tower == null)
                {
                    buffedTowers.RemoveAt(i);
                    continue;
                }

                // 塔已經離開光環，移除 Buff
                if (!towersCurrentlyInAura.Contains(tower))
                {
                    tower.RemoveBuff(
                        data.passive.buffType,
                        data.passive.buffMultiplier
                    );

                    Debug.Log(
                        $"光環移除：{data.heroName} → {tower.name}",
                        tower
                    );

                    buffedTowers.RemoveAt(i);
                }
            }
        }
        #endregion

        private bool IsBuffableTower(Tower tower)
        {
            if (tower == null || data == null)
                return false;

            TowerEffectType[] targetTypes =
                data.passive.targetEffectTypes;

            // 清單留空代表全部塔都能受到影響
            if (targetTypes == null || targetTypes.Length == 0)
                return true;

            foreach (TowerEffectType type in targetTypes)
            {
                if (tower.EffectType == type)
                    return true;
            }

            return false;
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
        }
        private void OnDrawGizmosSelected()
        {
            if (data == null)
                return;

            // 被動光環範圍
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                transform.position,
                data.passive.auraRadius
            );

            // 近戰攻擊範圍
            if (data.attackType == HeroAttackType.Melee)
            {
                Vector3 hitCenter =
                    transform.position +
                    transform.forward * data.meleeHitOffset;

                Gizmos.color = Color.red;

                Gizmos.DrawWireSphere(
                    hitCenter,
                    data.meleeHitRadius
                );
            }

        }
        private IEnumerator CheckAnimatorAfterAttack()
        {
            yield return null;

            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            Debug.Log(
                $"{data.heroName} 動畫狀態：" +
                $"短名稱Hash={state.shortNameHash} | " +
                $"完整名稱Hash={state.fullPathHash} | " +
                $"轉場中={animator.IsInTransition(0)} | " +
                $"Layer權重={animator.GetLayerWeight(0)}",
                animator
            );
        }



    }
}