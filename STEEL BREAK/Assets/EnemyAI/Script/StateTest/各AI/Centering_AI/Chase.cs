using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    public class Chase_CenteringAI : State<CenteringAI>
    {
        //コンストラクタ
        public Chase_CenteringAI(CenteringAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_Player.transform, owner.myAgent);

            Debug.Log("追跡開始");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //Flyにy軸回転動を同期
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;

            //追いかける
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //攻撃できる範囲か
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance)
            {
                //攻撃開始
                owner.ChangeState(AIState_CenteringAI.CenterPoint);
                //Debug.Log("戦闘切り替え");
            }

        }
        public override void Exit()
        {

        }
    }
}