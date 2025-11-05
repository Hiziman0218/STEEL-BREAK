using UnityEngine;

namespace StateMachineAI
{
    //突進しながら拡散ビーム
    public class RushBeam_T : State<Titania_T>
    {
        private Vector3 startPos;
        //コンストラクタ
        public RushBeam_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("突進ビーム");

            //追従飛行を解除
            PoolManager.Instance.Return("Titania", owner.myAgent);

            startPos = owner.transform.position;

            //もし最大速度より低ければ最大速度に
            if (owner.m_RCController.speed < owner.m_maxspeed)
            {
                //最大速度を代入
                owner.m_RCController.speed = owner.m_maxspeed;
            }

            owner.m_CoolDown.StartCoolDown("Lockon", 2f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //ロックオン時間であれば
            if (owner.m_CoolDown.IsCoolDown("Lockon"))
            {
                //プレイヤーへの緩い追従処理
                PlayerLookAt.LookAtFlat(owner.transform, owner.m_Player, owner.m_turnsmooth);
            }
            else
            {
                //加速処理
                owner.m_currentspeed = Acceleration.Smooth(owner.m_currentspeed, owner.m_maxspeed, owner.m_acceleration);

                //まっすぐ進むだけ
                owner.m_currentspeed = Mathf.Lerp(owner.m_currentspeed, owner.m_maxspeed, 1 - Mathf.Exp(owner.m_acceleration * Time.deltaTime));
                owner.transform.position += owner.transform.forward * owner.m_currentspeed * Time.deltaTime;

                //拡散ビーム


                // 移動距離で判定
                float traveled = Vector3.Distance(startPos, owner.transform.position);
                if (traveled >= 80f) // 突進距離
                {
                    owner.ChangeState(AIState_Titania_T.Idle_T);
                }
            }
        }

        public override void Exit()
        {
            //クールタイムを設ける
            //owner.m_CoolDown.StartCoolDown("Rush", 8f);
            //デフォルト速度に戻す
            owner.m_RCController.speed = owner.m_speed;
            //エージェントを再取得
            owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position + owner.transform.forward, owner.m_CenterMarker.transform);
        }
    }
}