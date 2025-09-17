using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// 【CompareEnumWithRangePropertyDrawer】
/// CompareEnumWithRangeAttribute が付与されたフィールド/プロパティを、
/// 指定条件を満たす場合にだけインスペクタへ描画します。
/// さらに、属性の設定（Float/Int スライダー、最小/最大）に応じて
/// スライダー UI で描画します。
/// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(CompareEnumWithRangeAttribute))]
public class CompareEnumWithRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("対象プロパティに付与された CompareEnumWithRangeAttribute の参照")]
    // このプロパティに紐づく属性への参照
    CompareEnumWithRangeAttribute CompareEnumWithRangeAttribute;

    [Header("比較対象となる SerializedProperty（属性の comparedPropertyName で特定）")]
    // 比較に使用するフィールド（列挙など）
    SerializedProperty comparedField;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!ShowMe(property))
            return 0f;

        // 既定の高さを返す
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// 表示判定。エラー時はデフォルトで「表示する」挙動にフォールバックします。
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        CompareEnumWithRangeAttribute = attribute as CompareEnumWithRangeAttribute;

        // プロパティのパスから、比較対象プロパティのパスを解決
        string path = property.propertyPath.Contains(".")
            ? System.IO.Path.ChangeExtension(property.propertyPath, CompareEnumWithRangeAttribute.comparedPropertyName)
            : CompareEnumWithRangeAttribute.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            // 旧: "Cannot find property with name: " + path
            Debug.LogError("指定された名前のプロパティが見つかりません: " + path);
            return true; // エラー時は表示しておく
        }

        switch (comparedField.type)
        {
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue1) ||
                CompareEnumWithRangeAttribute.comparedValue2 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue2) ||
                CompareEnumWithRangeAttribute.comparedValue3 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue3);
            default:
                // 旧: "Error: " + comparedField.type + " is not supported of " + path
                Debug.LogError("エラー: " + comparedField.type + " はサポートされていません（" + path + "）");
                return true; // エラー時は表示しておく
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 条件を満たす場合のみ描画
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
