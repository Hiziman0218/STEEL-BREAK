using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Reflection;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// �yAnimationViewerManager�z
    /// �A�j���[�V�����C�x���g�̃v���r���[��ҏW�i�ǉ��E�ړ��E�폜�j���s���G�f�B�^�E�B���h�E�B
    /// �E���ݑI�����Ă��� AI �� Animation Profile ����N���b�v�ꗗ���擾
    /// �E�^�C�����C����ōĐ��^�ꎞ��~�A�C�Ӄt���[���ֈړ�
    /// �E�v���Z�b�g�C�x���g�̒ǉ��A�ʃp�����[�^�ҏW�A�K�p�^�j���̊Ǘ�
    /// �E���[�g���[�V�����̗L����؂�ւ��ăv���r���[
    /// </summary>
    public class AnimationViewerManager : EditorWindow
    {
        [Header("�V���O���g���Q�Ɓi���̃E�B���h�E�̗B��̃C���X�^���X�j")]
        public static AnimationViewerManager Instance;

        [Header("�^�C�����C���O�g�̐F")]
        Color TimelineOutlineColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("���[�g���[�V�������g�p���邩")]
        public bool UseRootMotion;

        [Header("���݃v���r���[���̃A�j���[�V�����C���f�b�N�X")]
        public int CurrentPreviewAnimationIndex = 0;

        [Header("�v���Z�b�g�̃A�j���[�V�����C�x���g��ʃC���f�b�N�X")]
        public int PresetAnimationEventIndex = 0;

        [Header("�Đ����x�i1.0=�����j")]
        float TimeScale = 1.0f;

        [Header("�E�B���h�E�����C�A�E�g�p�I�t�Z�b�g")]
        Vector2 WindowOffset;

        [Header("���݂̃v���r���[�Đ����ԁi�b�j")]
        protected float time = 0.0f;

        [Header("�w�b�_�p�A�C�R���iAnimation Profile�j")]
        Texture AnimationProfileEditorIcon;

        [Header("�Đ��{�^���p�A�C�R��")]
        Texture PlayButtonIcon;

        [Header("�ꎞ��~�{�^���p�A�C�R��")]
        Texture PauseButtonIcon;

        [Header("�A�j���[�V�����C�x���g�ǉ��{�^���p�A�C�R��")]
        Texture AnimationEventIcon;

        [Header("�ꎞ�ێ��F�����ʒu")]
        public static Vector3 DefaultPosition;

        [Header("�ꎞ�ێ��F�����I�C���[�p")]
        public static Vector3 DefaultEuler;

        [Header("�A�j���[�V�������Đ�����")]
        bool AnimationIsPlaying;

        [Header("���[�g���[�V�����ؑւ̓������")]
        bool RootMotionChanged;

        [Header("���݃v���r���[�Ώۂ�AI�i�A�j���[�V������\������Ώہj")]
        GameObject CurrentAnimationViewerAI = null;

        [Header("���݃v���r���[���̃A�j���[�V�����N���b�v")]
        AnimationClip PreviewClip = null;

        [Header("�v���r���[�Ώۂ̃A�j���[�V�����N���b�v�ꗗ")]
        List<AnimationClip> PreviewClips = new List<AnimationClip>();

        [Header("GUI�\���p�F�A�j���[�V�������ꗗ")]
        List<string> AnimationNames = new List<string>();

        [Header("GUI�\���p�F�C�x���g���ꗗ�i�v���Z�b�g�j")]
        List<string> AnimationEventNames = new List<string>();

        [Header("�v���Z�b�g�̃A�j���[�V�����C�x���g�Q")]
        List<EmeraldAnimationEventsClass> AnimationEventPresets = new List<EmeraldAnimationEventsClass>();

        [Header("�A�j���[�V�����N���b�v�̃^�C�����C���̈�Rect")]
        Rect AnimationClipTimelineArea;

        [Header("�^�C�����C����̌��݈ʒu�i�c�o�[�jRect")]
        Rect AnimationClipTimelinePoint;

        [Header("���ݑI�𒆂̃C�x���g�C���f�b�N�X")]
        int AnimationEventIndex;

        [Header("���O�̃v���r���[�A�j���[�V�����C���f�b�N�X")]
        int PreviousPreviewAnimationIndex;

        [Header("���ݑI�𒆂̃A�j���[�V�����C�x���g")]
        public AnimationEvent CurrentAnimationEvent;

        [Header("���ݑI�𒆃C�x���g�̕`��̈�Rect")]
        Rect CurrentEventArea;

        [Header("�v���r���[�p�e�I�u�W�F�N�g�i���[�g���[�V�����΍�j")]
        GameObject AnimationPreviewParent;

        [Header("�����̐eTransform�i���A�p�j")]
        Transform PreviousParent;

        [Header("�����ʒu�i���A�p�j")]
        Vector3 StartingPosition;

        [Header("�����I�C���[�p�i���A�p�j")]
        Vector3 StartingEuler;

        [Header("�^�C�����C���h���b�O�̏������t���O")]
        bool InitializeTimelineMovement;

        [Header("�C�x���g�h���b�O�̏������t���O")]
        bool InitializeAnimationEventMovement;

        [Header("�f�o�b�O���O��L�����i�����p�j")]
        bool EnableDebugging = false; // Internal Use Only

        [Header("�d�������A�j���[�V�����N���b�v�i�C�x���g�d���X�V�p�j")]
        [SerializeField]
        public List<AnimationClip> DuplicateAnimationEvents = new List<AnimationClip>();

        [Header("���݂̃A�j���[�V�����ƃC�x���g�̑Ή����X�g")]
        [SerializeField]
        public List<AnimationEventElement> CurrentAnimationEvents = new List<AnimationEventElement>();

        /// <summary>
        /// �A�j���[�V�����N���b�v�ƁA���̃C�x���g�Q��ێ�����v�f�B
        /// </summary>
        [System.Serializable]
        public class AnimationEventElement
        {
            [Header("�ΏۃA�j���[�V�����N���b�v")]
            public AnimationClip Clip;

            [Header("�N���b�v�ɐݒ肳��Ă���A�j���[�V�����C�x���g�ꗗ")]
            public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();

            [Header("���̗v�f���ҏW���ꂽ���i���K�p�̕ύX�����邩�j")]
            public bool Modified;

            public AnimationEventElement(AnimationClip m_Clip, List<AnimationEvent> m_AnimationEvents)
            {
                Clip = m_Clip;
                AnimationEvents = new List<AnimationEvent>(m_AnimationEvents);
            }
        }

        void OnEnable()
        {
            if (EditorApplication.isPlaying)
                return;

            if (AnimationProfileEditorIcon == null) AnimationProfileEditorIcon = Resources.Load("Editor Icons/EmeraldDetection") as Texture;
            if (PlayButtonIcon == null) PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
            if (PauseButtonIcon == null) PauseButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
            if (AnimationEventIcon == null) AnimationEventIcon = Resources.Load("Editor Icons/EmeraldAnimationEvent") as Texture;

            this.minSize = new Vector2(Screen.currentResolution.width / 6f, Screen.currentResolution.height / 1.7f);
            this.maxSize = new Vector2(Screen.currentResolution.width / 4f, 1500);

            Instance = this;
            SubscribeCallbacks(); // �R�[���o�b�N�֓o�^
            InitiailizeList();
        }

        void OnDisable()
        {
            if (EnableDebugging) Debug.Log("OnDisable �����s");
            UnsubscribeCallbacks(); // �R�[���o�b�N�o�^����
        }

        /// <summary>
        /// �R�[���o�b�N�o�^�F�ۑ��E�V�[���ύX�E�ăR���p�C�����̃v���r���[��Ԃ�K�؂ɏ������邽�߂Ɏg�p�B
        /// </summary>
        void SubscribeCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// �R�[���o�b�N�o�^�����F�ۑ��E�V�[���ύX�E�ăR���p�C�����̃v���r���[��Ԃ̏������~�B
        /// </summary>
        void UnsubscribeCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// �iSceneView.duringSceneGui �o�R�j
        /// �A�j���[�V�����C�x���g�����΂���t���[���ɁAAI �̑����� GUI ��`��B
        /// �^�C�����C���|�C���g���C�x���g���Ԃɏd�Ȃ�ƐF��ς��Ď��F�����グ��B
        /// </summary>
        private void OnSceneGUI(SceneView obj)
        {
            if (PreviewClip != null)
            {
                Vector3 pos = CurrentAnimationViewerAI.transform.position;
                Color OutLineColor = new Color(0, 0, 0, 1);
                Color FaceColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

                Vector3[] verts = new Vector3[]
                {
                    new Vector3(pos.x - 0.5f, pos.y, pos.z - 0.5f),
                    new Vector3(pos.x - 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z - 0.5f)
                };

                // �^�C�����C���̌��݈ʒu����A�C�x���g���Ύ��Ԃɋߐڂ��Ă��邩�𔻒肵�ĐF��ύX
                float MouseLerp = (AnimationClipTimelinePoint.x / AnimationClipTimelineArea.width);
                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 2.5f, AnimationClipTimelineArea.min.x + 1f, MouseLerp);
                float ModifiedTime = ((AnimationClipTimelinePoint.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (ModifiedTime >= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time - 0.005f) && ModifiedTime <= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time + 0.005f))
                    {
                        OutLineColor = new Color(0, 0, 0f, 1);
                        FaceColor = new Color(0.5f, 1f, 0.5f, 0.15f);
                    }
                }
                Handles.DrawSolidRectangleWithOutline(verts, FaceColor, OutLineColor);
            }
        }

        /// <summary>
        /// �V�[���ۑ��̒��O�R�[���o�b�N�B�A�j���[�V�����T���v�����O�𖳌������ĕۑ��ΏۂɊ܂߂Ȃ��B
        /// </summary>
        private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
            SetAnimatorStates(true);
            DeparentPreviewObject();
            if (EnableDebugging) Debug.Log("OnSceneSaving �����s");
        }

        /// <summary>
        /// �V�[���ۑ�����R�[���o�b�N�B�ۑ����珜�O�����v���r���[��Ԃ𕜌��B
        /// </summary>
        void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            SetAnimatorStates(false);

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();

            if (GameObject.Find("Animation Viewer Parent") == null)
            {
                AnimationPreviewParent = new GameObject("Animation Viewer Parent");
                Selection.activeObject = AnimationPreviewParent;
                AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
                AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
                CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
            }

            if (EnableDebugging) Debug.Log("OnSceneSaved �����s");
        }

        /// <summary>
        /// �u���K�p�̕ύX�v�_�C�A���O��\���B�K�p�Ȃ珑�����݁A�j���Ȃ牽���������s�B
        /// </summary>
        void DisplayApplyChangesMenu(GameObject G = null)
        {
            if (G != CurrentAnimationViewerAI && CurrentAnimationEvents.Any(x => x.Modified == true))
            {
                if (EditorUtility.DisplayDialog("���K�p�̕ύX�����o", "���̃I�u�W�F�N�g�ɖ��K�p�̕ύX������܂�:\n" + CurrentAnimationViewerAI.name + "\n\n�ύX��K�p���܂����H", "�K�p����", "�j������"))
                {
                    ApplyChanges(false);
                    if (EnableDebugging) Debug.Log("�ύX��K�p���܂����i���j���[�j");
                }
                else
                {
                    if (EnableDebugging) Debug.Log("�ύX��j�����܂����i���j���[�j");
                }
            }
        }

        void ConfirmDiscardingMessage()
        {
            if (EditorUtility.DisplayDialog("�ύX��j�����܂����H", "���̑���͎������܂���B�ύX��j�����Ă���낵���ł����H", "�͂�", "������"))
            {
                Initialize(CurrentAnimationViewerAI);
                CurrentAnimationEvent = null;
                if (EnableDebugging) Debug.Log("�ύX�j���ɓ��ӂ��܂���");
            }
            else
            {
                if (EnableDebugging) Debug.Log("�ύX�j�����L�����Z�����܂���");
            }
        }

        /// <summary>
        /// �A�Z���u���̍ēǂݍ��݁i�R���p�C���j�O�ɁA�A�j���[�V�����T���v�����O���~���ČŒ�������B
        /// </summary>
        public void OnBeforeAssemblyReload()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
        }

        /// <summary>
        /// �A�Z���u���̍ēǂݍ��݁i�R���p�C���j��ɁA�v���r���[�̂��߂̏�Ԃ𕜋A�B
        /// </summary>
        public void OnAfterAssemblyReload()
        {
            SetAnimatorStates(false);

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// �v���C���[�h�؂�ւ����̓E�B���h�E����A�v���r���[�Œ�������B
        /// </summary>
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            this.Close();
            if (EnableDebugging) Debug.Log("PlayMode �ύX�����o");
        }

        /// <summary>
        /// �E�B���h�E�N�����F�v���Z�b�g�C�x���g���Ȃǂ̏������B
        /// </summary>
        void InitiailizeList()
        {
            // Emerald AI �̎�v�C�x���g�v���Z�b�g���擾
            AnimationEventPresets = AnimationEventInitializer.GetEmeraldAnimationEvents();

            // �\������񋓂ɓ����
            for (int i = 0; i < AnimationEventPresets.Count; i++)
            {
                AnimationEventNames.Add(AnimationEventPresets[i].eventDisplayName);
            }
        }

        /// <summary>
        /// Animation Editor �� Animation Profile �̃f�[�^�ŃG�f�B�^�E�B���h�E���������B
        /// </summary>
        public void Initialize(GameObject G)
        {
            DisplayApplyChangesMenu(G); // �������O�ɖ��K�p�̕ύX���m�F

            SetAnimatorStates(false); // Animator �� Always Animate �Ɂi�o�O����j�B�I�����ɖ߂��B

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            DeparentPreviewObject(); // �����̃v���r���[�Ώۂ�����Ε��A
            CurrentAnimationViewerAI = G; // ���݂̕ҏW�Ώۂ��L���b�V��

            // �����ʒu�E�p�x��ێ�
            StartingPosition = CurrentAnimationViewerAI.transform.position;
            StartingEuler = CurrentAnimationViewerAI.transform.eulerAngles;

            ParentPreviewObject(); // ���[�g���[�V�����̈ړ������_�֎����čs���Ȃ����߂̐e�I�u�W�F�N�g��p��

            InitializeAnimationData(); // Animation Profile ����N���b�v�ꗗ���쐬

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// Animation Profile ���炷�ׂẴA�j���[�V������񋓂��� UI �ɔ��f�B
        /// </summary>
        void InitializeAnimationData()
        {
            PreviewClips.Clear();
            AnimationNames.Clear();
            CurrentAnimationEvents.Clear();
            var m_AnimationProfile = CurrentAnimationViewerAI.GetComponent<EmeraldAnimation>().m_AnimationProfile;

            AssignAnimationNames(m_AnimationProfile.NonCombatAnimations, "");         // ��퓬
            AssignAnimationNames(m_AnimationProfile.Type1Animations, "Type 1 -");     // ����^�C�v1
            AssignAnimationNames(m_AnimationProfile.Type2Animations, "Type 2 -");     // ����^�C�v2

            // �N���b�v����������ꍇ�͏I��
            if (CurrentAnimationEvents.Count == 0)
            {
                Close();
                if (EditorUtility.DisplayDialog("�A�j���[�V����������܂���", "�A�^�b�`����Ă��� Animation Profile �ɃA�j���[�V����������܂���B�uEdit Animation Profile�v����A�j���[�V������ǉ����Ă��������B", "OK"))
                {
                    Selection.activeGameObject = CurrentAnimationViewerAI;
                    return;
                }
            }
        }

        /// <summary>
        /// ���t���N�V������ AnimationParentClass �̃t�B�[���h����N���b�v�������W���� GUI �\������g�ݗ��Ă�B
        /// </summary>
        void AssignAnimationNames(AnimationParentClass AnimationCategory, string AnimationCategoryName)
        {
            foreach (var field in AnimationCategory.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "System.Collections.Generic.List`1[EmeraldAI.AnimationClass]")
                {
                    List<AnimationClass> m_AnimationClass = (List<AnimationClass>)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);

                    for (int i = 0; i < m_AnimationClass.Count; i++)
                    {
                        AnimationClip m_AnimationClip = m_AnimationClass[i].AnimationClip;

                        if (m_AnimationClip != null) // && !PreviewClips.Contains(m_AnimationClip)
                        {
                            CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                            PreviewClips.Add(m_AnimationClip);

                            string TempName = field.Name.Replace("List", "");
                            TempName = TempName.Replace("Animation", "");
                            TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                            TempName = "(" + AnimationCategoryName + TempName + " " + (i + 1) + ")";
                            TempName = TempName.Replace("( ", "(");
                            AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                        }
                    }
                }

                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "EmeraldAI.AnimationClass")
                {
                    AnimationClass m_AnimationClass = (AnimationClass)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);
                    AnimationClip m_AnimationClip = m_AnimationClass.AnimationClip;

                    if (m_AnimationClip != null)
                    {
                        CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                        PreviewClips.Add(m_AnimationClip);

                        string TempName = field.Name.Replace("List", " ");
                        TempName = TempName.Replace("Animation", "");
                        TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                        TempName = "(" + AnimationCategoryName + TempName + ")";
                        TempName = TempName.Replace("( ", "(");
                        AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                    }
                }
            }
        }

        /// <summary>
        /// �ύX���e�A�j���[�V�����N���b�v�֕ۑ����܂��B
        /// </summary>
        void ApplyChanges(bool AnimationModeEnabled)
        {
            List<string> PathList = new List<string>();

            // �ύX���ꂽ�N���b�v�̃p�X�����W
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified)
                {
                    if (!PathList.Contains(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip)))
                    {
                        PathList.Add(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip));
                    }
                }
            }

            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified && !DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    var path = AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip);
                    string PathType = AssetImporter.GetAtPath(path).ToString(); // FBX ���P�̃N���b�v���ŕ���
                    PathType = PathType.Replace(" ", "");
                    PathType = PathType.Replace("(", "");
                    PathType = PathType.Replace(")", "");

                    if (PathType == "UnityEngine.FBXImporter")
                    {
                        var modelImporter = (ModelImporter)AssetImporter.GetAtPath(path) as ModelImporter;
                        SerializedObject so = new SerializedObject(modelImporter);
                        SerializedProperty clips = so.FindProperty("m_ClipAnimations");

                        // �ύX���ꂽ�N���b�v�ɑ΂��A�C�x���g���㏑��
                        for (int m = 0; m < modelImporter.clipAnimations.Length; m++)
                        {
                            if (clips.GetArrayElementAtIndex(m).displayName == CurrentAnimationEvents[i].Clip.name)
                            {
                                Debug.Log(clips.GetArrayElementAtIndex(m).displayName + " �̃A�j���[�V�����C�x���g���X�V���܂���");
                                SerializedProperty prop = clips.GetArrayElementAtIndex(m).FindPropertyRelative("events");
                                if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                                {
                                    prop.ClearArray();
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                                else
                                {
                                    SetEvents(clips.GetArrayElementAtIndex(m), CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                            }
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        // �P�̂� AnimationClip �ɑ΂���ۑ�
                        AnimationClip animClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
                        SerializedObject so = new SerializedObject(animClip);

                        if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                        {
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }
                        else
                        {
                            Debug.Log(animClip.name + " �̃A�j���[�V�����C�x���g���X�V���܂���");
                            SetEventsOnClip(CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                }
                else if (DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    CurrentAnimationEvents[i].Modified = false;
                }
            }

            for (int i = 0; i < PathList.Count; i++)
            {
                string PathType = AssetImporter.GetAtPath(PathList[i]).ToString();
                PathType = PathType.Replace(" ", "");
                PathType = PathType.Replace("(", "");
                PathType = PathType.Replace(")", "");

                if (PathType == "UnityEngine.FBXImporter")
                {
                    var modelImporter = (ModelImporter)AssetImporter.GetAtPath(PathList[i]) as ModelImporter;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(modelImporter));
                    AssetDatabase.SaveAssets();
                }
            }

            if (!AnimationMode.InAnimationMode() && AnimationModeEnabled)
                AnimationMode.StartAnimationMode();

            // ���ݑI�𒆃C�x���g�̃t�H�[�J�X����
            CurrentAnimationEvent = null;
            CurrentEventArea = new Rect();
            GUI.FocusControl(null);
            Repaint();
            DuplicateAnimationEvents.Clear();
        }

        // ���C���̃G�f�B�^�E�B���h�E
        public void OnGUI()
        {
            GUIStyle Style = EditorStyles.wordWrappedLabel;
            Style.fontStyle = FontStyle.Bold;
            Style.fontSize = 16;
            Style.padding.top = -11;
            Style.alignment = TextAnchor.UpperCenter;

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Window");
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent(AnimationProfileEditorIcon), Style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
            EditorGUILayout.LabelField("�A�j���[�V���� �r���[�A", Style, GUILayout.ExpandWidth(true));
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            CustomEditorProperties.TextTitleWithDescription("�A�j���[�V�����N���b�v�̑I��", "AI �̌��݂� Animation Profile ����A�j���[�V�����N���b�v��I�����A�V�[�����AI�Œ��ڃv���r���[���܂��B", false);
            CustomEditorProperties.NoticeTextDescription("��: ���̃v���r���[�ł� Inverse Kinematics �ƃA�j���[�V�����u�����h�͎g�p���܂���B���s���̕����ŏI�I�ȕi���͍��������܂��B", true);

            // ���݂̃A�j���[�V������񋓂���I��
            CurrentPreviewAnimationIndex = EditorGUILayout.Popup("���݂̃A�j���[�V����", CurrentPreviewAnimationIndex, AnimationNames.ToArray());
            PreviewClip = CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip;

            if (PreviousPreviewAnimationIndex != CurrentPreviewAnimationIndex)
            {
                CurrentAnimationEvent = null;
                PreviousPreviewAnimationIndex = CurrentPreviewAnimationIndex;
            }

            GUILayout.Space(15);

            CustomEditorProperties.CustomHelpLabelField("Project �^�u�Ō��݂̃N���b�v��I����Ԃɂ��܂��B", false);
            if (GUILayout.Button("���݂̃N���b�v�� Project �ŕ\��"))
            {
                Selection.activeObject = PreviewClip;
            }

            GUILayout.Space(10);
            UseRootMotion = EditorGUILayout.Toggle("���[�g���[�V�������g�p", UseRootMotion);
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            if (PreviewClip != null)
            {
                CustomEditorProperties.TextTitleWithDescription("�A�j���[�V���� �^�C�����C��", "���̃^�C�����C���̈���N���b�N���āA�A�j���[�V������C�ӂ̈ʒu�Ńv���r���[�ł��܂��B�Đ��{�^���ŘA���Đ����A������x�����ƈꎞ��~���܂��B�C�x���g��ʁi�v���Z�b�g�^�J�X�^���j��I��Ō��݈ʒu�ɒǉ��ł��܂��B�h���b�v�_�E���փ}�E�X���d�˂�ƁA�I�𒆂̃C�x���g�̐������c�[���`�b�v�ŕ\������܂��B", false);
                GUILayout.Space(2.5f);

                GUIStyle BoldStyle = GUI.skin.button;
                BoldStyle.fontStyle = FontStyle.Bold;

                if (AnimationIsPlaying)
                {
                    GUI.backgroundColor = new Color(1.5f, 0.1f, 0f, 0.75f);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button(new GUIContent(PlayButtonIcon, "���݂̃A�j���[�V�������Đ��^�ꎞ��~���܂��B"), BoldStyle))
                {
                    AnimationIsPlaying = !AnimationIsPlaying;
                    if (AnimationIsPlaying)
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
                    else
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
                }

                GUI.backgroundColor = Color.white;

                // �C�x���g�ǉ��{�^��
                if (GUILayout.Button(new GUIContent(AnimationEventIcon, "���݂̃t���[���ɁA�I�𒆂̃C�x���g��ʂ�ǉ����܂��i�K�v�ȃp�����[�^���K�p�j�B"), BoldStyle))
                {
                    GUI.FocusControl(null); // �I���C�x���g�̃t�H�[�J�X����
                    var m_event = new AnimationEvent();
                    m_event.functionName = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.functionName;
                    m_event.stringParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.stringParameter;
                    m_event.floatParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.floatParameter;
                    m_event.intParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.intParameter;
                    m_event.time = time + Mathf.Lerp(0.009f, -0.0111f, time / PreviewClip.length);

                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Add(m_event); // �C�x���g��ǉ�
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    // �ǉ������C�x���g��I����Ԃ�
                    CurrentAnimationEvent = m_event;
                    AnimationEventIndex = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.IndexOf(m_event);
                    Repaint();
                }

                Rect r2 = EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                PresetAnimationEventIndex = EditorGUILayout.Popup("", PresetAnimationEventIndex, AnimationEventNames.ToArray(), GUILayout.MinWidth(100));
                Rect LastRect = GUILayoutUtility.GetLastRect();
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);

                TimeScale = EditorGUILayout.Slider("", TimeScale, 0.01f, 1f, GUILayout.MaxHeight(0), GUILayout.MinWidth(100));

                EditorGUILayout.EndVertical();
                EditorGUI.LabelField(new Rect((r2.min.x + 45), r2.position.y - 2, (r2.width), 20), new GUIContent("�C�x���g���"));
                EditorGUI.LabelField(new Rect((r2.min.x + 6), r2.position.y + 18, LastRect.width, 20), new GUIContent("", AnimationEventPresets[PresetAnimationEventIndex].eventDescription));
                EditorGUI.LabelField(new Rect((r2.max.x - 265) + r2.min.x, r2.position.y - 2, 100, 20), "�Đ����x");
                EditorGUI.LabelField(new Rect((r2.min.x + r2.width - 40), r2.position.y + 19, (r2.width), 20), System.Math.Round(TimeScale, 2) + "x");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                float stopTime = PreviewClip.length;
                Rect r = EditorGUILayout.BeginVertical();
                WindowOffset.x = 5;
                var EditedTime = time * PreviewClip.frameRate;
                var niceTime = Mathf.Floor(EditedTime / PreviewClip.frameRate).ToString("00") + ":" + Mathf.FloorToInt(EditedTime % PreviewClip.frameRate).ToString("00");
                var ClipPercentage = ((time / stopTime) * 100f).ToString("F2");

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y - 3f, r.width - (WindowOffset.x * 2) + 6, 105.5f), TimelineOutlineColor); // �S�̘g
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x, r.position.y + 0.25f, r.width - (WindowOffset.x * 2), 100f), new Color(0.3f, 0.3f, 0.3f, 1f)); // �w�i

                AnimationClipTimelineArea = new Rect(r.x + WindowOffset.x - 3, r.position.y - 0.75f, r.width - (WindowOffset.x * 2) + 3, 50f);
                float AdjustedTime = (time / stopTime);
                EditorGUI.DrawRect(new Rect((r.x + WindowOffset.x), r.position.y + 0.25f, (r.width - (WindowOffset.x * 2) - 2) * AdjustedTime, 50), new Color(0.1f, 0.25f, 0.5f, 1f)); // �i���o�[

                // �^�C�����C�����
                GUIStyle LabelStyle = new GUIStyle();
                LabelStyle.alignment = TextAnchor.MiddleCenter;
                LabelStyle.fontStyle = FontStyle.Bold;
                LabelStyle.normal.textColor = Color.white;
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x, r.position.y + 50f, r.width - (WindowOffset.x * 2), 50f), new Color(0.1f, 0.1f, 0.1f, 1f));
                EditorGUI.LabelField(new Rect(r.x + WindowOffset.x, r.position.y + 50, r.width - (WindowOffset.x * 2), 50), niceTime + "   (" + (ClipPercentage + "%") + ")    �t���[�� " + Mathf.Round(time * PreviewClip.frameRate).ToString(), LabelStyle);
                // �^�C�����C����񂱂��܂�

                //------------- �A�j���[�V�����C�x���g �^�C�����C�� -------------
                GUIStyle AnimationEventStyle = GUI.skin.box;

                // 10%���݂̖ڐ���
                for (int i = 1; i <= 60; i++)
                {
                    EditorGUI.DrawRect(new Rect(((r.x + WindowOffset.x) + (float)(i / 60f) * (r.width - (WindowOffset.x * 2) - (r.min.x * 2.5f))), r.position.y + 37.5f, 1f, 12f), new Color(0.6f, 0.6f, 0.6f, 1f));
                }

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        GUI.backgroundColor = new Color(1, 1, 1, 2f);
                    else
                        GUI.backgroundColor = new Color(3, 3, 3, 3);

                    // �e�C�x���g�����Ԉʒu�ɉ����ĕ`��
                    Rect AnimationEventRect = new Rect((WindowOffset.x - AnimationClipTimelineArea.min.x / WindowOffset.x) + (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time / stopTime) * (r.width - (WindowOffset.x - (AnimationClipTimelineArea.min.x / WindowOffset.x) * 2) - (r.min.x)), r.position.y + 19.5f, 7.5f, 30f);

                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        EditorGUI.DrawRect(AnimationEventRect, new Color(1f, 0.25f, 0.25f, 1f));
                    else
                        EditorGUI.DrawRect(AnimationEventRect, Color.white);

                    if (AnimationEventRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;
                        CurrentAnimationEvent = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i];
                        UpdateIdenticalAnimationClips();
                        AnimationEventIndex = i;
                        CurrentEventArea = AnimationEventRect;
                        InitializeAnimationEventMovement = true;
                        Repaint();
                        GUI.FocusControl(null); // �I���C�x���g�̃t�H�[�J�X����
                    }
                }

                // �^�C�����C���̌��݈ʒu�o�[
                AnimationClipTimelinePoint = new Rect(((r.x + WindowOffset.x) + (time / stopTime) * (r.width - (WindowOffset.x * 2) - (r.min.x))), r.position.y, 3.5f, 50f);
                EditorGUI.DrawRect(AnimationClipTimelinePoint, new Color(0.8f, 0.8f, 0.8f, 1f));

                ChangeEventTime();

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y + 50, r.width - (WindowOffset.x * 2) + 6, 2.5f), TimelineOutlineColor); // �O�g

                GUI.backgroundColor = Color.white;
                //------------- �A�j���[�V�����C�x���g �^�C�����C�� �����܂� -------------
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(120);

            //------------- �A�j���[�V�����C�x���g�̏ڍוҏW -------------
            if (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count > 0 && CurrentAnimationEvent != null)
            {
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName = EditorGUILayout.TextField("�֐���", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter = EditorGUILayout.FloatField("Float", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter = EditorGUILayout.IntField("Int", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter = EditorGUILayout.TextField("String", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter = EditorGUILayout.ObjectField("Object", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter, typeof(object), false);

                GUILayout.Space(25);
            }

            EditorGUI.BeginDisabledGroup(CurrentAnimationEvents.Count > 0 && !CurrentAnimationEvents.Any(x => x.Modified == true));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("�ύX��K�p", GUILayout.MaxHeight(30)))
            {
                ApplyChanges(true);
            }

            if (GUILayout.Button("�ύX��j��", GUILayout.MaxHeight(30)))
            {
                ConfirmDiscardingMessage();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            //------------- �ڍוҏW�����܂� -------------

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// �n���ꂽ FBX �N���b�v�� SerializedProperty �ɁACurrentAnimationEvents �̓��e�������߂��B
        /// </summary>
        public void SetEvents(SerializedProperty sp, AnimationEvent[] newEvents, AnimationClip clip)
        {
            SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
            if (serializedProperty != null && serializedProperty.isArray && newEvents != null && newEvents.Length > 0)
            {
                serializedProperty.ClearArray();
                for (int i = 0; i < newEvents.Length; i++)
                {
                    AnimationEvent animationEvent = newEvents[i];
                    serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);

                    SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex(i);
                    eventProperty.FindPropertyRelative("floatParameter").floatValue = animationEvent.floatParameter;
                    eventProperty.FindPropertyRelative("functionName").stringValue = animationEvent.functionName;
                    eventProperty.FindPropertyRelative("intParameter").intValue = animationEvent.intParameter;
                    eventProperty.FindPropertyRelative("objectReferenceParameter").objectReferenceValue = animationEvent.objectReferenceParameter;
                    eventProperty.FindPropertyRelative("data").stringValue = animationEvent.stringParameter;
                    eventProperty.FindPropertyRelative("time").floatValue = animationEvent.time / clip.length;
                }
            }
        }

        /// <summary>
        /// �P�̂� AnimationClip �ɑ΂��ACurrentAnimationEvents �̓��e�������߂��B
        /// </summary>
        public void SetEventsOnClip(AnimationEvent[] newEvents, AnimationClip clip)
        {
            if (newEvents != null && newEvents.Length > 0)
            {
                AnimationUtility.SetAnimationEvents(clip, newEvents);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// �}�E�X���͂ƈʒu����A���݃N���b�v��̃C�x���g��Đ����Ԃ�ύX�B
        /// </summary>
        void ChangeEventTime()
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive); // �G�f�B�^�E�B���h�E�O�ł� Event.current ���@�\������

            var current = Event.current;
            if (current != null)
            {
                switch (current.type)
                {
                    case EventType.MouseDrag:
                        if (CurrentEventArea != new Rect())
                        {
                            if (InitializeAnimationEventMovement)
                            {
                                GUIUtility.hotControl = controlId;
                                float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 1.75f, AnimationClipTimelineArea.min.x + 3f, MouseLerp);
                                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0.015f, PreviewClip.length - 0.015f);
                                Repaint();
                            }
                        }
                        else if (CurrentEventArea == new Rect() && InitializeTimelineMovement)
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0, PreviewClip.length);
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (EnableDebugging) Debug.Log("MouseUp �����o");
                        InitializeTimelineMovement = false;
                        InitializeAnimationEventMovement = false;

                        if (CurrentEventArea != new Rect())
                        {
                            UpdateIdenticalAnimationClips();
                            CurrentEventArea = new Rect();
                            Repaint();
                        }
                        break;
                    case EventType.MouseDown:
                        if (EnableDebugging) Debug.Log("MouseDown �����o");
                        if (new Rect(AnimationClipTimelineArea.x + 3.5f, AnimationClipTimelineArea.position.y, AnimationClipTimelineArea.width - 6.5f, AnimationClipTimelineArea.height).Contains(current.mousePosition) && CurrentEventArea == new Rect())
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = ((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);
                            InitializeTimelineMovement = true; // �{�^���������ꑱ���Ă���Ԃ͘A�����ĒǏ]
                            Repaint();
                        }
                        break;
                }
            }

            if (Event.current.isKey)
            {
                KeyCode key = Event.current.keyCode;

                if (key == KeyCode.Delete && CurrentAnimationEvent != null)
                {
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.RemoveAt(AnimationEventIndex);
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    Repaint();
                    CurrentAnimationEvent = null;
                }
            }
        }

        /// <summary>
        /// �G�f�B�^���v���C���łȂ��AAnimationMode ���ŁA�N���b�v������Ƃ��ɍX�V�B
        /// �v���r���[�̂��߂̃T���v�����O�� UI �̍ĕ`����s���B
        /// </summary>
        void Update()
        {
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode() && PreviewClip != null)
            {
                // ���[�g���[�V���������֐؂�ւ����ۂ͈ʒu�����ɖ߂�
                if (UseRootMotion != RootMotionChanged)
                {
                    CurrentAnimationViewerAI.transform.localPosition = Vector3.zero;
                    CurrentAnimationViewerAI.transform.localEulerAngles = Vector3.zero;
                    RootMotionChanged = UseRootMotion;
                }

                // �Đ����͎��Ԃ�i�߂�
                if (AnimationIsPlaying)
                {
                    time += Time.deltaTime * TimeScale;
                    if (time >= PreviewClip.length)
                        time = 0;
                }

                AnimationMode.BeginSampling();
                DefaultPosition = CurrentAnimationViewerAI.transform.position;
                DefaultEuler = CurrentAnimationViewerAI.transform.eulerAngles;
                AnimationMode.SampleAnimationClip(CurrentAnimationViewerAI, PreviewClip, time);

                // ���[�g���[�V�����������͈ʒu�E�p�x���Œ�
                if (!UseRootMotion)
                {
                    CurrentAnimationViewerAI.transform.position = DefaultPosition;
                    CurrentAnimationViewerAI.transform.eulerAngles = DefaultEuler;
                }

                AnimationMode.EndSampling();

                // �T���v�����O���͏�� Repaint ���� UI �����炩��
                Repaint();
            }
        }

        /// <summary>
        /// �E�B���h�E�j�����ɏ�Ԃ𕜋A�i���K�p�̕ύX�m�F�A�T���v�����O��~�A�e�q�֌W�̉����Ȃǁj�B
        /// </summary>
        void OnDestroy()
        {
            DisplayApplyChangesMenu();

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
            DeparentPreviewObject();
        }

        /// <summary>
        /// ����Q�Ƃ̃A�j���[�V�����N���b�v�ɑ΂��āA�C�x���g�̕ύX���e�𓯊��i�����ӏ��ɓ���N���b�v������ꍇ�j�B
        /// </summary>
        void UpdateIdenticalAnimationClips()
        {
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Clip == CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip)
                {
                    CurrentAnimationEvents[i].AnimationEvents = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents;
                    CurrentAnimationEvents[i].Modified = true;
                }
            }
        }

        /// <summary>
        /// �v���r���[�p�̐e�I�u�W�F�N�g���� AI �����O���A���̐e�E�ʒu�E�p�x�ɖ߂��B
        /// </summary>
        public void DeparentPreviewObject()
        {
            if (CurrentAnimationViewerAI != null)
            {
                CurrentAnimationViewerAI.transform.SetParent(PreviousParent);
                CurrentAnimationViewerAI.transform.position = StartingPosition;
                CurrentAnimationViewerAI.transform.eulerAngles = StartingEuler;
                if (AnimationPreviewParent != null && AnimationPreviewParent.transform.childCount == 0)
                    DestroyImmediate(AnimationPreviewParent);
            }
        }

        /// <summary>
        /// ���[�g���[�V���������_�֗���Ă��܂�Ȃ��悤�AAI ���ꎞ�I�ɐe�I�u�W�F�N�g�z���ֈړ��B
        /// </summary>
        public void ParentPreviewObject()
        {
            AnimationPreviewParent = new GameObject("Animation Viewer Parent");
            Selection.activeObject = AnimationPreviewParent;
            AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
            AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
            PreviousParent = CurrentAnimationViewerAI.transform.parent;
            CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
        }

        /// <summary>
        /// Unity �̊��m�̕s��΍�ŁAAnimationMode ���� Animator ��S�Ė���������B
        /// �ۑ��E�ăR���p�C���EPlayMode �ύX���̃R�[���o�b�N�ŏ�Ԃ��Ǘ��B
        /// </summary>
        public void SetAnimatorStates(bool state)
        {
            var AnimatorsInScene = GameObject.FindObjectsOfType<Animator>();
            for (int i = 0; i < AnimatorsInScene.Length; i++)
            {
                AnimatorsInScene[i].enabled = state;
            }
        }
    }
}
