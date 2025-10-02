using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAction), true)]
    public class ActionObjectEditor : Editor
    {
        [Header("インスペクターの折りたたみ見出し用 GUI スタイル")]
        GUIStyle FoldoutStyle;

        [Header("アクションエディタ用のアイコン（Resources/Editor Icons/EmeraldBehaviors）")]
        Texture ActionEditorIcon;

        [Header("派生クラス（子クラス）の公開/非公開インスタンスフィールド一覧（反射で取得）")]
        FieldInfo[] CustomFields;

        [Header("インスペクター: 『設定を隠す』フラグ（折りたたみ用）")]
        SerializedProperty HideSettingsFoldout;

        [Header("インスペクター: 『デフォルト設定』の折りたたみ状態")]
        SerializedProperty DefaultSettingsFoldout;

        [Header("インスペクター: 『カスタム設定』の折りたたみ状態")]
        SerializedProperty CustomSettingsFoldout;

        [Header("インスペクター: 『情報設定』の折りたたみ状態")]
        SerializedProperty InfoSettingsFoldout;

        [Header("アクションの開始条件（EnterConditions / AnimationStateTypes フラグ）")]
        SerializedProperty EnterConditions;

        [Header("アクションの終了条件（ExitConditions / AnimationStateTypes フラグ）")]
        SerializedProperty ExitConditions;

        [Header("クールダウンの経過を許可する条件（CooldownConditions / AnimationStateTypes フラグ）")]
        SerializedProperty CooldownConditions;

        [Header("クールダウンの長さ（秒）")]
        SerializedProperty CooldownLength;

        [Header("クールダウンを使用するかどうか")]
        SerializedProperty UseCooldown;

        [Header("アクション名（インスペクター表示用）")]
        SerializedProperty ActionName;

        void OnEnable()
        {
            if (ActionEditorIcon == null) ActionEditorIcon = Resources.Load("Editor Icons/EmeraldBehaviors") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DefaultSettingsFoldout = serializedObject.FindProperty("DefaultSettingsFoldout");
            CustomSettingsFoldout = serializedObject.FindProperty("CustomSettingsFoldout");
            InfoSettingsFoldout = serializedObject.FindProperty("InfoSettingsFoldout");

            EnterConditions = serializedObject.FindProperty("EnterConditions");
            ExitConditions = serializedObject.FindProperty("ExitConditions");
            CooldownConditions = serializedObject.FindProperty("CooldownConditions");
            CooldownLength = serializedObject.FindProperty("CooldownLength");
            UseCooldown = serializedObject.FindProperty("UseCooldown");
            ActionName = serializedObject.FindProperty("ActionName");

            // 親クラスに属さない（=このエディタで表示する）変数をすべて取得します。
            CustomFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public override void OnInspectorGUI()
        {
            EmeraldAction self = (EmeraldAction)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();
            CustomEditorProperties.BeginScriptHeaderNew(self.ActionName, ActionEditorIcon, new GUIContent(), HideSettingsFoldout, false);

            /*
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-15f);
            // タイトルと説明（不要の場合は非表示のまま）
            CustomEditorProperties.TextTitleWithDescription(self.ActionName + " アクション", self.ActionDescription, false);
            GUILayout.Space(-4f);
            EditorGUILayout.EndHorizontal();
            */

            EditorGUILayout.Space();
            DefaultSettings(self);
            EditorGUILayout.Space();
            CustomSettings(self);
            EditorGUILayout.Space();
            InfoSettings(self);
            EditorGUILayout.Space();
            CustomEditorProperties.EndScriptHeader();
            serializedObject.ApplyModifiedProperties();
        }

        void DefaultSettings(EmeraldAction self)
        {
            DefaultSettingsFoldout.boolValue = EditorGUILayout.Foldout(DefaultSettingsFoldout.boolValue, "デフォルト設定", true, FoldoutStyle);

            if (DefaultSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "デフォルト設定",
                    "親クラス（EmeraldAction）の既定変数です。子クラスの変数は下の『カスタム設定』に表示されます。\n" +
                    "このアクションの名称と説明文は『情報設定』から編集できます。",
                    true
                );

                EditorGUILayout.PropertyField(EnterConditions, new GUIContent("開始条件（Enter Conditions）"));
                EditorGUILayout.PropertyField(ExitConditions, new GUIContent("終了条件（Exit Conditions）"));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(UseCooldown, new GUIContent("クールダウンを使用"));

                if (self.UseCooldown)
                {
                    CustomEditorProperties.BeginIndent();
                    EditorGUILayout.PropertyField(CooldownConditions, new GUIContent("クールダウン許可条件（Cooldown Conditions）"));
                    EditorGUILayout.PropertyField(CooldownLength, new GUIContent("クールダウン長（秒）"));
                    CustomEditorProperties.EndIndent();
                }
                else
                {
                    CustomEditorProperties.NoticeTextDescription(
                        "クールダウンを無効にすると、開始条件（およびコードでの制御）に合致する限り、このアクションは継続的に実行されます。",
                        false
                    );
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 子クラスのカスタム変数を、エディタの専用セクション内に表示します。
        /// </summary>
        void CustomSettings(EmeraldAction self)
        {
            CustomSettingsFoldout.boolValue = EditorGUILayout.Foldout(CustomSettingsFoldout.boolValue, self.ActionName + " 設定", true, FoldoutStyle);

            if (CustomSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(self.ActionName + " 設定", self.ActionDescription, true);

                foreach (FieldInfo field in CustomFields)
                {
                    // 配列は余白を追加してオフセット表示
                    if (field.FieldType.GetElementType() != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // List は余白を追加してオフセット表示
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // 単体の変数はオフセットせず表示
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                    }
                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InfoSettings(EmeraldAction self)
        {
            InfoSettingsFoldout.boolValue = EditorGUILayout.Foldout(InfoSettingsFoldout.boolValue, "情報設定", true, FoldoutStyle);

            if (InfoSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("情報設定", "このアクションの『名前』と『説明文』を編集します。", true);

                CustomEditorProperties.CustomStringPropertyField(ActionName, "アクション名", "", true);

                GUIStyle style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("アクションの説明文");
                string Value = EditorGUILayout.TextArea(self.ActionDescription, style);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(self, "アクションの説明文を変更");
                    self.ActionDescription = Value;
                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
