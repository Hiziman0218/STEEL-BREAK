using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace StateMachineAI
{
    //ガーディアン
    public class Shot_Gyardian : State<GyardianFairysAI>
    {
        public float m_TImes;
        private int shots;
        //コンストラクタ
        public Shot_Gyardian(GyardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");

            m_TImes = 3.0f;
            shots = Random.Range(1, owner.m_MaxRange);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {

            //プレイヤーの方向に向く
            owner.transform.LookAt(owner.m_Player);

            //攻撃
            owner.StartCoroutine(Attack_Shots.ShotRandom(owner.m_Enemy, owner.m_CoolDown, owner.m_CoolTime, shots, 0.2f));


            if (m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Gyardian.RandamMove);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
        }
    }
}