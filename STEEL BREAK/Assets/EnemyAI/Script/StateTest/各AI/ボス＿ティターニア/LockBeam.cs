using UnityEngine;
using System.Collections;

namespace StateMachineAI
{
    //プレイヤーにロックオンしてからビームを撃つ
    public class LockBeam_T : State<Titania_T>
    {
        private bool isAttacking = false;
        private Quaternion originalRotation;

        //コンストラクタ
        public LockBeam_T(Titania_T owner) : base(owner) { }

        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //現在の角度を記憶させておく
            originalRotation = owner.transform.rotation;

            owner.m_CoolDown.StartCoolDown("Lockon", 3f);
        }

        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            if (owner.m_CoolDown.IsCoolDown("Lockon"))
            {
                //プレイヤーをロックオン
                PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);
            }
            else if (!isAttacking) // まだ攻撃開始していないなら
            {
                isAttacking = true;
                //コルーチンを走らせる
                owner.StartCoroutine(AttackAndChangeState());
            }
        }

        private IEnumerator AttackAndChangeState()
        {
            // 攻撃コルーチンを実行
            yield return owner.StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, 0));

            // ビームが消えるまでクールダウンで止めておく
            owner.m_CoolDown.StartCoolDown("Cool", 4f);

            // クールダウンが終わるまで待機
            while (owner.m_CoolDown.IsCoolDown("Cool"))
                yield return null;

            //次の行動の際にガクッと角度が変わらないようにここで滑らかにリセット
            owner.StartCoroutine(SmoothResetRotation());
        }

        public override void Exit()
        {
            //クールタイムを設ける
            owner.m_CoolDown.StartCoolDown("Turn", 8f);
            //TurnBeam_Tで解除したエージェント再取得
            owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position, owner.m_CenterMarker.transform);
        }

        //角度リセット処理（スムーズ）
        private IEnumerator SmoothResetRotation()
        {
            float t = 0f;
            Vector3 startEuler = owner.transform.rotation.eulerAngles;
            Vector3 targetEuler = originalRotation.eulerAngles;

            while (t < 1f)
            {
                t += Time.deltaTime / 1.0f; // 1秒かけて戻す

                // Yawは現在値を維持、X/Zだけ補間
                float newX = Mathf.LerpAngle(startEuler.x, targetEuler.x, t);
                float newY = startEuler.y; // 向きは固定
                float newZ = Mathf.LerpAngle(startEuler.z, targetEuler.z, t);

                owner.transform.rotation = Quaternion.Euler(newX, newY, newZ);
                yield return null;
            }

            owner.ChangeState(AIState_Titania_T.Idle_T);
        }

    }
}