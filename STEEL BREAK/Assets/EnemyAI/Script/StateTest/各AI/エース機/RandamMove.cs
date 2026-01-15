using UnityEngine;

namespace StateMachineAI
{
    //ランダムな移動
    public class RandamMove_Ase : State<AseAI>
    {
        private float avoidSide = 1f; // -1 or +1

        //コンストラクタ
        public RandamMove_Ase(AseAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ランダムに移動");
            //エージェント解除
            PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);

            Chak();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            // 目的地への基本方向
            Vector3 targetPos = owner.m_CenterMarker.transform.position;
            Vector3 dir = (targetPos - owner.transform.position).normalized;

            // 水平距離だけで判定する
            Vector3 enemyXZ = new Vector3(owner.transform.position.x, 0, owner.transform.position.z);
            Vector3 playerXZ = new Vector3(owner.m_Player.position.x, 0, owner.m_Player.position.z);

            float distToPlayer = Vector3.Distance(enemyXZ, playerXZ);

            // プレイヤーを避ける力（距離が近いほど強くなる）
            float avoidRadius = owner.m_AttackDistance * 0.33f;
            if (distToPlayer < avoidRadius)
            {
                dir += owner.m_Player.right * avoidSide * 0.5f;
                dir.Normalize();
            }

            // 実際に動かす
            owner.m_Rigidbody.linearVelocity = dir * owner.m_MoveSpeed;


            //クールタイムでないなら
            if (!owner.m_CoolDown.IsCoolDown("MoveCool"))
            {
                float chance = Random.value; // 0〜1の間のランダムな値

                //攻撃範囲内なら
                if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) <= owner.m_AttackDistance)
                {
                    if (chance < 0.7f)
                    {
                        // 70%の確率で射撃
                        owner.ChangeState(AIState_Ase.Shot);
                    }
                    else
                    {
                        // 残りの確率でランダムな移動
                        Chak();
                    }
                }
                else
                {
                    Chak();
                }

                //クールタイムを設ける
                owner.m_CoolDown.StartCoolDown("MoveCool", 5f);
            }
            else
            {
                //目的地に近くなったら別の場所を指定する
                if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < 3.0f)
                {
                    Chak();
                }
            }

        }

        public override void Exit()
        {
            //リジットボディがおかしくならないようにリセット
            owner.m_Rigidbody.linearVelocity = Vector3.zero;
            owner.m_Rigidbody.angularVelocity = Vector3.zero;
        }

        public void Chak()
        {
            // 今回の回避方向を決める（右 or 左）
            avoidSide = Random.value < 0.5f ? -1f : 1f;

            float minDist = 6f; // プレイヤー周囲の禁止ゾーン半径
            float maxDist = owner.m_AttackDistance;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minDist, maxDist);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                Random.Range(-3f, 3f),
                Mathf.Sin(angle) * radius
            );

            owner.m_CenterMarker.transform.position = owner.m_Player.position + offset;
        }
    }
}