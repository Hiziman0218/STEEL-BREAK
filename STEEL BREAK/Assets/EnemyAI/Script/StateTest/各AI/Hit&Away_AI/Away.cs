using UnityEngine;

namespace StateMachineAI
{
    public class Away : State<HitAndAwayAI>
    {
        private Vector3 retreatTarget;

        //コンストラクタ
        public Away(HitAndAwayAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("離脱開始");
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーとの距離チェック
            (float distance, _,  _) = Distance_Check.Check(owner.transform, owner.m_Player);
            
            //Flyの回転動きを同期
            owner.transform.rotation = owner.myAgent.transform.rotation;

            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            // 攻撃可能範囲からでたら
            if (distance > owner.m_AttackDistance)
            {
                //旋回してプレイヤーの方へ向きなおす
                owner.ChangeState(AIState_HitAndAwayAI.Chase);
            }

        }
        public override void Exit()
        {
        }
    }
}