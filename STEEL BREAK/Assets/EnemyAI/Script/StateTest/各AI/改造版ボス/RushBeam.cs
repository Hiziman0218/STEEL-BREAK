using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�ːi���Ȃ���g�U�r�[��
    public class RushBeam_T : State<Titania_T>
    {
        //�R���X�g���N�^
        public RushBeam_T(Titania_T owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�ːi�r�[��");
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            owner.ChangeState(AIState_Titania_T.Idle_T);
        }
        public override void Exit()
        {

        }
    }
}