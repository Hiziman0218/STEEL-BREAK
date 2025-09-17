using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    //突進
    public class RandamMove : State<FairyAI>
    {
        //コンストラクタ
        public RandamMove(FairyAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Chak();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.transform.position = owner.m_Detector.transform.position;

            if (Vector3.Distance(owner.m_MoveTarget.transform.position, owner.transform.position) < 3.0f)
            {
                float chance = Random.value; // 0〜1の間のランダムな値

                if (chance < 0.3f)
                {
                    // 30%の確率で射撃
                    owner.ChangeState(AIState_Fairy.Shot);
                }
                else if (chance < 0.6f)
                {
                    // 30%で追いかける
                    owner.ChangeState(AIState_Fairy.Chase_Fairy);
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
            owner.m_MoveTarget.transform.Translate(new Vector3(10, 10, 10));
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
        }
    }
}