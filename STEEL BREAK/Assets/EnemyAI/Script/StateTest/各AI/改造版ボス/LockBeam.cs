using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�v���C���[�Ƀ��b�N�I�����Ă���r�[��������
    public class LockBeam_T : State<Titania_T>
    {
        //�R���X�g���N�^
        public LockBeam_T(Titania_T owner) : base(owner) { }
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