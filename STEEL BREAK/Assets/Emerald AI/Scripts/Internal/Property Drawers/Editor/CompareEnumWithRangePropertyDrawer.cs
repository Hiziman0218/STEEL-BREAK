using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// �yCompareEnumWithRangePropertyDrawer�z
/// CompareEnumWithRangeAttribute ���t�^���ꂽ�t�B�[���h/�v���p�e�B���A
/// �w������𖞂����ꍇ�ɂ����C���X�y�N�^�֕`�悵�܂��B
/// ����ɁA�����̐ݒ�iFloat/Int �X���C�_�[�A�ŏ�/�ő�j�ɉ�����
/// �X���C�_�[ UI �ŕ`�悵�܂��B
/// �Q�l: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(CompareEnumWithRangeAttribute))]
public class CompareEnumWithRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("�Ώۃv���p�e�B�ɕt�^���ꂽ CompareEnumWithRangeAttribute �̎Q��")]
    // ���̃v���p�e�B�ɕR�Â������ւ̎Q��
    CompareEnumWithRangeAttribute CompareEnumWithRangeAttribute;

    [Header("��r�ΏۂƂȂ� SerializedProperty�i������ comparedPropertyName �œ���j")]
    // ��r�Ɏg�p����t�B�[���h�i�񋓂Ȃǁj
    SerializedProperty comparedField;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!ShowMe(property))
            return 0f;

        // ����̍�����Ԃ�
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// �\������B�G���[���̓f�t�H���g�Łu�\������v�����Ƀt�H�[���o�b�N���܂��B
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        CompareEnumWithRangeAttribute = attribute as CompareEnumWithRangeAttribute;

        // �v���p�e�B�̃p�X����A��r�Ώۃv���p�e�B�̃p�X������
        string path = property.propertyPath.Contains(".")
            ? System.IO.Path.ChangeExtension(property.propertyPath, CompareEnumWithRangeAttribute.comparedPropertyName)
            : CompareEnumWithRangeAttribute.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            // ��: "Cannot find property with name: " + path
            Debug.LogError("�w�肳�ꂽ���O�̃v���p�e�B��������܂���: " + path);
            return true; // �G���[���͕\�����Ă���
        }

        switch (comparedField.type)
        {
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue1) ||
                CompareEnumWithRangeAttribute.comparedValue2 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue2) ||
                CompareEnumWithRangeAttribute.comparedValue3 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue3);
            default:
                // ��: "Error: " + comparedField.type + " is not supported of " + path
                Debug.LogError("�G���[: " + comparedField.type + " �̓T�|�[�g����Ă��܂���i" + path + "�j");
                return true; // �G���[���͕\�����Ă���
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �����𖞂����ꍇ�̂ݕ`��
        if (ShowMe(property))
        {
            Rect offsetPosition = position;
            //offsetPosition.x = offsetPosition.x + 30;
            //offsetPosition.width = offsetPosition.width - 30;

            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            if (CompareEnumWithRangeAttribute.styleType == CompareEnumWithRangeAttribute.StyleType.FloatSlider)
            {
                var newValue = EditorGUI.Slider(offsetPosition, label, property.floatValue, CompareEnumWithRangeAttribute.min, CompareEnumWithRangeAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = newValue;
                }
            }
            else if (CompareEnumWithRangeAttribute.styleType == CompareEnumWithRangeAttribute.StyleType.IntSlider)
            {
                var newValue = EditorGUI.IntSlider(offsetPosition, label, property.intValue, (int)CompareEnumWithRangeAttribute.min, (int)CompareEnumWithRangeAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = newValue;
                }
            }

            EditorGUI.EndProperty();
            //EditorGUI.PropertyField(position, property, label);
        }
    }
}
