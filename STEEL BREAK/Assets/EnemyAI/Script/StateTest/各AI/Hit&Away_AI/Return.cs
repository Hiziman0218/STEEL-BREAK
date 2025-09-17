using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StateMachineAI
{
    public class Return : State<HitAndAwayAI>
    {
        private Vector3 retreatOrigin;
        private Vector3 moveDirection;

        //�R���X�g���N�^
        public Return(HitAndAwayAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("����");
            //�G�[�W�F���g��ԋp���ĕ����ł̏����ɐ؂�ւ�
            PoolManager.Instance.Return("FlyingFollowing",owner.myAgent);
            
            // ���E����̈ʒu
            retreatOrigin = owner.transform.position;

            //owner.m_timer = 0f;

        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            /*
            owner.m_timer += Time.deltaTime;

            float angularSpeed = 1.0f;
            float t = owner.m_timer * angularSpeed;
            float x = Mathf.Sin(t) * owner.m_radius;
            float z = Mathf.Cos(t) * owner.m_radius;

            // ���E��̋N�_
            Vector3 offset = new Vector3(x, 0, z);
            */
            //moveDirection = retreatOrigin + offset;
            
        }

        //��������
        public override void FixedStay()
        {
            if (owner.m_Rigidbody.linearVelocity.sqrMagnitude > 0.01f)
            {
                owner.transform.forward = owner.m_Rigidbody.linearVelocity.normalized;
            }

            // ���ۂ̈ړ�
            owner.m_Rigidbody.MovePosition(moveDirection);

            // �ʒu����ɂ��X�e�[�g�J��
            if ((owner.m_Rigidbody.position - moveDirection).sqrMagnitude < 0.01f)
            {
                owner.ChangeState(AIState_HitAndAwayAI.Chase);
            }

        }

        public override void Exit()
        {
            //�G�[�W�F���g���擾���Ȃ���
            owner.myAgent = PoolManager.Instance.Get("FlyingFollowing", owner.transform.position + owner.transform.forward, owner.m_Player);
        }
    }
}
