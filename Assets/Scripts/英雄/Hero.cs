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

        public HeroLevelStats CurrentStats => data.levelStats[currentLevel - 1]; // ⭐ 改成public，給UiManager讀取

        // ⭐ 給UiManager訂閱：經驗值/等級變動時通知面板刷新
        public event System.Action OnHeroDataChanged;

        // 被光環影響到的塔，先簡單用 List 存
        private List<Tower> buffedTowers = new List<Tower>();

        // ⭐ 普通攻擊
        private List<Enemy> enemiesInRange = new List<Enemy>();
        private float attackTimer;
        private float AttackInterval => 1f / CurrentStats.attackSpeed;
        private Enemy pendingFireTarget; // ⭐ 等動畫事件觸發時要打誰

        #region 生命週期
        private void Start()
        {
            HeroManager.Instance.RegisterHero(this);
            ApplyPassiveAura(); // 一放下去就套用光環
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
            // 英雄死亡/移除時，記得把光環效果收回
            foreach (var tower in buffedTowers)
            {
                if (tower != null)
                    tower.RemoveBuff(data.passive.buffType, data.passive.buffMultiplier);
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

        // ⭐【自然系英雄】技能1：藤蔓禁錮 —— 範圍暈眩（纏繞），CD短，用來控場拖延
        #region 主動技能1：藤蔓禁錮（暈眩控場）
        public bool CanUseSkill1() => skill1Timer <= 0f;

        public void UseSkill1()
        {
            if (!CanUseSkill1()) return;
            skill1Timer = data.skill1.cooldown;

            var hits = Physics.OverlapSphere(transform.position, data.skill1.radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Enemy>(out var enemy))
                {
                    // data.skill1.value 這裡拿來當「纏繞秒數」用，不是傷害
                    enemy.AddEffect(new StunEffect(enemy, data.skill1.value));
                }
            }
            // 特效：在英雄腳下位置生成，播完自動銷毀
            PlayVFX(data.skill1.vfxPrefab, transform.position);
        }

        public float Skill1CooldownRatio =>
            data.skill1.cooldown <= 0 ? 0 : Mathf.Clamp01(skill1Timer / data.skill1.cooldown);
        #endregion

        // ⭐【自然系英雄】技能2：荊棘爆裂 —— 大範圍高傷害，CD長，用來爆發清怪
        #region 主動技能2：荊棘爆裂（範圍爆發傷害）
        public bool CanUseSkill2() => skill2Timer <= 0f;

        public void UseSkill2()
        {
            if (!CanUseSkill2()) return;
            skill2Timer = data.skill2.cooldown;

            var hits = Physics.OverlapSphere(transform.position, data.skill2.radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Enemy>(out var enemy))
                    enemy.TakeDamage(data.skill2.value);
            }
            // 特效：在英雄腳下位置生成，播完自動銷毀
            PlayVFX(data.skill2.vfxPrefab, transform.position);
        }

        public float Skill2CooldownRatio =>
            data.skill2.cooldown <= 0 ? 0 : Mathf.Clamp01(skill2Timer / data.skill2.cooldown);
        #endregion

        // ⭐【自然系英雄】被動：森林共鳴 —— 只增強「毒塔」跟「火塔」的傷害
        #region 被動技能：森林共鳴（毒塔/火塔傷害光環）
        private void ApplyPassiveAura()
        {
            var hits = Physics.OverlapSphere(transform.position, data.passive.auraRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Tower>(out var tower))
                {
                    if (!IsBuffableTower(tower)) continue;

                    tower.ApplyBuff(data.passive.buffType, data.passive.buffMultiplier);
                    buffedTowers.Add(tower);
                }
            }
        }

        // 判斷這座塔的屬性是否在被動技能的加成清單裡（例如只選 Burn 和 Poison）
        private bool IsBuffableTower(Tower tower)
        {
            var targetTypes = data.passive.targetEffectTypes;

            // 沒設定清單 = 影響全部塔（保留彈性，但自然系英雄應該要在Inspector勾好 Burn 和 Poison）
            if (targetTypes == null || targetTypes.Length == 0)
                return true;

            foreach (var type in targetTypes)
            {
                if (tower.EffectType == type) return true;
            }
            return false;
        }
        #endregion
    }
}