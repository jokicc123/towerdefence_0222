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
        #endregion

        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform head;
        [SerializeField] private Animator animator; // ⭐ 攻擊動畫
        public Coroutine attackCoroutine; // ⭐ 攻擊協程，給動畫事件呼叫

        public HeroLevelStats CurrentStats => data.levelStats[currentLevel - 1]; // ⭐ 改成public，給UiManager讀取

        // ⭐ 給UiManager訂閱：經驗值/等級變動時通知面板刷新
        public event System.Action OnHeroDataChanged;

        // 被光環影響到的塔，先簡單用 List 存
        private List<Tower> buffedTowers = new List<Tower>();
        [Header("被動光環更新")]
        [SerializeField] private float auraUpdateInterval = 0.5f;
        private Coroutine auraCoroutine;

        // ⭐ 普通攻擊
        private List<Enemy> enemiesInRange = new List<Enemy>();
        private float attackTimer;
        private float AttackInterval => 1f / CurrentStats.attackSpeed;
        private Enemy pendingFireTarget; // ⭐ 等動畫事件觸發時要打誰

        #region 生命週期

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }
        private void Start()
        {
            if (data == null)
            {
                Debug.LogError(
                    $"{name} 沒有設定 HeroData，請到 Inspector 拖入資料",
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

            // 每隔一段時間更新光環
            auraCoroutine = StartCoroutine(UpdatePassiveAuraRoutine());

            OnHeroDataChanged?.Invoke();
        }

        private void Update()
        {
            if (skill1Timer > 0f) skill1Timer -= Time.deltaTime;
            if (skill2Timer > 0f) skill2Timer -= Time.deltaTime;

            UpdateEnemiesInRange();
            HandleAttack();
        }
        private void OnDestroy()
        {
            CancelInvoke();

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            if (auraCoroutine != null)
            {
                StopCoroutine(auraCoroutine);
                auraCoroutine = null;
            }

            // 英雄消失時，移除所有塔的光環 Buff
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

            Collider[] hits = Physics.OverlapSphere(transform.position, CurrentStats.range);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Enemy enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        private void HandleAttack()
        {
            enemiesInRange.RemoveAll(e => e == null);
            if (enemiesInRange.Count == 0) return;

            RotateBodyToTarget(enemiesInRange[0]); // ⭐ 整個身體轉向敵人
            RotateHeadToTarget(enemiesInRange[0]); // ⭐ Head再多轉一點做細部瞄準

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                Debug.Log("準備播放攻擊動畫");
                pendingFireTarget = enemiesInRange[0];

                if (animator != null)
                {
                    animator.SetTrigger("Attack"); // ⭐ 播動畫，真正發射交給動畫事件
                }
                else
                {
                    // 沒掛Animator就退回舊行為：立刻發射，避免完全打不出去
                    Fire(pendingFireTarget);
                }

                attackTimer = AttackInterval;
            }
        }

        // ⭐ 給Animation Event呼叫：在丟石頭動畫「甩出去」那一幀觸發
        // 在Animator的Attack動畫片段上，於甩手瞬間新增Animation Event，
        // Function選這個方法名稱：FireFromAnimationEvent
        public void FireFromAnimationEvent()
        {
            if (pendingFireTarget == null || pendingFireTarget.Equals(null)) return; // 敵人可能已經死亡/被銷毀
            Fire(pendingFireTarget);
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
            if (data.bulletPrefab == null || firePoint == null)
            {
                Debug.LogWarning($"{data.heroName} 無法攻擊，缺少 bulletPrefab 或 firePoint");
                return;
            }

            GameObject bulletObj = Instantiate(data.bulletPrefab, firePoint.position, firePoint.rotation);
            if (bulletObj.TryGetComponent(out Bullet b))
            {
                // ⚠️ TowerEffectType.None 是假設值，如果你的enum沒有這個選項，換成你實際的「無效果」值
                b.SetTarget(target, CurrentStats.damage, TowerEffectType.None, 0f, 0f, 0f);
            }


        }
        #endregion

        #region 特效播放共用方法
        // ⭐ 生成特效並依ParticleSystem的實際時長自動銷毀，避免場上堆積殘留物件
        private void PlayVFX(GameObject vfxPrefab, Vector3 position)
        {
            if (vfxPrefab == null) return; // 沒指定特效就跳過，不報錯

            GameObject vfxObj = Instantiate(vfxPrefab, position, Quaternion.identity);

            if (vfxObj.TryGetComponent<ParticleSystem>(out var ps))
            {
                float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(vfxObj, lifetime);
            }
            else
            {
                // 沒有ParticleSystem（例如是純動畫或Prefab本身有自帶銷毀腳本）就給個保底時間
                Destroy(vfxObj, 3f);
            }
        }
        #endregion

        #region 經驗值 / 升級
        public void GainXP(int amount)
        {
            if (currentLevel >= data.levelStats.Length) return; // 已滿級

            currentXP += amount;
            while (currentLevel <= data.levelStats.Length &&
                   currentXP >= CurrentStats.xpToNextLevel)
            {
                currentXP -= CurrentStats.xpToNextLevel;
                currentLevel++;
                OnLevelUp();

                if (currentLevel > data.levelStats.Length)
                {
                    currentLevel = data.levelStats.Length; // 封頂
                    break;
                }
            }

            OnHeroDataChanged?.Invoke(); // ⭐ 不管有沒有升級，經驗值都變了，通知面板刷新
        }

        private void OnLevelUp()
        {
            // TODO: 播放升級特效、更新UI等級條
            Debug.Log($"{data.heroName} 升到 {currentLevel} 級：{CurrentStats.unlockDescription}");
        }
        #endregion

        // ⭐【自然系英雄】技能1：荊棘蔓延 —— 範圍暈眩（纏繞），CD短，用來控場拖延
        #region 主動技能1：荊棘蔓延（暈眩控場）
        public bool CanUseSkill1() => skill1Timer <= 0f;

        public void UseSkill1()
        {
            if (!CanUseSkill1())
                return;

            skill1Timer = data.skill1.cooldown;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                data.skill1.radius
            );

            HashSet<Enemy> affectedEnemies =
                new HashSet<Enemy>();

            foreach (Collider hit in hits)
            {
                Enemy enemy =
                    hit.GetComponentInParent<Enemy>();

                if (enemy == null)
                    continue;

                // 已經處理過這隻敵人就跳過
                if (!affectedEnemies.Add(enemy))
                    continue;

                enemy.AddEffect(
                    new StunEffect(
                        enemy,
                        data.skill1.value
                    )
                );

                Vector3 effectPosition =
                    enemy.transform.position;

                PlayVFX(
                    data.skill1.vfxPrefab,
                    effectPosition
                );
            }
        }

        public float Skill1CooldownRatio =>
            data.skill1.cooldown <= 0 ? 0 : Mathf.Clamp01(skill1Timer / data.skill1.cooldown);
        #endregion

        #region 主動技能2：樹人降臨

        public bool CanUseSkill2()
        {
            return skill2Timer <= 0f;
        }

        public void UseSkill2()
        {
            if (!CanUseSkill2())
            {
                Debug.Log(
                    $"樹人降臨冷卻中：{skill2Timer:0.0} 秒"
                );

                return;
            }

            if (data == null)
            {
                Debug.LogWarning("英雄沒有設定 HeroData");
                return;
            }

            if (data.skill2.summonPrefab == null)
            {
                Debug.LogWarning(
                    "樹人降臨沒有設定 Summon Prefab"
                );

                return;
            }

            Vector3 spawnPosition =
                transform.position +
                transform.forward * 2.5f;

            GameObject summonedObject = Instantiate(
                data.skill2.summonPrefab,
                spawnPosition,
                transform.rotation
            );

            SummonedCreature creature =
                summonedObject.GetComponent<SummonedCreature>();

            if (creature == null)
            {
                creature =
                    summonedObject.GetComponentInChildren<SummonedCreature>();
            }

            if (creature == null)
            {
                Debug.LogError(
                    "樹人 Prefab 沒有 SummonedCreature 腳本"
                );

                Destroy(summonedObject);
                return;
            }

            creature.Initialize(
                data.skill2.value,
                data.skill2.duration,
                data.skill2.summonAttackSpeed,
                data.skill2.radius
            );

            PlayVFX(
                data.skill2.vfxPrefab,
                spawnPosition
            );

            // 確定召喚成功後才進入冷卻
            skill2Timer = data.skill2.cooldown;

            Debug.Log("樹人降臨施放成功");
        }

        #endregion





        public float Skill2CooldownRatio =>
            data.skill2.cooldown <= 0 ? 0 : Mathf.Clamp01(skill2Timer / data.skill2.cooldown);


        // ⭐【自然系英雄】被動：森林共鳴 —— 只增強「毒塔」跟「火塔」的傷害
        #region 被動技能：森林共鳴

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

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                transform.position,
                data.passive.auraRadius
            );
        }
        #endregion
     } 

}