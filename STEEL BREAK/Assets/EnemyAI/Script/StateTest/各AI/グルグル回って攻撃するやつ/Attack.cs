using UnityEngine;

namespace StateMachineAI
{
    public class Attack_CenteringAI : State<CenteringAI>
    {
        //コンストラクタ
        public Attack_CenteringAI(CenteringAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
        }

        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //攻撃可能かのチェック
            (float distance, float direction, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            // クールダウン中でないかつ左右どちらかにいて攻撃範囲内なら
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack") && Mathf.Abs(direction) < owner.m_SideDotThreshold && distance <= owner.m_AttackDistance)
            {
                //プレイヤーへ向く
                PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

                //攻撃処理
                // コルーチンとして実行
                owner.StartCoroutine(
                    Attack_Shots.ExecuteRandomBurst(owner.m_Enemy, owner.m_CoolDown, 4f, owner.m_MaxRange, 0.5f)
                );
            }
            //クールダウン中攻撃範囲外なら
            else
            {
                //周りをグルグル回る
                owner.ChangeState(AIState_CenteringAI.CenterPoint);
            }

            //攻撃可能範囲から出た
            if (distance > owner.m_AttackDistance)
            {
                //追跡開始
                owner.ChangeState(AIState_CenteringAI.Chase);
                Debug.Log("追跡");
            }
        }
        public override void Exit()
        {

        }
    }
}