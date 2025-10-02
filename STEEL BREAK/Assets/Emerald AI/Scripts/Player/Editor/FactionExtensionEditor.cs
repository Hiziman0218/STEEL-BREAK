using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(FactionExtension))]
    [CanEditMultipleObjects]
    public class FactionExtensionEditor : Editor
    {
        [Header("Foldout（折りたたみ）タイトルのスタイル（内部用）")]
        GUIStyle FoldoutStyle;

        [Header("FactionExtension エディタ用アイコン（内部用）")]
        Texture FactionExtensionEditorIcon;

        [Header("現在の派閥IDの SerializedProperty（CurrentFaction）")]
        SerializedProperty CurrentFactionProp;

        [Header("設定全体の折りたたみを隠すフラグ（HideSettingsFoldout）")]
        SerializedProperty HideSettingsFoldout;

        [Header("派閥設定セクションの折りたたみ（FactionFoldout）")]
        SerializedProperty FactionFoldout;

        void OnEnable()
        {
            if (FactionExtensionEditorIcon == null) FactionExtensionEditorIcon = Resources.Load("Editor Icons/FactionExtension") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            CurrentFactionProp = serializedObject.FindProperty("CurrentFaction");
            FactionFoldout = serializedObject.FindProperty("FactionFoldout");
            LoadFactionData();
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            FactionExtension self = (FactionExtension)target;
            serializedObject.Update();

            // ヘッダ（日本語化）
            CustomEditorProperties.BeginScriptHeaderNew("派閥拡張", FactionExtensionEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                FactionSetting(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        void FactionSetting(FactionExtension self)
        {
            // セクション見出し（日本語化）
            FactionFoldout.boolValue = EditorGUILayout.Foldout(FactionFoldout.boolValue, "派閥設定", true, FoldoutStyle);

            if (FactionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトルと説明（日本語化）
                CustomEditorProperties.TextTitleWithDescription(
                    "派閥設定",
                    "Unity の Tag システムに依存せず、この GameObject を AI が識別できるように派閥（Faction）IDを付与します。これにより、潜在的な全ターゲットが同じ Unity の Tag や Layer を共有していても区別できます。",
                    true
                );

                // 派閥選択（ラベルを日本語化）
                CustomEditorProperties.FactionListEnum(new Rect(), new GUIContent(), CurrentFactionProp, "派閥", FactionExtension.StringFactionList);

                // 説明（日本語化）
                CustomEditorProperties.CustomHelpLabelField(
                    "この派閥は、この GameObject（主にプレイヤーなどの非AIオブジェクト）を識別するために使用されます。AI がターゲットを探索する際、この名前で判別します。",
                    true
                );

                CustomEditorProperties.CustomHelpLabelField("派閥は『派閥マネージャ（Faction Manager）』で作成・削除できます。", false);

                // ボタン（日本語化）
                if (GUILayout.Button("派閥マネージャを開く"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void LoadFactionData()
        {
            FactionExtension.StringFactionList.Clear();
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));

            if (FactionData != null)
            {
                foreach (string s in FactionData.FactionNameList)
                {
                    if (!FactionExtension.StringFactionList.Contains(s) && s != "")
                    {
                        FactionExtension.StringFactionList.Add(s);
                    }
                }
            }
        }
    }
}
