using System.Collections;                          // （注）コレクション：IEnumerator 等
using System.Collections.Generic;                  // （注）List などのジェネリックコレクション
using UnityEngine;                                 // （注）Unity 基本 API
using UnityEditor;                                 // （注）Editor 拡張 API
using System.Reflection;                           // （注）リフレクション（PropertyEditor の生成に使用）
using UnityEditorInternal;                         // （注）StageUtility 等
using System.Linq;                                 // （注）LINQ ユーティリティ
using UnityEditor.SceneManagement;                 // （注）シーン変更のダーティフラグ制御
using UnityEngine.SceneManagement;                 // （注）Scene 参照

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAnimation))]       // （注）対象コンポーネントのカスタムインスペクタ
    [CanEditMultipleObjects]                       // （注）複数選択時も編集可能
    // 【クラス概要】EmeraldAnimationEditor：
    //  EmeraldAnimation 用のカスタムインスペクタ。Animation Profile の割り当て/生成/編集、
    //  アニメビューワの起動、各種ガイドメッセージの表示を行う Editor 拡張クラス。
    public class EmeraldAnimationEditor : Editor
    {
        [Header("エディタウィンドウ参照（PropertyEditor のキャッシュ）")]
        public static EditorWindow EditorWindowRef;            // （注）静的参照。複数起動を防ぐためのキャッシュ。

        #region SerializedProperties
        [Header("近接攻撃(Weapon Type 1)の攻撃アニメ列挙（名前文字列）")]
        List<string> Type1AttackAnimationEnum = new List<string>();  // （注）AnimationProfile から抽出したクリップ名を格納

        [Header("遠隔攻撃(Weapon Type 2)の攻撃アニメ列挙（名前文字列）")]
        List<string> Type2AttackAnimationEnum = new List<string>();  // （注）同上（Type2）

        [Header("折りたたみ見出しの GUIStyle（カスタム）")]
        GUIStyle FoldoutStyle;                                // （注）CustomEditorProperties から更新

        [Header("アニメーションエディタ用アイコン（Resources から取得）")]
        Texture AnimationsEditorIcon;                         // （注）Editor Icons/EmeraldAnimation

        [Header("対象 GameObject がシーン上に存在するか")]
        bool IsInScene;                                       // （注）プロジェクト内のプレハブではなく、Hierarchy 上か

        [Header("Prefab ステージ（プレハブ編集中）かどうか")]
        bool IsInPrefabInstance;                              // （注）MainStage 以外＝Prefab 編集中

        [Header("Animation Profile 参照の SerializedProperty")]
        SerializedProperty AnimationProfileProp;              // （注）self.m_AnimationProfile を参照する SP

        [Header("エディタ：設定折りたたみの表示/非表示フラグ")]
        SerializedProperty HideSettingsFoldout;               // （注）Inspector の大見出しを閉じるか

        [Header("エディタ：Animation Profile セクションの折りたたみ")]
        SerializedProperty AnimationProfileFoldout;           // （注）アニメプロファイル枠の開閉
        #endregion

        void OnEnable()
        {
            EmeraldAnimation self = (EmeraldAnimation)target;        // （注）対象コンポーネント参照を取得
            self.AIAnimator = self.GetComponent<Animator>();         // （注）Animator をキャッシュ（未割当なら取得）
            if (AnimationsEditorIcon == null)                        // （注）アイコンが未取得ならロード
                AnimationsEditorIcon = Resources.Load("Editor Icons/EmeraldAnimation") as Texture;

            ApplyRuntimeAnimatorController(self);                    // （訳）「Animation Profile の RuntimeAnimatorController を Animator に適用」
            UpdateAbilityAnimationEnums();                           // （訳）「Attack クリップ名を列挙に展開して EmeraldAnimation 側へ渡す」
            InitializeProperties();                                  // （訳）「SerializedProperty の初期化」

            IsInScene = self.gameObject.scene.IsValid();             // （注）シーン上に存在するかをチェック
            IsInPrefabInstance = StageUtility.GetStage(self.gameObject) != StageUtility.GetMainStage(); // （注）Prefab ステージか
        }

        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");    // （注）Editor 用フラグ
            AnimationProfileFoldout = serializedObject.FindProperty("AnimationProfileFoldout"); // （注）Foldout フラグ
            AnimationProfileProp = serializedObject.FindProperty("m_AnimationProfile");     // （注）Animation Profile への参照
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // （注）見出し等の GUIStyle を取得/更新
            EmeraldAnimation self = (EmeraldAnimation)target;           // （注）対象
            serializedObject.Update();                                  // （注）SP の更新開始

            // （訳）「スクリプトヘッダー開始：タイトル 'Animations' とアイコンを表示」
            CustomEditorProperties.BeginScriptHeaderNew("Animations", AnimationsEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingAnimationProfileMessage(self);                       // （訳）「Animation Profile が無い/不完全な時の警告表示」

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                AnimationProfiles(self);                                // （訳）「Animation Profile 設定ブロック」
                EditorGUILayout.Space();
                UpdateEditor(self);                                     // （訳）「Undo・Dirty 処理などのエディタ更新」
            }

            serializedObject.ApplyModifiedProperties();                 // （注）SP の変更を適用
            CustomEditorProperties.EndScriptHeader();                   // （訳）「スクリプトヘッダー終了」
        }

        /// <summary>
        /// （日本語）EmeraldAnimation 内で Animation Profile が未設定/未生成の場合に、警告メッセージを表示します。
        /// </summary>
        void MissingAnimationProfileMessage(EmeraldAnimation self)
        {
            if (self.m_AnimationProfile == null)
            {
                // 英文 UI はそのまま保持：Create New Animation Profile を促すメッセージ
                // （訳）「この AI には Animation Profile が必要です。下のボタンで新規作成するか、既存の Profile を割り当ててください。」
                CustomEditorProperties.DisplaySetupWarning("This AI needs to have an Animation Profile. Press the 'Create New Animation Profile' button below to create a new one or assign one that has already been created.");
            }
            else if (self.m_AnimationProfile.AIAnimator == null)
            {
                // （訳）「Animation Profile はありますが Animator Controller が生成/割当されていません。Animation Profile から設定してください。」
                CustomEditorProperties.DisplaySetupWarning("This AI has an Animation Profile, but an Animator Controller has not been generated for it. Please create one and assign all needed animations through the Animation Profile object. " +
                    "You can press the 'Edit Animation Profile' to open up an editor window to begin editing.");
            }
        }

        void UpdateAbilityAnimationEnums()
        {
            EmeraldAnimation self = (EmeraldAnimation)target;

            if (self.m_AnimationProfile == null)
                return;

            // （訳）「Type1 の攻撃リストから、クリップ名を列挙に詰める」
            if (self.m_AnimationProfile.Type1Animations.AttackList.Count > 0)
            {
                for (int i = 0; i < self.m_AnimationProfile.Type1Animations.AttackList.Count; i++)
                {
                    if (self.m_AnimationProfile.Type1Animations.AttackList[i].AnimationClip != null)
                        Type1AttackAnimationEnum.Add(self.m_AnimationProfile.Type1Animations.AttackList[i].AnimationClip.name);
                }
            }

            // （訳）「Type2 の攻撃リストから、クリップ名を列挙に詰める」
            if (self.m_AnimationProfile.Type2Animations.AttackList.Count > 0)
            {
                for (int i = 0; i < self.m_AnimationProfile.Type2Animations.AttackList.Count; i++)
                {
                    if (self.m_AnimationProfile.Type2Animations.AttackList[i].AnimationClip != null)
                        Type2AttackAnimationEnum.Add(self.m_AnimationProfile.Type2Animations.AttackList[i].AnimationClip.name);
                }
            }

            // （訳）「作成した配列を EmeraldAnimation 側の列挙フィールドへ渡す」
            self.Type1AttackEnumAnimations = Type1AttackAnimationEnum.ToArray();
            self.Type2AttackEnumAnimations = Type2AttackAnimationEnum.ToArray();
        }

        void AnimationProfiles(EmeraldAnimation self)
        {
            // （訳）「Animation Profile Settings の折りたたみ見出し」
            AnimationProfileFoldout.boolValue = CustomEditorProperties.Foldout(AnimationProfileFoldout.boolValue, "Animation Profile Settings", true, FoldoutStyle);

            if (AnimationProfileFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // （訳）「Animation Profile の説明：この AI のアニメデータと Animator Controller を保持し、複数 AI で共有できる」
                CustomEditorProperties.TextTitleWithDescription("Animation Profile", "An Animation Profile holds all of an AI's animation data, including the Animator Controller this AI will use. This allows AI to share the same animation data with only needing to rely on a single " +
                    "Animation Profile. Any changes made to an Animation Profile will affect any AI using that Animation Profile.", false);

                // （訳）注意：リグタイプの互換性など
                CustomEditorProperties.TextTitleWithDescription("Note", "The animations must be compatible with this model and share the same Rig Type. If your AI doesn't play animations correctly, or falls through the floor, it is likely that you are missing an animation, the " +
                    "Rig Type is not compatible, or that the animation is not compatible.", true);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(AnimationProfileProp); // （訳）「Animation Profile を割り当てる」
                // （訳）「この AI が使用する Animation Profile。Animator Controller を含むすべてのアニメ設定を共有します。」
                CustomEditorProperties.CustomHelpLabelField("The Animation Profile this AI is using. All animations, including the Animator Controller, will be used for this AI and any other AI using it.", false);

                if (!IsInScene)
                {
                    // （訳）「Project ビューでは Animation Viewer は使えません。シーン上のオブジェクトで利用してください。」
                    CustomEditorProperties.DisplayImportantMessage("The Animation Viewer can't be used in the Project tab. The AI must be within the Scene and in the Hierarchy tab.");
                }

                if (IsInPrefabInstance)
                {
                    // （訳）「Prefab 編集モードでは Animation Viewer は使えません。」
                    CustomEditorProperties.DisplayImportantMessage("The Animation Viewer can't be used while editing a prefab.");
                }

                // （訳）「シーン/非Prefab編集 かつ Profile/Animator が有効なときのみ有効」
                EditorGUI.BeginDisabledGroup(!IsInScene || IsInPrefabInstance);
                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null || self.m_AnimationProfile.AIAnimator == null);
                EditorGUILayout.Space();
                // ボタン：Open Animation Viewer（訳：現在の Animation Profile をシーン上でリアルタイム再生）
                if (GUILayout.Button(new GUIContent("Open Animation Viewer", "Preview all animations on the current Animation Profile, in real-time, on this AI within the Unity Scene."), GUILayout.Height(20)))
                {
                    OpenAnimationPreview(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                // ボタン：Edit Animation Profile（訳：別ウィンドウでプロファイル編集）
                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null);
                if (GUILayout.Button(new GUIContent("Edit Animation Profile", "Edit the current Animation Profile in a separate window so you can preview animations while keeping a reference to the current Animation Profile."), GUILayout.Height(20)))
                {
                    EditAnimationProfile(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                // ボタン：Clear Animation Profile（訳：スロットを空にする。アセット自体は残る）
                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null);
                if (GUILayout.Button(new GUIContent("Clear Animation Profile", "Clears the Animation Profile slot so a new one can be created. Note: The current Animation Profile object will remain in your project at its current path."), GUILayout.Height(20)))
                {
                    AnimationProfileProp.objectReferenceValue = null;
                    serializedObject.FindProperty("AIAnimator").objectReferenceValue = null;
                    self.AnimatorControllerGenerated = false;
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                // ボタン：Create New Animation Profile（訳：新規作成。ファイル保存ダイアログを開く）
                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile != null);
                if (GUILayout.Button(new GUIContent("Create New Animation Profile", "Creates a new Animation Profile within the Emerald AI/Animation Profiles folder. If you would like to create a new Animation Profile, remove the one in the current slot by pressing the 'Clear Animation Profile' button."), GUILayout.Height(20)))
                {
                    CreateAnimationProfile(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）Animation Profile の RuntimeAnimatorController を、この AI の Animator に適用します。
        /// </summary>
        void ApplyRuntimeAnimatorController(EmeraldAnimation self)
        {
            if (self.AIAnimator != null && self.m_AnimationProfile != null && self.AIAnimator.runtimeAnimatorController == null && self.m_AnimationProfile.AIAnimator != null ||
                self.AIAnimator != null && self.m_AnimationProfile != null && self.m_AnimationProfile.AIAnimator != null && self.AIAnimator != self.m_AnimationProfile.AIAnimator)
                self.AIAnimator.runtimeAnimatorController = self.m_AnimationProfile.AIAnimator; // （注）未設定/異なる場合のみ置き換え
        }

        /// <summary>
        /// （日本語）新しい Animation Profile アセットを作成し、保存先をユーザーに指定させます。
        /// 既存パスを選択した場合は、そのアセットを割り当てます。
        /// </summary>
        void CreateAnimationProfile(EmeraldAnimation self)
        {
            string FilePath = EditorUtility.SaveFilePanelInProject("Save as Animation Profile", "", "asset", "Please enter a file name to save the file to"); // （訳）ファイル名入力ダイアログ

            if (string.IsNullOrEmpty(FilePath))
            {
                // （訳）特定環境での EditorGUILayout エラー回避のためのダミー呼び出し
                CustomEditorProperties.BeginScriptHeader("", null);
                CustomEditorProperties.BeginFoldoutWindowBox();
                return;
            }

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(FilePath)))
            {
                AnimationProfile NewAnimationProfile = CreateInstance<AnimationProfile>(); // （注）新規インスタンスを生成
                AssetDatabase.CreateAsset(NewAnimationProfile, FilePath);                  // （注）アセットとして保存
                self.m_AnimationProfile = NewAnimationProfile;                             // （注）割り当て
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var ExistingAnimationProfile = AssetDatabase.LoadAssetAtPath(FilePath, typeof(AnimationProfile)); // （注）既存アセットをロード
                self.m_AnimationProfile = (AnimationProfile)ExistingAnimationProfile;                              // （注）割り当て
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // （訳）上記と同様の EditorGUILayout エラー回避
            CustomEditorProperties.BeginScriptHeader("", null);
            CustomEditorProperties.BeginFoldoutWindowBox();
        }

        void OpenAnimationPreview(EmeraldAnimation self)
        {
            // （訳）「Animation Viewer Manager」ウィンドウを開き、この AI を対象として初期化
            var m_AnimationPreviewEditor = (AnimationViewerManager)EditorWindow.GetWindow(typeof(AnimationViewerManager), true, "Animation Viewer Manager");
            m_AnimationPreviewEditor.Initialize(self.gameObject);
        }

        void EditAnimationProfile(EmeraldAnimation self)
        {
            if (self.m_AnimationProfile == null)
                return;

            // （注）既存の PropertyEditor（"Animation Profile" 名）をクローズして重複防止
            if (EditorWindowRef != null && EditorWindowRef.name == "Animation Profile")
                EditorWindowRef.Close();

            System.Type propertyEditorType = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"); // （注）内部型を取得
            System.Type[] callTypes = new[] { typeof(Object), typeof(bool) };
            object[] callOpenBuffer = { null, true };

            // （訳）Unity 2021.2 以前は API が無いため、リフレクションで PropertyEditor を開く
            MethodInfo openPropertyEditorInfo;
            openPropertyEditorInfo = propertyEditorType.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic, null, callTypes, null);
            self.m_AnimationProfile.EmeraldAnimationComponent = self; // （注）編集中の AI へ反映するための参照
            callOpenBuffer[0] = self.m_AnimationProfile;
            openPropertyEditorInfo.Invoke(null, callOpenBuffer);

            // （注）開いた PropertyEditor をキャッシュしてウィンドウ名/最小サイズを設定
            EditorWindowRef = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"));
            EditorWindowRef.name = "Animation Profile";
            EditorWindowRef.minSize = new Vector2(Screen.currentResolution.width / 4f, Screen.currentResolution.height / 2f);
        }

        void UpdateEditor(EmeraldAnimation self)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RecordObject(self, "Undo");                                   // （注）Undo 記録

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);                                 // （注）オブジェクトをダーティ化
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());// （注）シーンに変更ありをマーク
                }
            }
#endif
        }
    }
}
