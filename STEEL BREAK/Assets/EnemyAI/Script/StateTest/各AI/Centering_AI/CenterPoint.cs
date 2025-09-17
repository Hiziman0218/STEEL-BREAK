using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

namespace StateMachineAI
{
    public class CenterPoint : State<CenteringAI>
    {
        //コンストラクタ
        public CenterPoint(CenteringAI owner) : base(owner) { }


        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーの周りをセンターポインターがグルグル回る
            Centering.CenterPoint(owner.m_CenterMarker, owner.transform, owner.m_Player, owner.m_Muki, owner.m_AttackDistance);

            //追いかける
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //プレイヤーへ向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            //回転クールタイムじゃなければ
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("siderot"))
            {
                //確率で回転方向の変更
                if (Random.Range(0, 100) > 90)
                {
                    owner.m_Muki *= -1;
                    owner.m_CoolDown.StartCoolDown("siderot", Random.Range(3, 10));
                }
            }

            // ランダムで攻撃する
            if (Random.Range(0, 100) > 95)
            {
                owner.ChangeState(AIState_CenteringAI.Attack);
            }

        }
        public override void Exit()
        {
        }
    }
}