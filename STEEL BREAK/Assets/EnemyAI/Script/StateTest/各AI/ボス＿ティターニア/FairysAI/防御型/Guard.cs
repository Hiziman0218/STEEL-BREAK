using UnityEngine;

namespace StateMachineAI
{
    //ティターニアを守るフェアリー
    public class Guard_Guardian : State<GuardianFairysAI>
    {
        //コンストラクタ
        public Guard_Guardian(GuardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_GuardPointer.transform, owner.myAgent);

            Debug.Log("ガード");
            //m_GuardPointernullなら守護位置を検索
            if (owner.m_GuardPointer == null)
            {
                owner.ChangeState(AIState_Guardian.CeackGuard);
            }
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //攻撃可能かのチェック
            (float distance, _, _) = Distance_Check.Check(owner.transform, owner.m_Player);
            //守護位置と自分の距離
            (float guarddistance, _, _) = Distance_Check.Check(owner.transform, owner.m_GuardPointer.transform);

            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.m_Controller, owner.m_Player, owner.m_Rigidbody);

            //Flyにy軸回転動を同期
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;

            //一定距離まで近づいたら停止
            if (guarddistance < 2.0f)
            {
                //自分の位置を目的地にして停止させる
                owner.m_Detector.destination = owner.transform;
            }
            else
            {
                //守護位置を目的地にする
                owner.m_Detector.destination = owner.m_GuardPointer.transform;
            }

            // 攻撃範囲内に入れば
            if (distance <= owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_Guardian.Shot);
            }

        }

        public override void Exit()
        {

        }
    }
}