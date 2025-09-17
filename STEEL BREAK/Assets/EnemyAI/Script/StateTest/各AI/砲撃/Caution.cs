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
        //�R���X�g���N�^
        public Caution(GunBatteryAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�x����");
        }

        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�C�g�̏㉺�ړ�
            foreach (Transform muzzle in owner.m_Muzzles)
            {
                LookVertical.Look_Vertical(muzzle, owner.m_Player, owner.minPitchAngle, owner.maxPitchAngle, owner.m_rotationSpeedV);
            }
            //�C��̉��ړ�
            Lookhorizontal.Look_horizontal(owner.transform, owner.m_Player ,owner.m_rotationSpeedH);

            //�U���\�͈͂ɓ�����
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