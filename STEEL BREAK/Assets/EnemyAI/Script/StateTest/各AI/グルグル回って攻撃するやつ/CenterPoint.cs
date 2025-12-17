using UnityEngine;

namespace StateMachineAI
{
    public class CenterPoint : State<CenteringAI>
    {
        //コンストラクタ
        public CenterPoint(CenteringAI owner) : base(owner) { }
        private float m_RayDistance = 3f;


        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーの周りをセンターポインターがグルグル回る
            Centering.CenterPoint(owner.m_CenterMarker, owner.transform, owner.m_Player, owner.m_Muki, owner.m_AttackDistance);

            //追いかける
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //プレイヤーへ向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            // 壁があるかチェックするために旋回方向ベクトルを計算
            Vector3 dir = owner.transform.right * -owner.m_Muki;

            // デバッグ用スクリプトに渡す
            RayDebugVisualizer visualizer = owner.GetComponent<RayDebugVisualizer>();
            if (visualizer != null)
            {
                visualizer.origin = owner.m_CenterMarker.transform;
                visualizer.direction = dir;
                visualizer.distance = m_RayDistance;
            }

            // 壁チェック
            if (Physics.Raycast(owner.m_CenterMarker.transform.position, dir, out RaycastHit hit, m_RayDistance, LayerMask.GetMask("Field")))
            {
                Debug.Log("壁に当たった");
                //エージェントを持っているキャラクタの位置に戻す
                owner.myAgent.transform.position = owner.transform.position;
                // 旋回方向を反転
                owner.m_Muki *= -1;

                // クールダウン開始
                owner.m_CoolDown.StartCoolDown("siderot", Random.Range(3, 10));

            }
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("siderot"))
            {
                // --- 確率で反転 ---
                if (Random.Range(0, 100) > 90)
                {
                    // 旋回方向を反転
                    owner.m_Muki *= -1;

                    // クールダウン開始
                    owner.m_CoolDown.StartCoolDown("siderot", Random.Range(3, 10));

                }
            }

            // ランダムで攻撃する
            if (Random.Range(0, 100) > 95)
            {
                owner.ChangeState(AIState_CenteringAI.Attack);
            }

        }

        public override void Exit()
        {
        }
    }
}