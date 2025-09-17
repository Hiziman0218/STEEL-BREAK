using UnityEngine;                    // Unity �̃����^�C��API
using UnityEditor;                    // �G�f�B�^�g��API�iEditor, SerializedProperty �Ȃǁj

namespace EmeraldAI.Utility           // EmeraldAI �̃��[�e�B���e�B���O���
{
    [CustomEditor(typeof(CoverNode))] // ���̃G�f�B�^�� CoverNode �p�̃J�X�^���C���X�y�N�^
    [CanEditMultipleObjects]          // �����I��ҏW������

    // �y�N���X�T�v�zCoverNodeEditor�F
    //  CoverNode �R���|�[�l���g�p�̃J�X�^���C���X�y�N�^��񋟂���G�f�B�^�g���N���X�B
    //  �m�[�h��ʁE�����⏕�EFOV�p�x�EGizmo�F�Ȃǂ̐ݒ�UI���܂Ƃ߁A�����e�L�X�g���\������B
    public class CoverNodeEditor : Editor
    {
        // --- �����o�ϐ��iSerializedProperty��G�f�B�^�p���\�[�X�j ---
        [Header("�J�o�[����̎�ށiCoverType�j�փo�C���h����SerializedProperty")]
        SerializedProperty CoverType;                     // CoverNode.CoverType

        [Header("�i���g�p�j��Q���̂Ȃ��ʒu��T������t���O�iLookForUnobstructedPosition�j")]
        SerializedProperty LookForUnobstructedPosition;   // ���������̂݁BUI�ł͖��g�p�i�������̂܂ܕێ��j

        [Header("�^�[�Q�b�g�������Ȃ��ꍇ�Ɏ������ʂ�ʒu��T���t���O�iGetLineOfSightPosition�j")]
        SerializedProperty GetLineOfSightPosition;        // CoverNode.GetLineOfSightPosition

        [Header("�J�o�[�m�[�h�̎��E�p���~�b�g�iCoverAngleLimit, 60�`180�x�j")]
        SerializedProperty CoverAngleLimit;               // CoverNode.CoverAngleLimit

        [Header("���������K�C�h�i���jGizmo �̐F�iArrowColor�j")]
        SerializedProperty ArrowColor;                    // CoverNode.ArrowColor

        [Header("�J�o�[�m�[�h�{�́i���jGizmo �̐F�iNodeColor�j")]
        SerializedProperty NodeColor;                     // CoverNode.NodeColor

        [Header("�C���X�y�N�^�㕔�w�b�_�[�p�̃A�C�R���e�N�X�`��")]
        Texture NodeEditorIcon;                           // Resources ����ǂݍ��ރG�f�B�^�p�A�C�R��

        void OnEnable()                                   // �G�f�B�^�L�������ɌĂ΂��
        {
            if (NodeEditorIcon == null)                  // �A�C�R�������[�h�Ȃ�
                NodeEditorIcon = Resources.Load("Editor Icons/EmeraldCover") as Texture; // �w��p�X���烍�[�h
            InitializeProperties();                      // SerializedProperty �̕R�t����������
        }

        void InitializeProperties()                      // �ΏۃI�u�W�F�N�g�̊e�v���p�e�B�փo�C���h
        {
            CoverType = serializedObject.FindProperty("CoverType");                               // ���
            LookForUnobstructedPosition = serializedObject.FindProperty("LookForUnobstructedPosition"); // ���g�p�i�������ʂ�j
            GetLineOfSightPosition = serializedObject.FindProperty("GetLineOfSightPosition");     // �����⏕�t���O
            CoverAngleLimit = serializedObject.FindProperty("CoverAngleLimit");                   // �p�x����
            ArrowColor = serializedObject.FindProperty("ArrowColor");                             // ���F
            NodeColor = serializedObject.FindProperty("NodeColor");                               // �m�[�h�F
        }

        public override void OnInspectorGUI()            // �J�X�^���C���X�y�N�^�̕`��
        {
            serializedObject.Update();                   // ���񉻃I�u�W�F�N�g���X�V�i�����j

            // ���o���i�^�C�g���ƃA�C�R���j���p�ꂩ����{��֍����ւ�
            CustomEditorProperties.BeginScriptHeader("�J�o�[�m�[�h", NodeEditorIcon);

            EditorGUILayout.Space();                     // �]��
            CoverNodeSettings();                         // �ݒ�UI�̕`��
            EditorGUILayout.Space();                     // �]��

            serializedObject.ApplyModifiedProperties();  // �ύX�̓K�p

            CustomEditorProperties.EndScriptHeader();    // ���o���̏I��
        }

        void CoverNodeSettings()                         // CoverNode �ݒ�Z�N�V����
        {
            CoverNode self = (CoverNode)target;         // �ΏۃR���|�[�l���g�Q�Ƃ��擾

            CustomEditorProperties.BeginFoldoutWindowBox(); // �܂肽���݃{�b�N�X�J�n

            // �Z�N�V�����^�C�g���Ɛ����i�p�ꁨ���{��j
            CustomEditorProperties.TextTitleWithDescription(
                "�J�o�[�m�[�h�ݒ�",
                "���̃J�o�[�m�[�h�g�p����AI�̐U�镑���ƁAGizmo�i�����j�̐F��ݒ肵�܂��B",
                true);

            // �J�o�[�^�C�v�̑I��
            EditorGUILayout.PropertyField(CoverType);

            // �J�o�[�^�C�v�ʂ̐����i�p�ꁨ���{��ɍ����ւ��j
            if (self.CoverType == CoverTypes.CrouchAndPeak)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Crouch and Peak�i���Ⴊ�݁��s�[�N�j�F�������ꂽ�u�B���b���v�̊Ԃ��Ⴊ�݁A�������痧���オ���Ĕ`�����݁i�s�[�N�j�܂��B�s�[�N�񐔂͐������ꂽ�u�s�[�N�񐔁v�Ɋ�Â��A�e�s�[�N���͐������ꂽ�u�U���b���v�̊ԍU�����܂��B",
                    false);
            }
            else if (self.CoverType == CoverTypes.CrouchOnce)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Crouch Once�i1�񂾂����Ⴊ�ށj�F�������ꂽ�u�B���b���v�̊Ԃ�����x���Ⴊ�݁A�������痧���オ��܂��B�����Ă���Ԃ͐������ꂽ�u�U���b���v�̊ԍU�����܂��B",
                    false);
            }
            else if (self.CoverType == CoverTypes.Stand)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Stand�i�����j�F���̃J�o�[�|�C���g����p���I�ɗ�������Ԃ��ێ����܂��B�����Ă���Ԃ͐������ꂽ�u�U���b���v�̊ԍU�����܂��B",
                    false);
            }

            // �d�v���b�Z�[�W�i�p�ꁨ���{��j
            CustomEditorProperties.DisplayImportantMessage(
                "��L�̈ꕔ�ݒ�́AAI�{�̂� Cover �R���|�[�l���g���̐ݒ�Ɉˑ����܂��B");

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // �����ʒu�̕␳�t���O
            EditorGUILayout.PropertyField(GetLineOfSightPosition);
            CustomEditorProperties.CustomHelpLabelField(
                "���݂̃J�o�[�m�[�h�Ń^�[�Q�b�g�����F�ł��Ȃ��ꍇ�A�U���\�Ȗ��Օ��ʒu���ꎞ�I�ɐ������Ĉړ����邩�ǂ����𐧌䂵�܂��B",
                false);
            if (self.GetLineOfSightPosition == YesOrNo.Yes)
                CustomEditorProperties.DisplayImportantMessage("���̐ݒ�ɂ��AAI�͌��݂̃J�o�[�m�[�h���痣��Ĉʒu�������s���ꍇ������܂��B");

            EditorGUILayout.Space();

            // �p�x���~�b�g
            EditorGUILayout.PropertyField(CoverAngleLimit);
            CustomEditorProperties.CustomHelpLabelField(
                "���̃J�o�[�m�[�h�̊p�x������ݒ肵�܂��B�^�[�Q�b�g�͂��͈͓̔��ɂ���K�v������܂��i�V�[����̗ΐF�G���A�ŉ�������܂��j�B",
                true);

            // ���Gizmo�F
            EditorGUILayout.PropertyField(ArrowColor);
            CustomEditorProperties.CustomHelpLabelField(
                "���������K�C�h�i���jGizmo �̐F��ݒ肵�܂��B",
                true);

            // �m�[�hGizmo�F
            EditorGUILayout.PropertyField(NodeColor);
            CustomEditorProperties.CustomHelpLabelField(
                "�J�o�[�m�[�h�i���́jGizmo �̐F��ݒ肵�܂��B",
                true);

            CustomEditorProperties.EndFoldoutWindowBox(); // �܂肽���݃{�b�N�X�I��
        }
    }
}
