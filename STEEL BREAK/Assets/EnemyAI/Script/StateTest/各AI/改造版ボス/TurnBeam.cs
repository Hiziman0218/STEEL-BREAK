using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //ビームを撃ちつつ回転しながら上昇
    public class TurnBeam_T : State<Titania_T>
    {
        //コンストラクタ
        public TurnBeam_T(Titania_T owner) : base(owner) { }
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