using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;

namespace StateMachineAI
{
    public class Idle_T : State<Titania_T>
    {
        //�R���X�g���N�^
        public Idle_T(Titania_T owner) : base(owner) { }
        //����AI���N�������u�ԂɎ��s(Start�Ɠ��`)
        public override void Enter()
        {
            Debug.Log("�s�����ߑҋ@����");
            //idle��ԂɂȂ������̍U����
            //owner.m_CoolDown.StartCoolDown("Idle", 5f);
        }
        //����AI���N�����ɏ�Ɏ��s(Update�Ɠ��`)
        public override void Stay()
        {
            owner.ChangeState(AIState_Titania_T.Spawn_T);
            /*
            //�v���C���[�ւ̊ɂ��Ǐ]����
            PlayerLookAt.SoftLock(owner.transform, owner.m_Player, owner.m_turnsmooth);

            //�N�[���^�C����������Ă�����s�����J�n
            if (!owner.m_CoolDown.IsCoolDown("Idle"))
            {
                //�X�|�[���N�[���^�C�����I����Ă��邩�����ȉ��Ȃ�
                if (!owner.m_CoolDown.IsCoolDown("Spawn")�@&& owner.m_spawnedEnemies.Count < owner.m_MaxFairys / 2)
                {
                    //�t�F�A���[����
                    owner.ChangeState(AIState_Titania.Spawn);
                    return;
                }

                owner.ChangeState(AIState_Titania.RushBeam);
            }
            */
        }
        public override void Exit()
        {
        }
    }
}