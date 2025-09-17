using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineAI
{
    public class Wandering: State<AITester>
    {
        //コンストラクタ
        public Wandering(AITester owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("パトロール開始");
            owner.agent = owner.GetComponent<NavMeshAgent>();
            if (owner.agent == null)
            {
                Debug.LogError("NavMeshAgentが見つからない！");
                return;
            }

            // 初期移動ポイント
            MoveToRandomPoint();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            owner.transform.Rotate(0, 1, 0);


            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_ChaseDistanse)
            {
                {
                    //追いかける
                    owner.ChangeState(AIState_ABType.Chase);
                }
            }

            // エージェントが目的地に到達したら待機
            if (!owner.agent.pathPending && owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                owner.ChangeState(AIState_ABType.Idle);
                Debug.Log("パトロール終了");
            }

        }
        public override void Exit()
        {
            
        }

        //ランダムに座標を指定
        private void MoveToRandomPoint()
        {
            // ランダムな方向を生成
            Vector3 randomDirection = Random.insideUnitSphere * owner.m_PatrolRadius;
            // 現在位置を基準に設定
            randomDirection += owner.transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, owner.m_PatrolRadius, NavMesh.AllAreas))
            {
                // ランダムな位置に移動
                owner.agent.SetDestination(hit.position);
                Debug.Log("新しい目的地: " + hit.position);
            }
        }

    }
}