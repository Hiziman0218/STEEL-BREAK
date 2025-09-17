using UnityEditor;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【LayerAttributeEditor】
    /// <see cref="LayerAttribute"/> が付与された int フィールドを、
    /// インスペクタ上で「Layer 選択 UI」として描画する PropertyDrawer。
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeEditor : PropertyDrawer
    {
        /// <summary>
        /// 対象プロパティをレイヤー選択フィールドとして描画し、選択結果を int 値として保持します。
        /// </summary>
        /// <param name="position">描画領域の Rect</param>
        /// <param name="property">対象の SerializedProperty（int を想定）</param>
        /// <param name="label">表示用ラベル</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Unity標準の LayerField を用いて、整数値のレイヤーIDを選択させる
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}
