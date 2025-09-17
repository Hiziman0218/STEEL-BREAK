using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    //�\���W���[�~�T�C���^�C�v
    public class Shot_Fairys : State<FairysAI>
    {
        public float m_TImes;
        //�R���X�g���N�^
        public Shot_Fairys(FairysAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�U��");

            m_TImes = 3.0f;
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {

            //�v���C���[�̕����Ɍ���
            owner.transform.LookAt(owner.m_Player);

            //�U��


            if (m_TImes <= 0)
            {
                //�����_���ɓ���
                owner.ChangeState(AIState_Fairys.RandamMove_Fairys);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {
            //�G�[�W�F���g�������̈ʒu�֖߂��Ă�������
            //owner.myAgent.transform = owner.transform.gameObject;
        }
    }
}