using UnityEngine;                                // Unity ランタイムAPI
using UnityEditor;                                // Unity エディタ拡張API（Editor など）

namespace EmeraldAI.Utility                       // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldDebugger))]       // このエディタは EmeraldDebugger 用
    [CanEditMultipleObjects]                      // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldDebuggerEditor：
    //  EmeraldDebugger コンポーネントのインスペクタ表示を拡張し、
    //  デバッグ可視化（視線ライン、NavMeshパス/目的地、IKルックポイント、未検出ターゲット線、フットステップ、カバーノード等）
    //  の有効/無効や色設定を分かりやすく編集できるようにするエディタクラス。
    public class EmeraldDebuggerEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI用）")]
        GUIStyle FoldoutStyle;                     // フォールドアウト見た目

        [Header("ヘッダーに表示するアイコンテクスチャ（Resources から読込）")]
        Texture EventsEditorIcon;                  // エディタ上部のアイコン

        //Bools
        [Header("エディタ表示用フラグ（折りたたみ/非表示）")]
        SerializedProperty SettingsFoldoutProp,    // 「デバッグ設定」セクションの開閉フラグ
                          HideSettingsFoldoutProp; // 全体非表示フラグ（ヘッダー右側）

        [Header("デバッグ機能トグル & 可視化色などのプロパティ群")]
        SerializedProperty EnableDebuggingToolsProp,        // デバッグ機能 全体ON/OFF
                          DrawLineOfSightLinesProp,         // 視線（Line of Sight）ライン描画
                          DrawNavMeshPathProp,              // NavMesh パス描画
                          DrawNavMeshDestinationProp,       // NavMesh 目的地描画
                          DrawLookAtPointsProp,             // IK ルックポイント描画
                          DrawUndetectedTargetsLineProp,    // 未検出ターゲットへのライン描画
                          DebugLogTargetsProp,              // 検出ターゲットを Console に出力
                          DebugLogObstructionsProp,         // 遮蔽物を Console に出力
                          NavMeshPathColorProp,             // パス描画の色
                          NavMeshDestinationColorProp,      // 目的地描画の色
                          DrawFootstepPositions,            // 足音衝突位置を可視化
                          DebugLogFootsteps,                // 足音の当たり/Surface Object を Console に出力
                          DrawDetectedCoverNodes;           // 検出したカバーノード位置を可視化

        /// <summary>
        /// （日本語）エディタ有効化時に呼ばれる。アイコンをロードし、SerializedProperty を初期化する。
        /// </summary>
        void OnEnable()
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldDebugger") as Texture; // ヘッダー用アイコンを読込
            InitializeProperties();                                                                                      // 各プロパティへバインド
        }

        /// <summary>
        /// （日本語）対象オブジェクト（EmeraldDebugger）のシリアライズ済みフィールドへプロパティを紐付ける。
        /// </summary>
        void InitializeProperties()
        {
            //Bool
            SettingsFoldoutProp = serializedObject.FindProperty("SettingsFoldout");       // セクション折りたたみ
            HideSettingsFoldoutProp = serializedObject.FindProperty("HideSettingsFoldout");   // 全体非表示

            EnableDebuggingToolsProp = serializedObject.FindProperty("EnableDebuggingTools");   // デバッグON/OFF
            DrawLineOfSightLinesProp = serializedObject.FindProperty("DrawLineOfSightLines");   // 視線ライン
            DrawNavMeshPathProp = serializedObject.FindProperty("DrawNavMeshPath");        // パス描画
            DrawNavMeshDestinationProp = serializedObject.FindProperty("DrawNavMeshDestination");// 目的地描画
            DrawLookAtPointsProp = serializedObject.FindProperty("DrawLookAtPoints");       // ルックポイント
            DrawUndetectedTargetsLineProp = serializedObject.FindProperty("DrawUndetectedTargetsLine"); // 未検出ターゲット線
            DebugLogTargetsProp = serializedObject.FindProperty("DebugLogTargets");        // ターゲットのログ
            DebugLogObstructionsProp = serializedObject.FindProperty("DebugLogObstructions");   // 遮蔽物のログ
            NavMeshPathColorProp = serializedObject.FindProperty("NavMeshPathColor");       // パス色
            NavMeshDestinationColorProp = serializedObject.FindProperty("NavMeshDestinationColor"); // 目的地色
            DrawFootstepPositions = serializedObject.FindProperty("DrawFootstepPositions");  // 足音の位置
            DebugLogFootsteps = serializedObject.FindProperty("DebugLogFootsteps");      // 足音ログ
            DrawDetectedCoverNodes = serializedObject.FindProperty("DrawDetectedCoverNodes"); // カバーノード可視化
        }

        /// <summary>
        /// （日本語）インスペクタGUIの描画。ヘッダーとデバッグ設定セクションを表示する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル（太字等）更新
            serializedObject.Update();                                  // 直列化オブジェクトを最新化

            // ヘッダー開始：タイトルを日本語「デバッガ」へ
            CustomEditorProperties.BeginScriptHeaderNew("デバッガ", EventsEditorIcon, new GUIContent(), HideSettingsFoldoutProp);

            if (!HideSettingsFoldoutProp.boolValue) // 非表示でない場合のみ内容を描画
            {
                EditorGUILayout.Space();
                GeneralEvents();                    // デバッグ設定の本体を描画
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了

            serializedObject.ApplyModifiedProperties(); // 変更を適用
        }

        /// <summary>
        /// （日本語）デバッグ設定セクションの描画。各トグルや色設定、ヘルプメッセージを日本語で表示する。
        /// </summary>
        void GeneralEvents()
        {
            EmeraldDebugger self = (EmeraldDebugger)target; // 対象コンポーネント

            // 見出し（英語→日本語）
            SettingsFoldoutProp.boolValue = EditorGUILayout.Foldout(SettingsFoldoutProp.boolValue, "デバッグ設定", true, FoldoutStyle);

            if (SettingsFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox(); // セクション枠

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "デバッグ設定",
                    "内部処理の状態や問題の特定に役立つ多くの情報を可視化します。どのデバッグ項目を有効にするかを選択できます。" +
                    "「デバッグツールを有効にする」を使えば、このコンポーネントをAIに付けたまま必要になるまで全項目をまとめて無効にできます。",
                    true);

                // デバッグ機能のON/OFF
                EditorGUILayout.PropertyField(EnableDebuggingToolsProp);
                CustomEditorProperties.CustomHelpLabelField("デバッグツール全体を有効化するかどうかを切り替えます。", true);

                if (self.EnableDebuggingTools == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent(15);

                    // 視線ライン
                    EditorGUILayout.PropertyField(DrawLineOfSightLinesProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "Unity エディタ上で、AI の Line of Sight（視線）レイキャストを可視化します。正しく照準されているか、視界が遮蔽されているかの確認に便利です。",
                        true);

                    // NavMesh パス
                    EditorGUILayout.PropertyField(DrawNavMeshPathProp);
                    CustomEditorProperties.CustomHelpLabelField("AI の現在のパスを可視化します。", true);
                    if (self.DrawNavMeshPath == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();
                        EditorGUILayout.PropertyField(NavMeshPathColorProp);
                        CustomEditorProperties.CustomHelpLabelField("NavMesh パスの色を設定します。", true);
                        CustomEditorProperties.EndIndent();
                    }

                    // NavMesh 目的地
                    EditorGUILayout.PropertyField(DrawNavMeshDestinationProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の現在の目的地を可視化します。注：この表示には Emerald Debugger が最小化されていない必要があります。",
                        true);
                    if (self.DrawNavMeshDestination == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent(15);
                        EditorGUILayout.PropertyField(NavMeshDestinationColorProp);
                        CustomEditorProperties.CustomHelpLabelField("NavMesh 目的地の色を設定します。", true);
                        CustomEditorProperties.EndIndent();
                    }

                    // IK ルックポイント
                    EditorGUILayout.PropertyField(DrawLookAtPointsProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "（Inverse Kinematics コンポーネント使用時）AI の現在の注視ポイントを可視化します。ターゲットのどの位置を見ているか、" +
                        "Target Position Modifier コンポーネントによる位置補正が適切かの確認に役立ちます。",
                        true);

                    // 未検出ターゲットへのライン
                    EditorGUILayout.PropertyField(DrawUndetectedTargetsLineProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の検知半径内にいるが、まだ検出されていないターゲットへ線を描画します。" +
                        "未検出の理由が遮蔽や視野外（Field of View 外）である場合の確認に便利です。",
                        true);

                    // 遮蔽物ログ
                    EditorGUILayout.PropertyField(DebugLogObstructionsProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI とターゲットの間にある遮蔽物を Unity コンソールへ表示します。現在の遮蔽要因の特定に役立ちます。",
                        true);

                    // ターゲットログ
                    EditorGUILayout.PropertyField(DebugLogTargetsProp);
                    CustomEditorProperties.CustomHelpLabelField(
                        "検出されたターゲットオブジェクトを Unity コンソールへ表示します。AI が狙っているオブジェクトが想定どおりかを確認できます。",
                        true);

                    // 足音位置の可視化
                    EditorGUILayout.PropertyField(DrawFootstepPositions);
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の足音の衝突位置を可視化します（黄色い円）。Footsteps コンポーネントが無い場合は無視されます。",
                        true);

                    // 足音ログ
                    EditorGUILayout.PropertyField(DebugLogFootsteps);
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の足音が衝突したオブジェクト名と、使用された Footstep Surface Object を Unity コンソールへ表示します。" +
                        "意図しない衝突がないか、足音計算が正しく機能しているかの確認に役立ちます。Footsteps コンポーネントが無い場合は無視されます。",
                        true);

                    // カバーノード可視化
                    EditorGUILayout.PropertyField(DrawDetectedCoverNodes);
                    CustomEditorProperties.CustomHelpLabelField(
                        "カバーコンポーネント使用時、最近検出されたカバーノードを円で可視化します（黄色が候補、緑が選択されたノード）。" +
                        "エディタ上でカバーノード検出が適切かを確認するのに役立ちます。Cover コンポーネントが無い場合は無視されます。",
                        true);

                    CustomEditorProperties.EndIndent();
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
