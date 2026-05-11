using UnityEngine;

namespace CHANG
{
    public class TowerAttack : StateTower
    {
        public TowerAttack(string name, StateMachine stateMachine, Tower tower) : base(name, stateMachine, tower)
        {
        }

       
        public override void Enter()
        {
            base.Enter();
        }
        public override void Update()
        {
            base.Update();

            Debug.Log("TowerAttack 狀態運行中");

            if (!tower.HasTarget())
            {
                stateMachine.ChangeState(tower.Idle);
                return;
            }

            Enemy target = tower.GetTarget();

            if (target != null && tower.Head != null)
            {
                Debug.Log($"轉向目標：{target.name}");  // ⭐ 加這行
                Vector3 dir = target.transform.position - tower.Head.position;
                Quaternion rot = Quaternion.LookRotation(dir);
                tower.Head.rotation = Quaternion.Lerp(
                    tower.Head.rotation,
                    rot,
                    10f * Time.deltaTime
                );
            }

            tower.TickAttackTimer();

            if (tower.IsAttackReady())
            {
                if (target != null)
                {
                    Debug.Log("塔攻擊");
                    tower.Fire(target);
                    tower.ResetAttackTimer();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}

