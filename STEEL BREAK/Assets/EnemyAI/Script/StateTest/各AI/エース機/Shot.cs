using UnityEngine;

namespace StateMachineAI
{
    //ソルジャーミサイルタイプ
    public class Shot_Ase : State<AseAI>
    {
        //コンストラクタ
        public Shot_Ase(AseAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");
            //プレイヤーの方向に向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            // 右武器で攻撃（単発）
            owner.StartCoroutine(Attack_Shots.ShotRandom(owner.m_Enemy, owner.m_CoolDown, owner.m_CoolTime, owner.m_shots, owner.m_interval));

            //待機時間
            owner.m_TImes = 3.0f;
            //リジットボディがおかしくならないようにリセット
            owner.m_Rigidbody.linearVelocity = Vector3.zero;
            owner.m_Rigidbody.angularVelocity = Vector3.zero;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーの方向に向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            if (owner.m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Ase.RandamMove);
            }
            else
            {
                owner.m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
            //エージェントを自分の位置へ戻ってこさせる
            owner.myAgent.transform.position = owner.transform.position;
        }
    }
}