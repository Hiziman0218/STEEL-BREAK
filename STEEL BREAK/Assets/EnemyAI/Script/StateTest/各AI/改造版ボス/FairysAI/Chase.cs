using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    //�e�B�^�[�j�A�����t�F�A���[
    public class Chase_Fairys : State<FairysAI>
    {
        //�R���X�g���N�^
        public Chase_Fairys(FairysAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�ǂ�������");

            //�Ǐ]����^�[�Q�b�g�̕ύX
            ChangeTarget.Change(owner.m_CenterMarker.transform, owner.myAgent);

            int X = Random.Range(-1, 1);
            int Y = Random.Range(-1, 1);
            if (X == 0) X = 1;
            if (Y == 0) Y = 1;

            Vector3 POS = owner.m_Player.position +
                new Vector3((float)X * Random.Range(1.0f, 10.0f), 0, (float)Y * Random.Range(1.0f, 10.0f));
            owner.m_CenterMarker.transform.position = POS;
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�Ǐ]
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //�U���ł���͈͂�
            if (Vector3.Distance(owner.m_CenterMarker.transform.position, owner.transform.position) < owner.m_AttackDistance)
            {
                // �U��
                owner.ChangeState(AIState_Fairys.Shot_Fairys);
            }
        }

        public override void Exit()
        {
        }
    }
}
