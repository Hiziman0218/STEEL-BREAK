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
using RaycastPro.Detectors;
using Plugins.RaycastPro.Demo.Scripts;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���ł��Ȃ��B
    /// </summary>
    /// 
    public enum AIState_HitAndAwayAI
    {
        Chase,
        Attack,
        Away,
        Return,
    }

    public class HitAndAwayAI
        : StatefulObjectBase<HitAndAwayAI, AIState_HitAndAwayAI>
    {
        [Header("�v���C���[")]
        public Transform m_Player;
        [Header("�G�l�~�[���f��")]
        public Transform m_EnemyModel;
        [Header("�Z���^�[�|�C���g�̎擾")]
        public GameObject m_CenterMarker;
        [Header("�U���\����")]
        public float m_AttackDistance = 10;
        [Header("���ʂ̍U���\�p�x[-1 = ���S�ɔw��, 0 = �^��, 1 = ����]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("�ˌ����̍ő�ːi�X�s�[�h")]
        [Range(10f, 40f)]
        public float m_maxspeed = 20f;
        [Header("�����x")]
        [Range(10f, 100f)]
        public float m_acceleration = 40f;
        [Header("�Ǐ]�␳�i�l���������قǊɂ��Ǐ]����j")]
        [Range(0.001f, 0.1f)]
        public float m_turnsmooth = 0.005f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // ������p���j�b�g
        public GameObject myAgent;
        [HideInInspector]
        //���݃X�s�[�h
        public float m_currentspeed = 0;
        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�Z���^�[�|�C���^�[���ʂɎ擾����
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //agent����
            myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position + transform.forward, m_Player);

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Chase"))
                Destroy(gameObject);
            if (!AddStateByName("Attack"))
                Destroy(gameObject);
            if (!AddStateByName("Away"))
                Destroy(gameObject);
            if (!AddStateByName("Return"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<HitAndAwayAI>();

            //�����N�����́A�v���C���[��ǂ��������ԂɈڍs������
            ChangeState(AIState_HitAndAwayAI.Chase);
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
                if (!typeof(State<HitAndAwayAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(HitAndAwayAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<HitAndAwayAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<HitAndAwayAI>;

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
