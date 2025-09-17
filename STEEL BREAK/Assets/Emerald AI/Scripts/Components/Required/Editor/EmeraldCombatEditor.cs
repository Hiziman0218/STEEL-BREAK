using System.Collections;                         // コルーチン・汎用コレクション
using System.Collections.Generic;                 // List<T> 等
using UnityEngine;                                // Unity 基本 API
using System;                                     // 汎用
using UnityEditor;                                // エディタ拡張 API
using UnityEditorInternal;                        // ReorderableList 等
using System.Linq;                                // LINQ

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldCombat))]         // このエディタは EmeraldCombat コンポーネントに対応
    [CanEditMultipleObjects]                      // 複数オブジェクトの同時編集を許可
    // 【クラス説明】EmeraldCombatEditor（エディタ拡張クラス）
    // EmeraldCombat のインスペクタ UI を提供し、攻撃設定・武器タイプ設定・アクション設定などを編集可能にします。
    public class EmeraldCombatEditor : Editor
    {
        [Header("折りたたみ見出しの GUIStyle（インスペクタ用注釈）")]
        GUIStyle FoldoutStyle;                     // セクション見出しのスタイル

        [Header("シーンガイド：現在の攻撃距離（インスペクタ用注釈）")]
        float CurrentAttackDistance = 0;           // シーンビューに描画する攻撃距離

        [Header("シーンガイド：近すぎ距離（インスペクタ用注釈）")]
        float CurrentTooCloseDistance = 0;         // シーンビューに描画する「近すぎ距離」

        [Header("シーンガイド描画フラグ（インスペクタ用注釈）")]
        bool DrawDistanceActive = false;           // シーンガイド表示の切替

        [Header("見出し用アイコン（Resources から取得・インスペクタ用注釈）")]
        Texture CombatEditorIcon;                  // インスペクタのヘッダーに表示するアイコン

        [Header("対象 AI の EmeraldAnimation 参照（インスペクタ用注釈）")]
        EmeraldAnimation EmeraldAnimation;         // 攻撃アニメ選択に使用

        [Header("対象 AI の EmeraldBehaviors 参照（インスペクタ用注釈）")]
        EmeraldBehaviors EmeraldBehaviors;         // 振る舞い種別の判定に使用

        [Header("インスペクタ折りたたみ状態（SerializedProperty・インスペクタ用注釈）")]
        SerializedProperty HideSettingsFoldout,    // 設定全体を隠す
                          DamageSettingsFoldout,   // 戦闘設定
                          WeaponType1SettingsFoldout, // 武器タイプ1設定
                          WeaponType2SettingsFoldout, // 武器タイプ2設定
                          SwitchWeaponSettingsFoldout, // 武器切替設定
                          CombatActionSettingsFoldout; // 戦闘アクション設定

        //Enums
        [Header("列挙型プロパティ（ターゲット選択/武器切替/攻撃選択・インスペクタ用注釈）")]
        SerializedProperty Type1PickTargetTypeProp, // タイプ1のターゲット選択方法
                          Type2PickTargetTypeProp,  // タイプ2のターゲット選択方法
                          SwitchWeaponTypeProp,     // 武器切替方法
                          StartingWeaponTypeProp,   // 開始時の武器タイプ
                          Type1AttackPickTypeProp,  // タイプ1の攻撃選択方法
                          Type2AttackPickTypeProp;  // タイプ2の攻撃選択方法

        //Int
        [Header("整数プロパティ（時間/距離/復帰時間など・インスペクタ用注釈）")]
        SerializedProperty SwitchWeaponTimeMinProp,     // 武器切替の最小時間
                          SwitchWeaponTimeMaxProp,      // 武器切替の最大時間
                          SwitchWeaponTypesDistanceProp,// 近接↔遠隔の切替距離
                          SwitchWeaponTypesCooldownProp,// 武器切替クールダウン
                          MinResumeWanderProp,          // 徘徊再開の最小遅延
                          MaxResumeWanderProp;          // 徘徊再開の最大遅延

        //Float
        [Header("小数プロパティ（攻撃クールダウン・インスペクタ用注釈）")]
        SerializedProperty Type1AttackCooldownProp,     // タイプ1攻撃のクールダウン
                          Type2AttackCooldownProp;      // タイプ2攻撃のクールダウン

        [Header("ReorderableList 群（攻撃/攻撃トランスフォーム/アクション・インスペクタ用注釈）")]
        ReorderableList Type1Attacks,                   // タイプ1の攻撃リスト
                        Type2Attacks,                   // タイプ2の攻撃リスト
                        WeaponType1AttackTransforms,    // タイプ1の攻撃トランスフォーム
                        WeaponType2AttackTransforms,    // タイプ2の攻撃トランスフォーム
                        Type1ActionsList,               // タイプ1の戦闘アクションリスト
                        Type2ActionsList;               // タイプ2の戦闘アクションリスト

        [Header("攻撃トランスフォームの説明ツールチップ（日本語化済・インスペクタ用注釈）")]
        string AttackTransformTooltip =
            "各『攻撃トランスフォーム』は、アニメーションイベントの String パラメータへトランスフォーム名を渡すことで、" +
            "CreateAbility 実行中に個別に使用できます。これにより、手から投げる手榴弾、銃口から発射される弾丸、杖から放たれる呪文など、" +
            "攻撃やアビリティの出現位置を AI ごとに柔軟にカスタマイズ可能です。\n\n" +
            "注記：複数の AI で使い回す場合に備え、攻撃トランスフォーム名は一貫した命名にすることを推奨します。";

        /// <summary>
        /// 【OnEnable（日本語）】
        /// 対象コンポーネントの参照・各 SerializedProperty と ReorderableList の初期化を行います。
        /// </summary>
        void OnEnable()
        {
            EmeraldCombat self = (EmeraldCombat)target;                 // 対象の EmeraldCombat
            EmeraldAnimation = self.GetComponent<EmeraldAnimation>();   // 参照キャッシュ
            EmeraldBehaviors = self.GetComponent<EmeraldBehaviors>();   // 参照キャッシュ
            if (CombatEditorIcon == null)                               // アイコン未ロード時のみロード
                CombatEditorIcon = Resources.Load("Editor Icons/EmeraldCombat") as Texture;
            InitializeProperties();                                     // SerializedProperty の紐付け
            InitializeLists(self);                                       // 各リストの初期化
        }

        /// <summary>
        /// 【プロパティ初期化（日本語）】
        /// インスペクタで使用する SerializedProperty を serializedObject から取得します。
        /// </summary>
        void InitializeProperties()
        {
            // Bool 折りたたみ
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DamageSettingsFoldout = serializedObject.FindProperty("DamageSettingsFoldout");
            CombatActionSettingsFoldout = serializedObject.FindProperty("CombatActionSettingsFoldout");
            SwitchWeaponSettingsFoldout = serializedObject.FindProperty("SwitchWeaponSettingsFoldout");
            WeaponType1SettingsFoldout = serializedObject.FindProperty("WeaponType1SettingsFoldout");
            WeaponType2SettingsFoldout = serializedObject.FindProperty("WeaponType2SettingsFoldout");

            // Enum
            SwitchWeaponTypeProp = serializedObject.FindProperty("SwitchWeaponType");
            StartingWeaponTypeProp = serializedObject.FindProperty("StartingWeaponType");
            Type1AttackPickTypeProp = serializedObject.FindProperty("Type1Attacks.AttackPickType");
            Type2AttackPickTypeProp = serializedObject.FindProperty("Type2Attacks.AttackPickType");
            Type1PickTargetTypeProp = serializedObject.FindProperty("Type1PickTargetType");
            Type2PickTargetTypeProp = serializedObject.FindProperty("Type2PickTargetType");

            // Int
            SwitchWeaponTimeMinProp = serializedObject.FindProperty("SwitchWeaponTimeMin");
            SwitchWeaponTimeMaxProp = serializedObject.FindProperty("SwitchWeaponTimeMax");
            SwitchWeaponTypesDistanceProp = serializedObject.FindProperty("SwitchWeaponTypesDistance");
            SwitchWeaponTypesCooldownProp = serializedObject.FindProperty("SwitchWeaponTypesCooldown");
            MinResumeWanderProp = serializedObject.FindProperty("MinResumeWander");
            MaxResumeWanderProp = serializedObject.FindProperty("MaxResumeWander");

            // Float
            Type1AttackCooldownProp = serializedObject.FindProperty("Type1AttackCooldown");
            Type2AttackCooldownProp = serializedObject.FindProperty("Type2AttackCooldown");
        }

        /// <summary>
        /// 【リスト初期化（日本語）】
        /// 攻撃トランスフォーム/攻撃データ/アクションの ReorderableList を初期化し、描画コールバックを設定します。
        /// </summary>
        void InitializeLists(EmeraldCombat self)
        {
            // Type 1 AttackTransforms（攻撃出現位置の一覧）
            WeaponType1AttackTransforms = new ReorderableList(serializedObject, serializedObject.FindProperty("WeaponType1AttackTransforms"), true, true, true, true);
            WeaponType1AttackTransforms.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "武器タイプ1：攻撃トランスフォーム一覧", EditorStyles.boldLabel); // 日本語に置換
            };
            WeaponType1AttackTransforms.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = WeaponType1AttackTransforms.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight),
                        element,
                        new GUIContent($"攻撃トランスフォーム {index + 1}", AttackTransformTooltip)); // ラベル/ツールチップ日本語
                };

            // Type 2 AttackTransforms
            WeaponType2AttackTransforms = new ReorderableList(serializedObject, serializedObject.FindProperty("WeaponType2AttackTransforms"), true, true, true, true);
            WeaponType2AttackTransforms.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "武器タイプ2：攻撃トランスフォーム一覧", EditorStyles.boldLabel); // 日本語に置換
            };
            WeaponType2AttackTransforms.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = WeaponType2AttackTransforms.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight),
                        element,
                        new GUIContent($"攻撃トランスフォーム {index + 1}", AttackTransformTooltip)); // 日本語
                };

            // 攻撃リストの初期化
            DrawType1Attacks(self);
            DrawType2Attacks(self);

            // Type 1 Combat Actions
            Type1ActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1CombatActions"), true, true, true, true);
            Type1ActionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = Type1ActionsList.serializedProperty.GetArrayElementAtIndex(index);
                    Type1ActionsList.elementHeight = EditorGUIUtility.singleLineHeight * 1.35f;

                    // 有効トグル
                    EditorGUI.PropertyField(new Rect(rect.x + 5f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("Enabled"), GUIContent.none);
                    EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("有効", "このアクションを有効にするか。無効なら無視されます。"));

                    // 無効時はオブジェクト指定をグレーアウト
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("Enabled").boolValue);
                    EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + 4, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("emeraldAction"), GUIContent.none);
                    EditorGUI.EndDisabledGroup();
                };

            Type1ActionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "タイプ1：戦闘アクション一覧", EditorStyles.boldLabel); // 日本語
            };

            // Type 2 Combat Actions
            Type2ActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2CombatActions"), true, true, true, true);
            Type2ActionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = Type2ActionsList.serializedProperty.GetArrayElementAtIndex(index);
                    Type2ActionsList.elementHeight = EditorGUIUtility.singleLineHeight * 1.35f;

                    EditorGUI.PropertyField(new Rect(rect.x + 5f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("Enabled"), GUIContent.none);
                    EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("有効", "このアクションを有効にするか。無効なら無視されます。"));
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("Enabled").boolValue);
                    EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + 4, rect.width - 100, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("emeraldAction"), GUIContent.none);
                    EditorGUI.EndDisabledGroup();
                };

            Type2ActionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "タイプ2：戦闘アクション一覧", EditorStyles.boldLabel); // 日本語
            };
        }

        /// <summary>
        /// 【インスペクタ描画（日本語）】
        /// 共通スタイルの更新・ヘッダー表示・セクションごとの描画を行います。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();  // 共通スタイル更新
            EmeraldCombat self = (EmeraldCombat)target;                  // 対象参照
            serializedObject.Update();                                   // 反映開始

            // ヘッダー（タイトル文字列を日本語へ）
            CustomEditorProperties.BeginScriptHeaderNew("戦闘", CombatEditorIcon, new GUIContent(), HideSettingsFoldout);

            // Aggressive でない場合の注意メッセージ（日本語）
            if (EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive)
            {
                CustomEditorProperties.DisplayImportantHeaderMessage("Combat コンポーネントは『Aggressive（攻撃的）』の AI のみ使用します。");
            }

            // Aggressive 以外は編集不可（UI を無効化）
            EditorGUI.BeginDisabledGroup(EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive);
            DisplayWarningMessage(self); // 設定不足などの警告表示

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DamageSettings(self);          // 戦闘設定
                EditorGUILayout.Space();
                CombatActionSettings(self);    // 戦闘アクション設定
                EditorGUILayout.Space();

                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    SwitchWeaponSettings(self); // 武器切替設定
                    EditorGUILayout.Space();
                }

                WeaponType1Settings(self);      // 武器タイプ1設定
                EditorGUILayout.Space();

                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    WeaponType2Settings(self);  // 武器タイプ2設定
                    EditorGUILayout.Space();
                }
            }
            EditorGUI.EndDisabledGroup();

            CustomEditorProperties.EndScriptHeader();     // ヘッダー終了
            serializedObject.ApplyModifiedProperties();   // 変更を反映
        }

        /// <summary>
        /// 【戦闘設定（日本語）】
        /// 武器タイプ数や戦闘後の徘徊復帰時間など、戦闘に関する基本設定を行います。
        /// </summary>
        void DamageSettings(EmeraldCombat self)
        {
            DamageSettingsFoldout.boolValue = EditorGUILayout.Foldout(DamageSettingsFoldout.boolValue, "戦闘設定", true, FoldoutStyle);

            if (DamageSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("戦闘設定", "戦闘に関する各種設定を行います。", true);

                // 武器タイプ数（1/2）
                self.WeaponTypeAmount = (EmeraldCombat.WeaponTypeAmounts)EditorGUILayout.EnumPopup("武器タイプ数", self.WeaponTypeAmount);
                CustomEditorProperties.CustomHelpLabelField(
                    "この AI が 1 つまたは 2 つの武器タイプを使用するかを制御します。例えば「タイプ1は剣の近接攻撃」「タイプ2は魔法の遠隔攻撃」といった使い分けが可能です。" +
                    "既定は 1 です。なお、戦闘アクションは必要に応じて両方の武器タイプで共有できます。",
                    true);

                EditorGUILayout.Space();

                // 徘徊再開までの最小/最大遅延
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MinResumeWanderProp, "徘徊再開の最小遅延", 0, 6);
                CustomEditorProperties.CustomHelpLabelField(
                    "戦闘終了後、AI が（徘徊タイプに応じて）徘徊へ戻るまでの最小待機時間です。最大値と組み合わせてランダム化されます。",
                    true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxResumeWanderProp, "徘徊再開の最大遅延", 0, 6);
                CustomEditorProperties.CustomHelpLabelField(
                    "戦闘終了後、AI が（徘徊タイプに応じて）徘徊へ戻るまでの最大待機時間です。最小値と組み合わせてランダム化されます。",
                    true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【未設定警告表示（日本語）】
        /// 攻撃リストが空などのセットアップ不足を警告として表示します。
        /// </summary>
        void DisplayWarningMessage(EmeraldCombat self)
        {
            if (EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two && self.Type2Attacks.AttackDataList.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning(
                    "現在、タイプ2の攻撃が 1 つも設定されていません。『武器タイプ2設定』の折りたたみ内で、少なくとも 1 つの攻撃を追加してください。");
            }
            else if (self.Type1Attacks.AttackDataList.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning(
                    "現在、タイプ1の攻撃が 1 つも設定されていません。『武器タイプ1設定』の折りたたみ内で、少なくとも 1 つの攻撃を追加してください。");
            }
        }

        /// <summary>
        /// 【戦闘アクション設定（日本語）】
        /// 戦闘中に AI が使用可能なアクション（回避など）を設定します。無効化されたものは無視されます。
        /// </summary>
        void CombatActionSettings(EmeraldCombat self)
        {
            CombatActionSettingsFoldout.boolValue = EditorGUILayout.Foldout(CombatActionSettingsFoldout.boolValue, "戦闘アクション設定", true, FoldoutStyle);

            if (CombatActionSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "戦闘アクション設定",
                    "戦闘中に AI が使用するアクションを制御します。無効化されたアクションは無視されます。使用できるのは Aggressive（攻撃的）な AI のみです。",
                    true);

                EditorGUILayout.Space();

                // Type1 アクション
                CustomEditorProperties.CustomHelpLabelField(
                    "タイプ1の武器を使用中に、AI が戦闘中に実行可能なモジュール式アクションの一覧です。",
                    false);
                GUILayout.Box(new GUIContent("これは何？", "タイプ1武器使用時に適用される戦闘アクションの一覧です。"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                Type1ActionsList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // Type2 アクション（武器タイプ2がある場合）
                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    EditorGUILayout.Space();
                    CustomEditorProperties.CustomHelpLabelField(
                        "タイプ2の武器を使用中に、AI が戦闘中に実行可能なモジュール式アクションの一覧です。",
                        false);
                    GUILayout.Box(new GUIContent("これは何？", "タイプ2武器使用時に適用される戦闘アクションの一覧です。"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type2ActionsList.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【武器切替設定（日本語）】
        /// 武器タイプ1/2 の切替方法・条件（時間/距離/クールダウン等）を設定します。
        /// </summary>
        void SwitchWeaponSettings(EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                SwitchWeaponSettingsFoldout.boolValue = EditorGUILayout.Foldout(SwitchWeaponSettingsFoldout.boolValue, "武器切替設定", true, FoldoutStyle);

                if (SwitchWeaponSettingsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();
                    CustomEditorProperties.TextTitleWithDescription("武器切替設定", "武器タイプ切替に関する設定です。", true);

                    // 開始時の武器タイプ
                    EditorGUILayout.PropertyField(StartingWeaponTypeProp, new GUIContent("開始時の武器タイプ"));
                    CustomEditorProperties.CustomHelpLabelField(
                        "戦闘に入った直後に AI が最初に使用する武器タイプです。",
                        true);

                    // 距離切替の場合の注意
                    if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Distance)
                    {
                        CustomEditorProperties.DisplayImportantMessage(
                            "重要：切替方法に『距離』を使用する場合、開始時の武器タイプは遠隔戦闘で使用するタイプに設定してください。近接と遠隔の両方を持つ AI を想定した設定です。");
                        GUILayout.Space(10);
                    }

                    // 切替方法
                    EditorGUILayout.PropertyField(SwitchWeaponTypeProp, new GUIContent("切替方法"));
                    CustomEditorProperties.CustomHelpLabelField(
                        "タイプ1とタイプ2のどちらを使うかの切替方法です。『なし』の場合、開始時の武器タイプのまま固定されます。",
                        true);

                    // 時間で切替
                    if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Timed)
                    {
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTimeMinProp, "切替時間（最小）", 5, 45);
                        CustomEditorProperties.CustomHelpLabelField("武器を切り替えるまでの最小時間です。", false);

                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTimeMaxProp, "切替時間（最大）", 10, 90);
                        CustomEditorProperties.CustomHelpLabelField("武器を切り替えるまでの最大時間です。", true);
                    }
                    // 距離で切替
                    else if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Distance)
                    {
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTypesDistanceProp, "武器タイプ切替距離", 2, 15);
                        CustomEditorProperties.CustomHelpLabelField(
                            "この距離以下なら近接、それより大きければ遠隔へ切り替えます。",
                            true);

                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTypesCooldownProp, "切替クールダウン", 1, 60);
                        CustomEditorProperties.CustomHelpLabelField(
                            "切替距離を満たしても、ここで指定したクールダウンの間は再切替を行いません（切替頻度を抑制）。",
                            true);
                    }

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        /// <summary>
        /// 【武器タイプ1設定（日本語）】
        /// ターゲット選択法・攻撃クールダウン・攻撃出現位置・攻撃の選び方等を設定します。
        /// </summary>
        void WeaponType1Settings(EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.One || self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                WeaponType1SettingsFoldout.boolValue = EditorGUILayout.Foldout(WeaponType1SettingsFoldout.boolValue, "武器タイプ1設定", true, FoldoutStyle);

                if (WeaponType1SettingsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();
                    CustomEditorProperties.TextTitleWithDescription("武器タイプ1設定", "タイプ1武器に関する設定です。", true);

                    // ターゲットの選び方
                    PickTargetTypeSetting(Type1PickTargetTypeProp);

                    // 攻撃クールダウン
                    CustomEditorProperties.CustomFloatSliderPropertyField(
                        Type1AttackCooldownProp,
                        "タイプ1：攻撃クールダウン",
                        "攻撃を発生させるために必要なクールダウン時間。\n注：他アクションの実行中は、クールダウンを過ぎても攻撃が遅れる場合があります。",
                        0.35f, 5, false);
                    GUILayout.Space(12);

                    // 攻撃トランスフォーム（出現位置の説明）
                    GUILayout.Box(new GUIContent("これは何？", AttackTransformTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    WeaponType1AttackTransforms.DoLayoutList();
                    GUILayout.Space(12);

                    // 攻撃の選び方（確率/順番/ランダム）
                    CustomEditorProperties.CustomHelpLabelField("タイプ1の攻撃をどの方法で選ぶかを制御します。", false);
                    CustomEditorProperties.CustomPropertyField(Type1AttackPickTypeProp, "攻撃の選び方", "", false);

                    if (self.Type1Attacks.AttackPickType == AttackPickTypes.Odds)
                    {
                        CustomEditorProperties.CustomHelpLabelField("確率：各攻撃に設定した確率に基づいて選択します。", true);
                    }
                    else if (self.Type1Attacks.AttackPickType == AttackPickTypes.Order)
                    {
                        CustomEditorProperties.CustomHelpLabelField("順番：攻撃リストの順に循環して選択します。", true);
                    }
                    else if (self.Type1Attacks.AttackPickType == AttackPickTypes.Random)
                    {
                        CustomEditorProperties.CustomHelpLabelField("ランダム：攻撃リストからランダムに選択します。", true);
                    }
                    GUILayout.Space(10);

                    // 攻撃アニメ未設定の注意
                    if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.Type1Animations.AttackList.Count == 0 || EmeraldAnimation.Type1AttackEnumAnimations == null)
                    {
                        CustomEditorProperties.BeginIndent(12);
                        CustomEditorProperties.DisplaySetupWarning(
                            "少なくとも 1 つの『タイプ1攻撃アニメーション』を、この AI の Animation Profile > Type 1 Attack Animation List に追加してください。");
                        CustomEditorProperties.EndIndent();
                    }

                    // 攻撃リスト（押下で選択中攻撃の距離ガイドをシーンに表示）
                    GUILayout.Box(new GUIContent("これは何？", "AI の攻撃一覧です。各設定にカーソルを合わせるとツールチップが表示されます。攻撃を選択すると、その攻撃距離がシーン上に円で表示されます。"),
                                  EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type1Attacks.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        /// <summary>
        /// 【武器タイプ2設定（日本語）】
        /// ターゲット選択・攻撃クールダウン・攻撃出現位置・攻撃の選び方等を設定します（武器タイプが2つある場合）。
        /// </summary>
        void WeaponType2Settings(EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                WeaponType2SettingsFoldout.boolValue = EditorGUILayout.Foldout(WeaponType2SettingsFoldout.boolValue, "武器タイプ2設定", true, FoldoutStyle);

                if (WeaponType2SettingsFoldout.boolValue)
                {
                    EditorGUILayout.BeginVertical("Box");
                    CustomEditorProperties.TextTitleWithDescription("武器タイプ2設定", "タイプ2武器に関する設定です。", true);

                    // ターゲットの選び方
                    PickTargetTypeSetting(Type2PickTargetTypeProp);

                    // 攻撃クールダウン
                    CustomEditorProperties.CustomFloatSliderPropertyField(
                        Type2AttackCooldownProp,
                        "タイプ2：攻撃クールダウン",
                        "攻撃を発生させるために必要なクールダウン時間。\n注：他アクションの実行中は、クールダウンを過ぎても攻撃が遅れる場合があります。",
                        0.35f, 5, false);
                    GUILayout.Space(12);

                    // 攻撃トランスフォーム
                    GUILayout.Box(new GUIContent("これは何？", AttackTransformTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    WeaponType2AttackTransforms.DoLayoutList();
                    GUILayout.Space(12);

                    // 攻撃の選び方
                    CustomEditorProperties.CustomHelpLabelField("タイプ2の攻撃をどの方法で選ぶかを制御します。", false);
                    CustomEditorProperties.CustomPropertyField(Type2AttackPickTypeProp, "攻撃の選び方", "", false);

                    if (self.Type2Attacks.AttackPickType == AttackPickTypes.Odds)
                    {
                        CustomEditorProperties.CustomHelpLabelField("確率：各攻撃に設定した確率に基づいて選択します。", true);
                    }
                    else if (self.Type2Attacks.AttackPickType == AttackPickTypes.Order)
                    {
                        CustomEditorProperties.CustomHelpLabelField("順番：攻撃リストの順に循環して選択します。", true);
                    }
                    else if (self.Type2Attacks.AttackPickType == AttackPickTypes.Random)
                    {
                        CustomEditorProperties.CustomHelpLabelField("ランダム：攻撃リストからランダムに選択します。", true);
                    }
                    GUILayout.Space(10);

                    // 攻撃アニメ未設定の注意
                    if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.Type2Animations.AttackList.Count == 0 || EmeraldAnimation.Type2AttackEnumAnimations == null)
                    {
                        CustomEditorProperties.BeginIndent(12);
                        CustomEditorProperties.DisplaySetupWarning(
                            "少なくとも 1 つの『タイプ2攻撃アニメーション』を、この AI の Animation Profile > Type 2 Attack Animation List に追加してください。");
                        CustomEditorProperties.EndIndent();
                    }

                    // 攻撃リスト
                    GUILayout.Box(new GUIContent("これは何？", "AI の攻撃一覧です。各設定にカーソルを合わせるとツールチップが表示されます。攻撃を選択すると、その攻撃距離がシーン上に円で表示されます。"),
                                  EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type2Attacks.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                }
            }
        }

        /// <summary>
        /// 【ターゲット選択方法の説明（日本語）】
        /// Enum 値に応じてヘルプテキストを表示します。
        /// </summary>
        void PickTargetTypeSetting(SerializedProperty PickTargetTypeProp)
        {
            EditorGUILayout.PropertyField(PickTargetTypeProp); // そのまま表示（Enum）
            CustomEditorProperties.CustomHelpLabelField("AI がターゲットを選ぶ方法を制御します。", false);

            if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.Closest)
            {
                CustomEditorProperties.CustomHelpLabelField("最も近い：AI から最も近く、現在可視のターゲットを選びます。", true);
            }
            else if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.FirstDetected)
            {
                CustomEditorProperties.CustomHelpLabelField("最初に検知：最初に検知し、現在可視のターゲットを選びます。", true);
            }
            else if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.Random)
            {
                CustomEditorProperties.CustomHelpLabelField("ランダム：検知半径内で現在可視のターゲットからランダムに選びます。", true);
            }
        }

        /// <summary>
        /// 【シーン GUI（日本語）】
        /// シーンビューに攻撃可能距離と近すぎ距離のガイド円を描画します。
        /// </summary>
        private void OnSceneGUI()
        {
            EmeraldCombat self = (EmeraldCombat)target;
            DrawCombatRadii(self);
        }

        /// <summary>
        /// 【タイプ1攻撃リスト描画（日本語）】
        /// ReorderableList の各要素（攻撃）を描画し、必要に応じて警告を表示します。
        /// </summary>
        void DrawType1Attacks(EmeraldCombat self)
        {
            // Type 1 Attacks
            Type1Attacks = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Attacks").FindPropertyRelative("AttackDataList"), true, true, true, true);

            Type1Attacks.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "タイプ1：攻撃一覧", EditorStyles.boldLabel); // 日本語
            };

            Type1Attacks.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = Type1Attacks.serializedProperty.GetArrayElementAtIndex(index);

                    // 条件モジュールが有効な攻撃の注意（アイコン付与）
                    if (self.Type1Attacks.AttackDataList[index].AbilityObject != null && self.Type1Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    {
                        Rect helpBoxRect = new Rect(rect.x, rect.y + 133, rect.width, EditorGUIUtility.singleLineHeight * 1.5f);
                        GUIContent content = new GUIContent(
                            " 条件モジュール有効（ツールチップ参照）",
                            EditorGUIUtility.IconContent("console.warnicon").image,
                            "条件モジュールが有効なアビリティは、条件を満たしたときのみ発動可能です。優先度『高』の条件は、攻撃の選び方（確率/順/ランダム）に関わらず優先されます。\n\n注：条件未達の場合、そのアビリティはスキップされます。");
                        EditorGUI.LabelField(helpBoxRect, content, EditorStyles.label);
                    }

                    // 攻撃アニメーション選択
                    if (self.Type1Attacks.AttackDataList.Count > 0 && EmeraldAnimation.Type1AttackEnumAnimations != null)
                    {
                        CustomEditorProperties.CustomListPopup(
                            new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent(),
                            element.FindPropertyRelative("AttackAnimation"),
                            "攻撃アニメーション",
                            EmeraldAnimation.Type1AttackEnumAnimations);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent("攻撃アニメーション", "この攻撃で使用されるアニメーション。\n注：アニメーションは Animation Profile の Attack Animation List に基づきます。"));
                    }
                    else
                    {
                        EditorGUI.Popup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight),
                            0, EmeraldAnimation.Type1AttackBlankOptions);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent("攻撃アニメーション"));
                    }

                    // 選択中要素ならシーンガイドを更新
                    if (isActive)
                    {
                        CurrentAttackDistance = element.FindPropertyRelative("AttackDistance").floatValue;
                        CurrentTooCloseDistance = element.FindPropertyRelative("TooCloseDistance").floatValue;
                        DrawDistanceActive = true;
                    }

                    // ゼロ初期化の安全策
                    if (element.FindPropertyRelative("AttackDistance").floatValue == 0) element.FindPropertyRelative("AttackDistance").floatValue = 2;
                    if (element.FindPropertyRelative("TooCloseDistance").floatValue == 0) element.FindPropertyRelative("TooCloseDistance").floatValue = 0.5f;

                    // アビリティ・距離・近すぎ距離・確率
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("AbilityObject"),
                        new GUIContent("アビリティオブジェクト", "この攻撃で使用されるアビリティオブジェクト。"));

                    CustomEditorProperties.CustomListFloatSlider(
                        new Rect(rect.x, rect.y + 60, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("攻撃距離", "この攻撃が実行可能な距離。"),
                        element.FindPropertyRelative("AttackDistance"), 0.5f, 75f);

                    CustomEditorProperties.CustomListFloatSlider(
                        new Rect(rect.x, rect.y + 85, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("近すぎ距離", "この距離より近い場合、AI は後退します（間合い維持に有効）。"),
                        element.FindPropertyRelative("TooCloseDistance"), 0f, 35f);

                    EditorGUI.BeginDisabledGroup(self.Type1Attacks.AttackPickType != AttackPickTypes.Odds);
                    CustomEditorProperties.CustomListIntSlider(
                        new Rect(rect.x, rect.y + 110, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("攻撃の確率", "『確率』方式を選んでいる場合の選択確率（1-100）。"),
                        element.FindPropertyRelative("AttackOdds"), 1, 100);
                    EditorGUI.EndDisabledGroup();
                };

            // 条件モジュールの有無で高さを可変にし、注意表示のスペースを確保
            Type1Attacks.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = Type1Attacks.serializedProperty.GetArrayElementAtIndex(index);
                float height = EditorGUIUtility.singleLineHeight * 7.5f;

                if (self.Type1Attacks.AttackDataList[index].AbilityObject && self.Type1Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    height += 26f;

                return height;
            };

            Type1Attacks.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "タイプ1：攻撃一覧", EditorStyles.boldLabel);
            };
        }

        /// <summary>
        /// 【タイプ2攻撃リスト描画（日本語）】
        /// ReorderableList の要素を描画。条件モジュールの注意や距離ガイド更新も行います。
        /// </summary>
        void DrawType2Attacks(EmeraldCombat self)
        {
            // Type 2 Attacks
            Type2Attacks = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Attacks").FindPropertyRelative("AttackDataList"), true, true, true, true);

            Type2Attacks.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "タイプ2：攻撃一覧", EditorStyles.boldLabel); // 日本語
            };

            Type2Attacks.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = Type2Attacks.serializedProperty.GetArrayElementAtIndex(index);

                    // 条件モジュール注意
                    if (self.Type2Attacks.AttackDataList[index].AbilityObject != null && self.Type2Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    {
                        Rect helpBoxRect = new Rect(rect.x, rect.y + 133, rect.width, EditorGUIUtility.singleLineHeight * 1.5f);
                        GUIContent content = new GUIContent(
                            " 条件モジュール有効（ツールチップ参照）",
                            EditorGUIUtility.IconContent("console.warnicon").image,
                            "条件モジュールが有効なアビリティは、条件を満たしたときのみ発動可能です。優先度『高』の条件は、攻撃の選び方（確率/順/ランダム）に関わらず優先されます。\n\n注：条件未達の場合、そのアビリティはスキップされます。");
                        EditorGUI.LabelField(helpBoxRect, content, EditorStyles.label);
                    }

                    // 攻撃アニメーション選択
                    if (self.Type2Attacks.AttackDataList.Count > 0 && EmeraldAnimation.Type2AttackEnumAnimations != null)
                    {
                        CustomEditorProperties.CustomListPopup(
                            new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent(),
                            element.FindPropertyRelative("AttackAnimation"),
                            "攻撃アニメーション",
                            EmeraldAnimation.Type2AttackEnumAnimations);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent("攻撃アニメーション", "この攻撃で使用されるアニメーション。\n注：アニメーションは Animation Profile の Attack Animation List に基づきます。"));
                    }
                    else
                    {
                        EditorGUI.Popup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight),
                            0, EmeraldAnimation.Type1AttackBlankOptions);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                            new GUIContent("攻撃アニメーション"));
                    }

                    if (isActive)
                    {
                        CurrentAttackDistance = element.FindPropertyRelative("AttackDistance").floatValue;   // ガイド更新：攻撃距離
                        CurrentTooCloseDistance = element.FindPropertyRelative("TooCloseDistance").floatValue; // ガイド更新：近すぎ距離
                        DrawDistanceActive = true;
                    }

                    // アビリティ・距離・近すぎ距離・確率
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("AbilityObject"),
                        new GUIContent("アビリティオブジェクト", "この攻撃で使用されるアビリティオブジェクト。"));

                    CustomEditorProperties.CustomListFloatSlider(
                        new Rect(rect.x, rect.y + 60, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("攻撃距離", "この攻撃が実行可能な距離。"),
                        element.FindPropertyRelative("AttackDistance"), 0.5f, 75f);

                    CustomEditorProperties.CustomListFloatSlider(
                        new Rect(rect.x, rect.y + 85, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("近すぎ距離", "この距離より近い場合、AI は後退します（間合い維持に有効）。"),
                        element.FindPropertyRelative("TooCloseDistance"), 0f, 35f);

                    EditorGUI.BeginDisabledGroup(self.Type2Attacks.AttackPickType != AttackPickTypes.Odds);
                    CustomEditorProperties.CustomListIntSlider(
                        new Rect(rect.x, rect.y + 110, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("攻撃の確率", "『確率』方式を選んでいる場合の選択確率（1-100）。"),
                        element.FindPropertyRelative("AttackOdds"), 1, 100);
                    EditorGUI.EndDisabledGroup();
                };

            // 条件モジュールの有無で要素の高さを調整（注意文の領域確保）
            Type2Attacks.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = Type2Attacks.serializedProperty.GetArrayElementAtIndex(index);
                float height = EditorGUIUtility.singleLineHeight * 7.5f;

                if (self.Type2Attacks.AttackDataList[index].AbilityObject && self.Type2Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    height += 26f;

                return height;
            };

            Type2Attacks.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "タイプ2：攻撃一覧", EditorStyles.boldLabel);
            };
        }

        /// <summary>
        /// 【シーンガイド描画（日本語）】
        /// 選択中攻撃の「攻撃距離（赤）」「近すぎ距離（黄）」を円で表示します。
        /// </summary>
        void DrawCombatRadii(EmeraldCombat self)
        {
            if (DrawDistanceActive)
            {
                Handles.color = new Color(255, 0, 0, 1.0f);                      // 攻撃可能距離（赤）※元コード準拠
                Handles.DrawWireDisc(self.transform.position, Vector3.up, CurrentAttackDistance);
                Handles.color = new Color(1, 0.9f, 0, 1.0f);                      // 近すぎ距離（黄）
                Handles.DrawWireDisc(self.transform.position, Vector3.up, CurrentTooCloseDistance);
            }
        }
    }
}
