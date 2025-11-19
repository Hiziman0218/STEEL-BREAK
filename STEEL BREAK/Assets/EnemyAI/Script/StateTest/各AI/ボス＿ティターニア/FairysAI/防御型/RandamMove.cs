using UnityEngine;

namespace StateMachineAI
{
    //ランダムな移動
    public class RandamMove_Gyardian : State<GyardianFairysAI>
    {
        //コンストラクタ
        public RandamMove_Gyardian(GyardianFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ランダムに移動");

            //追従するターゲットの変更
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);

            Chak();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < 3.0f)
            {
                float chance = Random.value; // 0〜1の間のランダムな値

                if (chance < 0.3f)
                {
                    // 30%の確率で射撃
                    owner.ChangeState(AIState_Gyardian.Shot);
                }
                else if (chance < 0.6f)
                {
                    // 30%で追いかける
                    owner.ChangeState(AIState_Gyardian.Chase);
                }
                else
                {
                    // 残り40%でランダムな移動
                    Chak();
                }
            }

        }
        public override void Exit()
        {

        }
        public void Chak()
        {
            
            owner.m_CenterMarker.transform.Translate(new Vector3(10, 10, 10));            
        }
    }
}