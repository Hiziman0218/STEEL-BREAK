using UnityEngine;
using static SuperCharacterController;

namespace StateMachineAI
{
    //プレイヤー中心にランダムな場所を指定して移動
    public class RandomMove_T : State<Titania_T>
    {
        //コンストラクタ
        public RandomMove_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("ランダムに移動");

            //エージェントを自分の位置へ戻す
            owner.myAgent.transform.position = owner.transform.position;
            Chak();
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {

            //エージェントの向いている方向にy軸回転動を同期
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;
            //回転処理を滑らかにする
            owner.transform.rotation = Quaternion.Slerp(
                owner.transform.rotation,
                yOnlyRotation,
                Time.deltaTime * owner.turnSpeed
            );

            //エージェントの近くに来たらIdleに戻る
            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < 3.0f)
            {
                owner.ChangeState(AIState_Titania_T.Idle_T);
            }
        }

        //物理挙動はFixedで処理
        public override void FixedStay()
        {
            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

        }
        public override void Exit()
        {

        }

        public void Chak()
        {
            
            float minDistance = 20f; // 次の移動先と現在地の最低でも離したい距離
            float maxDistance = owner.m_AttackDistance;

            // ランダムな方向を決める（XZ平面）
            Vector2 circle = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(circle.x, 0, circle.y);

            // 最小〜最大の範囲で距離を決める
            float distance = UnityEngine.Random.Range(minDistance, maxDistance);

            // オフセット計算
            Vector3 randomOffset = dir * distance;

            // プレイヤー中心に移動先を決定（高さはまだ未調整）
            Vector3 candidate = owner.m_Player.position + randomOffset;

            // 地面の高さを Raycast で取得
            float groundY = candidate.y;
            if (Physics.Raycast(candidate + Vector3.up * 1000f, Vector3.down, out RaycastHit hit, 2000f, LayerMask.GetMask("Ground")))
            {
                groundY = hit.point.y;
            }

            // 地面から m_ground ～ m_ground + n の高さに設定
            float heightOffset = UnityEngine.Random.Range(owner.m_ground, owner.m_ground + 5);
            candidate.y = groundY + heightOffset;

            // もし今いる位置と距離が近すぎる場合は再抽選しても良い
            if (Vector3.Distance(owner.transform.position, candidate) < minDistance)
            {
                // 再帰的に呼び直すか、ループで再抽選
                Chak();
                return;
            }

            owner.m_CenterMarker.transform.position = candidate;
        }
    }
}