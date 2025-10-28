using UnityEngine;

namespace StateMachineAI
{
    public class Idle_T : State<Titania_T>
    {
        float total;
        float rand;

        //コンストラクタ
        public Idle_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("行動決め待機時間");
            //idle状態になった時の攻撃隙
            owner.m_CoolDown.StartCoolDown("Idle", 2f);

            total = owner.wSpawn + owner.wRush + owner.wBeam + owner.wMove;
            rand = Random.value * total;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //クールタイムがおわっていたら行動を開始
            if (!owner.m_CoolDown.IsCoolDown("Idle"))
            {
                //重みによって確立が変わる
                if (rand < owner.wSpawn)
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
                else if (rand < owner.wSpawn + owner.wRush)
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
                else if (rand < owner.wSpawn + owner.wRush + owner.wBeam)
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