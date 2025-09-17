using UnityEditor;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// �yLayerAttributeEditor�z
    /// <see cref="LayerAttribute"/> ���t�^���ꂽ int �t�B�[���h���A
    /// �C���X�y�N�^��ŁuLayer �I�� UI�v�Ƃ��ĕ`�悷�� PropertyDrawer�B
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeEditor : PropertyDrawer
    {
        /// <summary>
        /// �Ώۃv���p�e�B�����C���[�I���t�B�[���h�Ƃ��ĕ`�悵�A�I�����ʂ� int �l�Ƃ��ĕێ����܂��B
        /// </summary>
        /// <param name="position">�`��̈�� Rect</param>
        /// <param name="property">�Ώۂ� SerializedProperty�iint ��z��j</param>
        /// <param name="label">�\���p���x��</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Unity�W���� LayerField ��p���āA�����l�̃��C���[ID��I��������
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}
