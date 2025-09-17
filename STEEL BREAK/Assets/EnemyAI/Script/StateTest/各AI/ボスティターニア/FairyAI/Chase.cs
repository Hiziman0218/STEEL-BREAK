using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //ティターニアを守るフェアリー
    public class Chase_Fairy : State<FairyAI>
    {
        //コンストラクタ
        public Chase_Fairy(FairyAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //エージェントを敵の前に出す
            //owner.myAgent.transform.position = owner.transform.forward;
            int X = Random.Range(-1, 1);
            int Y = Random.Range(-1, 1);
            if (X == 0) X = 1;
            if (Y == 0) Y = 1;

            Vector3 POS = owner.m_Player.position +
                new Vector3((float)X * Random.Range(1.0f, 10.0f), 0, (float)Y * Random.Range(1.0f, 10.0f));
            owner.m_MoveTarget.transform.position = POS;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
            owner.transform.position = owner.m_Detector.transform.position;
            //攻撃できる範囲か
            if (Vector3.Distance(owner.m_MoveTarget.transform.position, owner.transform.position) < owner.m_AttackDistance)
            {
                // 攻撃
                owner.ChangeState(AIState_Fairy.Shot);
            }
        }

        public override void Exit()
        {
        }
    }
}
