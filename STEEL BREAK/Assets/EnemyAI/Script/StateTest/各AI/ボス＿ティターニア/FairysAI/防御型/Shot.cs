using UnityEngine;

namespace StateMachineAI
{
    //ガーディアン
    public class Shot_Guardian : State<GuardianFairysAI>
    {
        public float m_TImes;
        private int shots;
        //コンストラクタ
        public Shot_Guardian(GuardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");
            //プレイヤーの方向に向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            //連続で撃つ回数を決める
            shots = Random.Range(1, owner.m_MaxRange);

            //攻撃(ランダム連射)
            owner.StartCoroutine(Attack_Shots.ShotRandom(owner.m_Enemy, owner.m_CoolDown, owner.m_CoolTime, shots, 0.2f));

            //待機時間
            m_TImes = 3.0f;
            //リジットボディがおかしくならないようにリセット
            owner.m_Rigidbody.linearVelocity = Vector3.zero;
            owner.m_Rigidbody.angularVelocity = Vector3.zero;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーの方向に向く
            owner.transform.LookAt(owner.m_Player);
            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.m_Controller, owner.m_Player, owner.m_Rigidbody);

            if (m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Guardian.RandamMove);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
            //エージェントを自分の位置へ戻ってこさせる
            owner.myAgent.transform.position = owner.transform.position;
        }
    }
}