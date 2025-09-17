using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //自爆AIのスタンダード
    public class Bomb_AI : State<BombAI>
    {
        //コンストラクタ
        public Bomb_AI(BombAI owner) : base(owner) { }
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