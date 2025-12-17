using UnityEngine;

namespace StateMachineAI
{
    //ランダムな移動
    public class RandamMove_Soldier : State<SoldierFairysAI>
    {
        //コンストラクタ
        public RandamMove_Soldier(SoldierFairysAI owner) : base(owner) { }
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
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);
            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.m_Controller, owner.m_Player, owner.m_Rigidbody);

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
                        owner.ChangeState(AIState_Soldier.Shot);
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

        }
        public void Chak()
        {
            // 中心を基準にランダムな位置を決める
            Vector3 randomOffset = new Vector3(
                Random.Range(-owner.m_AttackDistance, owner.m_AttackDistance), //ｘ軸
                Random.Range(-3,3),                                            // y軸（高さ）
                Random.Range(-owner.m_AttackDistance, owner.m_AttackDistance)  //ｚ軸
            );

            //次の移動場所を指定
            owner.m_CenterMarker.transform.position = owner.m_Player.position + randomOffset;
        }
    }
}