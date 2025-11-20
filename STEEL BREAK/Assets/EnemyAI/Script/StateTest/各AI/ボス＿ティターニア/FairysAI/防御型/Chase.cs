using UnityEngine;

namespace StateMachineAI
{
    //ティターニアを守るフェアリー
    public class Chase_Guardian : State<GuardianFairysAI>
    {
        //コンストラクタ
        public Chase_Guardian(GuardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("追いかける");

            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_Player.transform, owner.myAgent);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);
            //追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //攻撃できる範囲か
            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < owner.m_AttackDistance)
            {
                // 攻撃
                owner.ChangeState(AIState_Guardian.Shot);
            }
        }

        public override void Exit()
        {
        }
    }
}
