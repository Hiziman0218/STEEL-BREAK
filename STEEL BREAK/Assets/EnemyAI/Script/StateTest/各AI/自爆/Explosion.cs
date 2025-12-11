using UnityEngine;

namespace StateMachineAI
{
    public class Explosion : State<BombAI>
    {
        //コンストラクタ
        public Explosion(BombAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            // 爆風を生成
            if (owner.blastPrefab != null)
            {
                GameObject blastObj = Object.Instantiate(
                    owner.blastPrefab,
                    owner.transform.position,
                    Quaternion.identity
                );
            }

            //強制的にHPを０に
            owner.m_Enemy.Destruction();

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
        }
        public override void Exit()
        {

        }
    }
}