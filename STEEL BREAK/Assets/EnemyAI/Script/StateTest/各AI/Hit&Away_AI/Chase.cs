using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    public class Chase : State<HitAndAwayAI>
    {
        //コンストラクタ
        public Chase(HitAndAwayAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_Player.transform, owner.myAgent);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            if (!owner.m_Player)
                return;

            //プレイヤーとの距離のチェック
            (float distance, _, float direction) = Distance_Check.Check(owner.transform, owner.m_Player);

            //Flyの回転動きを同期
            owner.transform.rotation = owner.myAgent.transform.rotation;

            //追いかける
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //攻撃できる範囲か
            if (distance < owner.m_AttackDistance && direction > owner.m_forwardDotThreshold)
            {
                //攻撃開始
                owner.ChangeState(AIState_HitAndAwayAI.Attack);
            }

        }
        public override void Exit()
        {

        }
    }
}