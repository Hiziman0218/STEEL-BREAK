using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineAI
{
    public class Wandering: State<AITester>
    {
        //�R���X�g���N�^
        public Wandering(AITester owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�p�g���[���J�n");
            owner.agent = owner.GetComponent<NavMeshAgent>();
            if (owner.agent == null)
            {
                Debug.LogError("NavMeshAgent��������Ȃ��I");
                return;
            }

            // �����ړ��|�C���g
            MoveToRandomPoint();
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            owner.transform.Rotate(0, 1, 0);


            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < owner.m_ChaseDistanse)
            {
                {
                    //�ǂ�������
                    owner.ChangeState(AIState_ABType.Chase);
                }
            }

            // �G�[�W�F���g���ړI�n�ɓ��B������ҋ@
            if (!owner.agent.pathPending && owner.agent.remainingDistance <= owner.agent.stoppingDistance)
            {
                owner.ChangeState(AIState_ABType.Idle);
                Debug.Log("�p�g���[���I��");
            }

        }
        public override void Exit()
        {
            
        }

        //�����_���ɍ��W���w��
        private void MoveToRandomPoint()
        {
            // �����_���ȕ����𐶐�
            Vector3 randomDirection = Random.insideUnitSphere * owner.m_PatrolRadius;
            // ���݈ʒu����ɐݒ�
            randomDirection += owner.transform.position;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, owner.m_PatrolRadius, NavMesh.AllAreas))
            {
                // �����_���Ȉʒu�Ɉړ�
                owner.agent.SetDestination(hit.position);
                Debug.Log("�V�����ړI�n: " + hit.position);
            }
        }

    }
}