using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    public class Idle : State<Titania>
    {
        //コンストラクタ
        public Idle(Titania owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("行動決め待機時間");
            //idle状態になった時の攻撃隙
            //owner.m_CoolDown.StartCoolDown("Idle", 5f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.ChangeState(AIState_Titania.Spawn);
            /*
            //プレイヤーへの緩い追従処理
            PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);

            //クールタイムがおわっていたら行動を開始
            if (!owner.m_CoolDown.IsCoolDown("Idle"))
            {
                //スポーンクールタイムが終わっているかつ半分以下なら
                if (!owner.m_CoolDown.IsCoolDown("Spawn")　&& owner.m_spawnedEnemies.Count < owner.m_MaxFairys / 2)
                {
                    //フェアリー召喚
                    owner.ChangeState(AIState_Titania.Spawn);
                    return;
                }

                owner.ChangeState(AIState_Titania.RushBeam);
            }
            */
        }
        public override void Exit()
        {
        }
    }
}