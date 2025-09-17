using System.Collections;                         // コレクション基盤
using System.Collections.Generic;                 // List<T> 等
using UnityEngine;                                // Unity の基本 API
using UnityEditor;                                // エディタ拡張 API
using UnityEditorInternal;                        // ReorderableList 等
using System.Reflection;                          // リフレクション（PropertyEditor を開くのに使用）

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldSounds))]         // このエディタは EmeraldSounds コンポーネントに対応
    [CanEditMultipleObjects]                      // 複数オブジェクト同時編集を許可
    // ▼このクラスは「EmeraldSounds のインスペクタ拡張クラス」
    /// <summary>
    /// 【クラスの説明（日本語）】
    /// EmeraldSounds 用のカスタムインスペクタ。AI のサウンド設定を束ねる「サウンドプロファイル」の割り当て／作成／編集 UI を提供します。
    /// </summary>
    public class EmeraldSoundsEditor : Editor
    {
        [Header("エディタウィンドウ参照（注釈）")]
        public static EditorWindow EditorWindowRef; // 別タブで開いた PropertyEditor のキャッシュ

        [Header("折りたたみ見出しの GUIStyle（注釈）")]
        GUIStyle FoldoutStyle;                      // 見出し（Foldout）の描画スタイル

        [Header("サウンドエディタの見出しアイコン（注釈）")]
        Texture SoundsEditorIcon;                   // ヘッダーに表示するテクスチャ

        #region SerializedProperties
        [Header("SerializedProperty：折りたたみ/プロファイル参照（注釈）")]
        SerializedProperty HideSettingsFoldout,     // 全体ヘッダーを折りたたむか
                         SoundProfileProp,          // 割り当てられたサウンドプロファイルへの参照
                         SoundProfileFoldout;       // サウンドプロファイルセクションの折りたたみ
        #endregion

        /// <summary>
        /// 【OnEnable（日本語）】
        /// アイコンのロードと、SerializedProperty の初期化を行います。
        /// </summary>
        void OnEnable()
        {
            if (SoundsEditorIcon == null) SoundsEditorIcon = Resources.Load("Editor Icons/EmeraldSounds") as Texture; // リソースからアイコンを取得
            InitializeProperties();                                                                                   // プロパティの紐付け
        }

        /// <summary>
        /// 【プロパティ初期化（日本語）】
        /// serializedObject から対象フィールドの SerializedProperty を取得します。
        /// </summary>
        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");  // 見出しの折りたたみ
            SoundProfileFoldout = serializedObject.FindProperty("SoundProfileFoldout");  // サウンドプロファイルセクションの折りたたみ
            SoundProfileProp = serializedObject.FindProperty("SoundProfile");         // サウンドプロファイル参照
        }

        /// <summary>
        /// 【インスペクタ描画（日本語）】
        /// 見出し（ヘッダー）とサウンドプロファイルの設定セクションを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // 共通スタイル更新
            EmeraldSounds self = (EmeraldSounds)target;                 // 対象の EmeraldSounds 参照
            serializedObject.Update();                                  // 変更追跡開始

            // 見出し（"Sounds" → 「サウンド」に日本語化）
            CustomEditorProperties.BeginScriptHeaderNew("サウンド", SoundsEditorIcon, new GUIContent(), HideSettingsFoldout);

            // サウンドプロファイル未設定の警告表示
            MissingSoundProfileMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DisplaySoundProfile(self);  // サウンドプロファイル設定セクション
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties(); // 変更を反映
            CustomEditorProperties.EndScriptHeader();   // ヘッダー終了
        }

        /// <summary>
        /// 【サウンドプロファイル未設定の警告（日本語）】
        /// サウンドプロファイルが未割り当ての場合、作成/割り当てを促す警告を表示します。
        /// </summary>
        void MissingSoundProfileMessage(EmeraldSounds self)
        {
            if (self.SoundProfile == null)
            {
                // 元英語: This AI needs to have a Sound Profile. Press the 'Create New Sound Profile'...
                CustomEditorProperties.DisplaySetupWarning("この AI にはサウンドプロファイルが必要です。下の『新しいサウンドプロファイルを作成』ボタンで作成するか、既存のプロファイルを割り当ててください。");
            }
        }

        /// <summary>
        /// 【サウンドプロファイル設定 UI（日本語）】
        /// サウンドプロファイルの表示・編集・クリア・新規作成に関する UI を描画します。
        /// </summary>
        void DisplaySoundProfile(EmeraldSounds self)
        {
            // フォールドアウト（"Sound Profile Settings" → 「サウンドプロファイル設定」）
            SoundProfileFoldout.boolValue = CustomEditorProperties.Foldout(SoundProfileFoldout.boolValue, "サウンドプロファイル設定", true, FoldoutStyle);

            if (SoundProfileFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // セクションタイトル＋説明（"Sound Profile" → 「サウンドプロファイル」）
                CustomEditorProperties.TextTitleWithDescription(
                    "サウンドプロファイル",
                    "サウンドプロファイルは AI のサウンドデータをひとまとめに管理します。複数の AI で同一プロファイルを共有でき、プロファイルの変更はそれを使用するすべての AI に反映されます。必要に応じていくつでも作成可能です。下の各ボタンにマウスオーバーすると説明が表示されます。",
                    true);

                EditorGUILayout.Space();

                // プロファイル参照スロット
                EditorGUILayout.PropertyField(SoundProfileProp, new GUIContent("サウンドプロファイル"));
                CustomEditorProperties.CustomHelpLabelField("この AI が使用するサウンドプロファイルです。登録された全サウンドとボリュームが適用されます。", false);

                // 既存プロファイルがある場合：編集/クリアボタンを有効化
                EditorGUI.BeginDisabledGroup(self.SoundProfile == null);
                EditorGUILayout.Space();

                // 「Edit Sound Profile」→「サウンドプロファイルを編集」
                if (GUILayout.Button(
                        new GUIContent(
                            "サウンドプロファイルを編集",
                            "現在のサウンドプロファイルを別ウィンドウで開きます。プロファイルを参照しながら試聴・編集できます。"),
                        GUILayout.Height(20)))
                {
                    EditSoundProfile(self); // 別タブ（PropertyEditor）で開く
                }

                GUILayout.Space(2.5f);

                // 「Clear Sound Profile」→「サウンドプロファイルをクリア」
                if (GUILayout.Button(
                        new GUIContent(
                            "サウンドプロファイルをクリア",
                            "スロットをクリアして新しいプロファイルを割り当て可能にします。注：現在のプロファイルアセット自体はプロジェクト内に残ります。"),
                        GUILayout.Height(20)))
                {
                    SoundProfileProp.objectReferenceValue = null; // 参照を外すだけ（アセットは削除されない）
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(2.5f);

                // プロファイルが未設定の場合：新規作成ボタンを有効化
                EditorGUI.BeginDisabledGroup(self.SoundProfile != null);

                // 「Create New Sound Profile」→「新しいサウンドプロファイルを作成」
                if (GUILayout.Button(
                        new GUIContent(
                            "新しいサウンドプロファイルを作成",
                            "新規のサウンドプロファイルを作成します。既存を差し替えたい場合は、先に『サウンドプロファイルをクリア』でスロットを空にしてください。"),
                        GUILayout.Height(20)))
                {
                    CreateSoundProfile(self); // 新規アセット作成
                }

                EditorGUI.EndDisabledGroup();
                GUILayout.Space(2.5f);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【サウンドプロファイル新規作成（日本語）】
        /// ダイアログで保存先を指定し、新しい EmeraldSoundProfile アセットを生成します。
        /// </summary>
        void CreateSoundProfile(EmeraldSounds self)
        {
            // 「Save as Sound Profile」→「サウンドプロファイルとして保存」
            string FilePath = EditorUtility.SaveFilePanelInProject("サウンドプロファイルとして保存", "", "asset", "保存するファイル名を入力してください");

            if (string.IsNullOrEmpty(FilePath))
            {
                // 備考：一部のカスタムプロパティと併用時に EditorGUILayout エラーが出るためのワークアラウンド（原文のまま）
                CustomEditorProperties.BeginScriptHeader("", null);
                CustomEditorProperties.BeginFoldoutWindowBox();
                return;
            }

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(FilePath)))
            {
                // 新規アセットを作成して割り当て
                EmeraldSoundProfile NewSoundProfile = CreateInstance<EmeraldSoundProfile>();
                AssetDatabase.CreateAsset(NewSoundProfile, FilePath);
                self.SoundProfile = NewSoundProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                // 既存アセットをロードして割り当て
                var ExistingSoundProfile = AssetDatabase.LoadAssetAtPath(FilePath, typeof(EmeraldSoundProfile));
                self.SoundProfile = (EmeraldSoundProfile)ExistingSoundProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // 備考：EditorGUILayout エラー対策のワークアラウンド（原文のまま）
            CustomEditorProperties.BeginScriptHeader("", null);
            CustomEditorProperties.BeginFoldoutWindowBox();
        }

        /// <summary>
        /// 【サウンドプロファイル編集ウィンドウを開く（日本語）】
        /// 別タブの PropertyEditor として開き、試聴・編集しやすくします（Unity 2021.2 未満は反射で呼び出し）。
        /// </summary>
        void EditSoundProfile(EmeraldSounds self)
        {
            if (self.SoundProfile == null)
                return; // 参照がない場合は何もしない

            // 既に他のサウンドプロファイルの PropertyEditor が開いていれば閉じる（1つのみ有効）
            if (EditorWindowRef != null && EditorWindowRef.name == "Sound Profile")
                EditorWindowRef.Close();

            System.Type propertyEditorType = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"); // 内部型を取得
            System.Type[] callTypes = new[] { typeof(Object), typeof(bool) };                               // メソッドシグネチャ
            object[] callOpenBuffer = { null, true };                                                       // 引数バッファ（最初に対象を設定）

            // API が公開されていない Unity 2021.2 未満で、反射を使って PropertyEditor を開く
            MethodInfo openPropertyEditorInfo;
            openPropertyEditorInfo = propertyEditorType.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic, null, callTypes, null);
            callOpenBuffer[0] = self.SoundProfile;                      // 開きたい対象（サウンドプロファイル）を設定
            openPropertyEditorInfo.Invoke(null, callOpenBuffer);        // 呼び出し

            // 開いた PropertyEditor を取得・命名・最小サイズを設定（1つのみ）
            EditorWindowRef = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"));
            EditorWindowRef.name = "Sound Profile";                      // タブ名（※Unity の内部ウィンドウ名仕様に合わせて英語のまま）
            EditorWindowRef.minSize = new Vector2(Screen.currentResolution.width / 4f, Screen.currentResolution.height / 2f);
        }
    }
}
