using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineAI
{
    /// <summary>
    /// 攻撃の命令
    /// </summary>
    public class AttackMode : State<AITester>
    {
        //コンストラクタ
        public AttackMode(AITester owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("戦闘開始");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            float distanceToPlayer = Vector3.Distance(owner.m_Player.position, owner.transform.position);

            // 攻撃範囲内なら
            if (distanceToPlayer < owner.m_AttackDistanse)
            {

                /*
                //プレイヤーを正面に捉える
                owner.m_RotMove.LookTaget();
                //適当に動き回る
                owner.m_RondomMove.StartRandomMove();

                //攻撃をする
                if (!owner.m_CoolDown.IsOnCoolDown("Attack"))
                {
                    //攻撃
                    owner.ChangeState(AIState_ABType.Attack);
                }
                */
            }
            else
            {
                owner.ChangeState(AIState_ABType.Chase);
            }
        }
        public override void Exit()
        {
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) > owner.m_AttackDistanse)
            {
                Debug.Log("追撃する！");
            }
            else
            {
                Debug.Log("逃がすか！");
            }
        }
    }
}