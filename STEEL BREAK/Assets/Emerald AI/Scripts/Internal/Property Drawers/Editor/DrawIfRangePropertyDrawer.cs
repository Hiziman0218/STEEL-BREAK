using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// �yDrawIfRangePropertyDrawer�z
/// DrawIfRangeAttribute ���t�^���ꂽ�t�B�[���h/�v���p�e�B���A�w������𖞂������ꍇ�ɂ̂�
/// �C���X�y�N�^�֕`�悷�邽�߂� PropertyDrawer�B
/// ����ɁA�����ݒ�i����^float �X���C�_�[�^int �X���C�_�[�A����эŏ��E�ő�l�j�ɉ�����
/// �K�؂� UI�i�X���C�_�[���j�ŕ`�悵�܂��B
///
/// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(DrawIfRangeAttribute))]
public class DrawIfRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("�Ώۃv���p�e�B�ɕt�^���ꂽ DrawIfRangeAttribute �̎Q��")]
    // �v���p�e�B�ɕt�^���ꂽ�����ւ̎Q��
    DrawIfRangeAttribute drawRanageIf;

    [Header("��r�ΏۂƂȂ� SerializedProperty�i���� comparedPropertyName �œ���j")]
    // ��r�Ɏg�p����t�B�[���h
    SerializedProperty comparedField;

    [Header("�������������ꂽ���ǂ����itrue=�`�悷��j")]
    bool conditionMet;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // �� �����𖞂����Ȃ��ꍇ�͍����𕉒l�ɂ��āu��\���v�����̃��C�A�E�g��
        //if (!ShowMe(property))
        if (!conditionMet)
            return -2f;

        // ����̍�����Ԃ�
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// �G���[���͊���Łu�\������v�Ƀt�H�[���o�b�N���܂��B
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        drawRanageIf = attribute as DrawIfRangeAttribute;

        // �v���p�e�B�����A������ comparedPropertyName �֒u�������ĉ���
        string path = property.propertyPath.Contains(".")
            ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName)
            : drawRanageIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            // ��: "Cannot find property with name: " + path
            Debug.LogError("�w�肳�ꂽ���O�̃v���p�e�B��������܂���: " + path);
            return true;
        }

        // �l���擾���A�^�ɉ����Ĕ�r
        switch (comparedField.type)
        {   // �K�v�ɉ����ēƎ��^���g���\
            case "bool":
                return comparedField.boolValue.Equals(drawRanageIf.comparedValue);
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)drawRanageIf.comparedValue);
            //case "int":
            //return comparedField.intValue.Equals(drawRanageIf.comparedValue);
            //case "float":
            //return comparedField.floatValue.Equals(drawRanageIf.comparedValue);
            default:
                // ��: Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                //Debug.LogError("�G���[: " + comparedField.type + " �̓T�|�[�g����Ă��܂���i" + path + "�j");
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �����𖞂����ꍇ�̂ݕ`��
        if (ShowMe(property))
        {
            Rect offsetPosition = position;
            offsetPosition.x = offsetPosition.x + 30;
            offsetPosition.width = offsetPosition.width - 30;

            // ��r�ΏۂƔ�r�l�̎擾
            //string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName) : drawRanageIf.comparedPropertyName;
            //comparedField = property.serializedObject.FindProperty(path);
            var comparedFieldValue = comparedField;
            var comparedValue = drawRanageIf.comparedValue;
            //bool conditionMet = false;

            if (comparedField.type == "int" || comparedField.type == "float")
            {
                var numericComparedFieldValue = (int)comparedField.intValue;
                var numericComparedValue = (int)(drawRanageIf.comparedValue);

                // ��������i�t�B�[���h�l�� comparedValue ���r�j
                switch (drawRanageIf.comparisonType)
                {
                    case DrawIfRangeAttribute.ComparisonType.Equals:
                        if (comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.NotEqual:
                        if (!comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.GreaterThan:
                        if (numericComparedFieldValue > numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.SmallerThan:
                        if (numericComparedFieldValue < numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.SmallerOrEqual:
                        if (numericComparedFieldValue <= numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.GreaterOrEqual:
                        if (numericComparedFieldValue >= numericComparedValue)
                            conditionMet = true;
                        else
                            conditionMet = false;
                        break;
                }
            }
            else if (comparedField.type == "bool")
            {
                switch (drawRanageIf.comparisonType)
                {
                    case DrawIfRangeAttribute.ComparisonType.Equals:
                        if (comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.NotEqual:
                        if (!comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;
                }
            }

            if (conditionMet)
            {
                label = EditorGUI.BeginProperty(position, label, property);

                EditorGUI.BeginChangeCheck();
                if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.Default)
                {
                    EditorGUI.PropertyField(offsetPosition, property, label);
                }
                else if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.FloatSlider)
                {
                    var newValue = EditorGUI.Slider(offsetPosition, label, property.floatValue, drawRanageIf.min, drawRanageIf.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = newValue;
                    }
                }
                else if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.IntSlider)
                {
                    var newValue = EditorGUI.IntSlider(offsetPosition, label, property.intValue, (int)drawRanageIf.min, (int)drawRanageIf.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.intValue = newValue;
                    }
                }

                EditorGUI.EndProperty();
            }

        }
    }

}
