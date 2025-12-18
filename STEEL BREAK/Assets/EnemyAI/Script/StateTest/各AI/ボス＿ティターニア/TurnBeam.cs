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
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // チャージ完了後に初回だけ撃つ
            if (!hasShot)
            {
                owner.StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, 0));
                hasShot = true;
            }

            // 回転処理
            Quaternion deltaRot = Quaternion.Euler(0f, 180f * Time.deltaTime, 0f);
            owner.m_Rigidbody.MoveRotation(owner.m_Rigidbody.rotation * deltaRot);

            // 上昇処理
            Vector3 velocity = owner.m_Rigidbody.linearVelocity;
            velocity.y = 5f; // 上昇速度を固定
            owner.m_Rigidbody.linearVelocity = velocity;

            //クールダウンでなければ
            if (!owner.m_CoolDown.IsCoolDown("Rot"))
            {
                // ロックオン状態へ移行
                owner.ChangeState(AIState_Titania_T.LockBeam_T);
            }
        }

        public override void Exit()
        {
            hasShot = false;
            owner.m_Rigidbody.linearVelocity = Vector3.zero; // 上昇を止める
        }
    }
}