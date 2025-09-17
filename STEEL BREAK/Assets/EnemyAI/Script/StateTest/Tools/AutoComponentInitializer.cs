using System.Reflection;
using UnityEngine;

public static class AutoComponentInitializer
{

    /// <summary>
    /// AutoComponentInitializer.InitializeComponents(this);
    /// アタッチしているコンポーネントを自動でGetComponentしてくれるスプリクト
    /// </summary>
    /// <param name="target"></param>
    public static void InitializeComponents(MonoBehaviour target)
    {
        //リフレクションで対象となるフィールドを取得するためのフラグ設定
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        // 上のフラグを使って、対象オブジェクトのフィールド情報を取得
        var fields = target.GetType().GetFields(flags);

        foreach (var field in fields)
        {
            //フィールドの型が Unity の Component（例：Transform, Rigidbody, Collider など）かどうかをチェック
            if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var current = field.GetValue(target);
                // nullなら自動代入処理
                if (current == null)
                {
                    var component = target.GetComponent(field.FieldType);
                    if (component != null)
                    {
                        field.SetValue(target, component);
                        Debug.Log($"[AutoInit] {target.GetType().Name}.{field.Name} に {component.GetType().Name} を自動取得しました。");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoInit] {field.FieldType.Name} が {target.gameObject.name} のオブジェクトで見つかりませんでした → {field.Name}");
                    }
                }
            }
        }
    }
}
