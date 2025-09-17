using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    // プレイヤーへ一定距離近づいたらまっすぐ飛行し爆発
    public class Ramming_BombAI : State<BombAI>
    {
        //コンストラクタ
        public Ramming_BombAI(BombAI owner) : base(owner) { }

        //爆発への移行コルーチン
        private IEnumerator DelayedExplosion()
        {
            //m_explosion_count秒経つと次の処理へ
            yield return new WaitForSeconds(owner.m_explosion_count);
            //自爆する
            owner.ChangeState(AIState_BombAI.Explosion);
        }

        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //追従飛行を解除
            PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);
            //爆発コルーチン開始
            owner.StartCoroutine(DelayedExplosion());

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //加速処理
            owner.m_currentspeed = Acceleration.Smooth(owner.m_currentspeed, owner.m_maxspeed, owner.m_acceleration);
            //プレイヤーへの緩い追従処理
            PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);

            //まっすぐ進むだけ
            owner.m_currentspeed = Mathf.Lerp(owner.m_currentspeed, owner.m_maxspeed, 1 - Mathf.Exp(owner.m_acceleration * Time.deltaTime));
            owner.transform.position += owner.transform.forward * owner.m_currentspeed * Time.deltaTime;

        }
        public override void Exit()
        {
            //追従飛行を解除
            PoolManager.Instance.Return("FlyingFollowing", owner.myAgent);
        }
    }
}