using System.Collections;                               // （保持）コルーチン関連
using System.Collections.Generic;                       // （保持）汎用コレクション
using UnityEngine;                                      // Unity ランタイムAPI
using UnityEditor;                                      // エディタ拡張API
using UnityEditorInternal;                              // ReorderableList など
using System.Linq;                                      // （保持）LINQ

namespace EmeraldAI.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(LocationBasedDamage))]         // このカスタムインスペクタは LocationBasedDamage 用
    [CanEditMultipleObjects]                            // 複数オブジェクト同時編集を許可

    // 【クラス概要】LocationBasedDamageEditor：
    //  LocationBasedDamage（部位ダメージ/LBD）コンポーネントのインスペクタを拡張し、
    //  コライダーリストの編集、レイヤー/タグの一括設定、説明/警告の表示などを日本語UIで提供するエディタクラス。
    public class LocationBasedDamageEditor : Editor
    {
        [Header("フォールドアウト見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                          // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture LBDEditorIcon;                          // インスペクタ上部のアイコン

        [Header("コライダー一覧の ReorderableList（LBD 対象）")]
        ReorderableList ColliderList;                   // LBD 用コライダーリスト

        [Header("コライダー一覧の内部状態（将来拡張用）")]
        string ColliderListState;                       // リスト状態（未使用だが原文保持）

        [Header("エディタ表示/タグ設定の SerializedProperty 群")]
        SerializedProperty HideSettingsFoldout,         // 設定全体の非表示トグル
                          LBDSettingsFoldout,           // 「LBD設定」フォールドアウト
                          LBDComponentsTag,             // LBDコライダーに設定する Tag
                          SetCollidersLayerAndTag;      // レイヤー/タグを自動設定するか

        [Header("プロジェクト内レイヤー名一覧（UI選択用）")]
        List<string> layers = new List<string>();       // 0〜31 の Layer 名を列挙して保持

        private void OnEnable()                         // エディタ有効化時（初期化）
        {
            if (LBDEditorIcon == null) LBDEditorIcon = Resources.Load("Editor Icons/EmeraldLBD") as Texture; // ヘッダー用アイコンをロード
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");   // 非表示トグル
            LBDSettingsFoldout = serializedObject.FindProperty("LBDSettingsFoldout");     // LBD 設定の折りたたみ
            LBDComponentsTag = serializedObject.FindProperty("LBDComponentsTag");         // コライダータグ
            SetCollidersLayerAndTag = serializedObject.FindProperty("SetCollidersLayerAndTag"); // 自動設定のON/OFF
            InitializeList();                                                             // コライダー用 ReorderableList 構築
            InitializeLayers();                                                           // レイヤー名一覧を収集
        }

        /// <summary>
        /// （日本語）プロジェクト内の全レイヤー名（0〜31）を収集して選択肢リストを作成します。
        /// このレイヤーは LBD コライダーに適用でき、さらに全AIの「遮蔽検出 Layermask」に自動追加されます。
        /// これにより LBD コライダーが視線遮蔽（LoS）を引き起こさないよう全体で統一管理できます。
        /// </summary>
        void InitializeLayers()
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) != "")
                    layers.Add(LayerMask.LayerToName(i));   // 実在レイヤー名
                else
                    layers.Add("Empty");                    // 未使用スロットの占位名
            }
        }

        /// <summary>
        /// （日本語）LBD 用コライダーの ReorderableList を初期化します（ヘッダー/要素描画/高さなど）。
        /// </summary>
        void InitializeList()
        {
            // ラベル用スタイル（太字・白）
            var LabelStyle = new GUIStyle();
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.active.textColor = Color.white;
            LabelStyle.normal.textColor = Color.white;

            // リストを作成（配列プロパティ: "ColliderList" を対象）
            ColliderList = new ReorderableList(serializedObject, serializedObject.FindProperty("ColliderList"), false, true, false, true);

            // ヘッダー描画（英語→日本語）
            ColliderList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "コライダー一覧", EditorStyles.boldLabel);
            };

            // 要素描画
            ColliderList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = ColliderList.serializedProperty.GetArrayElementAtIndex(index);   // 要素
                    ColliderList.elementHeight = EditorGUIUtility.singleLineHeight * 2.5f;         // 高さ

                    // ラベル行
                    if (element.FindPropertyRelative("ColliderObject").objectReferenceValue != null)
                    {
                        // コライダー名を太字で表示
                        EditorGUI.PrefixLabel(
                            new Rect(rect.x + 120, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight),
                            new GUIContent(element.FindPropertyRelative("ColliderObject").objectReferenceValue.name), LabelStyle);

                        // 「Select Collider」→「コライダーを選択」
                        if (GUI.Button(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), "コライダーを選択"))
                        {
                            Selection.activeObject = element.FindPropertyRelative("ColliderObject").objectReferenceValue;
                        }

                        // 倍率スライダー（"<name> Multiplier" → "<name> 倍率"）
                        element.FindPropertyRelative("DamageMultiplier").floatValue =
                            EditorGUI.Slider(
                                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("ColliderObject").objectReferenceValue.name + " 倍率",
                                element.FindPropertyRelative("DamageMultiplier").floatValue, 0, 25);
                    }
                    else
                    {
                        // Null の場合は赤で警告
                        GUI.color = Color.red;
                        EditorGUI.PrefixLabel(
                            new Rect(rect.x + 120, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight),
                            new GUIContent("Null - 削除してください"), LabelStyle);
                        GUILayout.FlexibleSpace();
                        GUI.color = Color.white;

                        // 「Remove」→「削除」
                        GUI.contentColor = Color.red;
                        if (GUI.Button(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), "削除"))
                        {
                            LocationBasedDamage self = (LocationBasedDamage)target;
                            self.ColliderList.RemoveAt(index); // リストから除去
                        }
                        GUI.contentColor = Color.white;

                        // 無効化されたスライダー（"Null Multiplier" → "Null 倍率"）
                        EditorGUI.BeginDisabledGroup(true);
                        element.FindPropertyRelative("DamageMultiplier").floatValue =
                            EditorGUI.Slider(
                                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight),
                                "Null 倍率",
                                element.FindPropertyRelative("DamageMultiplier").floatValue, 0, 25);
                        EditorGUI.EndDisabledGroup();
                    }
                };
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。ヘッダーと LBD 設定セクションを表示します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            LocationBasedDamage self = (LocationBasedDamage)target;     // 対象コンポーネント
            serializedObject.Update();                                  // 直列化同期

            // ヘッダー表示（"Location Based Damage" → 「部位ダメージ（LBD）」）
            CustomEditorProperties.BeginScriptHeaderNew("部位ダメージ（LBD）", LBDEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)                         // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                LBDSettings(self);                                      // LBD 設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                   // ヘッダー終了
            serializedObject.ApplyModifiedProperties();                 // 変更を適用
        }

        /// <summary>
        /// （日本語）LBD（部位ダメージ）の各種設定UIを描画します。
        /// </summary>
        void LBDSettings(LocationBasedDamage self)
        {
            // 見出し（"Location Based Damage Settings" → 「部位ダメージ設定」）
            LBDSettingsFoldout.boolValue = EditorGUILayout.Foldout(LBDSettingsFoldout.boolValue, "部位ダメージ設定", true, FoldoutStyle);

            if (LBDSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "部位ダメージ設定",
                    "Location Based Damage コンポーネントは、各コライダーで被弾を検知し、被弾部位に応じた任意のダメージ倍率を適用できます。"
                    + "命中時に再生されるヒットエフェクトと命中位置は、AI の『ヒットエフェクト一覧』（AI Settings > Combat > Hit Effect）に基づきます。",
                    false
                );

                // チュートリアルボタン（文言を日本語に、リンクはそのまま）
                CustomEditorProperties.TutorialButton(
                    "Location Based Damage コンポーネントの使用方法は以下のチュートリアルを参照してください。注：LBD は AI へのダメージ処理に通常とは異なるコードを使用します。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/location-based-damage-component"
                );
                EditorGUILayout.Space();

                // 「Set Colliders Layer and Tag」→「コライダーのレイヤーとタグを自動設定」
                CustomEditorProperties.CustomPropertyField(
                    SetCollidersLayerAndTag,
                    "コライダーのレイヤーとタグを自動設定",
                    "LBD コンポーネントが検出したコライダーへ、指定のタグとレイヤーを自動で設定するかを制御します。有効にすると、下でタグ/レイヤーを選べます。",
                    true
                );

                if (SetCollidersLayerAndTag.boolValue)
                {
                    CustomEditorProperties.BeginIndent();

                    // 「Collider Layer」→「コライダーのレイヤー」
                    CustomEditorProperties.FactionListEnum(new Rect(), GUIContent.none, serializedObject.FindProperty("LBDComponentsLayer"), "コライダーのレイヤー", layers);
                    CustomEditorProperties.CustomHelpLabelField(
                        "LBD コライダーに設定するレイヤーです。このレイヤーは全AIの『遮蔽検出 Layermask』へ自動追加され、視線を妨げないようにします。"
                        + "ターゲット側のレイヤーや Default を指定しないでください。",
                        true
                    );
                    EditorGUILayout.Space();

                    // 「Collider Tag」→「コライダーのタグ」
                    CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), LBDComponentsTag, "コライダーのタグ");
                    CustomEditorProperties.CustomHelpLabelField(
                        "LBD コライダーに設定するタグです。Untagged 以外のタグを設定することを推奨します。",
                        true
                    );
                    EditorGUILayout.Space();

                    // 「Dead Collider Layer」→「死亡時のコライダーレイヤー」
                    CustomEditorProperties.FactionListEnum(new Rect(), GUIContent.none, serializedObject.FindProperty("DeadLBDComponentsLayer"), "死亡時のコライダーレイヤー", layers);
                    CustomEditorProperties.CustomHelpLabelField("AI が死亡した際に、LBD コライダーへ適用するレイヤーです。", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.EndIndent();
                }
                else
                {
                    // 警告メッセージ（英語→日本語）
                    GUILayout.Space(-15);
                    CustomEditorProperties.DisplayWarningMessage(
                        "レイヤー/タグを手動で管理しない限り、この設定は有効のままを推奨します。レイヤーを Default に設定しないでください。"
                        + "適切に管理されていないと、検出まわりの不具合（視線遮蔽など）の原因になります。"
                    );
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // 説明（英語→日本語）
                EditorGUILayout.LabelField("AI 内のすべてのコライダーを取得し、それぞれに Location Based Damage Area コンポーネントを付与します。", EditorStyles.helpBox);

                // 「Get Colliders」→「コライダーを取得」
                if (GUILayout.Button("コライダーを取得"))
                {
                    var m_Colliders = self.GetComponentsInChildren<Collider>();

                    foreach (Collider C in m_Colliders)
                    {
                        if (C != null && C.gameObject != self.gameObject)
                        {
                            if (!self.ColliderList.Exists(x => x.ColliderObject == C))
                            {
                                LocationBasedDamage.LocationBasedDamageClass lbdc = new LocationBasedDamage.LocationBasedDamageClass(C, 1);
                                self.ColliderList.Add(lbdc);
                            }
                        }
                    }

                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();

                    if (self.ColliderList.Count == 0)
                    {
                        Debug.Log("この AI にはコライダーが見つかりません。Unity の Ragdoll Wizard もしくはサードパーティのラグドールツールでセットアップされているか確認してください。");
                    }
                }

                // 「Clear Colliders」→「コライダーをクリア」
                if (GUILayout.Button("コライダーをクリア") &&
                    EditorUtility.DisplayDialog("コライダーリストをクリアしますか？",
                        "AI のコライダーリストを本当にクリアしますか？ この操作は取り消せません。", "クリア", "キャンセル"))
                {
                    self.ColliderList.Clear();
                    serializedObject.Update();
                }

                // Info（英語→日本語）
                EditorGUILayout.HelpBox(
                    "不要なコライダーは、下の『コライダー一覧』から対象を選択し、一覧下部の - ボタンで削除できます。",
                    MessageType.Info
                );

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // コライダーの ReorderableList を描画
                ColliderList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
