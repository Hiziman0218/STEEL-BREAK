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
            //エージェントをいったん返却

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //ビームを撃つ

            //回転しながら上昇

            //一定時間たったらステート変更

        }
        public override void Exit()
        {
            //エージェント再取得
        }
    }
}