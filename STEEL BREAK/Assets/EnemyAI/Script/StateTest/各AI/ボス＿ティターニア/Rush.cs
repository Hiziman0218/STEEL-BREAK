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
            //フラグ初期化
            isRushing = false;
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

            owner.m_CoolDown.StartCoolDown("RushLockon", 3f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
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

                // 突進中の進行方向を決める処理
                if (!reachedSameHeight)
                {
                    // プレイヤーとの高さ差（絶対値）
                    float heightDiff = Mathf.Abs(owner.transform.position.y - targetPos.y);

                    // プレイヤーとの水平距離（XZ 平面）
                    float horizontalDist = Vector3.Distance(
                        new Vector3(owner.transform.position.x, 0, owner.transform.position.z),
                        new Vector3(targetPos.x, 0, targetPos.z)
                    );

                    // 条件1：高さが十分近い
                    bool heightAligned = heightDiff <= 10f;

                    // 条件2：プレイヤーに十分近づいている
                    bool closeEnough = horizontalDist <= 45f;

                    // 条件3：Lock が切れた（高度補正フェーズの終了タイミング）
                    bool lockExpired = !owner.m_CoolDown.IsCoolDown("Lock");

                    // 高さが合っている AND プレイヤーに近い
                    // または Lock が切れた（安全装置）
                    if ((heightAligned && closeEnough) || lockExpired)
                    {
                        // 直進モードへ移行
                        reachedSameHeight = true;

                        // この瞬間の水平方向を固定（以降は方向を変えない）
                        rushDir = flatDir;

                        // 直進方向を返す
                        dir = rushDir;
                    }
                    else
                    {
                        // 高度補正フェーズ（Lock が生きている間 & 高さが合っていない）
                        // プレイヤーの高さへゆっくり追従する
                        float newY = Mathf.Lerp(owner.transform.position.y, targetPos.y, 0.05f);

                        // 高度補正を含めた方向ベクトル
                        dir = new Vector3(
                            flatDir.x,
                            newY - owner.transform.position.y,
                            flatDir.z
                        ).normalized;
                    }
                }
                else
                {
                    // 直進モード（方向固定）
                    dir = rushDir;
                }

                // Rigidbody に速度を与える
                owner.m_Rigidbody.linearVelocity = dir * owner.m_currentspeed;

                if (dir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    owner.m_Rigidbody.MoveRotation(
                        Quaternion.Slerp(owner.m_Rigidbody.rotation, targetRot, Time.deltaTime * owner.turnSpeed)
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
            else if (owner.m_CoolDown.IsCoolDown("RushLockon"))
            {
                Debug.Log("ロックオン中");
                // プレイヤー方向へ回転
                PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);

                // プレイヤー方向との角度差をチェック
                Vector3 toPlayer = (owner.m_Player.position - owner.transform.position).normalized;
                float angle = Vector3.Angle(owner.transform.forward, toPlayer);

                // 30度以内なら「向いた」と判定して突進開始
                if (angle < 30f)
                {
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
            }
        }

        private void StartRush()
        {
            Debug.Log("突進");
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

            //フラグ初期化
            isRushing = false;
            reachedSameHeight = false;
            rushDir = Vector3.zero;
            owner.m_CoolDown.ForceEnd("RushLockon");
            owner.m_CoolDown.ForceEnd("Move");
            owner.m_CoolDown.ForceEnd("Lock");

            //デフォルト速度に戻す
            owner.m_Controller.speed = owner.m_speed;
            //エージェントを再取得
            owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position + owner.transform.forward, owner.m_CenterMarker.transform);
        }
    }
}