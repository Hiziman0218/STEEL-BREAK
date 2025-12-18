using UnityEngine;

namespace StateMachineAI
{
    //突進
    public class Rush_T : State<Titania_T>
    {
        private Vector3 startPos;
        private bool isRushing = false;

        //コンストラクタ
        public Rush_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("突進ビーム");

            //追従飛行を解除
            PoolManager.Instance.Return("Titania", owner.myAgent);
            //突進開始位置を記録
            startPos = owner.transform.position;

            //もし最大速度より低ければ最大速度に
            if (owner.m_Controller.speed < owner.m_maxspeed)
            {
                //最大速度を代入
                owner.m_Controller.speed = owner.m_maxspeed;
            }

            owner.m_CoolDown.StartCoolDown("Lockon", 6f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            if (owner.m_CoolDown.IsCoolDown("Lockon"))
            {
                // プレイヤー方向へ回転
                PlayerLookAt.LookAtFlat(owner.m_Player, owner.transform, owner.m_turnsmooth);

                // プレイヤー方向との角度差をチェック
                Vector3 toPlayer = (owner.m_Player.position - owner.transform.position).normalized;
                float angle = Vector3.Angle(owner.transform.forward, toPlayer);

                // 30度以内なら「向いた」と判定して突進開始
                if (angle < 30f)
                {
                    isRushing = true;
                    StartRush();
                }
            }
            else
            {
                // ロックオンが終わっても突進していなければ開始
                if (!isRushing)
                {
                    StartRush();
                }

                if (isRushing)
                {
                    // Controller の speed と同期
                    owner.m_currentspeed = owner.m_Controller.speed;

                    // まっすぐ進む
                    owner.m_currentspeed = Mathf.Lerp(
                        owner.m_currentspeed,
                        owner.m_maxspeed,
                        1 - Mathf.Exp(owner.m_acceleration * Time.deltaTime)
                    );
                    owner.transform.position += owner.transform.forward * owner.m_currentspeed * Time.deltaTime;

                    // 移動距離で判定
                    float traveled = Vector3.Distance(startPos, owner.transform.position);
                    if (traveled >= 80f) // 突進距離
                    {
                        //当たり判定をオフにする
                        owner.m_rush.EndRush();
                        owner.ChangeState(AIState_Titania_T.Idle_T);
                    }
                }
            }
        }

        private void StartRush()
        {
            isRushing = true;
            //突進の判定をつける
            owner.m_rush.StartRush();
            // 突進開始位置を更新
            startPos = owner.transform.position;
        }

        public override void Exit()
        {
            //クールタイムを設ける
            owner.m_CoolDown.StartCoolDown("Rush", 20f);
            //デフォルト速度に戻す
            owner.m_Controller.speed = owner.m_speed;
            //エージェントを再取得
            owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position + owner.transform.forward, owner.m_CenterMarker.transform);
        }
    }
}