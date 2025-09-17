using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldFactionManager】
    /// 派閥（Faction）を作成・管理するためのエディタウィンドウ。
    /// ・作成した派閥はプロジェクト全体で使用可能（全AIが共有）
    /// ・AIの Detection コンポーネント > Faction Settings から各AIへ割り当て
    /// ・一覧の追加/削除/並び替えに対応（ReorderableList）
    /// </summary>
    public class EmeraldFactionManager : EditorWindow
    {
        [Header("ウィンドウタイトル等のGUIスタイル（内部用）")]
        GUIStyle TitleStyle;

        [Header("ウィンドウ左上のアイコン（内部用）")]
        Texture FactionIcon;

        [Header("スクロール位置（内部用）")]
        Vector2 scrollPos;

        [Header("Faction Data 用の SerializedObject（内部用）")]
        SerializedObject serializedObject;

        [Header("派閥リスト表示用の ReorderableList（内部用）")]
        ReorderableList FactionList;

        [Header("AI選択中でも派閥変更を反映するための再描画フラグ")]
        bool RefreshInspector;

        /// <summary>
        /// メニューからウィンドウを開きます。
        /// </summary>
        [MenuItem("Window/Emerald AI/派閥マネージャ #%r", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager), false, "派閥マネージャ");
            APS.minSize = new Vector2(300f, 300f);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// 有効化時：アイコンのロードとFactionデータの読み込み、エディタコールバック登録
        /// </summary>
        protected virtual void OnEnable()
        {
            if (FactionIcon == null) FactionIcon = Resources.Load("FactionExtension") as Texture;
            LoadFactionData();

#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        /// <summary>
        /// 無効化時：エディタコールバック解除
        /// </summary>
        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        /// <summary>
        /// 選択中のAIのインスペクタを開いたまま派閥を編集した際に、
        /// AI側の派閥 Enum を確実に更新するための処理（EditorApplication.update 経由のコールバック）。
        /// </summary>
        protected virtual void OnEditorUpdate()
        {
            if (RefreshInspector)
            {
                UpdateStaticFactionData();
                RefreshInspector = false;
            }
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 上部 左余白
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "派閥マネージャ", FactionIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  // 上部 右余白
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 下部 左余白
            EditorGUILayout.BeginVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "派閥マネージャ",
                "派閥マネージャでは、AIがターゲットを識別するための派閥を作成できます。ここで作成された派閥は全ての Emerald AI エージェントで共通して利用可能です。各AIへの割り当ては、Detection コンポーネントの Faction Settings から行ってください。",
                true
            );

            FactionList.DoLayoutList();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); // 下部 右余白
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 静的な派閥データ（文字列リスト）を更新し、各ビューを再描画します。
        /// </summary>
        void UpdateStaticFactionData()
        {
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));
            EmeraldDetection.StringFactionList = new List<string>(FactionData.FactionNameList);
            FactionExtension.StringFactionList = new List<string>(FactionData.FactionNameList);
            //Repaint();

            EditorUtility.SetDirty(FactionData);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
            Repaint();
        }

        /// <summary>
        /// Resources から Faction Data を読み込んで、ReorderableList を構築します。
        /// </summary>
        void LoadFactionData()
        {
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));
            serializedObject = new SerializedObject(FactionData);

            FactionList = new ReorderableList(serializedObject, serializedObject.FindProperty("FactionNameList"), false, true, true, true);
            FactionList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "派閥リスト", EditorStyles.boldLabel);
            };
            FactionList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = FactionList.serializedProperty.GetArrayElementAtIndex(index);
                    FactionList.elementHeight = EditorGUIUtility.singleLineHeight * 1.25f;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight), "派閥 " + (index + 1));
                    EditorGUI.PropertyField(new Rect(rect.x + 75, rect.y + 3f, rect.width - 75, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshInspector = true;
                        UpdateStaticFactionData();
                    }
                };

            FactionList.onChangedCallback = (FactionList) =>
            {
                UpdateStaticFactionData();
            };
        }
    }
}
