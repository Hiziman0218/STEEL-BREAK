using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class Chase_BombAI : State<BombAI>
    {
        //�R���X�g���N�^
        public Chase_BombAI(BombAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            //�G�̑O�ɃX�|�[���|�C���g�ݒ�
            Vector3 spawnPos = owner.transform.position + owner.transform.forward;
            owner.myAgent = PoolManager.Instance.Get("FlyingFollowing", spawnPos, owner.m_Player);
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //Fly�ɉ�]���𓯊�
            owner.transform.rotation = owner.myAgent.transform.rotation;

            //�ǂ�������
            Flying_Following.FlyingFollowing(owner.myAgent, owner.transform, owner.m_Player, owner.m_Rigidbody);

            //�U���ł���͈͂�
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_AttackDistance)
            {
                //����
                owner.ChangeState(AIState_BombAI.Ramming);
            }

        }
        public override void Exit()
        {

        }
    }
}