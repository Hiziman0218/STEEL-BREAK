using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class Chase_BombAI : State<BombAI>
    {
        //コンストラクタ
        public Chase_BombAI(BombAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //攻撃可能かのチェック
            (float distance, float direction, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            //Flyに回転動を同期
            owner.transform.rotation = owner.myAgent.transform.rotation;

            //追いかける
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //攻撃できる範囲かつ正面方向にプレイヤーがいたとき
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance && Mathf.Abs(direction) < owner.m_SideDotThreshold)
            {
                //自爆特攻
                owner.ChangeState(AIState_BombAI.Ramming);
            }

        }
        public override void Exit()
        {

        }
    }
}