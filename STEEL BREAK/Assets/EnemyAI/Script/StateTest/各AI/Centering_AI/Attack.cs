using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


namespace StateMachineAI
{
    public class Attack_CenteringAI : State<CenteringAI>
    {
        //�R���X�g���N�^
        public Attack_CenteringAI(CenteringAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
        }

        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�U���\���̃`�F�b�N
            (float distance, float direction, _) = Distance_Check.Check(owner.transform, owner.m_Player);

            // �N�[���_�E�����łȂ������E�ǂ��炩�ɂ��čU���͈͓��Ȃ�
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("Attack") && Mathf.Abs(direction) < owner.m_SideDotThreshold && distance <= owner.m_AttackDistance)
            {
                //�v���C���[�֌���
                PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

                //�U������
                Attack_Shot.Execute(owner.m_Enemy, owner.m_CoolDown);

            }
            //�N�[���_�E�����U���͈͊O�Ȃ�
            else
            {
                //������O���O�����
                owner.ChangeState(AIState_CenteringAI.CenterPoint);
            }

            //�U���\�͈͂���o��
            if (distance > owner.m_AttackDistance)
            {
                //�ǐՊJ�n
                owner.ChangeState(AIState_CenteringAI.Chase);
                Debug.Log("�ǐ�");
            }
        }
        public override void Exit()
        {

        }
    }
}