using UnityEngine;

namespace StateMachineAI
{
    public class Idle_T : State<Titania_T>
    {
        //コンストラクタ
        public Idle_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("行動決め待機時間");
            //idle状態になった時の攻撃隙
            owner.m_CoolDown.StartCoolDown("Idle", 2f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //クールタイムがおわっていたら行動を開始
            if (!owner.m_CoolDown.IsCoolDown("Idle"))
            {
                // 行動ごとの重み
                float wSpawn = owner.m_probSpawn;
                float wRush = owner.m_probRush;
                float wBeam = owner.m_probBeam;
                float wMove = owner.m_probMove;

                float total = wSpawn + wRush + wBeam + wMove;
                float rand = Random.value * total;

                //重みによって確立が変わる
                if (rand < wSpawn)
                {
                    if (!owner.m_CoolDown.IsCoolDown("Spawn"))
                    {
                        owner.ChangeState(AIState_Titania_T.Spawn_T);
                    }
                    else
                    {
                        owner.ChangeState(AIState_Titania_T.RandomMove_T);
                    }
                }
                else if (rand < wSpawn + wRush)
                {
                    if (!owner.m_CoolDown.IsCoolDown("Rush"))
                    {
                        owner.ChangeState(AIState_Titania_T.RushBeam_T);
                    }
                    else
                    {
                        owner.ChangeState(AIState_Titania_T.RandomMove_T);
                    }
                }
                else if (rand < wSpawn + wRush + wBeam)
                {
                    if (!owner.m_CoolDown.IsCoolDown("Turn"))
                    {
                        owner.ChangeState(AIState_Titania_T.TurnBeam_T);
                    }
                    else
                    {
                        owner.ChangeState(AIState_Titania_T.RandomMove_T);
                    }
                }
                else
                {
                    owner.ChangeState(AIState_Titania_T.RandomMove_T);
                }
            }
        }

        public override void Exit()
        {
        }
    }
}