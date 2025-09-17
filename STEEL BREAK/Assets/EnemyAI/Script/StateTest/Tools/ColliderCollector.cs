using System.Reflection;
using UnityEngine;

public static class ColliderAutoInitializer
{
    /// <summary>
    /// ColliderAutoInitializer.Initialize(this);
    /// �Ŏ擾�\
    /// �ΏۃI�u�W�F�N�g���� Collider �������擾�i�P�́j
    /// �t�B�[���h�� null �̏ꍇ�̂ݑ��
    /// </summary>
    /// <param name="target">MonoBehaviour ���p�������ΏۃX�N���v�g</param>
    public static void Initialize(MonoBehaviour target)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = target.GetType().GetFields(flags);

        foreach (var field in fields)
        {
            // �Ώۂ� Collider �̂�
            if (typeof(Collider).IsAssignableFrom(field.FieldType))
            {
                var current = field.GetValue(target);
                if (current == null)
                {
                    var col = target.GetComponent(field.FieldType)
                           ?? target.GetComponentInChildren(field.FieldType);

                    if (col != null)
                    {
                        field.SetValue(target, col);
                        Debug.Log($"[AutoCollider] {field.Name} �� {col.GetType().Name} �������܂����B");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoCollider] {field.FieldType.Name} �� {target.name} �Ɍ�����܂���ł��� �� {field.Name}");
                    }
                }
            }
        }
    }
}
