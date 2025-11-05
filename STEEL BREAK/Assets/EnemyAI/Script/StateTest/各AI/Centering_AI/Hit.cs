using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    public class Hit_CenteringAI : State<CenteringAI>
    {
        //コンストラクタ
        public Hit_CenteringAI(CenteringAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ダメージを受けた");
            //一旦エージェント解除
            if (owner.myAgent.activeSelf)
            {
                PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);
            }

            //HitStunがクールダウン中ならreturn
            if (owner.m_CoolDown.IsCoolDown("HitStun")) return;

            // 0.5秒間は再度Hitに入らない
            owner.m_CoolDown.StartCoolDown("HitStun", 0.5f);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // ヒット硬直
            if (owner.m_CoolDown.IsCoolDown("HitStun"))
                return;

            //ステート移行
            owner.ChangeState(AIState_CenteringAI.Chase);

        }
        public override void Exit()
        {
            //エージェントがアクティブでなければアクティブにする
            if (!owner.myAgent.activeSelf)
            {
                //エージェント再取得
                PoolManager.Instance.Get("FlyingFollowing", owner.transform.position, owner.m_Player);
            }
        }
    }
}