using UnityEngine;

namespace StateMachineAI
{
    //ビームを撃ちつつ回転しながら上昇
    public class TurnBeam_T : State<Titania_T>
    {
        private float elapsedTime;

        //コンストラクタ
        public TurnBeam_T(Titania_T owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //エージェントをいったん返却
            PoolManager.Instance.Return("Titania",owner.myAgent);
            //ビームを撃つ
            owner.StartCoroutine(Attack_Shots.ShotR(owner.m_Enemy, owner.m_CoolDown, 0));
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            // 回転処理（Y軸回転）
            owner.transform.Rotate(Vector3.up, 180f * Time.deltaTime);

            // 上昇処理（最低高度を維持しつつ上昇）
            Vector3 pos = owner.transform.position;
            pos.y += 5f * Time.deltaTime; // 上昇速度を調整
            if (pos.y < owner.m_ground) pos.y = owner.m_ground; // 最低高度を維持
            owner.transform.position = pos;

            // 一定時間経過でステート変更
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 5f) // 5秒間回転上昇したら次へ
            {
                owner.ChangeState(AIState_Titania_T.LockBeam_T);
            }
        }
        public override void Exit()
        {
            //エージェント再取得
            owner.myAgent = PoolManager.Instance.Get("Titania", owner.transform.position, owner.m_CenterMarker.transform);
        }
    }
}