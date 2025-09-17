using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("�Ώۃv���p�e�B�ɕt�^���ꂽ DrawIfAttribute �̎Q��")]
    // �v���p�e�B�ɕt�^���ꂽ�����ւ̎Q��
    DrawIfAttribute drawIf;

    [Header("��r�ΏۂƂȂ� SerializedProperty�i������ comparedPropertyName �œ���j")]
    // ��r�Ɏg�p����t�B�[���h
    SerializedProperty comparedField;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!ShowMe(property) && drawIf.disablingType == DrawIfAttribute.DisablingType.DontDraw)
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
        else
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                int numChildren = 0;
                float totalHeight = 0.0f;

                IEnumerator children = property.GetEnumerator();

                while (children.MoveNext())
                {
                    SerializedProperty child = children.Current as SerializedProperty;
                    //if (child.displayName == property.displayName)

                    GUIContent childLabel = new GUIContent(child.displayName);
                    totalHeight += EditorGUI.GetPropertyHeight(child, childLabel) + EditorGUIUtility.standardVerticalSpacing;
                    numChildren++;
                }

                // �����̗]�����폜�i�v�f�Ԃ̃X�y�[�X�̂ݎc���j
                totalHeight -= EditorGUIUtility.standardVerticalSpacing;

                return totalHeight;
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }

    /// <summary>
    /// �G���[���͊���Łu�\������v�����Ƀt�H�[���o�b�N���܂��B
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        drawIf = attribute as DrawIfAttribute;
        // �v���p�e�B�����A�����œn���ꂽ comparedPropertyName �ɒu�������ĉ���
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.comparedPropertyName) : drawIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("�w�肳�ꂽ���O�̃v���p�e�B��������܂���: " + path);
            return true;
        }

        // �l���擾���A�^�ɉ����Ĕ�r
        switch (comparedField.type)
        {   // �K�v�ɉ����ēƎ��^���g���\
            case "bool":
                return comparedField.boolValue.Equals(drawIf.comparedValue);
            case "Enum":
                return (comparedField.intValue & (int)drawIf.comparedValue) != 0;
            default:
                Debug.LogError("�G���[: " + comparedField.type + " �̓T�|�[�g����Ă��܂���i" + path + "�j");
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Debug.Log(ShowMe(property) + "  " + property.name);
        // �����𖞂����ꍇ�́A���̂܂܃t�B�[���h��`��
        if (ShowMe(property))
        {
            // Generic �̓J�X�^���N���X���Ӗ�����c
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                IEnumerator children = property.GetEnumerator();

                Rect offsetPosition = position;

                while (children.MoveNext())
                {
                    SerializedProperty child = children.Current as SerializedProperty;

                    GUIContent childLabel = new GUIContent(child.displayName);

                    float childHeight = EditorGUI.GetPropertyHeight(child, childLabel);
                    offsetPosition.height = childHeight;

                    EditorGUI.PropertyField(offsetPosition, child, childLabel);

                    offsetPosition.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

        } // �c��v���Ȃ��ꍇ�A���������@�� ReadOnly �Ȃ�u�����\���v�ŕ`��
        else if (drawIf.disablingType == DrawIfAttribute.DisablingType.ReadOnly)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }

}
