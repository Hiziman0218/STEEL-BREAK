using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�e�B�^�[�j�A�����t�F�A���[
    public class Guard : State<FairyAI>
    {
        //�R���X�g���N�^
        public Guard(FairyAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�Ǐ]����^�[�Q�b�g�̕ύX
            ChangeTarget.Change(owner.m_GuardPointer.transform, owner.myAgent);

            Debug.Log("�K�[�h");
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�U���\���̃`�F�b�N
            (float distance, _, _) = Distance_Check.Check(owner.transform, owner.m_Player);
            //���ʒu�Ǝ����̋���
            (float guarddistance, _, _) = Distance_Check.Check(owner.transform, owner.m_CenterMarker.transform);

            //Fly��y����]���𓯊�
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;

            //�U���\�͈͂����ʒu�̋������������
            if (guarddistance > owner.m_AttackDistance)
            {
                //���ʒu�֋߂Â�
                Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);
            }
            else
            {
                /*
                //�˂�����܂񂾉�]����
                Vector3 newTarget = Centering.RotAroundGuardPoint3DFixed(
                    owner.m_GuardPointer.transform.position,  // ���|�C���g
                    Time.time,                                // ���ԁi��]�̊p�x�Ɏg���j
                    owner.m_Radius,                           // ���a�i�����j
                    owner.m_RotSpeed,                         // ��]���x
                    owner.m_Vertical,                         // �㉺�h��̕�
                    owner.m_Twist_x                           // X���̂˂���̕�
                );
                owner.m_CenterMarker.transform.position = newTarget;
                */
                //�K�[�h�|�C���g�ɒǏ]
                Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);
            }

            // �U���͈͓��ɓ����
            if (distance <= owner.m_AttackDistance)
            {
                //owner.ChangeState(AIState_Fairy.RushShot);
            }

        }

        public override void Exit()
        {

        }
    }
}