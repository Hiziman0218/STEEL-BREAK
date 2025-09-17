using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //プレイヤーにロックオンしてからビームを撃つ
    public class LockBeam : State<Titania>
    {
        //コンストラクタ
        public LockBeam(Titania owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {

        }
        public override void Exit()
        {

        }
    }
}