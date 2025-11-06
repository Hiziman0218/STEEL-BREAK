using UnityEngine;

namespace StateMachineAI
{
    public class Hit_HitAndAwayAI : State<HitAndAwayAI>
    {
        //コンストラクタ
        public Hit_HitAndAwayAI(HitAndAwayAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //一旦エージェント解除
            PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);

            // Rigidbodyの回転を止める
            owner.m_Rigidbody.angularVelocity = Vector3.zero;
            owner.m_Rigidbody.freezeRotation = true;

            //HitStunがクールダウン中ならreturn
            if (owner.m_CoolDown.IsCoolDown("HitStun")) return;

            // 0.5秒間は再度Hitに入らない
            owner.m_CoolDown.StartCoolDown("HitStun", 0.5f);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // ヒット硬直
            if (owner.m_CoolDown.IsCoolDown("HitStun"))
                return;

            //ステート移行
            owner.ChangeState(AIState_HitAndAwayAI.Chase);

        }
        public override void Exit()
        {
            // 回転制御を戻す
            owner.m_Rigidbody.freezeRotation = false;

            //エージェントがアクティブでなければアクティブにする
            if (!owner.myAgent.activeSelf)
            {
                //エージェント再取得
                owner.myAgent = PoolManager.Instance.Get("FlyingFollowing", owner.transform.position, owner.m_Player);
            }
        }
    }
}