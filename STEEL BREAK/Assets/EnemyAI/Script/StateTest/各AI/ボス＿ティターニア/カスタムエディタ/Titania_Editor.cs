using Plugins.RaycastPro.Demo.Scripts;
using StateMachineAI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(StateMachineAI.Titania_T))]
public class Titania_TEditor : Editor
{
    private bool showDebug = false;

    public override void OnInspectorGUI()
    {
        // デフォルトの Inspector を描画
        DrawDefaultInspector();

        // 区切り線を入れる
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== 変数、参照確認用 ===", EditorStyles.boldLabel);

        // デバッグ用 Foldout
        showDebug = EditorGUILayout.Foldout(showDebug, "Debug Variables");
        if (showDebug)
        {
            Titania_T t = (Titania_T)target;

            EditorGUILayout.ObjectField("CoolDown", t.m_CoolDown, typeof(CoolDown), true);
            EditorGUILayout.ObjectField("Rigidbody", t.m_Rigidbody, typeof(Rigidbody), true);
            EditorGUILayout.ObjectField("Enemy", t.m_Enemy, typeof(Enemy), true);
            EditorGUILayout.ObjectField("MyAgent", t.myAgent, typeof(GameObject), true);
            EditorGUILayout.FloatField("Current Speed", t.m_currentspeed);
            EditorGUILayout.Toggle("Is Spawning Fairy", t.isSpawningFairy);
            EditorGUILayout.Vector3Field("Rush Direction", t.m_RushDirection);
            EditorGUILayout.ObjectField("RC Controller", t.m_Controller, typeof(SteeringController), true);
        }
    }
}
#endif