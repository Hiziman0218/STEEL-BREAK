using UnityEngine;
using UnityEditor;

public class TagSetterEditor : EditorWindow
{
    /// <summary>
    /// Unity の メニュー から Tools > Tag Setter を開く
    /// タグ名を入力（例: SetPoint）
    /// タグを設定したいオブジェクトを選択して、ボタンを押す
    /// </summary>
    //設定するタグ名
    private string targetTag = "SetPoint";
    private GameObject[] selectedObjects;

    [MenuItem("Tools/Tag Setter")]
    public static void ShowWindow()
    {
        GetWindow<TagSetterEditor>("Tag Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("オブジェクトにタグを設定", EditorStyles.boldLabel);
        targetTag = EditorGUILayout.TextField("タグ名", targetTag);

        if (GUILayout.Button("選択したオブジェクトにタグを設定"))
        {
            SetTagsToSelectedObjects();
        }
    }

    private void SetTagsToSelectedObjects()
    {
        selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていません！");
            return;
        }

        foreach (var obj in selectedObjects)
        {
            obj.tag = targetTag;
        }

        Debug.Log($"選択した {selectedObjects.Length} 個のオブジェクトに '{targetTag}' タグを設定しました！");
    }
}
