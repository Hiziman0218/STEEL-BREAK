using UnityEngine;

namespace StateMachineAI
{
    /// <summary>
    /// HitAndAwayAI が「退避（Return）」するステート
    /// 攻撃後に一度離脱して、再度追撃に移るための処理を担当
    /// </summary>
    public class Return : State<HitAndAwayAI>
    {
        private float awayTimer;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Return(HitAndAwayAI owner) : base(owner) { }

        /// <summary>
        /// ステートに入った瞬間に呼ばれる（Start 相当）
        /// </summary>
        public override void Enter()
        {
            Debug.Log("Return ステート開始");

            // 現在のエージェントを一旦プールに返却（追従を解除）
            PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);
            awayTimer = 0f;
        }

        /// <summary>
        /// ステート中に毎フレーム呼ばれる（Update 相当）
        /// </summary>
        public override void Stay()
        {
            // プレイヤー方向ベクトル
            Vector3 toPlayer = (owner.m_Player.position - owner.transform.position).normalized;
            //プレイヤーとの角度を計算
            float angle = Vector3.Angle(owner.transform.forward, toPlayer);
            //タイマーをカウント
            awayTimer += Time.deltaTime;

            Vector3 offsetDir = Quaternion.AngleAxis(90f, Vector3.up) * toPlayer; // 左右に90度旋回
            Vector3 retreatDir = (toPlayer + offsetDir).normalized;
            owner.transform.rotation = Quaternion.RotateTowards(
                owner.transform.rotation,
                Quaternion.LookRotation(retreatDir),
                owner.m_RotationSpeed * Time.deltaTime
            );


            // forward に応じて移動
            owner.m_Rigidbody.MovePosition(
                owner.transform.position + owner.transform.forward * owner.m_speed * Time.deltaTime
            );

            // 一定時間or十分な角度であれば攻撃ステートに移る
            if (angle < owner.m_ReEntryAngle || awayTimer > owner.m_AwayDuration)
            {
                owner.ChangeState(AIState_HitAndAwayAI.Attack);
            }
        }

        /// <summary>
        /// ステートを抜けるときに呼ばれる（Exit 相当）
        /// </summary>
        public override void Exit()
        {
            // エージェントを再取得して追従を再開
            owner.myAgent = PoolManager.Instance.Get(
                "FlyingFollowing",
                owner.transform.position,
                owner.m_Player
            );
        }
    }
}
