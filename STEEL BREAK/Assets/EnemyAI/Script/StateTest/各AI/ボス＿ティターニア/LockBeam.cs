using UnityEngine;

namespace StateMachineAI
{
    //プレイヤーにロックオンしてからビームを撃つ
    public class LockBeam_T : State<Titania_T>
    {
        //コンストラクタ
        public LockBeam_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーを緩くロックオン
            PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);
            //ビーム発射

        }
        public override void Exit()
        {

        }
    }
}