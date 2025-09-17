using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

namespace StateMachineAI
{
    public class CenterPoint : State<CenteringAI>
    {
        //�R���X�g���N�^
        public CenterPoint(CenteringAI owner) : base(owner) { }


        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�Ǐ]����^�[�Q�b�g�̕ύX
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�v���C���[�̎�����Z���^�[�|�C���^�[���O���O�����
            Centering.CenterPoint(owner.m_CenterMarker, owner.transform, owner.m_Player, owner.m_Muki, owner.m_AttackDistance);

            //�ǂ�������
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //�v���C���[�֌���
            PlayerLookAt.LookAt(owner.m_Player, owner.m_EnemyModel);

            //��]�N�[���^�C������Ȃ����
            if (owner.m_CoolDown != null && !owner.m_CoolDown.IsCoolDown("siderot"))
            {
                //�m���ŉ�]�����̕ύX
                if (Random.Range(0, 100) > 90)
                {
                    owner.m_Muki *= -1;
                    owner.m_CoolDown.StartCoolDown("siderot", Random.Range(3, 10));
                }
            }

            // �����_���ōU������
            if (Random.Range(0, 100) > 95)
            {
                owner.ChangeState(AIState_CenteringAI.Attack);
            }

        }
        public override void Exit()
        {
        }
    }
}