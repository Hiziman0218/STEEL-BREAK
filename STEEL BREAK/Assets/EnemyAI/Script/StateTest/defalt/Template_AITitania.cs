using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�e�B�^�[�j�A��AI�X�^���_�[�h
    public class Titania_AI : State<Titania>
    {
        //�R���X�g���N�^
        public Titania_AI(Titania owner) : base(owner) { }
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