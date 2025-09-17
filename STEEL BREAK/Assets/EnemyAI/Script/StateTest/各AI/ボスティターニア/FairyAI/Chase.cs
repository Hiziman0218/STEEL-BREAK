using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�e�B�^�[�j�A�����t�F�A���[
    public class Chase_Fairy : State<FairyAI>
    {
        //�R���X�g���N�^
        public Chase_Fairy(FairyAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�G�[�W�F���g��G�̑O�ɏo��
            //owner.myAgent.transform.position = owner.transform.forward;
            int X = Random.Range(-1, 1);
            int Y = Random.Range(-1, 1);
            if (X == 0) X = 1;
            if (Y == 0) Y = 1;

            Vector3 POS = owner.m_Player.position +
                new Vector3((float)X * Random.Range(1.0f, 10.0f), 0, (float)Y * Random.Range(1.0f, 10.0f));
            owner.m_MoveTarget.transform.position = POS;
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            owner.m_Detector.destination = owner.m_MoveTarget.transform;
            owner.transform.position = owner.m_Detector.transform.position;
            //�U���ł���͈͂�
            if (Vector3.Distance(owner.m_MoveTarget.transform.position, owner.transform.position) < owner.m_AttackDistance)
            {
                // �U��
                owner.ChangeState(AIState_Fairy.Shot);
            }
        }

        public override void Exit()
        {
        }
    }
}
