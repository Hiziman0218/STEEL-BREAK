using UnityEngine;                      // Unity ランタイム API
using UnityEditor;                      // Unity エディタ拡張 API（Editor など）

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(TargetPositionModifier))] // このエディタは TargetPositionModifier 用のカスタムインスペクタ
    [CanEditMultipleObjects]                       // 複数オブジェクト同時編集を許可
    [System.Serializable]

    // 【クラス概要】TargetPositionModifierEditor：
    //  TargetPositionModifier コンポーネントのインスペクタを拡張し、
    //  「注視・照準の基準点（球ギズモ）」の高さ/半径/色、および基準Transformの指定を
    //  日本語UIで編集できるようにするエディタクラス。
    public class TargetPositionModifierEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                               // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture TPMEditorIcon;                               // インスペクタ上部のアイコン

        [Header("SerializedProperty 参照（UIと同期する対象プロパティ）")]
        SerializedProperty PositionModifierProp,             // 高さオフセット
                           TransformSourceProp,              // 基準Transform（胸/背骨など推奨）
                           GizmoRadiusProp,                  // 球ギズモの半径
                           GizmoColorProp,                   // 球ギズモの色
                           TPMSettingsFoldout,               // 「ターゲット位置修正の設定」折りたたみ
                           HideSettingsFoldout;              // 全体非表示トグル

        /// <summary>
        /// （日本語）エディタ有効化時：対象プロパティのバインドとアイコン読込を行います。
        /// </summary>
        private void OnEnable()
        {
            InitializeProperties(); // 各 SerializedProperty を対象フィールドへ紐付け
        }

        /// <summary>
        /// （日本語）対象オブジェクト（TargetPositionModifier）のシリアライズ済みフィールドへプロパティを紐付けます。
        /// </summary>
        void InitializeProperties()
        {
            if (TPMEditorIcon == null) TPMEditorIcon = Resources.Load("Editor Icons/EmeraldTPM") as Texture; // ヘッダー用アイコン
            PositionModifierProp = serializedObject.FindProperty("PositionModifier");        // 高さオフセット
            TransformSourceProp = serializedObject.FindProperty("TransformSource");         // 基準Transform
            GizmoRadiusProp = serializedObject.FindProperty("GizmoRadius");            // ギズモ半径
            GizmoColorProp = serializedObject.FindProperty("GizmoColor");             // ギズモ色
            TPMSettingsFoldout = serializedObject.FindProperty("TPMSettingsFoldout");     // 折りたたみ
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");    // 非表示トグル
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。ヘッダーと「ターゲット位置修正の設定」セクションを表示します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            serializedObject.Update();                                  // 直列化オブジェクトを最新に

            // 必須チェック：Transform Source 未設定の警告（英語→日本語）
            if (TransformSourceProp.objectReferenceValue == null)
            {
                CustomEditorProperties.DisplaySetupWarning(
                    "Transform Position Source を使用するには『トランスフォームソース』が必要です。Target Position Modifier を使用するため、いずれかを割り当ててください。"
                );
            }

            // ヘッダー表示（英語 "Target Position Modifier" → 日本語「ターゲット位置修正」）
            CustomEditorProperties.BeginScriptHeaderNew("ターゲット位置修正", TPMEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue) // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                TPMSettings();                   // 設定セクション
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了
            serializedObject.ApplyModifiedProperties(); // 変更適用
        }

        /// <summary>
        /// （日本語）ターゲット位置修正（球ギズモ）の各種設定UIを描画します。
        /// </summary>
        void TPMSettings()
        {
            // 見出し（英語 "Target Position Modifier Settings" → 日本語）
            TPMSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                TPMSettingsFoldout.boolValue,
                "ターゲット位置修正の設定",
                true,
                FoldoutStyle);

            if (TPMSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "ターゲット位置修正の設定",
                    "このシステムは、AI エージェントが狙う『ターゲットの高さ』を調整します。シーン上には参照用の球ギズモが表示され、AI はこの位置を狙います。",
                    false
                );

                // 注意文（英語→日本語）
                CustomEditorProperties.NoticeTextDescription(
                    "球ギズモが地面の下に入らないようにしてください。地面に入ると、AI からターゲットを検出できなくなる可能性があります。ギズモが見えない場合は、Unity の Gizmos 表示が有効か確認してください。",
                    false
                );

                // チュートリアルボタン（英語→日本語。リンクはそのまま）
                CustomEditorProperties.TutorialButton(
                    "Target Position Modifier の詳細な使い方は以下のチュートリアルを参照してください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component"
                );
                EditorGUILayout.Space();

                // 必須フィールド未設定時の赤背景メッセージ（英語→日本語）
                if (TransformSourceProp.objectReferenceValue == null)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("このフィールドは空にできません", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                // Transform Source（英語→日本語）
                EditorGUILayout.PropertyField(TransformSourceProp, new GUIContent("トランスフォームソース"));
                CustomEditorProperties.CustomHelpLabelField(
                    "Target Position Modifier の基準とする Transform を指定します。AI の中心に近いボーン（胸・背骨など）を推奨します。",
                    true
                );

                // 高さオフセット（英語 "Height Modifier" → 日本語）
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), PositionModifierProp, "高さオフセット", -5, 5);
                CustomEditorProperties.CustomHelpLabelField("位置修正（球ギズモ）の高さ補正量を制御します。", true);

                // ギズモ半径
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), GizmoRadiusProp, "ギズモ半径", 0.05f, 2.5f);
                CustomEditorProperties.CustomHelpLabelField("球ギズモの半径を制御します。", true);

                // ギズモ色
                CustomEditorProperties.CustomColorField(new Rect(), new GUIContent(), GizmoColorProp, "ギズモの色");
                CustomEditorProperties.CustomHelpLabelField("球ギズモの色を制御します。", true);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
