using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //突進しながら拡散ビーム
    public class RushBeam : State<Titania>
    {
        //コンストラクタ
        public RushBeam(Titania owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("突進ビーム");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.ChangeState(AIState_Titania.Idle);
        }
        public override void Exit()
        {

        }
    }
}