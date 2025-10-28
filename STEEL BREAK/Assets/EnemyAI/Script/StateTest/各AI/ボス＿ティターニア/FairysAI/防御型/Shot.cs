using UnityEngine;

namespace StateMachineAI
{
    //ガーディアン
    public class Shot_Gyardian : State<GyardianFairysAI>
    {
        public float m_TImes;
        //コンストラクタ
        public Shot_Gyardian(GyardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");

            m_TImes = 3.0f;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {

            //プレイヤーの方向に向く
            owner.transform.LookAt(owner.m_Player);

            //攻撃


            if (m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Gyardian.RandamMove_Gyardian);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
            //エージェントを自分の位置へ戻ってこさせる
            //owner.myAgent.transform = owner.transform.gameObject;
        }
    }
}