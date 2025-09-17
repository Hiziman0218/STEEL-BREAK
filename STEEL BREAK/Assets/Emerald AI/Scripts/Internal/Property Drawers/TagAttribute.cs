using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

/// <summary>
/// �yTagAttribute�z
/// �C���X�y�N�^��� **Tag** ��I���ł���悤�ɂ��邽�߂� PropertyAttribute�B
/// string �t�B�[���h�ɕt�^���邱�ƂŁA�e�L�X�g���͂ł͂Ȃ� Tag �I�� UI ��\�����܂��B
/// </summary>
public class TagAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    /// <summary>
    /// �yTagAttributeDrawer�z
    /// ��L�������t�^���ꂽ string �t�B�[���h���A�C���X�y�N�^��� Tag �I�� UI �Ƃ��ĕ`�悵�܂��B
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    private class TagAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// �v���p�e�B�s�̍�����Ԃ��܂��i1�s���j�B
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// ���ۂ̕`�揈���B
        /// - �Ώۂ� string �ȊO�Ȃ�G���[���b�Z�[�W��\��
        /// - ���o�^�̃^�O���������Ă�����󕶎��փ��Z�b�g
        /// - ��̂Ƃ��͐ԐF�ŋ����\��
        /// - TagField �Ń^�O��I��
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // string �ȊO�ɕt�^����Ă����ꍇ�̓G���[�\��
            if (property.propertyType != SerializedPropertyType.String)
            {
                // ��: $"{nameof(TagAttribute)} can only be used for strings!"
                EditorGUI.HelpBox(position, $"{nameof(TagAttribute)} �� string �^�ɂ̂ݎg�p�ł��܂��I", MessageType.Error);
                return;
            }

            // ���o�^�̃^�O���������Ă�����󕶎��ɂ���
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(property.stringValue))
            {
                property.stringValue = "";
            }

            // �l����̂Ƃ��͐Ԃŋ���
            var color = GUI.color;
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                GUI.color = Color.red;
            }

            // Tag �I�� UI
            EditorGUI.BeginChangeCheck();
            var TempTag = EditorGUI.TagField(position, label, property.stringValue);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = TempTag;
            }

            GUI.color = color;

            EditorGUI.EndProperty();
        }
    }
#endif
}
