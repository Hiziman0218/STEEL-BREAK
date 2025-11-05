using UnityEngine;

namespace StateMachineAI
{
    /// <summary>
    /// HitAndAwayAI が「退避（Return）」するステート
    /// 攻撃後に一度離脱して、再度追撃に移るための処理を担当
    /// </summary>
    public class Return : State<HitAndAwayAI>
    {
        private Vector3 retreatOrigin;   // 退避開始地点
        private Vector3 moveDirection;   // 移動先の座標

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

            // 退避開始地点を記録
            retreatOrigin = owner.transform.position;

            // 必要ならタイマー初期化などもここで行う
            // owner.m_timer = 0f;
        }

        /// <summary>
        /// ステート中に毎フレーム呼ばれる（Update 相当）
        /// </summary>
        public override void Stay()
        {
            /*
            // 円運動などで退避先を決めたい場合の例
            owner.m_timer += Time.deltaTime;

            float angularSpeed = 1.0f;
            float t = owner.m_timer * angularSpeed;
            float x = Mathf.Sin(t) * owner.m_radius;
            float z = Mathf.Cos(t) * owner.m_radius;

            // 退避先のオフセットを計算
            Vector3 offset = new Vector3(x, 0, z);

            moveDirection = retreatOrigin + offset;
            */
        }

        /// <summary>
        /// 物理演算のタイミングで呼ばれる（FixedUpdate 相当）
        /// </summary>
        public override void FixedStay()
        {
            // 移動方向に応じてキャラの forward を更新
            if (owner.m_Rigidbody.linearVelocity.sqrMagnitude > 0.01f)
            {
                owner.transform.forward = owner.m_Rigidbody.linearVelocity.normalized;
            }

            // Rigidbody を使って退避先へ移動
            owner.m_Rigidbody.MovePosition(moveDirection);

            // 退避先に到達したら Chase ステートへ戻る
            if ((owner.m_Rigidbody.position - moveDirection).sqrMagnitude < 0.01f)
            {
                owner.ChangeState(AIState_HitAndAwayAI.Chase);
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
