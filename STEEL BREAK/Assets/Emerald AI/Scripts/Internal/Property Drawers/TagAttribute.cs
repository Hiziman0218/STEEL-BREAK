using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

/// <summary>
/// 【TagAttribute】
/// インスペクタ上で **Tag** を選択できるようにするための PropertyAttribute。
/// string フィールドに付与することで、テキスト入力ではなく Tag 選択 UI を表示します。
/// </summary>
public class TagAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    /// <summary>
    /// 【TagAttributeDrawer】
    /// 上記属性が付与された string フィールドを、インスペクタ上で Tag 選択 UI として描画します。
    /// </summary>
    [CustomPropertyDrawer(typeof(TagAttribute))]
    private class TagAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// プロパティ行の高さを返します（1行分）。
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// 実際の描画処理。
        /// - 対象が string 以外ならエラーメッセージを表示
        /// - 未登録のタグ名が入っていたら空文字へリセット
        /// - 空のときは赤色で強調表示
        /// - TagField でタグを選択
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // string 以外に付与されていた場合はエラー表示
            if (property.propertyType != SerializedPropertyType.String)
            {
                // 旧: $"{nameof(TagAttribute)} can only be used for strings!"
                EditorGUI.HelpBox(position, $"{nameof(TagAttribute)} は string 型にのみ使用できます！", MessageType.Error);
                return;
            }

            // 未登録のタグ名が入っていたら空文字にする
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(property.stringValue))
            {
                property.stringValue = "";
            }

            // 値が空のときは赤で強調
            var color = GUI.color;
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                GUI.color = Color.red;
            }

            // Tag 選択 UI
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
