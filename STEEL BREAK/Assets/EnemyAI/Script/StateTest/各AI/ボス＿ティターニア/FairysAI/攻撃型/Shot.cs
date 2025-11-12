using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace StateMachineAI
{
    //ソルジャーミサイルタイプ
    public class Shot_Soldier : State<SoldierFairysAI>
    {
        public float m_TImes;
        private bool isAttacking = false;

        //コンストラクタ
        public Shot_Soldier(SoldierFairysAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("攻撃");

            m_TImes = 3.0f;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーの方向に向く
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            //攻撃処理
            //コルーチンを開始
            owner.StartCoroutine(AttackRoutine());

            if (m_TImes <= 0)
            {
                //ランダムに動く
                owner.ChangeState(AIState_Soldier.RandamMove_Soldier);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
            //エージェントを自分の位置へ戻ってこさせる
            owner.myAgent.transform.position = owner.transform.position;
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