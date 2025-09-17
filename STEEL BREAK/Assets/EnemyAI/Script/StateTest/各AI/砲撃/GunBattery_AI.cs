using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEditorInternal;

namespace StateMachineAI
{
    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���ł��Ȃ��B
    /// </summary>
    /// 
    public enum AIState_GunBatteryAI
    {
        Caution,
        Attack,
    }

    public class GunBatteryAI
        : StatefulObjectBase<GunBatteryAI, AIState_GunBatteryAI>
    {
        [Header("�v���C���[")]
        public Transform m_Player;
        [Header("�C�g���f��")]
        public Transform[] m_Muzzles;

        [Header("�C�g�̋p����")]
        [Range(-10f, 0f)]
        public float minPitchAngle = -5f;
        [Range(0f, 80f)]
        public float maxPitchAngle = 60f;

        [Header("�C��̉���]�̃��O�^�C��")]
        [Range(1f, 10f)]
        public float m_rotationSpeedH;
        [Header("�C�g�̏c��]�̃��O�^�C��")]
        [Range(1f, 10f)]
        public float m_rotationSpeedV;

        [Header("�U���\����")]
        public float m_AttackDistance = 10f;

        [Header("�A�^�b�`������́i�ݒ肷��K�v�Ȃ�)")]
        public CoolDown m_CoolDown;

        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Caution"))
                Destroy(gameObject);
            if (!AddStateByName("Attack_GunBatteryAI"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<GunBatteryAI>();

            //�����N�����́A�v���C���[��ǂ��������ԂɈڍs������
            ChangeState(AIState_GunBatteryAI.Caution);
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

                // �^�� State<GunBattery_AI> ���ǂ������`�F�b�N
                if (!typeof(State<GunBatteryAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(GunBatteryAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<GunBatteryAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<GunBatteryAI>;

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
