using System.Collections.Generic;
using UnityEngine;

namespace StateMachineAI
{
    //ビームを撃ちつつ回転しながら上昇
    public class TurnBeam_T : State<Titania_T>
    {
        //コンストラクタ
        private bool hasShot = false;
        public TurnBeam_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //エージェントをいったん返却
            PoolManager.Instance.Return("Titania",owner.myAgent);
            //ビームを撃つ
            owner.StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, 0));
            //クールダウンを設定この間は回転しながら上昇する
            owner.m_CoolDown.StartCoolDown("Rot", 5f);
            //チャージ時間を作る
            owner.m_CoolDown.StartCoolDown("Charge", 3f);
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            if (owner.m_CoolDown.IsCoolDown("Charge"))
            {
                //チャージエフェクト再生
            }
            else
            {

                // チャージ完了後に初回だけ撃つ
                if (!hasShot)
                {
                    owner.StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, 0));
                    hasShot = true;
                }

                // 回転処理（Y軸回転）
                owner.transform.Rotate(Vector3.up, 180f * Time.deltaTime);

                // 上昇処理（最低高度を維持しつつ上昇）
                Vector3 pos = owner.transform.position;
                pos.y += 5f * Time.deltaTime; // 上昇速度を調整
                if (pos.y < owner.m_ground) pos.y = owner.m_ground; // 最低高度を維持
                owner.transform.position = pos;

                //クールダウンでなければ
                if (!owner.m_CoolDown.IsCoolDown("Rot"))
                {
                    // ロックオン状態へ移行
                    owner.ChangeState(AIState_Titania_T.LockBeam_T);
                }
            }
        }

        public override void Exit()
        {
            hasShot = false;
        }
    }
}