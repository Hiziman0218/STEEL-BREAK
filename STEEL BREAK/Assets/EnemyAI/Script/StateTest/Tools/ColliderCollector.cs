using System.Reflection;
using UnityEngine;

public static class ColliderAutoInitializer
{
    /// <summary>
    /// ColliderAutoInitializer.Initialize(this);
    /// で取得可能
    /// 対象オブジェクトから Collider を自動取得（単体）
    /// フィールドが null の場合のみ代入
    /// </summary>
    /// <param name="target">MonoBehaviour を継承した対象スクリプト</param>
    public static void Initialize(MonoBehaviour target)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var fields = target.GetType().GetFields(flags);

        foreach (var field in fields)
        {
            // 対象が Collider のみ
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
                        Debug.Log($"[AutoCollider] {field.Name} に {col.GetType().Name} を代入しました。");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoCollider] {field.FieldType.Name} が {target.name} に見つかりませんでした → {field.Name}");
                    }
                }
            }
        }
    }
}
