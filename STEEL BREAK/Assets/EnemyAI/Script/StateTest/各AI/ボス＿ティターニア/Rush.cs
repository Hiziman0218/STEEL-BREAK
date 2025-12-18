using UnityEngine;

namespace StateMachineAI
{
    //突進
    public class Rush_T : State<Titania_T>
    {
        private Vector3 startPos;
        private bool isRushing = false;
        private bool reachedSameHeight = false;
        private Vector3 rushDir;

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
                PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);

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

                    // 加速処理
                    owner.m_currentspeed = Mathf.Lerp(
                        owner.m_currentspeed,
                        owner.m_maxspeed,
                        1 - Mathf.Exp(-owner.m_acceleration * Time.deltaTime)
                    );

                    Vector3 targetPos = owner.m_Player.position;
                    // プレイヤー方向ベクトル（XZ）
                    Vector3 flatRaw = new Vector3(
                        targetPos.x - owner.transform.position.x,
                        0,
                        targetPos.z - owner.transform.position.z
                    );

                    // 横方向の影響を弱める係数
                    float xzFactor = 0.8f;

                    // X成分だけ弱める
                    flatRaw.x *= xzFactor;

                    // 正規化して方向ベクトルに
                    Vector3 flatDir = flatRaw.normalized;

                    Vector3 dir;

                    if (!reachedSameHeight)
                    {
                        // 高度差をチェック
                        float heightDiff = Mathf.Abs(owner.transform.position.y - targetPos.y);

                        if (heightDiff <= 10f || !owner.m_CoolDown.IsCoolDown("Lock")) // 10m以内の高さなら直進モードへ
                        {
                            reachedSameHeight = true;
                            // 直進方向をこの時点で固定
                            rushDir = flatDir;
                            dir = rushDir;
                        }
                        else
                        {
                            // 高度がまだ離れている → カーブをかけて降下or上昇
                            float newY = Mathf.Lerp(owner.transform.position.y, targetPos.y, 0.01f);
                            dir = new Vector3(flatDir.x, (newY - owner.transform.position.y), flatDir.z).normalized;
                        }
                    }
                    else
                    {
                        // 直進モード → 開始時に固定した方向で突進
                        dir = rushDir;
                    }

                    // Rigidbody に速度を与える
                    owner.m_Rigidbody.linearVelocity = dir * owner.m_currentspeed;

                    if (dir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        owner.transform.rotation = Quaternion.Slerp(
                            owner.transform.rotation,
                            targetRot,
                            Time.deltaTime * owner.turnSpeed
                        );
                    }

                    // 終了判定（距離 or 時間）
                    float traveled = Vector3.Distance(startPos, owner.transform.position);
                    if (traveled >= 80f || !owner.m_CoolDown.IsCoolDown("Move"))
                    {
                        owner.m_rush.EndRush();
                        owner.m_Rigidbody.linearVelocity = Vector3.zero;
                        // 次回突進に備えてリセット
                        reachedSameHeight = false;
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
            // 開始時点ではまだ方向を固定しない（高度が揃うまで補間する）
            rushDir = Vector3.zero;

            // 突進開始位置を更新
            startPos = owner.transform.position;
            //突進時間を設定
            owner.m_CoolDown.StartCoolDown("Move", 5f);
            //直進モードになるまでの猶予
            owner.m_CoolDown.StartCoolDown("Lock", 3f);
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