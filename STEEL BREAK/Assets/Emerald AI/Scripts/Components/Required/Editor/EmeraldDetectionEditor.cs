using System.Collections;                         // コレクション（IEnumerable 等）
using System.Collections.Generic;                 // List など
using UnityEngine;                                // Unity 基本 API
using UnityEditor;                                // エディタ拡張 API
using UnityEditorInternal;                        // ReorderableList など

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldDetection))]      // このエディタは EmeraldDetection に適用
    [CanEditMultipleObjects]                      // 複数選択編集を許可
    /// <summary>
    /// 【クラス説明（日本語）】
    /// EmeraldDetection のカスタムインスペクタ。検知（視野角・検知距離・更新頻度・遮蔽チェック）、
    /// タグ/レイヤー設定、派閥（Faction）関係の編集 UI を提供します。
    /// </summary>
    // ▼このクラスは「EmeraldDetection のインスペクタ拡張クラス」
    public class EmeraldDetectionEditor : Editor
    {
        #region Variables
        [Header("折りたたみ見出しのスタイル（注釈）")]
        GUIStyle FoldoutStyle;                     // セクション見出し用の GUIStyle

        [Header("対象 AI の EmeraldBehaviors 参照（注釈）")]
        EmeraldBehaviors BehaviorsComponent;       // 現在の挙動タイプ確認に使用

        [Header("検知エディタのアイコン（注釈）")]
        Texture DetectionEditorIcon;               // ヘッダー表示用のアイコン

        //Ints
        [Header("整数プロパティ：視野角/検知距離/現在の派閥（注釈）")]
        SerializedProperty FieldOfViewAngleProp, DetectionRadiusProp, CurrentFactionProp;

        //Floats
        [Header("浮動小数プロパティ：遮蔽検知の更新頻度（注釈）")]
        SerializedProperty ObstructionDetectionFrequencyProp;

        //Reorderable List
        [Header("ReorderableList：派閥関係リスト（注釈）")]
        ReorderableList FactionsList;

        //String
        [Header("文字列プロパティ：プレイヤータグ/ラグドールタグ（注釈）")]
        SerializedProperty PlayerTagProp, RagdollTagProp;

        //Bool
        [Header("ブールプロパティ：各折りたたみ状態（注釈）")]
        SerializedProperty HideSettingsFoldout, DetectionFoldout, TagFoldout, FactionFoldout;

        //Float
        [Header("浮動小数プロパティ：検知更新頻度/遮蔽持続秒数（注釈）")]
        SerializedProperty DetectionFrequencyProp, ObstructionSecondsProp;

        //Object
        [Header("オブジェクト参照：ヘッドトランスフォーム（注釈）")]
        SerializedProperty HeadTransformProp;

        //LayerMasks
        [Header("レイヤーマスク：検知対象レイヤー/遮蔽無視レイヤー（注釈）")]
        SerializedProperty DetectionLayerMaskProp, ObstructionDetectionLayerMaskProp;
        #endregion

        /// <summary>
        /// 【OnEnable（日本語）】
        /// 参照の初期化、アイコンのロード、派閥データの反映を行います。
        /// </summary>
        void OnEnable()
        {
            EmeraldDetection self = (EmeraldDetection)target;                                        // 対象の EmeraldDetection
            BehaviorsComponent = self.GetComponent<EmeraldBehaviors>();                              // 挙動コンポーネント参照
            if (DetectionEditorIcon == null) DetectionEditorIcon = Resources.Load("Editor Icons/EmeraldDetection") as Texture; // アイコン取得

            RefreshFactionData();                                                                    // 派閥データの再読み込み
        }

        /// <summary>
        /// 【派閥データを最新化（日本語）】
        /// Faction 名の読み込み→プロパティ初期化→リスト初期化を実行します。
        /// </summary>
        void RefreshFactionData()
        {
            LoadFactionData();              // Faction 名一覧の読込み
            InitializeProperties();         // SerializedProperty の紐付け
            InitializeFactionList();        // ReorderableList の初期化
        }

        /// <summary>
        /// 【不足コンポーネント等の警告表示（日本語）】
        /// 設定が不足している場合にヘッダー下へ警告メッセージを表示します。
        /// </summary>
        void MissingComponentsMessage(EmeraldDetection self)
        {
            if (!self.HeadTransform)
            {
                // 元文：The AI's Head Transform has not been applied...
                CustomEditorProperties.DisplaySetupWarning("AI の『ヘッドトランスフォーム』が未設定です。視線/遮蔽の正確なレイ計算に必要なため、検知設定の折りたたみ内で必ず割り当ててください。");
            }
            else if (self.FactionRelationsList.Count == 0 && BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            {
                // 元文：This AI needs at least 1 Faction Relation...
                CustomEditorProperties.DisplaySetupWarning("この AI が正しく機能するには、最低でも 1 つの『派閥関係』が必要です。下部の『派閥設定』フォールドアウトから追加してください。");
            }
        }

        /// <summary>
        /// 【プロパティ初期化（日本語）】
        /// serializedObject から各フィールドの SerializedProperty を取得します。
        /// </summary>
        void InitializeProperties()
        {
            //Ints
            FieldOfViewAngleProp = serializedObject.FindProperty("FieldOfViewAngle");                 // 視野角
            DetectionRadiusProp = serializedObject.FindProperty("DetectionRadius");                 // 検知距離
            CurrentFactionProp = serializedObject.FindProperty("CurrentFaction");                  // 現在の派閥

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");              // ヘッダーの折りたたみ
            DetectionFoldout = serializedObject.FindProperty("DetectionFoldout");                 // 検知設定の折りたたみ
            TagFoldout = serializedObject.FindProperty("TagFoldout");                       // タグ設定の折りたたみ
            FactionFoldout = serializedObject.FindProperty("FactionFoldout");                   // 派閥設定の折りたたみ

            //String
            PlayerTagProp = serializedObject.FindProperty("PlayerTag");                             // プレイヤーの Unity Tag
            RagdollTagProp = serializedObject.FindProperty("RagdollTag");                            // ラグドール Tag

            //Float
            DetectionFrequencyProp = serializedObject.FindProperty("DetectionFrequency");           // 検知更新頻度
            ObstructionDetectionFrequencyProp = serializedObject.FindProperty("ObstructionDetectionFrequency");// 遮蔽検知の更新頻度
            ObstructionSecondsProp = serializedObject.FindProperty("ObstructionSeconds");           // 遮蔽持続秒数

            //Object
            HeadTransformProp = serializedObject.FindProperty("HeadTransform");                      // ヘッドトランスフォーム参照

            //LayerMasks
            DetectionLayerMaskProp = serializedObject.FindProperty("DetectionLayerMask");            // 検知対象レイヤー
            ObstructionDetectionLayerMaskProp = serializedObject.FindProperty("ObstructionDetectionLayerMask"); // 遮蔽無視レイヤー
        }

        /// <summary>
        /// 【派閥リスト（ReorderableList）初期化（日本語）】
        /// リスト描画・ヘッダー描画のコールバックを設定します。
        /// </summary>
        void InitializeFactionList()
        {
            //Factions List
            FactionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("FactionRelationsList"), true, true, true, true);
            FactionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = FactionsList.serializedProperty.GetArrayElementAtIndex(index);
                    FactionsList.elementHeight = EditorGUIUtility.singleLineHeight * 3.75f;

                    // RelationType に応じて背景色をうっすら塗る
                    if (element.FindPropertyRelative("RelationType").intValue == 0)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(1.0f, 0.0f, 0.0f, 0.15f)); // 敵対（赤）
                    }
                    else if (element.FindPropertyRelative("RelationType").intValue == 1)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.1f, 0.1f, 0.1f, 0.1f)); // 中立（グレー）
                    }
                    else if (element.FindPropertyRelative("RelationType").intValue == 2)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.0f, 1.0f, 0.0f, 0.15f)); // 友好（緑）
                    }

                    // Relation Type（種別）
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("RelationType"),
                        new GUIContent("関係タイプ", "この派閥に対する本 AI の関係種別（敵対・中立・友好）。"));

                    // Faction（派閥名）
                    CustomEditorProperties.CustomListPopup(
                        new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight),
                        new GUIContent(),
                        element.FindPropertyRelative("FactionIndex"),
                        "派閥",
                        EmeraldDetection.StringFactionList.ToArray());

                    EditorGUI.PrefixLabel(
                        new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                        new GUIContent("派閥", "派閥は Faction Manager に登録された一覧に基づきます。AI は必要なだけ多くの派閥関係を持てます。"));
                };

            FactionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "AI の派閥関係", EditorStyles.boldLabel); // ヘッダー（日本語）
            };
        }

        /// <summary>
        /// 【インスペクタ描画（日本語）】
        /// ヘッダーブロックと各設定セクションを描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();     // 共通スタイル更新
            EmeraldDetection self = (EmeraldDetection)target;               // 対象
            serializedObject.Update();                                      // 変更追跡開始

            // ヘッダー（"Detection" → "検知" に変更）
            CustomEditorProperties.BeginScriptHeaderNew("検知", DetectionEditorIcon, new GUIContent(), HideSettingsFoldout);

            // 不足設定の警告表示
            MissingComponentsMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DetectionSettings(self);    // 検知設定
                EditorGUILayout.Space();
                TagSettings(self);          // タグ＆レイヤー設定
                EditorGUILayout.Space();
                FactionSettings(self);      // 派閥設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                       // ヘッダー終了

            serializedObject.ApplyModifiedProperties();                     // 変更を反映
        }

        /// <summary>
        /// 【検知設定（日本語）】
        /// 視野角・検知距離・検知更新頻度・遮蔽関係・ヘッドトランスフォームなどを設定します。
        /// </summary>
        void DetectionSettings(EmeraldDetection self)
        {
            // フォールドアウト（"Detection Settings" → "検知設定"）
            DetectionFoldout.boolValue = EditorGUILayout.Foldout(DetectionFoldout.boolValue, "検知設定", true, FoldoutStyle);

            if (DetectionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "検知設定",
                    "検知半径・ターゲット検出・視野角など、AI の各種『検知』挙動を制御します。",
                    true);

                // 視野角
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), FieldOfViewAngleProp, "視野角", 1, 360);
                CustomEditorProperties.CustomHelpLabelField("AI がターゲットを検知できる視野角を制御します。", true);

                // 検知距離
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), DetectionRadiusProp, "検知距離", 1, 100);
                CustomEditorProperties.CustomHelpLabelField("視野の到達距離、ならびに AI の検知半径を制御します。", true);

                // 検知更新頻度
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DetectionFrequencyProp, "検知更新頻度", 0.1f, 2f);
                CustomEditorProperties.CustomHelpLabelField("AI の検知計算をどの間隔で更新するかを制御します。", true);

                // 遮蔽持続秒数
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ObstructionSecondsProp, "遮蔽持続秒数", 0.5f, 5f);
                CustomEditorProperties.CustomHelpLabelField("遮蔽され続けた際、AI が新しいターゲットへ切り替えるまでの秒数を制御します。", true);

                // 遮蔽検知の更新頻度
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ObstructionDetectionFrequencyProp, "遮蔽検知の更新頻度", 0.05f, 1f);
                CustomEditorProperties.CustomHelpLabelField("AI と現在ターゲットの間に遮蔽物があるかを、どの頻度でチェックするかを制御します。", false);

                // 遮蔽時に無視するレイヤー
                CustomEditorProperties.BeginIndent();
                EditorGUILayout.PropertyField(ObstructionDetectionLayerMaskProp, new GUIContent("遮蔽判定で無視するレイヤー"));
                CustomEditorProperties.CustomHelpLabelField(
                    "攻撃における遮蔽検知で無視すべきレイヤーを指定します。これらのレイヤーに属するオブジェクトは AI の視線を妨げる『遮蔽物』として扱われません。" +
                    "ターゲットの視線を遮るものが無い場合は、Nothing を設定できます。",
                    true);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                // ヘッドトランスフォーム
                EditorGUILayout.PropertyField(HeadTransformProp, new GUIContent("ヘッドトランスフォーム"));
                CustomEditorProperties.CustomHelpLabelField(
                    "AI の頭部トランスフォーム。視線や遮蔽に関連するレイ計算、Look-At の基準に使用します。骨階層内の頭部オブジェクトを割り当ててください。",
                    false);

                // 自動検出ボタン
                CustomEditorProperties.AutoFindHeadTransform(new Rect(), new GUIContent(), HeadTransformProp, self.transform);
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【タグ＆レイヤー設定（日本語）】
        /// 検知対象レイヤーやプレイヤーの Unity Tag を設定します。
        /// </summary>
        void TagSettings(EmeraldDetection self)
        {
            // フォールドアウト（"Tag & Layer Settings" → "タグ＆レイヤー設定"）
            TagFoldout.boolValue = EditorGUILayout.Foldout(TagFoldout.boolValue, "タグ＆レイヤー設定", true, FoldoutStyle);

            if (TagFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "タグ＆レイヤー設定",
                    "AI の検知対象となるレイヤーを制御します。ここで指定されたレイヤーに属するオブジェクトをターゲットとして検知可能になります。",
                    true);

                // チュートリアルボタン（説明文を日本語化）
                CustomEditorProperties.TutorialButton(
                    "検知レイヤーとプレイヤータグの設定方法については、以下のチュートリアルをご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component/setting-up-the-detection-layers-and-player-tag");

                // 重要メッセージ
                CustomEditorProperties.NoticeTextTitleWithDescription(
                    "重要",
                    "プレイヤーとの関係は下部『派閥設定』内の『派閥関係リスト』で管理します。ここで設定する『Player の Unity Tag』は内部の各種判定に使用されます。",
                    false);

                // プレイヤーの Unity Tag
                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), PlayerTagProp, "プレイヤーの Unity タグ");
                CustomEditorProperties.CustomHelpLabelField(
                    "Player オブジェクトを定義するための Unity Tag です。オブジェクト上部の Tag プルダウンで割り当てた値を指定してください。",
                    true);
                EditorGUILayout.Space();

                // 検知レイヤー
                EditorGUILayout.PropertyField(DetectionLayerMaskProp, new GUIContent("検知レイヤー"));
                CustomEditorProperties.CustomHelpLabelField("この AI が検知対象として扱うレイヤーを指定します。", false);

                // 不正値の注意（Nothing/Default/Everything を弾く旨の注意）
                if (DetectionLayerMaskProp.intValue == 0 || DetectionLayerMaskProp.intValue == 1)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("検知レイヤーに Nothing / Default / Everything は指定できません。", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【派閥設定（日本語）】
        /// AI の所属派閥・派閥関係（敵/中立/友好）・Faction Manager 起動などを行います。
        /// </summary>
        void FactionSettings(EmeraldDetection self)
        {
            // フォールドアウト（"Faction Settings" → "派閥設定"）
            FactionFoldout.boolValue = EditorGUILayout.Foldout(FactionFoldout.boolValue, "派閥設定", true, FoldoutStyle);

            if (FactionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "派閥設定",
                    "AI がどの派閥を『敵』『味方（友好）』とみなすかを制御します。プレイヤーとの関係もここで設定します。",
                    true);

                // チュートリアルボタン（日本語化）
                CustomEditorProperties.TutorialButton(
                    "AI の派閥関係設定に関するチュートリアルは、以下をご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component/faction-relations#setting-up-an-ais-faction-relations");

                EditorGUILayout.Space();

                // 所属派閥
                CustomEditorProperties.CustomEnum(new Rect(), new GUIContent(), CurrentFactionProp, "派閥");
                CustomEditorProperties.CustomHelpLabelField(
                    "AI の所属派閥名です。他の AI はこの名前をもとに敵対ターゲットを探索します。",
                    true);

                // Faction Manager 起動案内
                CustomEditorProperties.CustomHelpLabelField("派閥は Faction Manager で作成/削除できます。", false);
                if (GUILayout.Button("Faction Manager を開く"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // 関係リスト
                EditorGUILayout.LabelField("AI の派閥関係", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField(
                    "この AI がどの派閥を敵/友好とみなすかを制御します。各設定にマウスオーバーするとツールチップが表示されます。",
                    false);

                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField(
                    "注意：AI の派閥関係は Unity のタグではなく『派閥名』を使用します。派閥の追加・削除は Faction Manager から行えます（下のボタンで開く）。",
                    EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                if (GUILayout.Button("Faction Manager を開く"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                FactionsList.DoLayoutList();      // 派閥関係の ReorderableList を描画
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【シーン GUI（日本語）】
        /// 検知設定が開かれている間、視野円弧/黄色領域/視野境界線を可視化します。
        /// </summary>
        private void OnSceneGUI()
        {
            EmeraldDetection self = (EmeraldDetection)target;
            DrawDetectionSettings(self);
        }

        /// <summary>
        /// 【角度から方向ベクトルを算出（日本語）】
        /// 指定角度（度）とトランスフォームに基づいて、ワールド空間の方向を返します。
        /// </summary>
        public Vector3 DirFromAngle(Transform transform, float angleInDegrees, bool angleIsGlobal, EmeraldDetection self)
        {
            if (!angleIsGlobal)
                angleInDegrees += transform.eulerAngles.y;
            return transform.rotation * Quaternion.Euler(new Vector3(0, -transform.eulerAngles.y, 0)) * new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        /// <summary>
        /// 【検知設定の可視化（日本語）】
        /// 視野角の赤/黄の円弧と、視野境界線を描画します。
        /// </summary>
        void DrawDetectionSettings(EmeraldDetection self)
        {
            if (DetectionFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                // 赤：視線にカバーされていない領域。黄：視線カバー領域。
                Handles.color = Color.red;
                Handles.DrawWireArc(self.transform.position, self.transform.up, self.transform.forward, (float)self.FieldOfViewAngle / 2f, self.DetectionRadius, 3f);
                Handles.DrawWireArc(self.transform.position, self.transform.up, self.transform.forward, -(float)self.FieldOfViewAngle / 2f, self.DetectionRadius, 3f);

                Handles.color = Color.yellow;
                Handles.DrawWireArc(self.transform.position, self.transform.up, -self.transform.forward, (360 - self.FieldOfViewAngle) / 2f, self.DetectionRadius, 3f);
                Handles.DrawWireArc(self.transform.position, self.transform.up, -self.transform.forward, -(360 - self.FieldOfViewAngle) / 2f, self.DetectionRadius, 3f);

                Vector3 viewAngleA = DirFromAngle(self.transform, -self.FieldOfViewAngle / 2f, false, self);
                Vector3 viewAngleB = DirFromAngle(self.transform, self.FieldOfViewAngle / 2f, false, self);

                Handles.color = Color.red;
                if (self.FieldOfViewAngle < 360)
                {
                    Handles.DrawLine(self.transform.position, self.transform.position + viewAngleA * self.DetectionRadius, 3f);
                    Handles.DrawLine(self.transform.position, self.transform.position + viewAngleB * self.DetectionRadius, 3f);
                }
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// 【Faction データ読み込み（日本語）】
        /// Resources の『Faction Data』から派閥名リストを取得し、重複/空文字を除外して反映します。
        /// </summary>
        void LoadFactionData()
        {
            EmeraldDetection.StringFactionList.Clear();                                                                     // まずクリア
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));                                      // アセットパス
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));

            if (FactionData != null)
            {
                foreach (string s in FactionData.FactionNameList)
                {
                    if (!EmeraldDetection.StringFactionList.Contains(s) && s != "")
                    {
                        EmeraldDetection.StringFactionList.Add(s);                                                         // 一意な名前のみ追加
                    }
                }
            }
        }

        /// <summary>
        /// 【カスタム Tag フィールド（日本語）】
        /// Tag 用プロパティの描画。変更があれば SerializedProperty に反映します。
        /// </summary>
        void CustomTag(Rect position, GUIContent label, SerializedProperty property)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.TagField(position, property.stringValue);

            if (EditorGUI.EndChangeCheck())
                property.stringValue = newValue;

            EditorGUI.EndProperty();
        }
    }
}
