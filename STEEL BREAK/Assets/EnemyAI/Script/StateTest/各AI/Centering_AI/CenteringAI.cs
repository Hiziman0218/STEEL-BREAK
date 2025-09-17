using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine.AI;
//using Unity.VisualScripting;
using RaycastPro.Detectors;
using static UnityEngine.UI.GridLayoutGroup;

namespace StateMachineAI
{
    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���ł��Ȃ��B
    /// </summary>
    /// 
    public enum AIState_CenteringAI
    {
        Chase,
        CenterPoint,
        Attack,
    }


    public class CenteringAI
        : StatefulObjectBase<CenteringAI, AIState_CenteringAI>
    {
        [Header("�v���C���[(�^�O�Ŏ擾)")]
        public Transform m_Player;
        [Header("�G�l�~�[���f��")]
        public Transform m_EnemyModel;
        [Header("�Z���^�[�|�C���g�̎擾")]//(���̂݊i�[�̕��������I)
        public GameObject m_CenterMarker;
        [Header("�����␳")]
        public float m_Muki = 1;
        [Header("���ʂ̍U���\�p�x[-1 = ���S�ɔw��, 0 = �^��, 1 = ����]")]
        public float m_SideDotThreshold = 0.3f;
        [Header("�U���\����")]
        [Range(1f, 20f)]
        public float m_AttackDistance = 10f;

        public Enemy m_Enemy;
        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        public BoxCollider m_BoxCollider;
        [HideInInspector]
        // ������p���j�b�g
        public GameObject myAgent;

        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�Z���^�[�|�C���^�[���ʂɎ擾����
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //�����̈ʒu��agent�����ʒu��ݒ�
            Vector3 spawnPos = transform.position + transform.forward;
            //agent����
            myAgent = PoolManager.Instance.Get("FlyingFollowing", spawnPos, m_Player);

            //�G�l�~�[�̃X�N���v�g���擾
            Enemy m_Enemy = GetComponent<Enemy>();

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);

            m_Rigidbody = GetComponent<Rigidbody>();

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Chase_CenteringAI"))
                Destroy(gameObject);
            if (!AddStateByName("CenterPoint"))
                Destroy(gameObject);
            if (!AddStateByName("Attack_CenteringAI"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<CenteringAI>();

            //�����N�����́A�v���C���[��ǂ��������ԂɈڍs������
            ChangeState(AIState_CenteringAI.Chase);
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

                // �^�� State<CenteringAI> ���ǂ������`�F�b�N
                if (!typeof(State<CenteringAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(CenteringAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<CenteringAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<CenteringAI>;

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
