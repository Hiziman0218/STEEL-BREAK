using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineAI
{
    /// <summary>
    /// �U���̖���
    /// </summary>
    public class AttackMode : State<AITester>
    {
        //�R���X�g���N�^
        public AttackMode(AITester owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�퓬�J�n");
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            float distanceToPlayer = Vector3.Distance(owner.m_Player.position, owner.transform.position);

            // �U���͈͓��Ȃ�
            if (distanceToPlayer < owner.m_AttackDistanse)
            {

                /*
                //�v���C���[�𐳖ʂɑ�����
                owner.m_RotMove.LookTaget();
                //�K���ɓ������
                owner.m_RondomMove.StartRandomMove();

                //�U��������
                if (!owner.m_CoolDown.IsOnCoolDown("Attack"))
                {
                    //�U��
                    owner.ChangeState(AIState_ABType.Attack);
                }
                */
            }
            else
            {
                owner.ChangeState(AIState_ABType.Chase);
            }
        }
        public override void Exit()
        {
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) > owner.m_AttackDistanse)
            {
                Debug.Log("�ǌ�����I");
            }
            else
            {
                Debug.Log("���������I");
            }
        }
    }
}