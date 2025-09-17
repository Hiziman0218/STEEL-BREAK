using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;

namespace StateMachineAI
{
    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���łȂ����B
    /// </summary>
    /// 
    public enum AIState_SquadAI
    {
        Attack,
        SetPosition,
        Escape,
    }


    public class SquadAI
        : StatefulObjectBase<SquadAI, AIState_SquadAI>
    {
        //�i�r���b�V��
        public NavMeshAgent agent;
        //�N�[���_�E���Ǘ��p
        public CoolDown m_CoolDown;
        //�L�����N�^�[
        public Transform m_Player;
        void Start()
        {

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("SetPosition"))
                Destroy(gameObject);
            if (!AddStateByName("Escape"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<SquadAI>();

            //�����N�����́A�ʃ|�C���g�֓������ԂɈڍs������
            ChangeState(AIState_SquadAI.Escape);
        }
        /// <summary>
        /// �N���X�������ɃX�e�[�g�𐶐����Ēǉ�����
        /// </summary>
        /// <param name="ClassName">��������N���X�̖��O</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // ���݂̃A�Z���u������N���X���擾
                Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");

                // �N���X��������Ȃ������ꍇ�̑Ώ�
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} �N���X��������܂���ł����B");
                    return true;
                }

                // �^�� State<SquadAI> ���ǂ������`�F�b�N
                if (!typeof(State<SquadAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B\n�������c�~�܂�񂶂�c�˂����c�B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(SquadAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<SquadAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<SquadAI>;

                if (StateInstance != null)
                {
                    // �X�e�[�g���X�g�ɒǉ�
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} ���X�e�[�g���X�g�ɒǉ����܂����B");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} �̃C���X�^���X�����Ɏ��s���܂����B");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"�G���[���������܂����B: {ex.Message}");
                return false;
            }
        }
    }
}
