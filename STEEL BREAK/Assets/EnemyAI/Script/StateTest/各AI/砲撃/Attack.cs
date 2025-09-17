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
        //�R���X�g���N�^
        public Attack_GunBatteryAI(GunBatteryAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            
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
            Lookhorizontal.Look_horizontal(owner.transform, owner.m_Player, owner.m_rotationSpeedH);

            //�U���\���̃`�F�b�N
            (float distance,_, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            //�N�[���_�E�����łȂ����
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack"))
            {
                //�U���͈͓��Ȃ�
                if (distance <= owner.m_AttackDistance)
                {
                    //�U��
                    //Attack_Shot.Execute(owner.transform, owner.m_CoolDown);
                }
            }

            //�U���\�͈͂���o��
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