using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


namespace StateMachineAI
{
    public class Caution : State<GunBatteryAI>
    {
        //コンストラクタ
        public Caution(GunBatteryAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            Debug.Log("警戒中");
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
            Lookhorizontal.Look_horizontal(owner.transform, owner.m_Player ,owner.m_rotationSpeedH);

            //攻撃可能範囲に入った
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_GunBatteryAI.Attack);
            }
        }
        public override void Exit()
        {

        }
    }
}