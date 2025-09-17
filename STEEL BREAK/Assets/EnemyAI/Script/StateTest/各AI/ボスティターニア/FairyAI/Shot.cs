using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    //�\���W���[�~�T�C���^�C�v
    public class Shot : State<FairyAI>
    {
        public float m_TImes;
        //�R���X�g���N�^
        public Shot(FairyAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            m_TImes = 3.0f;
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            owner.m_MoveTarget.transform.position = owner.transform.position;
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
            //�v���C���[�̕����Ɍ���
            owner.transform.LookAt(owner.m_Player);
            //�U��
            if (m_TImes <= 0)
            {
                owner.ChangeState(AIState_Fairy.RandamMove);
            }
            else
            {
                m_TImes -= Time.deltaTime;
            }
        }
        public override void Exit()
        {

        }
    }
}