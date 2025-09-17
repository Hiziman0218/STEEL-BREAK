using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// 【DrawIfRangePropertyDrawer】
/// DrawIfRangeAttribute が付与されたフィールド/プロパティを、指定条件を満たした場合にのみ
/// インスペクタへ描画するための PropertyDrawer。
/// さらに、属性設定（既定／float スライダー／int スライダー、および最小・最大値）に応じて
/// 適切な UI（スライダー等）で描画します。
///
/// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(DrawIfRangeAttribute))]
public class DrawIfRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("対象プロパティに付与された DrawIfRangeAttribute の参照")]
    // プロパティに付与された属性への参照
    DrawIfRangeAttribute drawRanageIf;

    [Header("比較対象となる SerializedProperty（属性 comparedPropertyName で特定）")]
    // 比較に使用するフィールド
    SerializedProperty comparedField;

    [Header("条件が満たされたかどうか（true=描画する）")]
    bool conditionMet;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // ※ 条件を満たさない場合は高さを負値にして「非表示」相当のレイアウトへ
        //if (!ShowMe(property))
        if (!conditionMet)
            return -2f;

        // 既定の高さを返す
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// エラー時は既定で「表示する」にフォールバックします。
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        drawRanageIf = attribute as DrawIfRangeAttribute;

        // プロパティ名を、引数の comparedPropertyName へ置き換えて解決
        string path = property.propertyPath.Contains(".")
            ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName)
            : drawRanageIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            // 旧: "Cannot find property with name: " + path
            Debug.LogError("指定された名前のプロパティが見つかりません: " + path);
            return true;
        }

        // 値を取得し、型に応じて比較
        switch (comparedField.type)
        {   // 必要に応じて独自型を拡張可能
            case "bool":
                return comparedField.boolValue.Equals(drawRanageIf.comparedValue);
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)drawRanageIf.comparedValue);
            //case "int":
            //return comparedField.intValue.Equals(drawRanageIf.comparedValue);
            //case "float":
            //return comparedField.floatValue.Equals(drawRanageIf.comparedValue);
            default:
                // 旧: Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                //Debug.LogError("エラー: " + comparedField.type + " はサポートされていません（" + path + "）");
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 条件を満たす場合のみ描画
        if (ShowMe(property))
        {
            Rect offsetPosition = position;
            offsetPosition.x = offsetPosition.x + 30;
            offsetPosition.width = offsetPosition.width - 30;

            // 比較対象と比較値の取得
            //string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName) : drawRanageIf.comparedPropertyName;
            //comparedField = property.serializedObject.FindProperty(path);
            var comparedFieldValue = comparedField;
            var comparedValue = drawRanageIf.comparedValue;
            //bool conditionMet = false;

            if (comparedField.type == "int" || comparedField.type == "float")
            {
                var numericComparedFieldValue = (int)comparedField.intValue;
                var numericComparedValue = (int)(drawRanageIf.comparedValue);

                // 条件判定（フィールド値と comparedValue を比較）
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
