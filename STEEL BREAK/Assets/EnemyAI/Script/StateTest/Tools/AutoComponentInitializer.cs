using System.Reflection;
using UnityEngine;

public static class AutoComponentInitializer
{

    /// <summary>
    /// AutoComponentInitializer.InitializeComponents(this);
    /// �A�^�b�`���Ă���R���|�[�l���g��������GetComponent���Ă����X�v���N�g
    /// </summary>
    /// <param name="target"></param>
    public static void InitializeComponents(MonoBehaviour target)
    {
        //���t���N�V�����őΏۂƂȂ�t�B�[���h���擾���邽�߂̃t���O�ݒ�
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        // ��̃t���O���g���āA�ΏۃI�u�W�F�N�g�̃t�B�[���h�����擾
        var fields = target.GetType().GetFields(flags);

        foreach (var field in fields)
        {
            //�t�B�[���h�̌^�� Unity �� Component�i��FTransform, Rigidbody, Collider �Ȃǁj���ǂ������`�F�b�N
            if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var current = field.GetValue(target);
                // null�Ȃ玩���������
                if (current == null)
                {
                    var component = target.GetComponent(field.FieldType);
                    if (component != null)
                    {
                        field.SetValue(target, component);
                        Debug.Log($"[AutoInit] {target.GetType().Name}.{field.Name} �� {component.GetType().Name} �������擾���܂����B");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoInit] {field.FieldType.Name} �� {target.gameObject.name} �̃I�u�W�F�N�g�Ō�����܂���ł��� �� {field.Name}");
                    }
                }
            }
        }
    }
}
