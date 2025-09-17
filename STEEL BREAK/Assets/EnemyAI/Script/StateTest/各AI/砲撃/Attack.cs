using System.Collections;
using System.Collections.Generic;
using System.Drawing;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


namespace StateMachineAI
{
    public class Attack_GunBatteryAI : State<GunBatteryAI>
    {
        //コンストラクタ
        public Attack_GunBatteryAI(GunBatteryAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            
        }

        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //砲身の上下移動
            foreach (Transform muzzle in owner.m_Muzzles)
            {
                LookVertical.Look_Vertical(muzzle, owner.m_Player, owner.minPitchAngle, owner.maxPitchAngle, owner.m_rotationSpeedV);
            }
            //砲台の横移動
            Lookhorizontal.Look_horizontal(owner.transform, owner.m_Player, owner.m_rotationSpeedH);

            //攻撃可能かのチェック
            (float distance,_, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            //クールダウン中でなければ
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack"))
            {
                //攻撃範囲内なら
                if (distance <= owner.m_AttackDistance)
                {
                    //攻撃
                    //Attack_Shot.Execute(owner.transform, owner.m_CoolDown);
                }
            }

            //攻撃可能範囲から出た
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) > owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_GunBatteryAI.Caution);
            }
        }
        public override void Exit()
        {

        }
    }
}