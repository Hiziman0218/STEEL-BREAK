using UnityEngine;                                  // Unity ランタイムAPI
using UnityEditor;                                  // エディタ拡張API（Editor 等）
using UnityEditorInternal;                          // ReorderableList
using EmeraldAI.Utility;                            // Emerald のカスタムエディタ補助

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(AttractModifier))]         // このカスタムインスペクタは AttractModifier 用

    // 【クラス概要】AttractModifierEditor：
    //  AttractModifier コンポーネント（音・衝突・カスタム呼び出し等で近隣AIを誘引）を
    //  インスペクタで設定できるようにするエディタ拡張。
    //  レイヤー、半径、クールダウン、トリガー種別、リアクション、サウンド一覧などを日本語UIで編集可能にします。
    public class AttractModifierEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                       // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture AttractModifierEditorIcon;           // インスペクタ上部のアイコン

        [Header("対象プロパティ（SerializedProperty）への参照")]
        SerializedProperty PlayerFactionProp,        // プレイヤー派閥（インデックス）
                         RadiusProp,                 // 誘引半径
                         MinVelocityProp,            // 最小速度（衝突トリガ時）
                         ReactionCooldownSecondsProp,// リアクションクールダウン秒
                         SoundCooldownSecondsProp,   // サウンドクールダウン秒
                         EmeraldAILayerProp,         // 検知対象の Emerald AI レイヤー
                         TriggerTypeProp,            // トリガータイプ
                         AttractReactionProp,        // アトラクトに用いる ReactionObject
                         TriggerLayersProp,          // 許可する衝突レイヤー
                         EnemyRelationsOnlyProp,     // 敵対関係のみ
                         HideSettingsFoldout,        // 全体非表示
                         AttractModifierFoldout;     // 設定セクション開閉

        [Header("トリガー時に再生するサウンドの一覧（ReorderableList）")]
        ReorderableList TriggerSoundsList;           // トリガーサウンドのリスト

        [Header("派閥データ（Resources から読込）")]
        EmeraldFactionData FactionData;              // プレイヤー派閥名の表示に使用

        /// <summary>
        /// （日本語）エディタ有効化時：アイコンのロード、各 SerializedProperty のバインド、サウンドリストの初期化を行います。
        /// </summary>
        private void OnEnable()
        {
            if (AttractModifierEditorIcon == null) AttractModifierEditorIcon = Resources.Load("AttractModifier") as Texture; // 既定のエディタアイコンをロード

            // 各フィールドを対象オブジェクトのシリアライズ済みプロパティへ紐付け
            RadiusProp = serializedObject.FindProperty("Radius");
            PlayerFactionProp = serializedObject.FindProperty("PlayerFaction.FactionIndex");
            MinVelocityProp = serializedObject.FindProperty("MinVelocity");
            ReactionCooldownSecondsProp = serializedObject.FindProperty("ReactionCooldownSeconds");
            SoundCooldownSecondsProp = serializedObject.FindProperty("SoundCooldownSeconds");
            EmeraldAILayerProp = serializedObject.FindProperty("EmeraldAILayer");
            TriggerTypeProp = serializedObject.FindProperty("TriggerType");
            AttractReactionProp = serializedObject.FindProperty("AttractReaction");
            TriggerLayersProp = serializedObject.FindProperty("TriggerLayers");
            EnemyRelationsOnlyProp = serializedObject.FindProperty("EnemyRelationsOnly");
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            AttractModifierFoldout = serializedObject.FindProperty("AttractModifierFoldout");
            FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            // Trigger Sounds（リストの初期化）
            TriggerSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("TriggerSounds"), true, true, true, true);
            TriggerSoundsList.drawHeaderCallback = rect =>
            {
                // 英語 "Trigger Sounds List" → 日本語「トリガーサウンド一覧」
                EditorGUI.LabelField(rect, "トリガーサウンド一覧", EditorStyles.boldLabel);
            };
            TriggerSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = TriggerSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画：ヘッダー、設定セクションの描画を行います。
        /// </summary>
        public override void OnInspectorGUI()
        {
            AttractModifier self = (AttractModifier)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();

            // ヘッダー（英語 "Attract Modifier" → 日本語「アトラクトモディファイア」）
            CustomEditorProperties.BeginScriptHeaderNew("アトラクトモディファイア", AttractModifierEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                AttractModifierSettings();          // 設定セクション
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties(); // 変更を適用
        }

        /// <summary>
        /// （日本語）アトラクトモディファイアの設定UIを描画します。
        /// </summary>
        void AttractModifierSettings()
        {
            // 英語 "Attract Modifier Settings" → 日本語「アトラクトモディファイア設定」
            AttractModifierFoldout.boolValue = EditorGUILayout.Foldout(AttractModifierFoldout.boolValue, "アトラクトモディファイア設定", true, FoldoutStyle);

            if (AttractModifierFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＆説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "アトラクトモディファイア設定",
                    "このシステムは、指定範囲内にいる AI を誘引し、設定した『アトラクトリアクション』を実行します。"
                  + "Attract Modifier がアタッチされているオブジェクトが誘引の発生源になります。"
                  + "サウンド検知コンポーネントの機能を拡張し、特定のオブジェクト・衝突・カスタム呼び出しによって、周囲の AI を引き寄せられるようにします。",
                    true
                );

                // チュートリアル（英語→日本語／リンクはそのまま）
                CustomEditorProperties.TutorialButton(
                    "Attract Modifier の使い方チュートリアルは以下を参照してください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component/using-an-attract-modifier"
                );

                // 各プロパティ（ラベルと説明を日本語化）
                CustomEditorProperties.CustomPropertyField(
                    EmeraldAILayerProp,
                    "Emerald AI のレイヤー",
                    "AI が使用している『Emerald AI 用レイヤー』です。このレイヤーに属し、かつ Sound Detection コンポーネントを持つ AI のみ検出されます。",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    AttractReactionProp,
                    "アトラクトリアクション",
                    "この修飾子が起動/トリガーされたときに呼び出す Reaction Object です。"
                  + "（Project ビューで右クリック → Create > Emerald AI > Create > Reaction Object で作成できます）",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    EnemyRelationsOnlyProp,
                    "敵対関係のみ",
                    "プレイヤーに対して『敵（Enemy）』関係の AI のみ、この Attract Modifier を受け取るように制御します。無効にすると、範囲内のすべての AI が対象になります。",
                    false
                );

                if (EnemyRelationsOnlyProp.boolValue)
                {
                    CustomEditorProperties.BeginIndent();
                    // "Player Faction" → 「プレイヤー派閥」
                    PlayerFactionProp.intValue = EditorGUILayout.Popup("プレイヤー派閥", PlayerFactionProp.intValue, FactionData.FactionNameList.ToArray());
                    EditorGUILayout.LabelField("プレイヤーが所属する派閥です。", EditorStyles.helpBox);
                    CustomEditorProperties.EndIndent();
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(
                    RadiusProp,
                    "半径",
                    "この Attract Modifier の効果範囲です。範囲内の AI はトリガー時に Reaction Object を受け取ります。",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    ReactionCooldownSecondsProp,
                    "リアクションクールダウン（秒）",
                    "アトラクトリアクションを再度実行できるようになるまでの秒数です。",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    SoundCooldownSecondsProp,
                    "サウンドクールダウン（秒）",
                    "トリガーサウンドを再生可能になるまでの秒数です。",
                    true
                );

                if ((TriggerTypes)TriggerTypeProp.intValue == TriggerTypes.OnCollision)
                {
                    CustomEditorProperties.CustomPropertyField(
                        MinVelocityProp,
                        "最小速度",
                        "衝突トリガータイプ時に、アトラクトリアクションを発火するために必要な最小相対速度です。",
                        true
                    );
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(
                    TriggerTypeProp,
                    "トリガータイプ",
                    "Attract Modifier をどのように起動するかを制御します。",
                    false
                );

                // 各トリガー種別の説明（英語→日本語）
                if (TriggerTypeProp.intValue == (int)TriggerTypes.OnStart)
                {
                    EditorGUILayout.LabelField("OnStart - Start 時に Reaction Object を実行し、この GameObject を誘引元とします。", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnTrigger)
                {
                    EditorGUILayout.LabelField("OnTrigger - このオブジェクトに対して『トリガー衝突』が発生したときに Reaction Object を実行します。誘引元はこの GameObject です。", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCollision)
                {
                    EditorGUILayout.LabelField("OnCollision - このオブジェクトに対して『非トリガー衝突（通常の衝突）』が発生したときに Reaction Object を実行します。誘引元はこの GameObject です。", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCustomCall)
                {
                    EditorGUILayout.LabelField("OnCustomCall - AttractModifier スクリプト内の ActivateAttraction 関数が呼ばれたときに Reaction Object を実行します。誘引元はこの GameObject です。", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }

                GUILayout.Space(5);

                // 情報メッセージ（英語→日本語）
                EditorGUILayout.LabelField("トリガー条件を満たすと、『トリガーサウンド一覧』からランダムに1つ再生されます。", EditorStyles.helpBox);
                TriggerSoundsList.DoLayoutList();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）シーンビュー上で効果範囲（半径）を赤いワイヤーディスクで可視化します。
        /// </summary>
        void OnSceneGUI()
        {
            AttractModifier self = (AttractModifier)target;
            Handles.color = new Color(1f, 0f, 0, 1f);
            Handles.DrawWireDisc(self.transform.position, self.transform.up, (float)self.Radius, 3);
        }

        /// <summary>
        /// （日本語）トリガーに使用可能なレイヤーを選択する UI を描画します（Nothing は不可）。
        /// </summary>
        void TriggerLayerMaskDrawer()
        {
            CustomEditorProperties.BeginIndent();
            CustomEditorProperties.CustomPropertyField(
                TriggerLayersProp,
                "トリガーレイヤー",
                "この Attract Modifier をトリガーできる衝突レイヤーを制御します。",
                true
            );

            if (TriggerLayersProp.intValue == 0)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("トリガーレイヤーの LayerMask を『Nothing』に設定することはできません。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            CustomEditorProperties.EndIndent();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}
