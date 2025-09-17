using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class CeackGuard : State<FairyAI>
    {
        //�R���X�g���N�^
        public CeackGuard(FairyAI owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�K�[�h�n�_����");
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            //�h�q�|�C���g���X�g�̒��g�����ׂđ���
            foreach (Transform guard in owner.m_GuardPoint)
            {
                Debug.Log("�K�[�h�n�_������");
                bool isOccupied = false;
                
                //�S�Ẵt�F�A���[AI�𑖍����Ėh�q�|�C���g�ɔ�肪�Ȃ����`�F�b�N
                foreach (var fairy in UnityEngine.Object.FindObjectsOfType<FairyAI>())
                {
                    //���������ł͂Ȃ��t�F�A���[�Ɣ���Ă����ture��Ԃ��ďI���
                    if (fairy != owner && fairy.m_GuardPointer == guard.gameObject)
                    {
                        Debug.Log("�K�[�h�n�_���U���Ɉڍs");
                        isOccupied = true;
                        break;
                    }
                }

                //�N���h�q�|�C���g���g���ĂȂ����
                if (!isOccupied)
                {
                    //�K�[�h�|�C���^�\��Ώۂ̃K�[�h�|�C���g�ɕR�Â�
                    owner.m_GuardPointer = guard.gameObject;
                    owner.ChangeState(AIState_Fairy.Guard);
                    return;
                }
            }

            //�󂫂��Ȃ���΍U�����ɍs��
            //owner.ChangeState(AIState_Fairy.RushShot);
        }

        public override void Exit()
        {

        }
    }
}