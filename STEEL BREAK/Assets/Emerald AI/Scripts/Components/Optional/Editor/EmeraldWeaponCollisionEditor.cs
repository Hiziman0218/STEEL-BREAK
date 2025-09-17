using System.Collections;                         // �i�ێ��j�R���[�`���֘A
using System.Collections.Generic;                 // �i�ێ��j�ėp�R���N�V����
using UnityEngine;                                // Unity �����^�C��API
using UnityEditor;                                // �G�f�B�^�g��API�iEditor �Ȃǁj
using UnityEditorInternal;                        // ReorderableList ���i�{�t�@�C���ł͖��g�p���������ێ��j

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldWeaponCollision))] // ���̃J�X�^���C���X�y�N�^�� EmeraldWeaponCollision �p
    [CanEditMultipleObjects]                       // �����I�u�W�F�N�g�����ҏW������

    // �y�N���X�T�v�zEmeraldWeaponCollisionEditor�F
    //  EmeraldWeaponCollision �R���|�[�l���g�i�ߐڕ���p�̓����蔻��{�b�N�X�j��
    //  �C���X�y�N�^��ŕҏW���₷�����邽�߂̃G�f�B�^�g���N���X�B
    //  �{�b�N�X�R���C�_�[�̉��F�A�����e�L�X�g�A�܂肽����UI�Ȃǂ�񋟂���B
    public class EmeraldWeaponCollisionEditor : Editor
    {
        [Header("�t�H�[���h�A�E�g���o���̃X�^�C���iEditorGUI �p�j")]
        GUIStyle FoldoutStyle;                     // ���o���̕`��X�^�C��

        [Header("�w�b�_�[�ɕ\������A�C�R���iResources ����ǂݍ��݁j")]
        Texture WeaponCollisionEditorIcon;         // �C���X�y�N�^�㕔�̃A�C�R��

        [Header("SerializedProperty �Q�Ɓi�F/�܂肽����/��\���g�O���j")]
        SerializedProperty CollisionBoxColor,      // �R���W�����{�b�N�X�̐F
                          HideSettingsFoldout,     // �ݒ�S�̂̔�\���g�O��
                          WeaponCollisionFoldout;  // �u����R���W�����ݒ�v�Z�N�V�����̊J��

        /// <summary>
        /// �i���{��j�G�f�B�^�L�������ɌĂ΂��B�A�C�R���ǂݍ��݂ƑΏۃv���p�e�B�̃o�C���h�A����� BoxCollider �̎����擾���s���B
        /// </summary>
        void OnEnable()
        {
            // �w�b�_�[�p�A�C�R����ǂݍ��݁i�p�X�͊����d�l�̂܂܁j
            if (WeaponCollisionEditorIcon == null) WeaponCollisionEditorIcon = Resources.Load("Editor Icons/EmeraldWeaponCollision") as Texture;

            // �ΏۃR���|�[�l���g�ւ̎Q�Ƃ��擾
            EmeraldWeaponCollision self = (EmeraldWeaponCollision)target;

            // ����� BoxCollider �������Ŏ擾���ĕێ��i�������̋����̂܂܁j
            self.WeaponCollider = self.GetComponent<BoxCollider>();

            // �V���A���C�Y�ς݃t�B�[���h���v���p�e�B�փo�C���h
            CollisionBoxColor = serializedObject.FindProperty("CollisionBoxColor");   // �R���W�����{�b�N�X�̐F
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout"); // ��\���g�O��
            WeaponCollisionFoldout = serializedObject.FindProperty("WeaponCollisionFoldout"); // �Z�N�V�����܂肽����
        }

        /// <summary>
        /// �i���{��j�C���X�y�N�^�̃��C���`��B�w�b�_�[�Ɓu����R���W�����ݒ�v�Z�N�V������\������B
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // �J�X�^���X�^�C���X�V
            serializedObject.Update();                                  // ���񉻃I�u�W�F�N�g���ŐV��

            // �w�b�_�[�i�p�� "Weapon Collision" �� ���{��u����R���W�����v�֍����ւ��j
            CustomEditorProperties.BeginScriptHeaderNew("����R���W����", WeaponCollisionEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue) // ��\���łȂ���Γ��e��`��
            {
                EditorGUILayout.Space();
                WeaponCollisionSettings();      // �ݒ�Z�N�V�����̕`��
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();   // �w�b�_�[�I��

            serializedObject.ApplyModifiedProperties(); // �ύX�K�p
        }

        /// <summary>
        /// �i���{��j����R���W�����̐ݒ�UI��`�悷��B�������E�F�ݒ�E�w���v����{��ŕ\���B
        /// </summary>
        void WeaponCollisionSettings()
        {
            // �Z�N�V�������o���i�p�� "Weapon Collision Settings" �� ���{��u����R���W�����ݒ�v�j
            WeaponCollisionFoldout.boolValue = EditorGUILayout.Foldout(WeaponCollisionFoldout.boolValue, "����R���W�����ݒ�", true, FoldoutStyle);

            if (WeaponCollisionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // �^�C�g���{�����i�p�ꁨ���{��j
                CustomEditorProperties.TextTitleWithDescription(
                    "����R���W�����ݒ�",
                    "Box Collider�iBoxCollider �R���|�[�l���g�j��p���āAAI �̕���ɍ��킹�ăR���C�_�[�̃T�C�Y�ƈʒu�𒲐����Ă��������B"
                    + "����R���W�����͋ߐځi�����[�j�U���p��z�肵�Ă��܂��B"
                    + "���� Weapon Collision �R���|�[�l���g���@�\������ɂ́A�A�j���[�V�����C�x���g��ʂ��ėL��������K�v������܂��B",
                    true
                );

                // �F�v���p�e�B�i�p�ꃉ�x�� �� ���{�ꃉ�x���֒u���j
                EditorGUILayout.PropertyField(CollisionBoxColor, new GUIContent("�R���W�����{�b�N�X�̐F"));
                CustomEditorProperties.CustomHelpLabelField("�R���W�����{�b�N�X�̉����Ɏg�p����F�𐧌䂵�܂��B", true);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
