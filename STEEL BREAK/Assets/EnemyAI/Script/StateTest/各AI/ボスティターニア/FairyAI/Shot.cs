using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    //ソルジャーミサイルタイプ
    public class Shot : State<FairyAI>
    {
        public float m_TImes;
        //コンストラクタ
        public Shot(FairyAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            m_TImes = 3.0f;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.m_MoveTarget.transform.position = owner.transform.position;
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
            //プレイヤーの方向に向く
            owner.transform.LookAt(owner.m_Player);
            //攻撃
            if (m_TImes <= 0)
            {
                owner.ChangeState(AIState_Fairy.RandamMove);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {

        }
    }
}