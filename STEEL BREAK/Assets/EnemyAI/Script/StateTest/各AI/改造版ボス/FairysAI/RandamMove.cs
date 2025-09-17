using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    //ランダムな移動
    public class RandamMove_Fairys : State<FairysAI>
    {
        //コンストラクタ
        public RandamMove_Fairys(FairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ランダムに移動");

            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);

            Chak();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < 3.0f)
            {
                float chance = Random.value; // 0〜1の間のランダムな値

                if (chance < 0.3f)
                {
                    // 30%の確率で射撃
                    owner.ChangeState(AIState_Fairys.Shot_Fairys);
                }
                else if (chance < 0.6f)
                {
                    // 30%で追いかける
                    owner.ChangeState(AIState_Fairys.Chase_Fairys);
                }
                else
                {
                    // 残り40%でランダムな移動
                    Chak();
                }
            }

        }
        public override void Exit()
        {

        }
        public void Chak()
        {
            /*
            owner.m_MoveTarget.transform.Translate(new Vector3(10, 10, 10));
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
            */
        }
    }
}