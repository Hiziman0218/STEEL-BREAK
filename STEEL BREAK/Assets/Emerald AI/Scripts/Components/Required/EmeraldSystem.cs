using UnityEngine;                               // Unity �̊�{ API
using System.Collections.Generic;                 // List �Ȃǂ̃R���N�V����
using UnityEngine.AI;                             // NavMeshAgent ��
using EmeraldAI.Utility;                          // Emerald �̃��[�e�B���e�B
using EmeraldAI.SoundDetection;                   // �T�E���h���m���O���

namespace EmeraldAI
{
    /// <summary>
    /// �i���{��j�قڂ��ׂĂ� Emerald �R���|�[�l���g���A���� 1 �� Update ����X�V���܂��B
    /// ����ɂ��A�e�R���|�[�l���g�� 1 �ӏ�����e�ՂɃA�N�Z�X�ł��AUpdate �̕��U��h���܂��B
    /// </summary>
    #region Required Components
    [RequireComponent(typeof(EmeraldAnimation))]   // �K�{�F�A�j���[�V����
    [RequireComponent(typeof(EmeraldDetection))]   // �K�{�F���m�i���E/�h���j
    [RequireComponent(typeof(EmeraldSounds))]      // �K�{�F�T�E���h
    [RequireComponent(typeof(EmeraldCombat))]      // �K�{�F�퓬
    [RequireComponent(typeof(EmeraldBehaviors))]   // �K�{�F�s��
    [RequireComponent(typeof(EmeraldMovement))]    // �K�{�F�ړ�
    [RequireComponent(typeof(EmeraldHealth))]      // �K�{�F�̗�
    [RequireComponent(typeof(BoxCollider))]        // �K�{�F�R���C�_�[
    [RequireComponent(typeof(NavMeshAgent))]       // �K�{�FNavMeshAgent
    [RequireComponent(typeof(AudioSource))]        // �K�{�FAudioSource
    [SelectionBase]                                // �V�[����ł̑I����_
    #endregion
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/")]
    // �y�N���X�T�v�zEmeraldSystem�F
    //  Emerald AI �́g�����h�R���|�[�l���g�B�e�K�{�R���|�[�l���g�̎Q�Ƃ�ێ����A
    //  Awake/OnEnable/Update �ŏ�������ꊇ�X�V���s���B
    public class EmeraldSystem : MonoBehaviour
    {
        #region Target Info
        // �i���j�����͕����̃R���|�[�l���g�ŋ��ʗ��p����邽�߁A���C���� EmeraldSystem �ɕێ����܂��B

        [Header("���݂̐퓬�^�[�Q�b�g�iTransform�j")]
        [HideInInspector] public Transform CombatTarget;

        [Header("�Ǐ]�i�t�H���[�j�Ώۂ� Transform")]
        [HideInInspector] public Transform TargetToFollow;

        [Header("�����iLookAt�j�Ώۂ� Transform")]
        [HideInInspector] public Transform LookAtTarget;

        [Header("���݂̃^�[�Q�b�g�Ɋւ��鑍�����iIDamageable/ICombat ���j")]
        [HideInInspector][SerializeField] public CurrentTargetInfoClass CurrentTargetInfo = null;

        [System.Serializable]
        public class CurrentTargetInfoClass
        {
            [Header("�^�[�Q�b�g���̂��̂� Transform�i�N�_�j")]
            public Transform TargetSource;

            [Header("�^�[�Q�b�g�� IDamageable �Q�Ɓi�̗͓��j")]
            public IDamageable CurrentIDamageable;

            [Header("�^�[�Q�b�g�� ICombat �Q�Ɓi�_���[�W�ʒu���j")]
            public ICombat CurrentICombat;
        }
        #endregion

        #region Internal Components
        [Header("���L�I�u�W�F�N�g�v�[���i�ÓI�E��x���������j")]
        public static GameObject ObjectPool;

        [Header("Combat Text �� Canvas �Q�Ɓi�ÓI�E��x���������j")]
        public static GameObject CombatTextSystemObject;

        [Header("���� AI �� NavMeshAgent �Q��")]
        [HideInInspector] public NavMeshAgent m_NavMeshAgent;

        [Header("���� AI �� BoxCollider �Q��")]
        [HideInInspector] public BoxCollider AIBoxCollider;

        [Header("���� AI �� Animator �Q��")]
        [HideInInspector] public Animator AIAnimator;

        [Header("�L��������Ă���̌o�ߎ����iTime.time ���L�^�j")]
        [HideInInspector] public float TimeSinceEnabled;
        #endregion

        #region AI Components
        [Header("���m�iEmeraldDetection�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldDetection DetectionComponent;

        [Header("�s���iEmeraldBehaviors�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldBehaviors BehaviorsComponent;

        [Header("�ړ��iEmeraldMovement�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldMovement MovementComponent;

        [Header("�A�j���iEmeraldAnimation�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldAnimation AnimationComponent;

        [Header("�퓬�iEmeraldCombat�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldCombat CombatComponent;

        [Header("�T�E���h�iEmeraldSounds�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldSounds SoundComponent;

        [Header("�̗́iEmeraldHealth�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldHealth HealthComponent;

        [Header("�œK���iEmeraldOptimization�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldOptimization OptimizationComponent;

        [Header("IK�iEmeraldInverseKinematics�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldInverseKinematics InverseKinematicsComponent;

        [Header("UnityEvent ���b�p�[�iEmeraldEvents�j�Q��")]
        [HideInInspector] public EmeraldEvents EventsComponent;

        [Header("�f�o�b�K�iEmeraldDebugger�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldDebugger DebuggerComponent;

        [Header("UI�iEmeraldUI�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldUI UIComponent;

        [Header("����/�A�C�e���iEmeraldItems�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldItems ItemsComponent;

        [Header("�T�E���h���m�iEmeraldSoundDetector�j�Q��")]
        [HideInInspector] public EmeraldSoundDetector SoundDetectorComponent;

        [Header("�^�[�Q�b�g�ʒu�␳�iTargetPositionModifier�j�Q��")]
        [HideInInspector] public TargetPositionModifier TPMComponent;

        [Header("���ʃ_���[�W�iLocationBasedDamage�j�Q��")]
        [HideInInspector] public LocationBasedDamage LBDComponent;

        [Header("�Օ��iEmeraldCover�j�R���|�[�l���g�Q��")]
        [HideInInspector] public EmeraldCover CoverComponent;
        #endregion

        // Initialize Emerald AI and its components
        void Awake()
        {
            // --- ��v�R���|�[�l���g�̎擾�ƐÓI�V�X�e���̏����� ---
            MovementComponent = GetComponent<EmeraldMovement>();                     // �ړ�
            AnimationComponent = GetComponent<EmeraldAnimation>();                   // �A�j��
            SoundComponent = GetComponent<EmeraldSounds>();                          // �T�E���h
            DetectionComponent = GetComponent<EmeraldDetection>();                   // ���m
            BehaviorsComponent = GetComponent<EmeraldBehaviors>();                   // �s��
            CombatComponent = GetComponent<EmeraldCombat>();                         // �퓬
            HealthComponent = GetComponent<EmeraldHealth>();                         // �̗�
            OptimizationComponent = GetComponent<EmeraldOptimization>();             // �œK��
            EventsComponent = GetComponent<EmeraldEvents>();                         // �C�x���g
            DebuggerComponent = GetComponent<EmeraldDebugger>();                     // �f�o�b�K
            UIComponent = GetComponent<EmeraldUI>();                                 // UI
            ItemsComponent = GetComponent<EmeraldItems>();                           // �A�C�e��
            SoundDetectorComponent = GetComponent<EmeraldSoundDetector>();           // �����m
            InverseKinematicsComponent = GetComponent<EmeraldInverseKinematics>();   // IK
            CoverComponent = GetComponent<EmeraldCover>();                           // �Օ�
            TPMComponent = GetComponent<TargetPositionModifier>();                   // �ʒu�␳
            m_NavMeshAgent = GetComponent<NavMeshAgent>();                           // NavMeshAgent
            AIBoxCollider = GetComponent<BoxCollider>();                             // BoxCollider
            AIAnimator = GetComponent<Animator>();                                    // Animator
            InitializeEmeraldObjectPool();                                           // �I�u�W�F�N�g�v�[���������i�ÓI�E��x�����j
            InitializeCombatText();                                                  // �R���o�b�g�e�L�X�g�������i�ÓI�E��x�����j
        }

        void OnEnable()
        {
            TimeSinceEnabled = Time.time; // �L�������ꂽ�������L�^

            // AI �����Ɏ��S��ԂŗL�������ꂽ�ꍇ�A�f�t�H���g��Ԃփ��Z�b�g�B
            // �i�I�u�W�F�N�g�v�[�����O��X�|�[���V�X�e�����p����z��j
            if (AnimationComponent.IsDead)
            {
                ResetAI();
            }
        }

        /// <summary>
        /// �i���{��jEmerald �̃I�u�W�F�N�g�v�[�������������܂��B
        /// ObjectPool �͐ÓI�ϐ��̂��߁A�ŏ��� 1 ��̂ݐ������܂��B
        /// </summary>
        void InitializeEmeraldObjectPool()
        {
            if (EmeraldSystem.ObjectPool == null)
            {
                EmeraldSystem.ObjectPool = new GameObject();
                EmeraldSystem.ObjectPool.name = "Emerald AI Pool"; // ���O�͌Œ�
                EmeraldObjectPool.Clear();                         // �����v�[�����N���A
            }
        }

        /// <summary>
        /// �i���{��jEmerald �̃R���o�b�g�e�L�X�g�V�X�e�������������܂��B
        /// CombatTextSystemObject �͐ÓI�ϐ��̂��߁A�ŏ��� 1 ��̂ݐ������܂��B
        /// </summary>
        void InitializeCombatText()
        {
            if (EmeraldSystem.CombatTextSystemObject == null)
            {
                GameObject m_CombatTextSystem = Instantiate((GameObject)Resources.Load("Combat Text System") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextSystem.name = "Combat Text System"; // �V�X�e���{��

                GameObject m_CombatTextCanvas = Instantiate((GameObject)Resources.Load("Combat Text Canvas") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextCanvas.name = "Combat Text Canvas"; // �\���L�����o�X

                EmeraldSystem.CombatTextSystemObject = m_CombatTextCanvas;
                CombatTextSystem.Instance.CombatTextCanvas = m_CombatTextCanvas;
                CombatTextSystem.Instance.Initialize();         // �V���O���g���̏�����
            }
        }

        /// <summary>
        /// �i���{��j���ׂĂ̎�v�X�N���v�g�� EmeraldSystem ����X�V���܂��B
        /// Health �� 0 �ȉ��̏ꍇ�͍X�V���X�L�b�v���܂��B
        /// </summary>
        void Update()
        {
            if (HealthComponent.CurrentHealth <= 0) return;     // ���S���͉������Ȃ�

            AnimationComponent.AnimationUpdate();               // �A�j���[�V�����̃J�X�^�� Update
            MovementComponent.MovementUpdate();                 // �ړ��̃J�X�^�� Update
            BehaviorsComponent.BehaviorUpdate();                // �s���̃J�X�^�� Update
            DetectionComponent.DetectionUpdate();               // ���m�̃J�X�^�� Update
            CombatComponent.CombatUpdate();                     // �퓬�̃J�X�^�� Update
            if (DebuggerComponent) DebuggerComponent.DebuggerUpdate(); // �f�o�b�K�̃J�X�^�� Update�i���ݎ��̂݁j
        }

        /// <summary>
        /// �i���{��jAI ��������Ԃփ��Z�b�g���܂��i���X�|�[�����ŗL�p�j�B
        /// </summary>
        public void ResetAI()
        {
            EmeraldAPI.Combat.ResetAI(this);                    // API ��ʂ��Ĉꊇ���Z�b�g
        }
    }
}
