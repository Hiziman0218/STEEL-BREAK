using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //プレイヤーとの距離のチェック
            (float distance, _, float direction) = Distance_Check.Check(owner.transform, owner.m_Player);

            //クールダウン中でないかつほぼ正面にプレイヤーを捉えていたら
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack"))
            {
                Debug.Log("射撃");
                //射撃
                //Attack_Shot.Execute(owner.transform, owner.m_CoolDown);
                //敵から見てプレイヤーの後ろにセンターポイントを指定
                Center_Rush.CenterRush(owner.m_CenterMarker, owner.transform, owner.m_Player, owner.m_AttackDistance);
                //クールダウン設定
                owner.m_CoolDown.StartCoolDown("Attack", 2);

            }
            else
            {
                owner.ChangeState(AIState_HitAndAwayAI.Away);
            }

        }
        public override void Exit()
        {
        }
    }
}