using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
    #region Fields

    [Header("対象プロパティに付与された DrawIfAttribute の参照")]
    // プロパティに付与された属性への参照
    DrawIfAttribute drawIf;

    [Header("比較対象となる SerializedProperty（属性の comparedPropertyName で特定）")]
    // 比較に使用するフィールド
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

                // 末尾の余白を削除（要素間のスペースのみ残す）
                totalHeight -= EditorGUIUtility.standardVerticalSpacing;

                return totalHeight;
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }
    }

    /// <summary>
    /// エラー時は既定で「表示する」挙動にフォールバックします。
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        drawIf = attribute as DrawIfAttribute;
        // プロパティ名を、引数で渡された comparedPropertyName に置き換えて解決
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.comparedPropertyName) : drawIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("指定された名前のプロパティが見つかりません: " + path);
            return true;
        }

        // 値を取得し、型に応じて比較
        switch (comparedField.type)
        {   // 必要に応じて独自型を拡張可能
            case "bool":
                return comparedField.boolValue.Equals(drawIf.comparedValue);
            case "Enum":
                return (comparedField.intValue & (int)drawIf.comparedValue) != 0;
            default:
                Debug.LogError("エラー: " + comparedField.type + " はサポートされていません（" + path + "）");
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Debug.Log(ShowMe(property) + "  " + property.name);
        // 条件を満たす場合は、そのままフィールドを描画
        if (ShowMe(property))
        {
            // Generic はカスタムクラスを意味する…
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

        } // …一致しない場合、無効化方法が ReadOnly なら「無効表示」で描画
        else if (drawIf.disablingType == DrawIfAttribute.DisablingType.ReadOnly)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }

}
