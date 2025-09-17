using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�e�B�^�[�j�A�����t�F�A���[
    public class Guard_Fairys : State<FairysAI>
    {
        //�R���X�g���N�^
        public Guard_Fairys(FairysAI owner) : base(owner) { }
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
                //�K�[�h�|�C���g�ɒǏ]
                Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);
            }

            // �U���͈͓��ɓ����
            if (distance <= owner.m_AttackDistance)
            {
                owner.ChangeState(AIState_Fairys.Shot_Fairys);
            }

        }

        public override void Exit()
        {

        }
    }
}