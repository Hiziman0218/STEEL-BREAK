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
    /// �������Ɋ���U�������
    /// Guardian�̓e�B�^�[�j�A�����
    /// Soldier�͍U������
    /// </summary>
    public enum FairysRole
    {
        Guardian,
        Soldier
    }

    /// <summary>
    /// �G�̃X�e�[�g���X�g
    /// �����ŃX�e�[�g��o�^���Ă��Ȃ��ꍇ�A
    /// �Y������s�����S���ł��Ȃ��B
    /// </summary>
    /// 
    public enum AIState_Fairys
    {
        Chase_Fairys,
        Shot_Fairys,
        RandamMove_Fairys,
        Guard_Fairys,
        CeackGuard_Fairys,
    }

    public class FairysAI
        : StatefulObjectBase<FairysAI, AIState_Fairys>
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
        public float m_maxspeed = 10f;

        [HideInInspector]
        public CoolDown m_CoolDown;
        [HideInInspector]
        //�N�[���^�C���ݒ�p
        public float m_CoolTime;
        [HideInInspector]
        public Rigidbody m_Rigidbody;
        [HideInInspector]
        // ������p���j�b�g
        public GameObject myAgent;
        [HideInInspector]
        //�e��E�p�ϐ�
        public EnemyRole m_Role;
        [HideInInspector]
        //���������|�C���g�擾�p
        public GameObject m_GuardPointer;
        [HideInInspector]
        //���ʒu�̃��X�g
        public List<Transform> m_GuardPoint;

        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //�K�[�h�|�C���g���擾
            Transform parent = GameObject.Find("GuardPoint").transform;
            foreach (Transform child in parent)
            {
                m_GuardPoint.Add(child);
            }

            //�Z���^�[�|�C���^�[���ʂɎ擾����
            m_CenterMarker = PoolManager.Instance.Get("CenterPoint", transform.position + transform.forward, m_Player);

            //�G�[�W�F���g���擾
            myAgent = PoolManager.Instance.Get("Soldier", transform.position + transform.forward, m_Player);

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Chase_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("Shot_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("RandamMove_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("Guard_Fairys"))
                Destroy(gameObject);
            if (!AddStateByName("CeackGuard_Fairys"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<FairysAI>();
            
            // �ǂ�������
            ChangeState(AIState_Fairys.Chase_Fairys);
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
                if (!typeof(State<FairysAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(FairysAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<FairysAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<FairysAI>;

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
