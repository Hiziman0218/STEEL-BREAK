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
    public enum AIState_Titania
    {
        Idle,
        Spawn,
        TurnBeam,
        LockBeam,
        RushBeam,
    }

    public class Titania
        : StatefulObjectBase<Titania, AIState_Titania>
    {
        [Header("�v���C���[")]
        public Transform m_Player;
        [Header("�G�l�~�[���f��")]
        public Transform m_EnemyModel;
        [Header("�r�[�����ˌ�")]
        public Transform m_BeamPoint;

        [Header("�G���G�̃X�|�[���ʒu���擾")]
        public List<GameObject> m_SpawnPoints = new List<GameObject>();
        [Header("�G���G�̃v���n�u")]
        public GameObject m_Fairys;
        [Header("�G���G�̏�Ɏc�鐶�����")]
        public int m_MaxAttackFairys = 5;
        public int m_MaxDefensFairys = 5;
        [Header("��E�m���i0.0�Ń\���W���[100%�A1.0�ŃK�[�f�B�A��100%�j")]
        [Range(0.0f, 1.0f)]
        public float m_SpawnPer = 0.3f;
        [Header("��������Ƃ��̏o���Ԋu")]
        public float m_waitSeconds = 3f;

        [Header("�Z���^�[�|�C���g�̎擾")]
        public GameObject m_CenterMarker;

        [Header("�U���\����")]
        public float m_AttackDistance = 30;
        [Header("���ʂ̍U���\�p�x[-1 = ���S�ɔw��, 0 = �^��, 1 = ����]")]
        public float m_forwardDotThreshold = 0.8f;

        [Header("�ˌ����̍ő�ːi�X�s�[�h")]
        [Range(10f, 40f)]
        public float m_maxspeed = 10f;
        [Header("�����x")]
        [Range(10f, 100f)]
        public float m_acceleration = 40f;
        [Header("�Ǐ]�␳�i�l���������قǊɂ��Ǐ]����j")]
        [Range(0.001f, 0.1f)]
        public float m_turnsmooth = 0.005f;

        [Header("�U���^�̓G�Ǘ�")]
        public List<GameObject> m_spawnedAttackEnemies = new List<GameObject>();
        [Header("�h��^�̓G�Ǘ�")]
        public List<GameObject> m_spawnedDefensEnemies = new List<GameObject>();


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
        [HideInInspector]
        //�R���[�`���̃t���O�Ǘ��p
        public bool isSpawningFairy = false;

        public GameObject m_DefensEnemySenterObject;


        void Start()
        {
            //�v���C���[���^�O�Ō������Ď擾
            m_Player = GameObject.FindWithTag("Player")?.transform;

            //�X�|�[���|�C���g���擾
            Transform parent = GameObject.Find("SpawnRoot").transform;
            foreach (Transform child in parent)
            {
                m_SpawnPoints.Add(child.gameObject);
            }

            //agent����
            //myAgent = PoolManager.Instance.Get("FlyingFollowing", transform.position + transform.forward, m_Player);

            //�A�^�b�`���Ă���X�v���N�g�̎����擾
            AutoComponentInitializer.InitializeComponents(this);
            m_Rigidbody = GetComponent<Rigidbody>();

            //���݂��Ă��Ȃ��N���X���w�肳�ꂽ��{�̏���
            if (!AddStateByName("Idle"))
                Destroy(gameObject);
            if (!AddStateByName("Spawn"))
                Destroy(gameObject);
            if (!AddStateByName("TurnBeam"))
                Destroy(gameObject);
            if (!AddStateByName("LockBeam"))
                Destroy(gameObject);
            if (!AddStateByName("RushBeam"))
                Destroy(gameObject);

            //�X�e�[�g�}�V�[�������g�Ƃ��Đݒ�
            stateMachine = new StateMachine<Titania>();

            //�����N�����́A�s�����ߏ�ԂɈڍs������
            ChangeState(AIState_Titania.Idle);
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
                if (!typeof(State<Titania>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} �� State<EnemyAI> �^�ł͂���܂���B");
                    return true;
                }

                // �C���X�^���X�𐶐�
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(Titania) });


                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} �̃R���X�g���N�^��������܂���ł����B");
                    return true;
                }

                State<Titania> StateInstance =
                    Constructor.Invoke(new object[] { this }) as State<Titania>;

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
