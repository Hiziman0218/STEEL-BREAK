using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Threading;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// �yEmeraldAIDuplicatorManager�z
    /// �Q��AI�iReference AI�j�̊e��R���|�[�l���g�ݒ���A�w�肵��������AI�ɕ������邽�߂̃G�f�B�^�E�B���h�E�B
    /// �E�K�{�^�I�v�V�����e�R���|�[�l���g�̗L�����g�O���őI��
    /// �E����I�u�W�F�N�g�AIK�A���O�h�[���ALBD�i���ʃ_���[�W�j�ȂǕ��G�Ȑݒ�����O��v�ŃR�s�[�\
    /// �E�i���o�[�\���A�m�F�_�C�A���O�Ȃǂ�UI����
    /// </summary>
    public class EmeraldAIDuplicatorManager : EditorWindow
    {
        [Header("�������AI�z��i�����Ƀh���b�O���h���b�v�őΏ�AI��ǉ��j")]
        public GameObject[] AIToDuplicateTo;

        [Header("�� AIToDuplicateTo �� SerializedProperty �Q�Ɓi�����p�j")]
        SerializedProperty AIToDuplicateToProp;

        // Main Components�i�K�{�R���|�[�l���g�j
        [Header("�K�{: EmeraldSystem �𕡐����邩")]
        public bool m_EmeraldSystem = true;

        [Header("�K�{: EmeraldAnimation �𕡐����邩")]
        public bool m_EmeraldAnimation = true;

        [Header("�K�{: EmeraldCombat �𕡐����邩")]
        public bool m_EmeraldCombat = true;

        [Header("�K�{: EmeraldSounds �𕡐����邩")]
        public bool m_EmeraldSounds = true;

        [Header("�K�{: EmeraldMovement �𕡐����邩")]
        public bool m_EmeraldMovement = true;

        [Header("�K�{: EmeraldHealth �𕡐����邩")]
        public bool m_EmeraldHealth = true;

        [Header("�K�{: EmeraldBehaviors �𕡐����邩")]
        public bool m_EmeraldBehaviors = true;

        [Header("�K�{: EmeraldDetection �𕡐����邩")]
        public bool m_EmeraldDetection = true;

        // Optional Components�i�C�ӃR���|�[�l���g�j
        [Header("�C��: EmeraldEvents �𕡐����邩")]
        public bool m_EmeraldEvents = false;

        [Header("�C��: EmeraldItems�i�A�C�e��/����j�𕡐����邩")]
        public bool m_EmeraldItems = false;

        [Header("�C��: EmeraldInverseKinematics�i�A�j���[�V�������M���O�j�𕡐����邩")]
        public bool m_EmeraldInverseKinematics = false;

        [Header("�C��: EmeraldOptimization�i�����ɂ��œK���j�𕡐����邩")]
        public bool m_EmeraldOptimization = false;

        [Header("�C��: EmeraldSoundDetector�i�����m�j�𕡐����邩")]
        public bool m_EmeraldSoundDetector = false;

        [Header("�C��: EmeraldUI�i�w���X�o�[/���O�\���j�𕡐����邩")]
        public bool m_EmeraldUI = false;

        [Header("�C��: EmeraldFootsteps�i�t�b�g�X�e�b�v�j�𕡐����邩")]
        public bool m_EmeraldFootsteps = false;

        [Header("�C��: EmeraldDebugger�i�f�o�b�O�\���j�𕡐����邩")]
        public bool m_EmeraldDebugger = false;

        [Header("�C��: ���O�h�[���֘A�R���|�[�l���g�𕡐����邩")]
        public bool m_RagdollComponents = false;

        [Header("�C��: LBD�i���ʃ_���[�W�j�R���|�[�l���g�𕡐����邩")]
        public bool m_LBDComponents = false;

        [Header("�C��: TargetPositionModifier�i�_���ʒu�␳�j�𕡐����邩")]
        public bool m_TargetPositionModifier = false;

        [Header("�E�B���h�E�^�C�g������GUI�X�^�C���i�����p�j")]
        GUIStyle TitleStyle;

        [Header("�E�B���h�E����A�C�R���i�����p�j")]
        Texture AIDuplicatorIcon;

        [Header("�X�N���[���ʒu�i�����p�j")]
        Vector2 scrollPos;

        [Header("�Q��AI�i�R�s�[���j")]
        GameObject ReferenceAI;

        [Header("���̃E�B���h�E���g�� SerializedObject�i�����p�j")]
        SerializedObject serializedObject;

        /// <summary>
        /// ���j���[����E�B���h�E���J��
        /// </summary>
        [MenuItem("Window/Emerald AI/AI �����}�l�[�W�� #%d", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldAIDuplicatorManager), false, "AI �����}�l�[�W��");
            APS.minSize = new Vector2(300, 250f); //500
        }

        void OnEnable()
        {
            if (AIDuplicatorIcon == null) AIDuplicatorIcon = Resources.Load("Editor Icons/EmeraldDuplicatorManager") as Texture;
            serializedObject = new SerializedObject(this);
            AIToDuplicateToProp = serializedObject.FindProperty("AIToDuplicateTo");
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // �㕔���]��
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "AI �����}�l�[�W��", AIDuplicatorIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  // �㕔�E�]��
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // �������]��
            EditorGUILayout.BeginVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "AI ���� �ݒ�",
                "AI Duplicator Manager �́A�Q��AI�iReference AI�j���瑼��AI�֐ݒ�𕡐��ł��鋭�͂ȃc�[���ł��B���ɐݒ�ς݂̃R���|�[�l���g���g���񂹂邽�߁A�J����啝�Ɍ������ł��܂��B����A�C�e���A�A�j���[�V�������M���O�iIK�j�A���O�h�[���ALBD�i���ʃ_���[�W�j�ȂǕ��G�ȏ����R�s�[�\�ł��B",
                true);

            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("���̃t�B�[���h�͋�ɂł��܂���B", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }
            ReferenceAI = (GameObject)EditorGUILayout.ObjectField("�Q��AI", ReferenceAI, typeof(GameObject), true);
            DescriptionElement("����AI�֕�������ۂ̊�ƂȂ�AI���w�肵�܂��B");
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            DisplayToggleOptions();

            //GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "AI ���X�g",
                "���̃��X�g�ɕ�����AI���h���b�O���h���b�v�Œǉ����Ă��������B�����Ɋ܂܂�邷�ׂẴI�u�W�F�N�g�ցA�Q��AI�̐ݒ肪�K�p����܂��B",
                true);
            EditorGUILayout.PropertyField(AIToDuplicateToProp);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            DisplayDuplicateAIButton();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); // �����E�]��
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// �g�O���Q�i�K�{�^�C�ӃR���|�[�l���g�j�̃��C�A�E�g�\��
        /// </summary>
        void DisplayToggleOptions()
        {
            if (position.width > 500)
            {
                // �K�{�R���|�[�l���g
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                // �C�ӃR���|�[�l���g
                GUILayout.Space(10);
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
            else
            {
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// �K�{�R���|�[�l���g�̐����ƃg�O��
        /// </summary>
        void RequiredSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "(�K�{�R���|�[�l���g)",
                "Emerald AI �𓮍삳���邽�߂ɕK�v�ȃR���|�[�l���g�Q�ł��B�I���ɂȂ��Ă��鍀�ڂ́A�Z�b�g�A�b�v���Ɏ����Œǉ�����܂��B�e�R���|�[�l���g�ɂ͐�p�G�f�B�^������A�ڍאݒ肪�\�ł��B�e���ڂ� ? �A�C�R���Ƀ}�E�X�I�[�o�[����Ɛ����c�[���`�b�v���\������܂��B",
                true);

            m_EmeraldSystem = ToggleComponentElement(true, "Emerald System�i��Ձj", "���ׂĂ̕K�{�X�N���v�g�̊Ǘ��ƍX�V���s���AEmerald AI �̊�ՃX�N���v�g�ł��B");
            m_EmeraldAnimation = ToggleComponentElement(m_EmeraldAnimation, "Emerald Animation�i�A�j���Ǘ��j", "Animation Profile �����AI�̃A�j���[�V�����Ǘ����s���܂��B", false);
            m_EmeraldCombat = ToggleComponentElement(m_EmeraldCombat, "Emerald Combat�i�퓬�j", "�퓬�@�\�A�A�r���e�B�ݒ�A�e��퓬�֘A�̃I�v�V������񋟂��܂��B", false);
            m_EmeraldSounds = ToggleComponentElement(m_EmeraldSounds, "Emerald Sounds�i�T�E���h�j", "Sound Profile �����AI�̃T�E���h�Ǘ����s���܂��B", false);
            m_EmeraldMovement = ToggleComponentElement(m_EmeraldMovement, "Emerald Movement�i�ړ��j", "�ړ��A��]�A�A���C�������g�Ɋւ���ݒ���Ǘ����܂��B", false);
            m_EmeraldHealth = ToggleComponentElement(m_EmeraldHealth, "Emerald Health�i�_���[�W�j", "AI �ɔ�_���[�W�@�\��t�^���܂��B�O������� IDamageable �C���^�[�t�F�C�X�o�R�ŗ��p���܂��B", false);
            m_EmeraldBehaviors = ToggleComponentElement(m_EmeraldBehaviors, "Emerald Behaviors�i�����j", "�v���Z�b�g�̋����i�r�w�C�r�A�j���g�p�\�ɂ��܂��B�K�v�ɉ����� EmeraldBehavior ���p�����ăJ�X�^���������\�ł��B", false);
            m_EmeraldDetection = ToggleComponentElement(m_EmeraldDetection, "Emerald Detection�i���m�j", "AI �Ƀ^�[�Q�b�g�̎��F�E���m�@�\��t�^���܂��B", false);
        }

        /// <summary>
        /// �C�ӃR���|�[�l���g�̐����ƃg�O��
        /// </summary>
        void OptionalSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "(�C�ӃR���|�[�l���g)",
                "Emerald AI �ɒǉ��@�\��񋟂���R���|�[�l���g�Q�ł��B�I���ɂȂ��Ă��鍀�ڂ̓Z�b�g�A�b�v���Ɏ����Œǉ�����܂��B�e�R���|�[�l���g�ɂ͐�p�G�f�B�^������A�ڍאݒ肪�\�ł��B�e���ڂ� ? �A�C�R���Ƀ}�E�X�I�[�o�[����Ɛ����c�[���`�b�v���\������܂��B",
                true);

            m_EmeraldEvents = ToggleComponentElement(m_EmeraldEvents, "Emerald Events�i�C�x���g�j", "AI �̑����̃A�N�V�����ɑ΂��ăJ�X�^���C�x���g��ݒ�\�ɂ��܂��B", false);
            m_EmeraldItems = ToggleComponentElement(m_EmeraldItems, "Emerald Items�i�A�C�e��/�����j", "�h���b�v�A�C�e���A�����A�C�e���Ȃǂ�AI�ɐݒ�ł���悤�ɂ��܂��B", false);
            m_EmeraldInverseKinematics = ToggleComponentElement(m_EmeraldInverseKinematics, "Emerald Inverse Kinematics�iIK�j", "Unity Animation Rigging ��p����IK��AI�Ŏg�p�\�ɂ��܂��B", false);
            m_EmeraldOptimization = ToggleComponentElement(m_EmeraldOptimization, "Emerald Optimization�i�œK���j", "�J��������O��AI�𖳌������A�����Ă��Ȃ�AI�̃R�X�g���팸���ăp�t�H�[�}���X�����コ���܂��B", false);
            m_EmeraldUI = ToggleComponentElement(m_EmeraldUI, "Emerald UI�iUI�\���j", "AI�̓���Ƀw���X�o�[�▼�O��\���������UI���g�p�\�ɂ��܂��B", false);
            m_EmeraldFootsteps = ToggleComponentElement(m_EmeraldFootsteps, "Emerald Footsteps�i�����j", "���o�����n�ʂɉ����ăt�b�g�X�e�b�v�̃T�E���h�^�G�t�F�N�g���Đ����܂��B�n�`�̃e�N�X�`����I�u�W�F�N�g�̃^�O���g�p�\�ł��B\n\n��: �t�b�g�X�e�b�v�̃A�j���[�V�����C�x���g�ݒ肪�K�v�ł��B�ړ��A�j���ɃC�x���g��ݒ肵�Ă��������B", false);
            m_EmeraldSoundDetector = ToggleComponentElement(m_EmeraldSoundDetector, "Emerald Sound Detector�i�����m�j", "�v���C���[��O���\�[�X�������鉹��AI����������悤�ɂ��܂��B", false);
            m_EmeraldDebugger = ToggleComponentElement(m_EmeraldDebugger, "Emerald Debugger�i�f�o�b�O�j", "�f�o�b�O���O�A���C���A�o�H���Ȃǂ̃f�o�b�O�\����L�������܂��B���̓����J���ɖ𗧂��܂��B", false);
            m_RagdollComponents = ToggleComponentElement(m_RagdollComponents, "Ragdoll Components�i���O�h�[���j", "�R���C�_�[�A�W���C���g�A���W�b�h�{�f�B���܂ރ��O�h�[���\�����R�s�[���܂��B\n\n��: �{�[����������ł���K�v������܂��B��v���Ȃ��ꍇ�A���̐ݒ�͖�������܂��B", false);
            m_LBDComponents = ToggleComponentElement(m_LBDComponents, "Location Based Damage Components�i���ʃ_���[�W�j", "LBD �R���|�[�l���g�����蓖�ď��������܂��B��v����R���C�_�[�ɑ΂��A�Q��AI�̃_���[�W�{�����R�s�[���܂��B\n\n��: �_���[�W�{���̃R�s�[�ɂ̓{�[�����̈�v���K�v�ł��B��v���Ȃ��{�[���̓R�s�[����܂���B", false);
            m_TargetPositionModifier = ToggleComponentElement(m_TargetPositionModifier, "Target Position Modifier�i�_���ʒu�␳�j", "�^�[�Q�b�g�̑_���ʒu�̍�����␳���A�Ə����x�����コ���܂��B\n\n��: �{�[����������ł���K�v������܂��B��v���Ȃ��ꍇ�A���̐ݒ�͖�������܂��B", false);
        }

        /// <summary>
        /// �uAI �𕡐��v�{�^���Ɗm�F�^�G���[���b�Z�[�W�\��
        /// </summary>
        void DisplayDuplicateAIButton()
        {
            GUILayout.Space(15);
            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("�����v���Z�X�����s����O�ɁA�Q��AI���w�肵�Ă��������B", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.BeginDisabledGroup(ReferenceAI == null);
            if (GUILayout.Button("AI �𕡐�", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("AI �����}�l�[�W��", "AI ���X�g���̑SAI�ɁA�I�������ݒ�𕡐����܂��B��낵���ł����H���̑���͌��ɖ߂��܂���B", "�͂�", "�L�����Z��"))
                {
                    DuplicateAI();
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(25);
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// ���ۂ̕��������{��
        /// </summary>
        void DuplicateAI()
        {
            for (int i = 0; i < AIToDuplicateTo.Length; i++)
            {
                GameObject G = AIToDuplicateTo[i];
                // �i���o�[��\�����ĕ����̐i�s�󋵂����[�U�[�Ɏ���
                EditorUtility.DisplayProgressBar("AI �𕡐���...", (i + 1) + " ���� / �S " + (AIToDuplicateTo.Length) + " ��", (float)i / (float)AIToDuplicateTo.Length);

                if (AIToDuplicateTo[i].GetComponent<Animator>() != null)
                {
                    UnpackPrefab(AIToDuplicateTo[i]);

                    CopyAnimationComponent(G);
                    CopySoundsComponent(G);
                    CopyItemsComponent(G);
                    CopySoundDetectorComponent(G);
                    CopyUIComponent(G);
                    CopyFootstepsComponent(G);
                    CopyDebuggerComponent(G);
                    CopyTPMComponent(G);
                    CopyBehaviorsComponent(G);
                    CopyDetectionComponent(G);
                    CopyMovementComponent(G);
                    CopyHealthComponent(G);
                    CopyEmeraldBaseComponent(G);
                    CopyEventsComponent(G);
                    CopyCombatComponent(G);
                    CopyInverseKinematicsComponent(G);
                    CopyRagdollComponents(G);
                    CopyLBDComponent(G);
                    CopyOptimizationComponent(G);

                    CopyBoxCollider(G);
                    CopyAudioSource(G);
                    CopyTagAndLayer(G);

                    G.GetComponent<BoxCollider>().size = ReferenceAI.GetComponent<BoxCollider>().size;
                    G.GetComponent<BoxCollider>().center = ReferenceAI.GetComponent<BoxCollider>().center;

                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<EmeraldSystem>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<Animator>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<BoxCollider>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<UnityEngine.AI.NavMeshAgent>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<AudioSource>());
                }
                else
                {
                    Debug.Log("�I�u�W�F�N�g '" + G.name + "' �ɂ� Animator Controller ���������߁A�������X�L�b�v���܂����B");
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// �n���ꂽ�R���|�[�l���g���C���X�y�N�^�����ֈړ�
        /// </summary>
        void MoveToBottom(GameObject AIToDuplicateTo, Component ComponentToMove)
        {
            Component[] AllComponents = AIToDuplicateTo.GetComponents<Component>();

            for (int i = 0; i < AllComponents.Length; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(ComponentToMove);
            }
        }

        /// <summary>
        /// ������ GameObject ���v���n�u�Ȃ� Unpack�i�����j����
        /// </summary>
        void UnpackPrefab(GameObject ObjectToUnpack)
        {
            PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(ObjectToUnpack);

            // �v���n�u�̏ꍇ�̂� Unpack �����{
            if (m_AssetType != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(ObjectToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// BoxCollider ���R�s�[
        /// </summary>
        void CopyBoxCollider(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<BoxCollider>());

            if (G.GetComponent<BoxCollider>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
            else
            {
                G.AddComponent<BoxCollider>();
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
        }

        /// <summary>
        /// AudioSource ���R�s�[
        /// </summary>
        void CopyAudioSource(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<AudioSource>());

            if (G.GetComponent<AudioSource>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
            else
            {
                G.AddComponent<AudioSource>();
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
        }

        /// <summary>
        /// Tag �� Layer ���R�s�[
        /// </summary>
        void CopyTagAndLayer(GameObject G)
        {
            G.tag = ReferenceAI.tag;
            G.layer = ReferenceAI.layer;
        }

        /// <summary>
        /// EmeraldSystem�i��Ձj���R�s�[
        /// </summary>
        void CopyEmeraldBaseComponent(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSystem>());

            if (G.GetComponent<EmeraldSystem>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
            else
            {
                G.AddComponent<EmeraldSystem>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
        }

        /// <summary>
        /// Movement ���R�s�[
        /// </summary>
        void CopyMovementComponent(GameObject G)
        {
            if (!m_EmeraldMovement)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldMovement>());

            if (G.GetComponent<EmeraldMovement>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
            else
            {
                G.AddComponent<EmeraldMovement>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
        }

        /// <summary>
        /// Behaviors ���R�s�[
        /// </summary>
        void CopyBehaviorsComponent(GameObject G)
        {
            if (!m_EmeraldBehaviors)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldBehaviors>());

            if (G.GetComponent<EmeraldBehaviors>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
            else
            {
                G.AddComponent<EmeraldBehaviors>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
        }

        /// <summary>
        /// Detection ���R�s�[�iHeadTransform �̎������o�t���j
        /// </summary>
        void CopyDetectionComponent(GameObject G)
        {
            if (!m_EmeraldDetection)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDetection>());

            if (G.GetComponent<EmeraldDetection>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }
            else
            {
                G.AddComponent<EmeraldDetection>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }

            AutoFindHeadTransform(G);
        }

        /// <summary>
        /// Animation ���R�s�[�iAvatar ��ێ��j
        /// </summary>
        void CopyAnimationComponent(GameObject G)
        {
            if (!m_EmeraldAnimation)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldAnimation>());

            if (G.GetComponent<EmeraldAnimation>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }
            else
            {
                G.AddComponent<EmeraldAnimation>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }

            // �Q��AI�� Animator �l���R�s�[���邪�AAvatar �͌��̂��̂��ێ�
            Avatar m_Avatar = G.GetComponent<Animator>().avatar;
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<Animator>());
            ComponentUtility.PasteComponentValues(G.GetComponent<Animator>());
            G.GetComponent<Animator>().avatar = m_Avatar;
        }

        /// <summary>
        /// Combat ���R�s�[�iAttack Transform �̒����t���j
        /// </summary>
        void CopyCombatComponent(GameObject G)
        {
            if (!m_EmeraldCombat)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldCombat>());

            if (G.GetComponent<EmeraldCombat>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }
            else
            {
                G.AddComponent<EmeraldCombat>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }

            CopyAttackTransforms(G);
        }

        /// <summary>
        /// Sounds ���R�s�[
        /// </summary>
        void CopySoundsComponent(GameObject G)
        {
            if (!m_EmeraldSounds)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSounds>());

            if (G.GetComponent<EmeraldSounds>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
            else
            {
                G.AddComponent<EmeraldSounds>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
        }

        /// <summary>
        /// Health ���R�s�[
        /// </summary>
        void CopyHealthComponent(GameObject G)
        {
            if (!m_EmeraldHealth)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldHealth>());

            if (G.GetComponent<EmeraldHealth>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
            else
            {
                G.AddComponent<EmeraldHealth>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
        }

        /// <summary>
        /// Events ���R�s�[
        /// </summary>
        void CopyEventsComponent(GameObject G)
        {
            if (!m_EmeraldEvents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldEvents>());

            if (G.GetComponent<EmeraldEvents>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
            else
            {
                G.AddComponent<EmeraldEvents>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
        }

        /// <summary>
        /// Items ���R�s�[�i����I�u�W�F�N�g�̕���/�z�u���s���j
        /// </summary>
        void CopyItemsComponent(GameObject G)
        {
            if (!m_EmeraldItems)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldItems>());

            if (G.GetComponent<EmeraldItems>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }
            else
            {
                G.AddComponent<EmeraldItems>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }

            CopyWeaponObjects(G);
        }

        /// <summary>
        /// SoundDetector ���R�s�[
        /// </summary>
        void CopySoundDetectorComponent(GameObject G)
        {
            if (!m_EmeraldSoundDetector)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<SoundDetection.EmeraldSoundDetector>());

            if (G.GetComponent<SoundDetection.EmeraldSoundDetector>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
            else
            {
                G.AddComponent<SoundDetection.EmeraldSoundDetector>();
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
        }

        /// <summary>
        /// UI ���R�s�[
        /// </summary>
        void CopyUIComponent(GameObject G)
        {
            if (!m_EmeraldUI)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldUI>());

            if (G.GetComponent<EmeraldUI>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
            else
            {
                G.AddComponent<EmeraldUI>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
        }

        /// <summary>
        /// Footsteps ���R�s�[�i���g�����X�t�H�[���̊��蓖�Ă��s���j
        /// </summary>
        void CopyFootstepsComponent(GameObject G)
        {
            if (!m_EmeraldFootsteps)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldFootsteps>());

            if (G.GetComponent<EmeraldFootsteps>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }
            else
            {
                G.AddComponent<EmeraldFootsteps>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }

            CopyFeetTransforms(G);
        }

        /// <summary>
        /// ���� Transform �𖼑O��v�Ŋ��蓖��
        /// </summary>
        void CopyFeetTransforms(GameObject G)
        {
            var m_FootstepsComponent = G.GetComponent<EmeraldFootsteps>();
            m_FootstepsComponent.FeetTransforms.Clear();
            var m_FootstepsReference = ReferenceAI.GetComponent<EmeraldFootsteps>();

            if (m_FootstepsReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    for (int i = 0; i < m_FootstepsReference.FeetTransforms.Count; i++)
                    {
                        if (m_FootstepsReference.FeetTransforms[i].name == t.name)
                        {
                            m_FootstepsComponent.FeetTransforms.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Debugger ���R�s�[
        /// </summary>
        void CopyDebuggerComponent(GameObject G)
        {
            if (!m_EmeraldDebugger)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDebugger>());

            if (G.GetComponent<EmeraldDebugger>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
            else
            {
                G.AddComponent<EmeraldDebugger>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
        }

        /// <summary>
        /// TargetPositionModifier ���R�s�[�iTransform �̈�v���蓖�ĕt���j
        /// </summary>
        void CopyTPMComponent(GameObject G)
        {
            if (!m_TargetPositionModifier)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<TargetPositionModifier>());

            if (G.GetComponent<TargetPositionModifier>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }
            else
            {
                G.AddComponent<TargetPositionModifier>();
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }

            CopyTPM(G);
        }

        /// <summary>
        /// TPM �� TransformSource �𖼑O��v�Ŋ��蓖��
        /// </summary>
        void CopyTPM(GameObject G)
        {
            var m_TMPComponent = G.GetComponent<TargetPositionModifier>();
            var TMPReference = ReferenceAI.GetComponent<TargetPositionModifier>();

            if (TMPReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    if (t.name == TMPReference.TransformSource.name)
                    {
                        m_TMPComponent.TransformSource = t;
                    }
                }
            }
        }

        /// <summary>
        /// IK ���R�s�[�iRigBuilder�w�̍č\�z�^�Q�ƍ����ւ��܂ށj
        /// </summary>
        void CopyInverseKinematicsComponent(GameObject G)
        {
            if (!m_EmeraldInverseKinematics)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldInverseKinematics>());

            if (G.GetComponent<EmeraldInverseKinematics>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }
            else
            {
                G.AddComponent<EmeraldInverseKinematics>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }

            CopyInverseKinematicsComponents(G);
        }

        /// <summary>
        /// ���O�h�[���iConfigurableJoint/CharacterJoint�j�\�����R�s�[
        /// </summary>
        void CopyRagdollComponents(GameObject G)
        {
            if (!m_RagdollComponents)
                return;

            CopyRagdollConfigurableJointComponents(G);
            CopyRagdollCharacterJointComponents(G);
        }

        /// <summary>
        /// LBD�i���ʃ_���[�W�j���R�s�[���A�R���C�_�[��������
        /// </summary>
        void CopyLBDComponent(GameObject G)
        {
            if (!m_LBDComponents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<LocationBasedDamage>());

            if (G.GetComponent<LocationBasedDamage>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }
            else
            {
                G.AddComponent<LocationBasedDamage>();
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }

            IntializeLBDComponent(G);
        }

        /// <summary>
        /// Optimization�i�œK���j���R�s�[���ARenderer �𖼑O��v�Ŋ��蓖��
        /// </summary>
        void CopyOptimizationComponent(GameObject G)
        {
            if (!m_EmeraldOptimization)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldOptimization>());

            if (G.GetComponent<EmeraldOptimization>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }
            else
            {
                G.AddComponent<EmeraldOptimization>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }

            FindSingleRenderer(G);
        }

        /// <summary>
        /// �P�� Renderer �^�̍œK���Ώۂ𖼑O��v�Ō����E���蓖��
        /// </summary>
        void FindSingleRenderer(GameObject G)
        {
            EmeraldOptimization LocalOptimization = G.GetComponent<EmeraldOptimization>();
            EmeraldOptimization RefOptimization = ReferenceAI.GetComponent<EmeraldOptimization>();

            // �ȉ��̏����ł͏������Ȃ�
            if (RefOptimization == null || RefOptimization.AIRenderer == null || RefOptimization.MeshType == EmeraldOptimization.MeshTypes.LODGroup)
                return;

            LocalOptimization.AIRenderer = null;

            // �q�K�w��񋓂��Ė��O��v��T��
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            for (int i = 0; i < AllChildTransforms.Count; i++)
            {
                if (AllChildTransforms[i].gameObject.name == RefOptimization.AIRenderer.gameObject.name)
                {
                    if (AllChildTransforms[i].gameObject.GetComponent<Renderer>() != null)
                        LocalOptimization.AIRenderer = AllChildTransforms[i].gameObject.GetComponent<Renderer>();
                }
            }

            if (LocalOptimization.AIRenderer == null)
            {
                Debug.Log("�I�u�W�F�N�g '" + G.name + "' �ƎQ��AI�ň�v���� Renderer ���������܂���ł����BOptimization �R���|�[�l���g����蓮�� AI Renderer �����蓖�ĂĂ��������B");
            }
        }

        /// <summary>
        /// ConfigurableJoint ��p�������O�h�[���\���̃R�s�[
        /// </summary>
        void CopyRagdollConfigurableJointComponents(GameObject G)
        {
            // �Q�Ƒ��̎��W
            List<ConfigurableJoint> ConfigurableJointList = new List<ConfigurableJoint>(ReferenceAI.GetComponentsInChildren<ConfigurableJoint>());

            // ������̎擾
            var m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();

            // �q�K�w���
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_ConfigurableJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (ConfigurableJointList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name) && AllChildTransforms[i].gameObject.GetComponent<ConfigurableJoint>() == null)
                    {
                        AllChildTransforms[i].gameObject.AddComponent<ConfigurableJoint>();
                    }
                }

                m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();
            }

            foreach (ConfigurableJoint C in m_ConfigurableJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && ConfigurableJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = ConfigurableJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_ConfigurableJoints.Find(go => go.name == RefJoint.name);

                        // ConfigurableJoint �̒l���R�s�[
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        // �Q�Ƒ��̃R���C�_�[������΃R�s�[
                        if (RefJoint.GetComponent<Collider>() != null)
                        {
                            UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        // ConnectedBody �𖼑O��v�Ŋ��蓖��
                        if (RefJoint != null && RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Find(go => go.name == RefJoint.connectedBody.transform.name);
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        // Rigidbody �̒l���R�s�[
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// CharacterJoint ��p�������O�h�[���\���̃R�s�[
        /// </summary>
        void CopyRagdollCharacterJointComponents(GameObject G)
        {
            List<Rigidbody> RigidbodyList = new List<Rigidbody>();
            var m_RigidbodysAIToCopy = ReferenceAI.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody C in m_RigidbodysAIToCopy)
            {
                if (C != null)
                {
                    if (!RigidbodyList.Contains(C) && C.gameObject != ReferenceAI)
                    {
                        RigidbodyList.Add(C);
                    }
                }
            }

            List<CharacterJoint> CharacterJointList = new List<CharacterJoint>(ReferenceAI.GetComponentsInChildren<CharacterJoint>());

            // ������
            var m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();

            // �q�K�w���
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_CharacterJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() != null))
                    {
                        AllChildTransforms[i].gameObject.AddComponent<CharacterJoint>();
                    }
                    else if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() == null))
                    {
                        // ���[�g�I�u�W�F�N�g�� Rigidbody/Collider �𕡐�
                        if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>() != null &&
                            RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>() != null)
                        {
                            // Rigidbody
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Rigidbody>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Rigidbody>());
                            }

                            // Collider
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Collider>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Collider>());
                            }
                        }
                    }
                }

                m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();
            }

            foreach (CharacterJoint C in m_CharacterJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && CharacterJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = CharacterJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_CharacterJoints.Find(go => go.name == RefJoint.name);

                        // CharacterJoint �̒l���R�s�[
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        // �R���C�_�[���R�s�[
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());

                        // �Ȃ���ΐV�K�ǉ��A����Βl�̂ݔ��f
                        if (LocalJoint.GetComponent<Collider>() == null)
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(LocalJoint.gameObject);
                        }
                        else
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        // ConnectedBody �𖼑O��v�Ŋ��蓖��
                        if (RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Where(go => go.name == RefJoint.connectedBody.transform.name).Single();
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        // Rigidbody �̒l���R�s�[
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// HeadTransform ���������o�i"root/Root/ROOT" �������� "head" ����T���j
        /// </summary>
        void AutoFindHeadTransform(GameObject G)
        {
            EmeraldDetection TempDetectionComponent = G.GetComponent<EmeraldDetection>();
            TempDetectionComponent.HeadTransform = null;

            foreach (Transform root in G.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 3; i++)
                {
                    // ���[�g�̎q�i�ő�3�j���� "root"/"Root"/"ROOT" ��T��
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT")
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            // "head" ���܂ޖ��O�� Transform �����o
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD"))
                            {
                                TempDetectionComponent.HeadTransform = t;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ����I�u�W�F�N�g�i�z���X�^�[�^�莝���j���R�s�[���A�����̃{�[���֔z�u
        /// �� ���ꃊ�O�O��
        /// </summary>
        void CopyWeaponObjects(GameObject G)
        {
            var m_ItemComponent = G.GetComponent<EmeraldItems>();

            for (int i = 0; i < m_ItemComponent.Type1EquippableWeapons.Count; i++)
            {
                GameObject HolsteredReference = m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject;

                if (HolsteredReference != null)
                {
                    GameObject HolsteredCopy = Instantiate(HolsteredReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HolsteredReference.transform.parent.name)
                        {
                            HolsteredCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HolsteredReference.transform);
                            ComponentUtility.PasteComponentValues(HolsteredCopy.transform);
                            HolsteredCopy.name = HolsteredReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject = HolsteredCopy;
                        }
                    }
                }

                GameObject HeldReference = m_ItemComponent.Type1EquippableWeapons[i].HeldObject;

                if (HeldReference != null)
                {
                    GameObject HeldCopy = Instantiate(HeldReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HeldReference.transform.parent.name)
                        {
                            HeldCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HeldReference.transform);
                            ComponentUtility.PasteComponentValues(HeldCopy.transform);
                            HeldCopy.name = HeldReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HeldObject = HeldCopy;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attack Transform�i�U���N�_�j���R�s�[���A�����{�[���֔z�u
        /// </summary>
        void CopyAttackTransforms(GameObject G)
        {
            var m_EmeraldCombat = G.GetComponent<EmeraldCombat>();

            // Weapon Type 1 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType1AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType1AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == TransformReference.parent.name)
                        {
                            TransformCopy.SetParent(t);
                            ComponentUtility.CopyComponent(TransformReference);
                            ComponentUtility.PasteComponentValues(TransformCopy.transform);
                            m_EmeraldCombat.WeaponType1AttackTransforms[i] = TransformCopy;
                        }
                    }
                }
            }

            // Weapon Type 2 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType2AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType2AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);
                    ComponentUtility.CopyComponent(TransformReference);
                    ComponentUtility.PasteComponentValues(TransformCopy.transform);
                    m_EmeraldCombat.WeaponType2AttackTransforms[i] = TransformCopy;
                }
            }
        }

        /// <summary>
        /// LBD �̏������i������AI�̃R���C�_�[�񋓂ƃ_���[�W�{���̃R�s�[�j
        /// </summary>
        void IntializeLBDComponent(GameObject G)
        {
            var m_LocationBasedDamageComponent = G.GetComponent<LocationBasedDamage>();
            m_LocationBasedDamageComponent.ColliderList.Clear();
            var m_ReferenceLBD = G.GetComponent<LocationBasedDamage>();
            var m_Colliders = G.GetComponentsInChildren<Collider>();
            m_LocationBasedDamageComponent.LBDComponentsLayer = m_ReferenceLBD.LBDComponentsLayer;

            foreach (Collider C in m_Colliders)
            {
                if (C != null)
                {
                    LocationBasedDamage.LocationBasedDamageClass lbdc = new LocationBasedDamage.LocationBasedDamageClass(C, 1);
                    if (!LocationBasedDamage.LocationBasedDamageClass.Contains(m_LocationBasedDamageComponent.ColliderList, lbdc) && C.gameObject != G)
                    {
                        m_LocationBasedDamageComponent.ColliderList.Add(lbdc);
                    }
                }
            }

            for (int i = 0; i < m_ReferenceLBD.ColliderList.Count; i++)
            {
                for (int j = 0; j < m_LocationBasedDamageComponent.ColliderList.Count; j++)
                {
                    if (m_ReferenceLBD.ColliderList[i].ColliderObject.name == m_LocationBasedDamageComponent.ColliderList[j].ColliderObject.name)
                    {
                        m_LocationBasedDamageComponent.ColliderList[j].DamageMultiplier = m_ReferenceLBD.ColliderList[i].DamageMultiplier;
                    }
                }
            }
        }

        /// <summary>
        /// IK �p Rig �̕����ƎQ�ƍ����ւ��iRigBuilder �w�č\�z�AMultiAim/TwoBoneIK �̊����j
        /// </summary>
        void CopyInverseKinematicsComponents(GameObject G)
        {
            var m_InverseKinematicsComponent = G.GetComponent<EmeraldInverseKinematics>();
            var m_RigBuilderComponent = G.GetComponent<UnityEngine.Animations.Rigging.RigBuilder>();

            if (m_RigBuilderComponent != null)
                m_RigBuilderComponent.layers.Clear();

            for (int i = 0; i < m_InverseKinematicsComponent.UpperBodyRigsList.Count; i++)
            {
                if (m_InverseKinematicsComponent.UpperBodyRigsList[i] != null)
                {
                    var RigReference = m_InverseKinematicsComponent.UpperBodyRigsList[i];

                    if (RigReference != null)
                    {
                        var RigCopy = Instantiate(RigReference, Vector3.zero, Quaternion.identity);
                        RigCopy.name = RigReference.name;
                        RigCopy.transform.SetParent(G.transform);
                        ComponentUtility.CopyComponent(RigReference.transform);
                        ComponentUtility.PasteComponentValues(RigCopy.transform);
                        RigCopy.name = RigReference.name;
                        m_InverseKinematicsComponent.UpperBodyRigsList[i] = RigCopy;

                        if (m_RigBuilderComponent != null)
                        {
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }
                        else
                        {
                            m_RigBuilderComponent = G.AddComponent<UnityEngine.Animations.Rigging.RigBuilder>();
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }

                        // MultiAimConstraint �̎Q�ƍ����ւ�
                        var m_MultiAimConstraints = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.MultiAimConstraint>();

                        for (int j = 0; j < m_MultiAimConstraints.Length; j++)
                        {
                            var m_SourceData = m_MultiAimConstraints[j].data;
                            Transform ConstrainedObjectReference = m_SourceData.constrainedObject;

                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == ConstrainedObjectReference.name)
                                {
                                    m_SourceData.constrainedObject = t;
                                    m_MultiAimConstraints[j].data = m_SourceData;
                                }
                            }
                        }

                        // TwoBoneIKConstraint �̎Q�ƍ����ւ��i�^�[�Q�b�g/�q���g�͕������ē����e�֔z�u�j
                        var m_TwoBoneIKConstraint = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.TwoBoneIKConstraint>();

                        for (int j = 0; j < m_TwoBoneIKConstraint.Length; j++)
                        {
                            var m_SourceData = m_TwoBoneIKConstraint[j].data;

                            // �q���g�I�u�W�F�N�g
                            Transform HintObjectReference = m_SourceData.hint;

                            if (HintObjectReference != null)
                            {
                                Transform HintObjectCopy = Instantiate(m_SourceData.hint, Vector3.zero, Quaternion.identity);
                                HintObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == HintObjectReference.parent.name)
                                    {
                                        HintObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(HintObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(HintObjectCopy.transform);
                                        m_SourceData.hint = HintObjectCopy;
                                    }
                                }
                            }

                            // �^�[�Q�b�g�I�u�W�F�N�g
                            Transform TargetObjectReference = m_SourceData.target;

                            if (TargetObjectReference != null)
                            {
                                Transform TargetObjectCopy = Instantiate(m_SourceData.target, Vector3.zero, Quaternion.identity);
                                TargetObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == TargetObjectReference.parent.name)
                                    {
                                        TargetObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(TargetObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(TargetObjectCopy.transform);
                                        m_SourceData.target = TargetObjectCopy;
                                    }
                                }
                            }

                            // �{�[���Q�Ɓiroot/mid/tip�j�𖼑O��v�ō����ւ�
                            Transform RootObjectReference = m_SourceData.root;
                            Transform MidObjectReference = m_SourceData.mid;
                            Transform TipObjectReference = m_SourceData.tip;

                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == RootObjectReference.name)
                                {
                                    m_SourceData.root = t;
                                }
                                else if (t.name == MidObjectReference.name)
                                {
                                    m_SourceData.mid = t;
                                }
                                else if (t.name == TipObjectReference.name)
                                {
                                    m_SourceData.tip = t;
                                }
                            }

                            // �ύX��K�p
                            m_TwoBoneIKConstraint[j].data = m_SourceData;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �g�O���v�f�i���̂Ɛ����j��\��
        /// </summary>
        bool ToggleComponentElement(bool Setting, string Name, string Description, bool RequiredComponent = true)
        {
            EditorGUI.BeginDisabledGroup(RequiredComponent);
            EditorGUILayout.BeginHorizontal();
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(Name));
            Setting = EditorGUILayout.ToggleLeft(new GUIContent(Name), Setting, GUILayout.ExpandWidth(false), GUILayout.Width(textDimensions.x + 13.5f));
            EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("_Help").image, Description), GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(5);
            return Setting;
        }

        /// <summary>
        /// ���������u���b�N
        /// </summary>
        void DescriptionElement(string DescriptionText)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.HelpBox(DescriptionText, MessageType.None, true);
            GUI.backgroundColor = Color.white;
        }
    }
}
