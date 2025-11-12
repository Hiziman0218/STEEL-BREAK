using UnityEngine;

namespace StateMachineAI
{
    public class Attack : State<HitAndAwayAI>
    {
        //コンストラクタ
        public Attack(HitAndAwayAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            // 現在のエージェントを一旦プールに返却（追従を解除）
            //PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // プレイヤー方向ベクトル
            //Vector3 toPlayer = (owner.m_Player.position - owner.transform.position).normalized;

            // プレイヤーとの距離チェック
            (float distance, _, float direction) = Distance_Check.Check(owner.transform, owner.m_Player);

            //Flyの回転動きを同期
            owner.transform.rotation = owner.myAgent.transform.rotation;

            //エージェントに追従
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            /*
            // プレイヤーに向かって突っ込みながら移動
            owner.transform.rotation = Quaternion.LookRotation(toPlayer);
            owner.m_Rigidbody.MovePosition(
                owner.transform.position + owner.transform.forward * owner.m_RCController.speed * Time.deltaTime
            );
            */

            //クールダウン中でない＆ほぼ正面にプレイヤーを捉えている＆攻撃可能距離である
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack") && distance < owner.m_AttackDistance)
            {
                Debug.Log("射撃");
                //射撃
                Attack_Shot.Execute(owner.m_Enemy, owner.m_CoolDown);
            }

            // 一定距離まで近づいたら離脱ステートへ
            if (distance < owner.m_RotationStart)
            {
                owner.ChangeState(AIState_HitAndAwayAI.Away);
            }
        }
        public override void Exit()
        {
            /*
            // エージェントを再取得して追従を再開
            owner.myAgent = PoolManager.Instance.Get(
                "FlyingFollowing",
                owner.transform.position,
                owner.m_Player
            );
            */
        }
    }
}