using UnityEngine;
using UnityEditor;

public class TagSetterEditor : EditorWindow
{
    /// <summary>
    /// Unity �� ���j���[ ���� Tools > Tag Setter ���J��
    /// �^�O������́i��: SetPoint�j
    /// �^�O��ݒ肵�����I�u�W�F�N�g��I�����āA�{�^��������
    /// </summary>
    //�ݒ肷��^�O��
    private string targetTag = "SetPoint";
    private GameObject[] selectedObjects;

    [MenuItem("Tools/Tag Setter")]
    public static void ShowWindow()
    {
        GetWindow<TagSetterEditor>("Tag Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("�I�u�W�F�N�g�Ƀ^�O��ݒ�", EditorStyles.boldLabel);
        targetTag = EditorGUILayout.TextField("�^�O��", targetTag);

        if (GUILayout.Button("�I�������I�u�W�F�N�g�Ƀ^�O��ݒ�"))
        {
            SetTagsToSelectedObjects();
        }
    }

    private void SetTagsToSelectedObjects()
    {
        selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("�I�u�W�F�N�g���I������Ă��܂���I");
            return;
        }

        foreach (var obj in selectedObjects)
        {
            obj.tag = targetTag;
        }

        Debug.Log($"�I������ {selectedObjects.Length} �̃I�u�W�F�N�g�� '{targetTag}' �^�O��ݒ肵�܂����I");
    }
}
