using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�v���C���[�Ƀ��b�N�I�����Ă���r�[��������
    public class LockBeam : State<Titania>
    {
        //�R���X�g���N�^
        public LockBeam(Titania owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {

        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {

        }
        public override void Exit()
        {

        }
    }
}