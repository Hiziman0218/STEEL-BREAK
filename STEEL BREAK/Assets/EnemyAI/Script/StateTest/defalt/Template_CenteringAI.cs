using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class A : State<CenteringAI>
    {
        //�R���X�g���N�^
        public A(CenteringAI owner) : base(owner) { }
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