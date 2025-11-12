using UnityEngine;
using System.Collections;

namespace StateMachineAI
{
    public class Attack_CenteringAI : State<CenteringAI>
    {
        //コンストラクタ
        public Attack_CenteringAI(CenteringAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        private bool isAttacking = false;

        public override void Stay()
        {
            //距離チェック
            (float distance, float direction, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            // 移動処理は常に実行
            if (distance > owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_CenteringAI.Chase);
                Debug.Log("追跡");
            }
            else
            {
                owner.ChangeState(AIState_CenteringAI.CenterPoint);
            }

            //クールタイム中でないかつscriptが存在している＆プレイヤーの横に位置していれば
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack") &&
                Mathf.Abs(direction) < owner.m_SideDotThreshold && distance <= owner.m_AttackDistance)
            {
                // まだ攻撃中でなければ
                if (!isAttacking)
                {
                    //プレイヤーの方向へ向かせる
                    PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);
                    //コルーチンを開始
                    owner.StartCoroutine(AttackRoutine());
                }
            }

            //攻撃可能距離から離れたら
            if (distance > owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_CenteringAI.Chase);
                Debug.Log("追跡");
            }
        }

        private IEnumerator AttackRoutine()
        {
            isAttacking = true;
            // ここで並行実行 → AttackRoutine はすぐ終わる
            owner.StartCoroutine(
                Attack_Shots.ExecuteRandomBurst(owner.m_Enemy, owner.m_CoolDown, 4f, owner.m_MaxRange, 0.5f)
            );
            // 少し待ってからフラグを解除（クールダウン終了に合わせる）
            yield return new WaitForSeconds(4f);
            isAttacking = false;
        }
    }
}