using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Reflection;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【AnimationViewerManager】
    /// アニメーションイベントのプレビューや編集（追加・移動・削除）を行うエディタウィンドウ。
    /// ・現在選択している AI の Animation Profile からクリップ一覧を取得
    /// ・タイムライン上で再生／一時停止、任意フレームへ移動
    /// ・プリセットイベントの追加、個別パラメータ編集、適用／破棄の管理
    /// ・ルートモーションの有無を切り替えてプレビュー
    /// </summary>
    public class AnimationViewerManager : EditorWindow
    {
        [Header("シングルトン参照（このウィンドウの唯一のインスタンス）")]
        public static AnimationViewerManager Instance;

        [Header("タイムライン外枠の色")]
        Color TimelineOutlineColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("ルートモーションを使用するか")]
        public bool UseRootMotion;

        [Header("現在プレビュー中のアニメーションインデックス")]
        public int CurrentPreviewAnimationIndex = 0;

        [Header("プリセットのアニメーションイベント種別インデックス")]
        public int PresetAnimationEventIndex = 0;

        [Header("再生速度（1.0=等速）")]
        float TimeScale = 1.0f;

        [Header("ウィンドウ内レイアウト用オフセット")]
        Vector2 WindowOffset;

        [Header("現在のプレビュー再生時間（秒）")]
        protected float time = 0.0f;

        [Header("ヘッダ用アイコン（Animation Profile）")]
        Texture AnimationProfileEditorIcon;

        [Header("再生ボタン用アイコン")]
        Texture PlayButtonIcon;

        [Header("一時停止ボタン用アイコン")]
        Texture PauseButtonIcon;

        [Header("アニメーションイベント追加ボタン用アイコン")]
        Texture AnimationEventIcon;

        [Header("一時保持：初期位置")]
        public static Vector3 DefaultPosition;

        [Header("一時保持：初期オイラー角")]
        public static Vector3 DefaultEuler;

        [Header("アニメーションが再生中か")]
        bool AnimationIsPlaying;

        [Header("ルートモーション切替の内部状態")]
        bool RootMotionChanged;

        [Header("現在プレビュー対象のAI（アニメーションを表示する対象）")]
        GameObject CurrentAnimationViewerAI = null;

        [Header("現在プレビュー中のアニメーションクリップ")]
        AnimationClip PreviewClip = null;

        [Header("プレビュー対象のアニメーションクリップ一覧")]
        List<AnimationClip> PreviewClips = new List<AnimationClip>();

        [Header("GUI表示用：アニメーション名一覧")]
        List<string> AnimationNames = new List<string>();

        [Header("GUI表示用：イベント名一覧（プリセット）")]
        List<string> AnimationEventNames = new List<string>();

        [Header("プリセットのアニメーションイベント群")]
        List<EmeraldAnimationEventsClass> AnimationEventPresets = new List<EmeraldAnimationEventsClass>();

        [Header("アニメーションクリップのタイムライン領域Rect")]
        Rect AnimationClipTimelineArea;

        [Header("タイムライン上の現在位置（縦バー）Rect")]
        Rect AnimationClipTimelinePoint;

        [Header("現在選択中のイベントインデックス")]
        int AnimationEventIndex;

        [Header("直前のプレビューアニメーションインデックス")]
        int PreviousPreviewAnimationIndex;

        [Header("現在選択中のアニメーションイベント")]
        public AnimationEvent CurrentAnimationEvent;

        [Header("現在選択中イベントの描画領域Rect")]
        Rect CurrentEventArea;

        [Header("プレビュー用親オブジェクト（ルートモーション対策）")]
        GameObject AnimationPreviewParent;

        [Header("初期の親Transform（復帰用）")]
        Transform PreviousParent;

        [Header("初期位置（復帰用）")]
        Vector3 StartingPosition;

        [Header("初期オイラー角（復帰用）")]
        Vector3 StartingEuler;

        [Header("タイムラインドラッグの初期化フラグ")]
        bool InitializeTimelineMovement;

        [Header("イベントドラッグの初期化フラグ")]
        bool InitializeAnimationEventMovement;

        [Header("デバッグログを有効化（内部用）")]
        bool EnableDebugging = false; // Internal Use Only

        [Header("重複したアニメーションクリップ（イベント重複更新用）")]
        [SerializeField]
        public List<AnimationClip> DuplicateAnimationEvents = new List<AnimationClip>();

        [Header("現在のアニメーションとイベントの対応リスト")]
        [SerializeField]
        public List<AnimationEventElement> CurrentAnimationEvents = new List<AnimationEventElement>();

        /// <summary>
        /// アニメーションクリップと、そのイベント群を保持する要素。
        /// </summary>
        [System.Serializable]
        public class AnimationEventElement
        {
            [Header("対象アニメーションクリップ")]
            public AnimationClip Clip;

            [Header("クリップに設定されているアニメーションイベント一覧")]
            public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();

            [Header("この要素が編集されたか（未適用の変更があるか）")]
            public bool Modified;

            public AnimationEventElement(AnimationClip m_Clip, List<AnimationEvent> m_AnimationEvents)
            {
                Clip = m_Clip;
                AnimationEvents = new List<AnimationEvent>(m_AnimationEvents);
            }
        }

        void OnEnable()
        {
            if (EditorApplication.isPlaying)
                return;

            if (AnimationProfileEditorIcon == null) AnimationProfileEditorIcon = Resources.Load("Editor Icons/EmeraldDetection") as Texture;
            if (PlayButtonIcon == null) PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
            if (PauseButtonIcon == null) PauseButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
            if (AnimationEventIcon == null) AnimationEventIcon = Resources.Load("Editor Icons/EmeraldAnimationEvent") as Texture;

            this.minSize = new Vector2(Screen.currentResolution.width / 6f, Screen.currentResolution.height / 1.7f);
            this.maxSize = new Vector2(Screen.currentResolution.width / 4f, 1500);

            Instance = this;
            SubscribeCallbacks(); // コールバックへ登録
            InitiailizeList();
        }

        void OnDisable()
        {
            if (EnableDebugging) Debug.Log("OnDisable を実行");
            UnsubscribeCallbacks(); // コールバック登録解除
        }

        /// <summary>
        /// コールバック登録：保存・シーン変更・再コンパイル時のプレビュー状態を適切に処理するために使用。
        /// </summary>
        void SubscribeCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// コールバック登録解除：保存・シーン変更・再コンパイル時のプレビュー状態の処理を停止。
        /// </summary>
        void UnsubscribeCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// （SceneView.duringSceneGui 経由）
        /// アニメーションイベントが発火するフレームに、AI の足元へ GUI を描画。
        /// タイムラインポイントがイベント時間に重なると色を変えて視認性を上げる。
        /// </summary>
        private void OnSceneGUI(SceneView obj)
        {
            if (PreviewClip != null)
            {
                Vector3 pos = CurrentAnimationViewerAI.transform.position;
                Color OutLineColor = new Color(0, 0, 0, 1);
                Color FaceColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

                Vector3[] verts = new Vector3[]
                {
                    new Vector3(pos.x - 0.5f, pos.y, pos.z - 0.5f),
                    new Vector3(pos.x - 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z - 0.5f)
                };

                // タイムラインの現在位置から、イベント発火時間に近接しているかを判定して色を変更
                float MouseLerp = (AnimationClipTimelinePoint.x / AnimationClipTimelineArea.width);
                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 2.5f, AnimationClipTimelineArea.min.x + 1f, MouseLerp);
                float ModifiedTime = ((AnimationClipTimelinePoint.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (ModifiedTime >= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time - 0.005f) && ModifiedTime <= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time + 0.005f))
                    {
                        OutLineColor = new Color(0, 0, 0f, 1);
                        FaceColor = new Color(0.5f, 1f, 0.5f, 0.15f);
                    }
                }
                Handles.DrawSolidRectangleWithOutline(verts, FaceColor, OutLineColor);
            }
        }

        /// <summary>
        /// シーン保存の直前コールバック。アニメーションサンプリングを無効化して保存対象に含めない。
        /// </summary>
        private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
            SetAnimatorStates(true);
            DeparentPreviewObject();
            if (EnableDebugging) Debug.Log("OnSceneSaving を実行");
        }

        /// <summary>
        /// シーン保存直後コールバック。保存から除外したプレビュー状態を復元。
        /// </summary>
        void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            SetAnimatorStates(false);

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();

            if (GameObject.Find("Animation Viewer Parent") == null)
            {
                AnimationPreviewParent = new GameObject("Animation Viewer Parent");
                Selection.activeObject = AnimationPreviewParent;
                AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
                AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
                CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
            }

            if (EnableDebugging) Debug.Log("OnSceneSaved を実行");
        }

        /// <summary>
        /// 「未適用の変更」ダイアログを表示。適用なら書き込み、破棄なら何もせず続行。
        /// </summary>
        void DisplayApplyChangesMenu(GameObject G = null)
        {
            if (G != CurrentAnimationViewerAI && CurrentAnimationEvents.Any(x => x.Modified == true))
            {
                if (EditorUtility.DisplayDialog("未適用の変更を検出", "次のオブジェクトに未適用の変更があります:\n" + CurrentAnimationViewerAI.name + "\n\n変更を適用しますか？", "適用する", "破棄する"))
                {
                    ApplyChanges(false);
                    if (EnableDebugging) Debug.Log("変更を適用しました（メニュー）");
                }
                else
                {
                    if (EnableDebugging) Debug.Log("変更を破棄しました（メニュー）");
                }
            }
        }

        void ConfirmDiscardingMessage()
        {
            if (EditorUtility.DisplayDialog("変更を破棄しますか？", "この操作は取り消せません。変更を破棄してもよろしいですか？", "はい", "いいえ"))
            {
                Initialize(CurrentAnimationViewerAI);
                CurrentAnimationEvent = null;
                if (EnableDebugging) Debug.Log("変更破棄に同意しました");
            }
            else
            {
                if (EnableDebugging) Debug.Log("変更破棄をキャンセルしました");
            }
        }

        /// <summary>
        /// アセンブリの再読み込み（コンパイル）前に、アニメーションサンプリングを停止して固着を回避。
        /// </summary>
        public void OnBeforeAssemblyReload()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
        }

        /// <summary>
        /// アセンブリの再読み込み（コンパイル）後に、プレビューのための状態を復帰。
        /// </summary>
        public void OnAfterAssemblyReload()
        {
            SetAnimatorStates(false);

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// プレイモード切り替え時はウィンドウを閉じ、プレビュー固着を回避。
        /// </summary>
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            this.Close();
            if (EnableDebugging) Debug.Log("PlayMode 変更を検出");
        }

        /// <summary>
        /// ウィンドウ起動時：プリセットイベント名などの初期化。
        /// </summary>
        void InitiailizeList()
        {
            // Emerald AI の主要イベントプリセットを取得
            AnimationEventPresets = AnimationEventInitializer.GetEmeraldAnimationEvents();

            // 表示名を列挙に入れる
            for (int i = 0; i < AnimationEventPresets.Count; i++)
            {
                AnimationEventNames.Add(AnimationEventPresets[i].eventDisplayName);
            }
        }

        /// <summary>
        /// Animation Editor と Animation Profile のデータでエディタウィンドウを初期化。
        /// </summary>
        public void Initialize(GameObject G)
        {
            DisplayApplyChangesMenu(G); // 初期化前に未適用の変更を確認

            SetAnimatorStates(false); // Animator を Always Animate に（バグ回避）。終了時に戻す。

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            DeparentPreviewObject(); // 既存のプレビュー対象があれば復帰
            CurrentAnimationViewerAI = G; // 現在の編集対象をキャッシュ

            // 初期位置・角度を保持
            StartingPosition = CurrentAnimationViewerAI.transform.position;
            StartingEuler = CurrentAnimationViewerAI.transform.eulerAngles;

            ParentPreviewObject(); // ルートモーションの移動を原点へ持って行かないための親オブジェクトを用意

            InitializeAnimationData(); // Animation Profile からクリップ一覧を作成

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// Animation Profile からすべてのアニメーションを列挙して UI に反映。
        /// </summary>
        void InitializeAnimationData()
        {
            PreviewClips.Clear();
            AnimationNames.Clear();
            CurrentAnimationEvents.Clear();
            var m_AnimationProfile = CurrentAnimationViewerAI.GetComponent<EmeraldAnimation>().m_AnimationProfile;

            AssignAnimationNames(m_AnimationProfile.NonCombatAnimations, "");         // 非戦闘
            AssignAnimationNames(m_AnimationProfile.Type1Animations, "Type 1 -");     // 武器タイプ1
            AssignAnimationNames(m_AnimationProfile.Type2Animations, "Type 2 -");     // 武器タイプ2

            // クリップが一つも無い場合は終了
            if (CurrentAnimationEvents.Count == 0)
            {
                Close();
                if (EditorUtility.DisplayDialog("アニメーションがありません", "アタッチされている Animation Profile にアニメーションがありません。「Edit Animation Profile」からアニメーションを追加してください。", "OK"))
                {
                    Selection.activeGameObject = CurrentAnimationViewerAI;
                    return;
                }
            }
        }

        /// <summary>
        /// リフレクションで AnimationParentClass のフィールドからクリップ名を収集して GUI 表示名を組み立てる。
        /// </summary>
        void AssignAnimationNames(AnimationParentClass AnimationCategory, string AnimationCategoryName)
        {
            foreach (var field in AnimationCategory.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "System.Collections.Generic.List`1[EmeraldAI.AnimationClass]")
                {
                    List<AnimationClass> m_AnimationClass = (List<AnimationClass>)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);

                    for (int i = 0; i < m_AnimationClass.Count; i++)
                    {
                        AnimationClip m_AnimationClip = m_AnimationClass[i].AnimationClip;

                        if (m_AnimationClip != null) // && !PreviewClips.Contains(m_AnimationClip)
                        {
                            CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                            PreviewClips.Add(m_AnimationClip);

                            string TempName = field.Name.Replace("List", "");
                            TempName = TempName.Replace("Animation", "");
                            TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                            TempName = "(" + AnimationCategoryName + TempName + " " + (i + 1) + ")";
                            TempName = TempName.Replace("( ", "(");
                            AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                        }
                    }
                }

                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "EmeraldAI.AnimationClass")
                {
                    AnimationClass m_AnimationClass = (AnimationClass)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);
                    AnimationClip m_AnimationClip = m_AnimationClass.AnimationClip;

                    if (m_AnimationClip != null)
                    {
                        CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                        PreviewClips.Add(m_AnimationClip);

                        string TempName = field.Name.Replace("List", " ");
                        TempName = TempName.Replace("Animation", "");
                        TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                        TempName = "(" + AnimationCategoryName + TempName + ")";
                        TempName = TempName.Replace("( ", "(");
                        AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                    }
                }
            }
        }

        /// <summary>
        /// 変更を各アニメーションクリップへ保存します。
        /// </summary>
        void ApplyChanges(bool AnimationModeEnabled)
        {
            List<string> PathList = new List<string>();

            // 変更されたクリップのパスを収集
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified)
                {
                    if (!PathList.Contains(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip)))
                    {
                        PathList.Add(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip));
                    }
                }
            }

            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified && !DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    var path = AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip);
                    string PathType = AssetImporter.GetAtPath(path).ToString(); // FBX か単体クリップかで分岐
                    PathType = PathType.Replace(" ", "");
                    PathType = PathType.Replace("(", "");
                    PathType = PathType.Replace(")", "");

                    if (PathType == "UnityEngine.FBXImporter")
                    {
                        var modelImporter = (ModelImporter)AssetImporter.GetAtPath(path) as ModelImporter;
                        SerializedObject so = new SerializedObject(modelImporter);
                        SerializedProperty clips = so.FindProperty("m_ClipAnimations");

                        // 変更されたクリップに対し、イベントを上書き
                        for (int m = 0; m < modelImporter.clipAnimations.Length; m++)
                        {
                            if (clips.GetArrayElementAtIndex(m).displayName == CurrentAnimationEvents[i].Clip.name)
                            {
                                Debug.Log(clips.GetArrayElementAtIndex(m).displayName + " のアニメーションイベントを更新しました");
                                SerializedProperty prop = clips.GetArrayElementAtIndex(m).FindPropertyRelative("events");
                                if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                                {
                                    prop.ClearArray();
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                                else
                                {
                                    SetEvents(clips.GetArrayElementAtIndex(m), CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                            }
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        // 単体の AnimationClip に対する保存
                        AnimationClip animClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
                        SerializedObject so = new SerializedObject(animClip);

                        if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                        {
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }
                        else
                        {
                            Debug.Log(animClip.name + " のアニメーションイベントを更新しました");
                            SetEventsOnClip(CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                }
                else if (DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    CurrentAnimationEvents[i].Modified = false;
                }
            }

            for (int i = 0; i < PathList.Count; i++)
            {
                string PathType = AssetImporter.GetAtPath(PathList[i]).ToString();
                PathType = PathType.Replace(" ", "");
                PathType = PathType.Replace("(", "");
                PathType = PathType.Replace(")", "");

                if (PathType == "UnityEngine.FBXImporter")
                {
                    var modelImporter = (ModelImporter)AssetImporter.GetAtPath(PathList[i]) as ModelImporter;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(modelImporter));
                    AssetDatabase.SaveAssets();
                }
            }

            if (!AnimationMode.InAnimationMode() && AnimationModeEnabled)
                AnimationMode.StartAnimationMode();

            // 現在選択中イベントのフォーカス解除
            CurrentAnimationEvent = null;
            CurrentEventArea = new Rect();
            GUI.FocusControl(null);
            Repaint();
            DuplicateAnimationEvents.Clear();
        }

        // メインのエディタウィンドウ
        public void OnGUI()
        {
            GUIStyle Style = EditorStyles.wordWrappedLabel;
            Style.fontStyle = FontStyle.Bold;
            Style.fontSize = 16;
            Style.padding.top = -11;
            Style.alignment = TextAnchor.UpperCenter;

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Window");
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent(AnimationProfileEditorIcon), Style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
            EditorGUILayout.LabelField("アニメーション ビューア", Style, GUILayout.ExpandWidth(true));
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            CustomEditorProperties.TextTitleWithDescription("アニメーションクリップの選択", "AI の現在の Animation Profile からアニメーションクリップを選択し、シーン上のAIで直接プレビューします。", false);
            CustomEditorProperties.NoticeTextDescription("注: このプレビューでは Inverse Kinematics とアニメーションブレンドは使用しません。実行時の方が最終的な品質は高く見えます。", true);

            // 現在のアニメーションを列挙から選択
            CurrentPreviewAnimationIndex = EditorGUILayout.Popup("現在のアニメーション", CurrentPreviewAnimationIndex, AnimationNames.ToArray());
            PreviewClip = CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip;

            if (PreviousPreviewAnimationIndex != CurrentPreviewAnimationIndex)
            {
                CurrentAnimationEvent = null;
                PreviousPreviewAnimationIndex = CurrentPreviewAnimationIndex;
            }

            GUILayout.Space(15);

            CustomEditorProperties.CustomHelpLabelField("Project タブで現在のクリップを選択状態にします。", false);
            if (GUILayout.Button("現在のクリップを Project で表示"))
            {
                Selection.activeObject = PreviewClip;
            }

            GUILayout.Space(10);
            UseRootMotion = EditorGUILayout.Toggle("ルートモーションを使用", UseRootMotion);
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            if (PreviewClip != null)
            {
                CustomEditorProperties.TextTitleWithDescription("アニメーション タイムライン", "下のタイムライン領域をクリックして、アニメーションを任意の位置でプレビューできます。再生ボタンで連続再生し、もう一度押すと一時停止します。イベント種別（プリセット／カスタム）を選んで現在位置に追加できます。ドロップダウンへマウスを重ねると、選択中のイベントの説明がツールチップで表示されます。", false);
                GUILayout.Space(2.5f);

                GUIStyle BoldStyle = GUI.skin.button;
                BoldStyle.fontStyle = FontStyle.Bold;

                if (AnimationIsPlaying)
                {
                    GUI.backgroundColor = new Color(1.5f, 0.1f, 0f, 0.75f);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button(new GUIContent(PlayButtonIcon, "現在のアニメーションを再生／一時停止します。"), BoldStyle))
                {
                    AnimationIsPlaying = !AnimationIsPlaying;
                    if (AnimationIsPlaying)
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
                    else
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
                }

                GUI.backgroundColor = Color.white;

                // イベント追加ボタン
                if (GUILayout.Button(new GUIContent(AnimationEventIcon, "現在のフレームに、選択中のイベント種別を追加します（必要なパラメータも適用）。"), BoldStyle))
                {
                    GUI.FocusControl(null); // 選択イベントのフォーカス解除
                    var m_event = new AnimationEvent();
                    m_event.functionName = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.functionName;
                    m_event.stringParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.stringParameter;
                    m_event.floatParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.floatParameter;
                    m_event.intParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.intParameter;
                    m_event.time = time + Mathf.Lerp(0.009f, -0.0111f, time / PreviewClip.length);

                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Add(m_event); // イベントを追加
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    // 追加したイベントを選択状態に
                    CurrentAnimationEvent = m_event;
                    AnimationEventIndex = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.IndexOf(m_event);
                    Repaint();
                }

                Rect r2 = EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                PresetAnimationEventIndex = EditorGUILayout.Popup("", PresetAnimationEventIndex, AnimationEventNames.ToArray(), GUILayout.MinWidth(100));
                Rect LastRect = GUILayoutUtility.GetLastRect();
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);

                TimeScale = EditorGUILayout.Slider("", TimeScale, 0.01f, 1f, GUILayout.MaxHeight(0), GUILayout.MinWidth(100));

                EditorGUILayout.EndVertical();
                EditorGUI.LabelField(new Rect((r2.min.x + 45), r2.position.y - 2, (r2.width), 20), new GUIContent("イベント種別"));
                EditorGUI.LabelField(new Rect((r2.min.x + 6), r2.position.y + 18, LastRect.width, 20), new GUIContent("", AnimationEventPresets[PresetAnimationEventIndex].eventDescription));
                EditorGUI.LabelField(new Rect((r2.max.x - 265) + r2.min.x, r2.position.y - 2, 100, 20), "再生速度");
                EditorGUI.LabelField(new Rect((r2.min.x + r2.width - 40), r2.position.y + 19, (r2.width), 20), System.Math.Round(TimeScale, 2) + "x");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                float stopTime = PreviewClip.length;
                Rect r = EditorGUILayout.BeginVertical();
                WindowOffset.x = 5;
                var EditedTime = time * PreviewClip.frameRate;
                var niceTime = Mathf.Floor(EditedTime / PreviewClip.frameRate).ToString("00") + ":" + Mathf.FloorToInt(EditedTime % PreviewClip.frameRate).ToString("00");
                var ClipPercentage = ((time / stopTime) * 100f).ToString("F2");

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y - 3f, r.width - (WindowOffset.x * 2) + 6, 105.5f), TimelineOutlineColor); // 全体枠
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x, r.position.y + 0.25f, r.width - (WindowOffset.x * 2), 100f), new Color(0.3f, 0.3f, 0.3f, 1f)); // 背景

                AnimationClipTimelineArea = new Rect(r.x + WindowOffset.x - 3, r.position.y - 0.75f, r.width - (WindowOffset.x * 2) + 3, 50f);
                float AdjustedTime = (time / stopTime);
                EditorGUI.DrawRect(new Rect((r.x + WindowOffset.x), r.position.y + 0.25f, (r.width - (WindowOffset.x * 2) - 2) * AdjustedTime, 50), new Color(0.1f, 0.25f, 0.5f, 1f)); // 進捗バー

                // タイムライン情報
                GUIStyle LabelStyle = new GUIStyle();
                LabelStyle.alignment = TextAnchor.MiddleCenter;
                LabelStyle.fontStyle = FontStyle.Bold;
                LabelStyle.normal.textColor = Color.white;
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x, r.position.y + 50f, r.width - (WindowOffset.x * 2), 50f), new Color(0.1f, 0.1f, 0.1f, 1f));
                EditorGUI.LabelField(new Rect(r.x + WindowOffset.x, r.position.y + 50, r.width - (WindowOffset.x * 2), 50), niceTime + "   (" + (ClipPercentage + "%") + ")    フレーム " + Mathf.Round(time * PreviewClip.frameRate).ToString(), LabelStyle);
                // タイムライン情報ここまで

                //------------- アニメーションイベント タイムライン -------------
                GUIStyle AnimationEventStyle = GUI.skin.box;

                // 10%刻みの目盛り
                for (int i = 1; i <= 60; i++)
                {
                    EditorGUI.DrawRect(new Rect(((r.x + WindowOffset.x) + (float)(i / 60f) * (r.width - (WindowOffset.x * 2) - (r.min.x * 2.5f))), r.position.y + 37.5f, 1f, 12f), new Color(0.6f, 0.6f, 0.6f, 1f));
                }

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        GUI.backgroundColor = new Color(1, 1, 1, 2f);
                    else
                        GUI.backgroundColor = new Color(3, 3, 3, 3);

                    // 各イベントを時間位置に応じて描画
                    Rect AnimationEventRect = new Rect((WindowOffset.x - AnimationClipTimelineArea.min.x / WindowOffset.x) + (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time / stopTime) * (r.width - (WindowOffset.x - (AnimationClipTimelineArea.min.x / WindowOffset.x) * 2) - (r.min.x)), r.position.y + 19.5f, 7.5f, 30f);

                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        EditorGUI.DrawRect(AnimationEventRect, new Color(1f, 0.25f, 0.25f, 1f));
                    else
                        EditorGUI.DrawRect(AnimationEventRect, Color.white);

                    if (AnimationEventRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;
                        CurrentAnimationEvent = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i];
                        UpdateIdenticalAnimationClips();
                        AnimationEventIndex = i;
                        CurrentEventArea = AnimationEventRect;
                        InitializeAnimationEventMovement = true;
                        Repaint();
                        GUI.FocusControl(null); // 選択イベントのフォーカス解除
                    }
                }

                // タイムラインの現在位置バー
                AnimationClipTimelinePoint = new Rect(((r.x + WindowOffset.x) + (time / stopTime) * (r.width - (WindowOffset.x * 2) - (r.min.x))), r.position.y, 3.5f, 50f);
                EditorGUI.DrawRect(AnimationClipTimelinePoint, new Color(0.8f, 0.8f, 0.8f, 1f));

                ChangeEventTime();

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y + 50, r.width - (WindowOffset.x * 2) + 6, 2.5f), TimelineOutlineColor); // 外枠

                GUI.backgroundColor = Color.white;
                //------------- アニメーションイベント タイムライン ここまで -------------
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(120);

            //------------- アニメーションイベントの詳細編集 -------------
            if (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count > 0 && CurrentAnimationEvent != null)
            {
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName = EditorGUILayout.TextField("関数名", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter = EditorGUILayout.FloatField("Float", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter = EditorGUILayout.IntField("Int", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter = EditorGUILayout.TextField("String", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter = EditorGUILayout.ObjectField("Object", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter, typeof(object), false);

                GUILayout.Space(25);
            }

            EditorGUI.BeginDisabledGroup(CurrentAnimationEvents.Count > 0 && !CurrentAnimationEvents.Any(x => x.Modified == true));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("変更を適用", GUILayout.MaxHeight(30)))
            {
                ApplyChanges(true);
            }

            if (GUILayout.Button("変更を破棄", GUILayout.MaxHeight(30)))
            {
                ConfirmDiscardingMessage();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            //------------- 詳細編集ここまで -------------

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// 渡された FBX クリップの SerializedProperty に、CurrentAnimationEvents の内容を書き戻す。
        /// </summary>
        public void SetEvents(SerializedProperty sp, AnimationEvent[] newEvents, AnimationClip clip)
        {
            SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
            if (serializedProperty != null && serializedProperty.isArray && newEvents != null && newEvents.Length > 0)
            {
                serializedProperty.ClearArray();
                for (int i = 0; i < newEvents.Length; i++)
                {
                    AnimationEvent animationEvent = newEvents[i];
                    serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);

                    SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex(i);
                    eventProperty.FindPropertyRelative("floatParameter").floatValue = animationEvent.floatParameter;
                    eventProperty.FindPropertyRelative("functionName").stringValue = animationEvent.functionName;
                    eventProperty.FindPropertyRelative("intParameter").intValue = animationEvent.intParameter;
                    eventProperty.FindPropertyRelative("objectReferenceParameter").objectReferenceValue = animationEvent.objectReferenceParameter;
                    eventProperty.FindPropertyRelative("data").stringValue = animationEvent.stringParameter;
                    eventProperty.FindPropertyRelative("time").floatValue = animationEvent.time / clip.length;
                }
            }
        }

        /// <summary>
        /// 単体の AnimationClip に対し、CurrentAnimationEvents の内容を書き戻す。
        /// </summary>
        public void SetEventsOnClip(AnimationEvent[] newEvents, AnimationClip clip)
        {
            if (newEvents != null && newEvents.Length > 0)
            {
                AnimationUtility.SetAnimationEvents(clip, newEvents);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// マウス入力と位置から、現在クリップ上のイベントや再生時間を変更。
        /// </summary>
        void ChangeEventTime()
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive); // エディタウィンドウ外でも Event.current を機能させる

            var current = Event.current;
            if (current != null)
            {
                switch (current.type)
                {
                    case EventType.MouseDrag:
                        if (CurrentEventArea != new Rect())
                        {
                            if (InitializeAnimationEventMovement)
                            {
                                GUIUtility.hotControl = controlId;
                                float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 1.75f, AnimationClipTimelineArea.min.x + 3f, MouseLerp);
                                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0.015f, PreviewClip.length - 0.015f);
                                Repaint();
                            }
                        }
                        else if (CurrentEventArea == new Rect() && InitializeTimelineMovement)
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0, PreviewClip.length);
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (EnableDebugging) Debug.Log("MouseUp を検出");
                        InitializeTimelineMovement = false;
                        InitializeAnimationEventMovement = false;

                        if (CurrentEventArea != new Rect())
                        {
                            UpdateIdenticalAnimationClips();
                            CurrentEventArea = new Rect();
                            Repaint();
                        }
                        break;
                    case EventType.MouseDown:
                        if (EnableDebugging) Debug.Log("MouseDown を検出");
                        if (new Rect(AnimationClipTimelineArea.x + 3.5f, AnimationClipTimelineArea.position.y, AnimationClipTimelineArea.width - 6.5f, AnimationClipTimelineArea.height).Contains(current.mousePosition) && CurrentEventArea == new Rect())
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = ((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);
                            InitializeTimelineMovement = true; // ボタンが押され続けている間は連続して追従
                            Repaint();
                        }
                        break;
                }
            }

            if (Event.current.isKey)
            {
                KeyCode key = Event.current.keyCode;

                if (key == KeyCode.Delete && CurrentAnimationEvent != null)
                {
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.RemoveAt(AnimationEventIndex);
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    Repaint();
                    CurrentAnimationEvent = null;
                }
            }
        }

        /// <summary>
        /// エディタがプレイ中でなく、AnimationMode 中で、クリップがあるときに更新。
        /// プレビューのためのサンプリングと UI の再描画を行う。
        /// </summary>
        void Update()
        {
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode() && PreviewClip != null)
            {
                // ルートモーション無効へ切り替えた際は位置を元に戻す
                if (UseRootMotion != RootMotionChanged)
                {
                    CurrentAnimationViewerAI.transform.localPosition = Vector3.zero;
                    CurrentAnimationViewerAI.transform.localEulerAngles = Vector3.zero;
                    RootMotionChanged = UseRootMotion;
                }

                // 再生中は時間を進める
                if (AnimationIsPlaying)
                {
                    time += Time.deltaTime * TimeScale;
                    if (time >= PreviewClip.length)
                        time = 0;
                }

                AnimationMode.BeginSampling();
                DefaultPosition = CurrentAnimationViewerAI.transform.position;
                DefaultEuler = CurrentAnimationViewerAI.transform.eulerAngles;
                AnimationMode.SampleAnimationClip(CurrentAnimationViewerAI, PreviewClip, time);

                // ルートモーション無効時は位置・角度を固定
                if (!UseRootMotion)
                {
                    CurrentAnimationViewerAI.transform.position = DefaultPosition;
                    CurrentAnimationViewerAI.transform.eulerAngles = DefaultEuler;
                }

                AnimationMode.EndSampling();

                // サンプリング中は常に Repaint して UI を滑らかに
                Repaint();
            }
        }

        /// <summary>
        /// ウィンドウ破棄時に状態を復帰（未適用の変更確認、サンプリング停止、親子関係の解除など）。
        /// </summary>
        void OnDestroy()
        {
            DisplayApplyChangesMenu();

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
            DeparentPreviewObject();
        }

        /// <summary>
        /// 同一参照のアニメーションクリップに対して、イベントの変更内容を同期（複数箇所に同一クリップがある場合）。
        /// </summary>
        void UpdateIdenticalAnimationClips()
        {
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Clip == CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip)
                {
                    CurrentAnimationEvents[i].AnimationEvents = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents;
                    CurrentAnimationEvents[i].Modified = true;
                }
            }
        }

        /// <summary>
        /// プレビュー用の親オブジェクトから AI を取り外し、元の親・位置・角度に戻す。
        /// </summary>
        public void DeparentPreviewObject()
        {
            if (CurrentAnimationViewerAI != null)
            {
                CurrentAnimationViewerAI.transform.SetParent(PreviousParent);
                CurrentAnimationViewerAI.transform.position = StartingPosition;
                CurrentAnimationViewerAI.transform.eulerAngles = StartingEuler;
                if (AnimationPreviewParent != null && AnimationPreviewParent.transform.childCount == 0)
                    DestroyImmediate(AnimationPreviewParent);
            }
        }

        /// <summary>
        /// ルートモーションが原点へ流れてしまわないよう、AI を一時的に親オブジェクト配下へ移動。
        /// </summary>
        public void ParentPreviewObject()
        {
            AnimationPreviewParent = new GameObject("Animation Viewer Parent");
            Selection.activeObject = AnimationPreviewParent;
            AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
            AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
            PreviousParent = CurrentAnimationViewerAI.transform.parent;
            CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
        }

        /// <summary>
        /// Unity の既知の不具合対策で、AnimationMode 中は Animator を全て無効化する。
        /// 保存・再コンパイル・PlayMode 変更時のコールバックで状態を管理。
        /// </summary>
        public void SetAnimatorStates(bool state)
        {
            var AnimatorsInScene = GameObject.FindObjectsOfType<Animator>();
            for (int i = 0; i < AnimatorsInScene.Length; i++)
            {
                AnimatorsInScene[i].enabled = state;
            }
        }
    }
}
