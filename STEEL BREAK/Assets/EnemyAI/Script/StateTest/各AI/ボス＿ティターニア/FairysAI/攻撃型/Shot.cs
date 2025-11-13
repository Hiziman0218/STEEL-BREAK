using UnityEngine;

namespace StateMachineAI
{
    //ソルジャーミサイルタイプ
    public class Shot_Soldier : State<SoldierFairysAI>
    {
        public float m_TImes;

        //コンストラクタ
        public Shot_Soldier(SoldierFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");
            //プレイヤーの方向に向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            //攻撃処理
            Attack_Shots.Execute(owner.m_Enemy, owner.m_CoolDown, owner.m_CoolTime);
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
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            if (m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Soldier.RandamMove_Soldier);
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