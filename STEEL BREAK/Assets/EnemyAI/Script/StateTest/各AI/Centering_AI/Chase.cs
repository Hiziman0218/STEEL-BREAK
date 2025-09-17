using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    public class Chase_CenteringAI : State<CenteringAI>
    {
        //�R���X�g���N�^
        public Chase_CenteringAI(CenteringAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�Ǐ]����^�[�Q�b�g�̕ύX
            ChangeTarget.Change(owner.m_Player.transform, owner.myAgent);

            Debug.Log("�ǐՊJ�n");
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //Fly��y����]���𓯊�
            Quaternion yOnlyRotation = Quaternion.Euler(0, owner.myAgent.transform.rotation.eulerAngles.y, 0);
            owner.transform.rotation = yOnlyRotation;

            //�ǂ�������
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //�U���ł���͈͂�
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance)
            {
                //�U���J�n
                owner.ChangeState(AIState_CenteringAI.CenterPoint);
                //Debug.Log("�퓬�؂�ւ�");
            }

        }
        public override void Exit()
        {

        }
    }
}