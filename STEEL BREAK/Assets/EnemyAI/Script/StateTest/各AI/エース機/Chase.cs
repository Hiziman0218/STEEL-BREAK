using UnityEngine;

namespace StateMachineAI
{
    //ティターニアを守るフェアリー
    public class Chase_Ase : State<AseAI>
    {
        //コンストラクタ
        public Chase_Ase(AseAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("追いかける");

            //エージェントを解除して取得していなければ
            if (owner.myAgent == null || !owner.myAgent.activeInHierarchy)
            {
                //エージェントがnullなら再取得する
                owner.myAgent = PoolManager.Instance.Get("FlyingFollowing", owner.transform.position, owner.m_CenterMarker.transform);
            }

            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);

            int X = Random.Range(-1, 1);
            int Y = Random.Range(-1, 1);
            if (X == 0) X = 1;
            if (Y == 0) Y = 1;

            Vector3 POS = owner.m_Player.position +
                new Vector3((float)X * Random.Range(1.0f, 10.0f), 0, (float)Y * Random.Range(1.0f, 10.0f));
            owner.m_CenterMarker.transform.position = POS;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);
            //追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.m_Controller, owner.m_Player, owner.m_Rigidbody);

            //攻撃できる範囲か
            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < owner.m_AttackDistance)
            {
                // ランダム行動へ移行
                owner.ChangeState(AIState_Ase.RandamMove);
            }
        }

        public override void Exit()
        {
        }
    }
}
