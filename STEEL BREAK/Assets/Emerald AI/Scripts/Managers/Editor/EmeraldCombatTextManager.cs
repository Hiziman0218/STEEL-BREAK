using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldCombatTextManager】
    /// コンバットテキスト（与ダメ・被ダメ・回復など）のグローバル設定を管理するエディタウィンドウ。
    /// ・フォント、サイズ、アニメーション、色、対象などの一括設定に対応
    /// ・VR対応（World Space化）、アウトラインの有無、フォントサイズのアニメーション量なども管理
    /// </summary>
    public class EmeraldCombatTextManager : EditorWindow
    {
        [Header("コンバットテキスト用データの参照（Resources/Combat Text Data を読み込み）")]
        EmeraldCombatTextData m_EmeraldAICombatTextData;

        [Header("ウィンドウタイトル等のGUIスタイル（内部用）")]
        GUIStyle TitleStyle;

        [Header("ウィンドウ左上のアイコン（内部用）")]
        Texture CombatTextIcon;

        [Header("現在のタブ（0=設定 / 1=色設定）")]
        int CurrentTab = 0;

        [Header("このウィンドウの SerializedObject（内部用）")]
        SerializedObject serializedObject;

        [Header("フォント参照（TextFont）")]
        SerializedProperty TextFont;

        [Header("コンバットテキストの有効/無効状態（CombatTextState）")]
        SerializedProperty CombatTextState;

        [Header("プレイヤーの通常ダメージ文字色（PlayerTextColor）")]
        SerializedProperty PlayerTextColor;

        [Header("プレイヤーのクリティカルダメージ文字色（PlayerCritTextColor）")]
        SerializedProperty PlayerCritTextColor;

        [Header("プレイヤーが被ダメージ時の文字色（PlayerTakeDamageTextColor）")]
        SerializedProperty PlayerTakeDamageTextColor;

        [Header("AI 同士のダメージ文字色（AITextColor）")]
        SerializedProperty AITextColor;

        [Header("AI 同士のクリティカル文字色（AICritTextColor）")]
        SerializedProperty AICritTextColor;

        [Header("回復時の文字色（HealingTextColor）")]
        SerializedProperty HealingTextColor;

        [Header("基本フォントサイズ（FontSize）")]
        SerializedProperty FontSize;

        [Header("アニメーションで一時的に拡大するフォントサイズ（MaxFontSize）")]
        SerializedProperty MaxFontSize;

        [Header("テキストアニメーションの種類（AnimationType）")]
        SerializedProperty AnimationType;

        [Header("コンバットテキストを表示する対象（CombatTextTargets）")]
        SerializedProperty CombatTextTargets;

        [Header("アウトライン効果を使用するか（OutlineEffect）")]
        SerializedProperty OutlineEffect;

        [Header("フォントサイズをアニメーションさせるか（UseAnimateFontSize）")]
        SerializedProperty UseAnimateFontSize;

        [Header("テキストが出現する高さ（DefaultHeight）")]
        SerializedProperty TextHeight;

        [Header("VRサポート（World Space Canvas に切替）（VRSupport）")]
        SerializedProperty VRSupport;

        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Window/Emerald AI/コンバットテキスト マネージャ #%c", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldCombatTextManager), false, "コンバットテキスト マネージャ");
            APS.minSize = new Vector2(600f, 650);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// 有効化時：リソース読込と SerializedProperty の取得、アイコン読込
        /// </summary>
        protected virtual void OnEnable()
        {
            m_EmeraldAICombatTextData = (EmeraldCombatTextData)Resources.Load("Combat Text Data") as EmeraldCombatTextData;
            if (CombatTextIcon == null) CombatTextIcon = Resources.Load("Editor Icons/EmeraldCTM") as Texture;

            serializedObject = new SerializedObject(m_EmeraldAICombatTextData);
            TextFont = serializedObject.FindProperty("TextFont");
            CombatTextState = serializedObject.FindProperty("CombatTextState");
            PlayerTextColor = serializedObject.FindProperty("PlayerTextColor");
            PlayerCritTextColor = serializedObject.FindProperty("PlayerCritTextColor");
            PlayerTakeDamageTextColor = serializedObject.FindProperty("PlayerTakeDamageTextColor");
            AITextColor = serializedObject.FindProperty("AITextColor");
            AICritTextColor = serializedObject.FindProperty("AICritTextColor");
            HealingTextColor = serializedObject.FindProperty("HealingTextColor");
            FontSize = serializedObject.FindProperty("FontSize");
            MaxFontSize = serializedObject.FindProperty("MaxFontSize");
            AnimationType = serializedObject.FindProperty("AnimationType");
            CombatTextTargets = serializedObject.FindProperty("CombatTextTargets");
            OutlineEffect = serializedObject.FindProperty("OutlineEffect");
            UseAnimateFontSize = serializedObject.FindProperty("UseAnimateFontSize");
            TextHeight = serializedObject.FindProperty("DefaultHeight");
            VRSupport = serializedObject.FindProperty("VRSupport");
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 上部左余白
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "コンバットテキスト マネージャ", CombatTextIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  // 上部右余白
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 下部左余白
            EditorGUILayout.BeginVertical();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);

            // 見出しと説明（日本語）
            CustomEditorProperties.TextTitleWithDescription(
                "コンバットテキスト マネージャ",
                "コンバットテキスト マネージャでは、テキストサイズ、フォント、色、アニメーション、対象など、全てのコンバットテキスト設定をプロジェクト全体で一括管理できます。",
                true
            );

            GUIContent[] CombatTextManagerButtons = new GUIContent[2]
            {
                new GUIContent("コンバットテキストの設定"),
                new GUIContent("色の設定")
            };
            CurrentTab = GUILayout.Toolbar(CurrentTab, CombatTextManagerButtons, EditorStyles.miniButton, GUILayout.Height(25));

            CombatTextSettings();
            ColorSettings();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            GUILayout.Space(15); // 下部右余白
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying && GUI.changed)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// 「コンバットテキストの設定」タブの描画
        /// </summary>
        void CombatTextSettings()
        {
            if (CurrentTab == 0)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("コンバットテキストの設定", EditorStyles.boldLabel);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(CombatTextState, new GUIContent("コンバットテキストの状態"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("コンバットテキスト システムを有効／無効にします。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(VRSupport, new GUIContent("VR サポート"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("コンバットテキストの Canvas を World Space に変更し、VR で動作可能にします（この設定は CTS 初期化時に行われるため、実行中の切替はできません）。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(TextHeight, new GUIContent("コンバットテキストの高さ"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("コンバットテキストがターゲットの上方どの位置に生成されるかの全体的な高さを設定します。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(CombatTextTargets, new GUIContent("コンバットテキストの対象"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("コンバットテキストを表示できる対象の種別を制御します。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(OutlineEffect, new GUIContent("アウトライン効果を使用"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("コンバットテキストに輪郭（アウトライン）効果を使用するかどうかを制御します。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AnimationType, new GUIContent("アニメーション種別"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("コンバットテキストのアニメーションタイプを制御します。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), TextFont, "テキストフォント", typeof(Font), true);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("コンバットテキストに使用するフォントを指定します。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), FontSize, "フォントサイズ", 10, 50);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("コンバットテキストの基本フォントサイズを設定します。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(UseAnimateFontSize, new GUIContent("フォントサイズをアニメーション"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("フォントサイズを一時的に拡大するアニメーションを使用するかを制御します。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                if (m_EmeraldAICombatTextData.UseAnimateFontSize == EmeraldCombatTextData.UseAnimateFontSizeEnum.Enabled)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxFontSize, "アニメ後の増分サイズ", 1, 30);
                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                    EditorGUILayout.HelpBox("「フォントサイズをアニメーション」が有効な場合に、一時的に増加させるフォントサイズ量です。アニメーション後は基本フォントサイズに戻ります。", MessageType.None, true);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
        }

        /// <summary>
        /// 「色の設定」タブの描画
        /// </summary>
        void ColorSettings()
        {
            if (CurrentTab == 1)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("コンバットテキストの色", EditorStyles.boldLabel);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerTextColor, new GUIContent("プレイヤーの色"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("プレイヤーが与ダメージした際のコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerTakeDamageTextColor, new GUIContent("被ダメージ時の色（プレイヤー）"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("プレイヤーがダメージを受けた際のコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerCritTextColor, new GUIContent("クリティカル時の色（プレイヤー）"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("プレイヤーがクリティカルヒットを与えた際のコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AITextColor, new GUIContent("AI の色"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("AI 同士の攻撃に用いられるコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AICritTextColor, new GUIContent("クリティカル時の色（AI）"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("AI 同士のクリティカルヒット時に用いられるコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(HealingTextColor, new GUIContent("回復の色"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("回復に用いられるコンバットテキストの色です。", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }
        }
    }
}
