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
    public enum EnemyRole
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
    public enum AIState_Fairy
    {
        Chase_Fairy,
        Shot,
        RandamMove,
        Guard,
        CeackGuard,
    }

    public class FairyAI
        : StatefulObjectBase<FairyAI, AIState_Fairy>
    {
        [Header("�v���C���[")]
        public Transform m_Player;
        [Header("�G�l�~�[���f��")]
        public Transform m_EnemyModel;
        [Header("�Z���^�[�|�C���g�̎擾")]
        public GameObject m_CenterMarker;

        [Header("���a")]
        public float m_Radius = 8f;
        [Header("��]���x")]
        public float m_RotSpeed = 2f;
        [Header("�㉺���̗h��")]
        public float m_Vertical = 0.3f;
        [Header("x���̂˂���")]
        public float m_Twist_x = 0.5f;

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


        public GameObject FlyingAgentObject;
        public SteeringDetector m_Detector;

        public GameObject m_MoveTarget;

        void Start()
        {
            m_MoveTarget = new GameObject();

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
            if (!AddStateByName("Chase_Fairy"))
                Destroy(gameObject);
            if (!AddStateByName("Shot"))
                Destroy(gameObject);
            if (!AddStateByName("RandamMove"))
                Destroy(gameObject);
            if (!AddStateByName("Guard"))
                Destroy(gameObject);
            if (!AddStateByName("CeackGuard"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<FairyAI>();

            GameObject Dummy = GameObject.Instantiate(FlyingAgentObject, transform.position, transform.rotation);
            m_Detector = Dummy.GetComponent<SteeringDetector>();
            m_Detector.destination = m_Player;

            // myAgent = PoolManager.Instance.Get("Soldier", transform.position + transform.forward, m_Player);
            // �U�����ɍs��
            ChangeState(AIState_Fairy.Chase_Fairy);
        }
        /*
        // ���Ŏ��ɐF�X����  ���S�X�e�[�g����ꂽ�炻�����Ɉړ������ق�����������
        void OnDestroy()
        {
            // ���[�����Ƃ�agent����
            if (m_Role == EnemyRole.Guardian)
            {
                PoolManager.Instance.Return("Guardian", myAgent);
            }
            else
            {
                PoolManager.Instance.Return("Soldier", myAgent);
            }

            // �G�Ǘ����X�g���珜�O
            Titania titania = FindObjectOfType<Titania>();
            if (titania != null)
            {
                // �o�^����
                //titania.m_spawnedEnemies.Remove(gameObject);
            }

        }*/


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
                if (!typeof(State<FairyAI>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(FairyAI) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<FairyAI> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<FairyAI>;

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
