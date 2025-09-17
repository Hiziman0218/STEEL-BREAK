using System.Collections;                         // （保持）コルーチン関連
using System.Collections.Generic;                 // （保持）汎用コレクション
using UnityEngine;                                // Unity ランタイムAPI
using UnityEditor;                                // エディタ拡張API
using UnityEditorInternal;                        // ReorderableList 等（本ファイルでは直接未使用だが原文保持）

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldUI))]             // このカスタムインスペクタは EmeraldUI 用
    [CanEditMultipleObjects]                      // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldUIEditor：
    //  EmeraldUI コンポーネントのインスペクタを拡張し、
    //  UI のセットアップ（タグ/レイヤー/スケール上限）、名前表示、称号表示、レベル表示、
    //  HPバー（ヘルスバー）、コンバットテキストなどを日本語UIで編集できるようにするエディタクラス。
    public class EmeraldUIEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                    // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture UIEditorIcon;                     // インスペクタ上部のアイコン

        #region SerializedProperties
        //Ints
        [Header("整数プロパティ（最大UIスケール）")]
        SerializedProperty MaxUIScaleSizeProp;    // UI スケールの上限

        //Enums
        [Header("列挙型プロパティ（各種ON/OFF・カスタムフォント・カスタムバー）")]
        SerializedProperty CreateHealthBarsProp,  // 自動HPバー作成
                          UseCustomFontAINameProp,// 名前フォントをカスタム
                          UseCustomFontAILevelProp,// レベルフォントをカスタム
                          CustomizeHealthBarProp, // カスタムHPバー使用
                          DisplayAINameProp,      // 名前表示
                          DisplayAITitleProp,     // 称号表示
                          DisplayAILevelProp,     // レベル表示
                          UseAINameUIOutlineEffectProp, // 名前のアウトライン使用
                          UseAILevelUIOutlineEffectProp;// レベルのアウトライン使用

        //Bools
        [Header("表示制御フラグ（折りたたみ/非表示）")]
        SerializedProperty HideSettingsFoldout,   // 全体非表示
                          UISettingsFoldoutProp,  // UIセットアップ
                          HealthBarsFoldoutProp,  // ヘルスバー設定
                          CombatTextFoldoutProp,  // コンバットテキスト設定
                          NameTextFoldoutProp,    // 名前テキスト設定
                          LevelTextFoldoutProp;   // レベルテキスト設定

        //Layermask
        [Header("レイヤーマスク（UIを有効化する検出対象レイヤー）")]
        SerializedProperty UILayerMaskProp;       // UI レイヤー

        //Float
        [Header("小数プロパティ（名前と称号の行間）")]
        SerializedProperty AINameLineSpacingProp; // 名前と称号の行間

        //Colors
        [Header("色プロパティ（HPバー色/背景色/文字色/アウトライン色/フォント参照）")]
        SerializedProperty HealthBarColorProp,            // HPバー色
                          HealthBarColorDamageProp,       // 被ダメ時のHPバー色
                          HealthBarBackgroundColorProp,   // HPバー背景色
                          NameTextColorProp,              // 名前テキスト色
                          LevelTextColorProp,             // レベルテキスト色
                          AINameUIOutlineColorProp,       // 名前アウトライン色
                          AILevelUIOutlineColorProp,      // レベルアウトライン色
                          AINameFontProp,                 // 名前フォント
                          AILevelFontProp;                // レベルフォント

        //Vectors
        [Header("ベクター/数値プロパティ（位置/サイズ/スケールなど）")]
        SerializedProperty AINamePosProp,         // 名前テキスト位置
                          AILevelPosProp,         // レベルテキスト位置
                          AINameUIOutlineSizeProp,// 名前アウトライン太さ
                          AILevelUIOutlineSizeProp,// レベルアウトライン太さ
                          HealthBarPosProp,       // HPバー位置
                          NameTextFontSizeProp,   // 名前フォントサイズ
                          HealthBarScaleProp;     // HPバーのスケール

        //Objects
        [Header("オブジェクト参照（HPバー用スプライト）")]
        SerializedProperty HealthBarImageProp,            // HPバー本体スプライト
                          HealthBarBackgroundImageProp;   // HPバー背景スプライト

        //String
        [Header("文字列プロパティ（タグ/カメラタグ/表示名/称号/レベル値）")]
        SerializedProperty UITagProp,             // UIを発火させるタグ
                          CameraTagProp,          // プレイヤーカメラのタグ
                          AINameProp,             // AIの名前
                          AITitleProp,            // AIの称号
                          AILevelProp;            // AIのレベル
        #endregion

        void OnEnable()                           // エディタ有効化時の初期化
        {
            if (UIEditorIcon == null) UIEditorIcon = Resources.Load("Editor Icons/EmeraldUI") as Texture; // ヘッダー用アイコンをロード
            InitializeProperties();               // SerializedProperty を対象フィールドへ紐付け
        }

        void InitializeProperties()              // 各 SerializedProperty のバインド
        {
            //Int
            MaxUIScaleSizeProp = serializedObject.FindProperty("MaxUIScaleSize"); // UIスケール上限

            //Floats
            AINameLineSpacingProp = serializedObject.FindProperty("AINameLineSpacing"); // 行間

            //Enums
            UseAINameUIOutlineEffectProp = serializedObject.FindProperty("UseAINameUIOutlineEffect");   // 名前アウトライン
            UseAILevelUIOutlineEffectProp = serializedObject.FindProperty("UseAILevelUIOutlineEffect"); // レベルアウトライン
            CreateHealthBarsProp = serializedObject.FindProperty("AutoCreateHealthBars");               // HPバー自動作成
            CustomizeHealthBarProp = serializedObject.FindProperty("UseCustomHealthBar");               // カスタムHPバー
            DisplayAINameProp = serializedObject.FindProperty("DisplayAIName");                         // 名前表示
            DisplayAITitleProp = serializedObject.FindProperty("DisplayAITitle");                       // 称号表示
            DisplayAILevelProp = serializedObject.FindProperty("DisplayAILevel");                       // レベル表示
            UseCustomFontAINameProp = serializedObject.FindProperty("UseCustomFontAIName");             // 名前フォントをカスタム
            UseCustomFontAILevelProp = serializedObject.FindProperty("UseCustomFontAILevel");           // レベルフォントをカスタム

            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");                 // 全体非表示
            UISettingsFoldoutProp = serializedObject.FindProperty("UISettingsFoldout");                 // UIセットアップ折りたたみ
            HealthBarsFoldoutProp = serializedObject.FindProperty("HealthBarsFoldout");                 // HPバー折りたたみ
            CombatTextFoldoutProp = serializedObject.FindProperty("CombatTextFoldout");                 // コンバットテキスト折りたたみ
            NameTextFoldoutProp = serializedObject.FindProperty("NameTextFoldout");                     // 名前テキスト折りたたみ
            LevelTextFoldoutProp = serializedObject.FindProperty("LevelTextFoldout");                   // レベルテキスト折りたたみ

            //Layermask
            UILayerMaskProp = serializedObject.FindProperty("UILayerMask");                             // UIレイヤー

            //Vectors
            HealthBarPosProp = serializedObject.FindProperty("HealthBarPos");                           // HPバー位置
            NameTextFontSizeProp = serializedObject.FindProperty("NameTextFontSize");                   // 名前フォントサイズ
            HealthBarScaleProp = serializedObject.FindProperty("HealthBarScale");                       // HPバーのスケール
            AINamePosProp = serializedObject.FindProperty("AINamePos");                                 // 名前位置
            AINameUIOutlineSizeProp = serializedObject.FindProperty("AINameUIOutlineSize");             // 名前アウトライン太さ
            AILevelPosProp = serializedObject.FindProperty("AILevelPos");                               // レベル位置
            AILevelUIOutlineSizeProp = serializedObject.FindProperty("AILevelUIOutlineSize");           // レベルアウトライン太さ

            //Color
            HealthBarColorProp = serializedObject.FindProperty("HealthBarColor");                       // HPバー色
            HealthBarColorDamageProp = serializedObject.FindProperty("HealthBarDamageColor");           // 被ダメ時色
            HealthBarBackgroundColorProp = serializedObject.FindProperty("HealthBarBackgroundColor");   // 背景色
            NameTextColorProp = serializedObject.FindProperty("NameTextColor");                         // 名前色
            LevelTextColorProp = serializedObject.FindProperty("LevelTextColor");                       // レベル色
            AINameUIOutlineColorProp = serializedObject.FindProperty("AINameUIOutlineColor");           // 名前アウトライン色
            AILevelUIOutlineColorProp = serializedObject.FindProperty("AILevelUIOutlineColor");         // レベルアウトライン色
            AINameFontProp = serializedObject.FindProperty("AINameFont");                               // 名前フォント
            AILevelFontProp = serializedObject.FindProperty("AILevelFont");                             // レベルフォント

            //String
            UITagProp = serializedObject.FindProperty("UITag");                                         // UIタグ
            CameraTagProp = serializedObject.FindProperty("CameraTag");                                 // カメラタグ
            AINameProp = serializedObject.FindProperty("AIName");                                       // AIの名前
            AITitleProp = serializedObject.FindProperty("AITitle");                                     // AIの称号
            AILevelProp = serializedObject.FindProperty("AILevel");                                     // AIのレベル

            //Objects
            HealthBarImageProp = serializedObject.FindProperty("HealthBarImage");                       // バー画像
            HealthBarBackgroundImageProp = serializedObject.FindProperty("HealthBarBackgroundImage");   // バー背景画像
        }

        public override void OnInspectorGUI()      // インスペクタのメイン描画
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            EmeraldUI self = (EmeraldUI)target;                         // 対象コンポーネント
            serializedObject.Update();                                  // 直列化オブジェクト同期

            // ヘッダー（見出しは「UI」のまま表記）
            CustomEditorProperties.BeginScriptHeaderNew("UI", UIEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)    // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                UISettings(self);                  // UIセットアップ
                EditorGUILayout.Space();
                NameTextSettings(self);            // 名前テキスト設定
                EditorGUILayout.Space();
                LevelTextSettings(self);           // レベルテキスト設定
                EditorGUILayout.Space();
                HealthbarSettings(self);           // HPバー設定
                EditorGUILayout.Space();
                CombatTextSettings(self);          // コンバットテキスト設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了
            serializedObject.ApplyModifiedProperties(); // 変更を適用
        }

        /// <summary>
        /// （日本語）UI 全体のセットアップ（タグ/レイヤー/スケール上限など）を表示・編集します。
        /// </summary>
        void UISettings(EmeraldUI self)
        {
            // 「UI Setup」→「UIセットアップ」
            UISettingsFoldoutProp.boolValue = CustomEditorProperties.Foldout(UISettingsFoldoutProp.boolValue, "UIセットアップ", true, FoldoutStyle);

            if (UISettingsFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "UIセットアップ",
                    "Emerald の組み込みUIの利用とセットアップを制御します。UI を表示するには、適切なタグを持つプレイヤーが AI のトリガー半径に入る必要があります。" +
                    "AI の UI タグは Detection and Tag タブ内で設定できます。",
                    true
                );

                // 警告メッセージ（英語→日本語）
                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField(
                    "UI システムを正しく機能させるには、タグとレイヤーの割り当てが必要です。一般的にはプレイヤーのタグとレイヤーを使用します。" +
                    "これにより、適切なオブジェクトが検出された場合のみ UI システムが動作し、効率化されます。UI を正しく配置するために、プレイヤーのカメラのタグも適用してください。",
                    EditorStyles.helpBox
                );
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();

                // 「Camera Tag」→「カメラのタグ」
                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), CameraTagProp, "カメラのタグ");
                CustomEditorProperties.CustomHelpLabelField(
                    "カメラのタグは、プレイヤーが使用している Unity のタグです。UI を正しい位置に配置するためにカメラ情報が必要です。",
                    true
                );

                // 「UI Tag」→「UIのタグ」
                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), UITagProp, "UIのタグ");
                CustomEditorProperties.CustomHelpLabelField(
                    "UI のタグは、AI の UI（有効時）をトリガーする Unity のタグです。",
                    true
                );

                // 「UI Layers」→「UIレイヤー」
                EditorGUILayout.PropertyField(UILayerMaskProp, new GUIContent("UIレイヤー"));
                CustomEditorProperties.CustomHelpLabelField(
                    "UIレイヤーは、AI が UI を有効化する際に検出するレイヤーを制御します（対象オブジェクトが UI のタグも持っている必要があります）。通常はプレイヤー用に使用します。",
                    false
                );

                if (UILayerMaskProp.intValue == 0 || UILayerMaskProp.intValue == 1)
                {
                    // 「…cannot contain Nothing, Default, or Everything.」→ 日本語
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("UIレイヤーに『Nothing』『Default』『Everything』を含めることはできません。", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();

                // 「Max UI Scale」→「UIスケール上限」
                EditorGUILayout.PropertyField(MaxUIScaleSizeProp, new GUIContent("UIスケール上限"));
                CustomEditorProperties.CustomHelpLabelField(
                    "プレイヤーが AI の UI から離れていく際に、UI が拡大される最大サイズを制御します。",
                    true
                );

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）AI の名前および称号の表示・フォント・位置・色・アウトラインなどを編集します。
        /// </summary>
        void NameTextSettings(EmeraldUI self)
        {
            // 「Name Text Settings」→「名前テキスト設定」
            NameTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(NameTextFoldoutProp.boolValue, "名前テキスト設定", true, FoldoutStyle);

            if (NameTextFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "名前テキスト設定",
                    "Emerald AI の組み込み（Unityベース）UI システムを用いて、この AI の『名前』を表示・位置調整するための設定です。",
                    true
                );

                // 「Display AI Name」→「AIの名前を表示」
                EditorGUILayout.PropertyField(DisplayAINameProp, new GUIContent("AIの名前を表示"));
                CustomEditorProperties.CustomHelpLabelField(
                    "AI の名前を表示するかどうかを切り替えます。有効にすると、ヘルスバーの上に AI の名前が表示されます。",
                    true
                );

                if (self.DisplayAIName == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    // AI の名前
                    EditorGUILayout.PropertyField(AINameProp, new GUIContent("AIの名前"));
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の名前です。Emerald の組み込みUIまたはカスタムUIのどちらでも表示できます。",
                        true
                    );
                    EditorGUILayout.Space();

                    // 「Use Custom Name Font」→「名前フォントをカスタム」
                    EditorGUILayout.PropertyField(UseCustomFontAINameProp, new GUIContent("名前フォントをカスタム"));
                    CustomEditorProperties.CustomHelpLabelField(
                        "名前テキストのフォントをカスタマイズするかどうかを制御します。",
                        false
                    );
                    EditorGUILayout.Space();

                    if (self.UseCustomFontAIName == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();
                        EditorGUILayout.PropertyField(AINameFontProp, new GUIContent("名前フォント"));
                        CustomEditorProperties.CustomHelpLabelField("AI の名前テキストに使用するフォントを設定します。", true);
                        CustomEditorProperties.EndIndent();
                        EditorGUILayout.Space();
                    }

                    // 位置・サイズ・色
                    EditorGUILayout.PropertyField(AINamePosProp, new GUIContent("AIの名前 位置"));
                    CustomEditorProperties.CustomHelpLabelField("AI の名前テキストの表示位置を制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(NameTextFontSizeProp, new GUIContent("AIの名前 フォントサイズ"));
                    CustomEditorProperties.CustomHelpLabelField("AI の名前テキストのフォントサイズを制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(NameTextColorProp, new GUIContent("AIの名前 色"));
                    CustomEditorProperties.CustomHelpLabelField("AI の名前テキストの色を制御します。", true);
                    EditorGUILayout.Space();

                    // アウトライン
                    EditorGUILayout.PropertyField(UseAINameUIOutlineEffectProp, new GUIContent("名前テキストにアウトラインを使用"));
                    CustomEditorProperties.CustomHelpLabelField("AI の名前UIにアウトライン効果を使用するかどうかを制御します。", true);
                    EditorGUILayout.Space();

                    if (self.UseAINameUIOutlineEffect == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AINameUIOutlineColorProp, new GUIContent("名前テキスト アウトライン色"));
                        CustomEditorProperties.CustomHelpLabelField("AI の名前テキストのアウトライン色を制御します。", true);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AINameUIOutlineSizeProp, new GUIContent("名前テキスト アウトライン太さ"));
                        CustomEditorProperties.CustomHelpLabelField("AI の名前テキストのアウトラインの太さを制御します。", true);
                        EditorGUILayout.Space();

                        CustomEditorProperties.EndIndent();
                    }

                    // 称号の表示
                    EditorGUILayout.PropertyField(DisplayAITitleProp, new GUIContent("AIの称号を表示"));
                    CustomEditorProperties.CustomHelpLabelField(
                        "AI の称号を表示するかどうかを切り替えます。有効にすると、ヘルスバーの上に AI の称号が表示されます。",
                        false
                    );

                    if (self.DisplayAITitle == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AITitleProp, new GUIContent("AIの称号"));
                        CustomEditorProperties.CustomHelpLabelField(
                            "AI の称号です。Emerald の組み込みUIまたはカスタムUIのどちらでも表示できます。",
                            true
                        );
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AINameLineSpacingProp, new GUIContent("名前と称号の行間"));
                        CustomEditorProperties.CustomHelpLabelField("AI の『名前』と『称号』の行間を制御します。", true);

                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.Space();
                    CustomEditorProperties.EndIndent();
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）AI のレベル表示（値/フォント/位置/色/アウトラインなど）を編集します。
        /// </summary>
        void LevelTextSettings(EmeraldUI self)
        {
            // 「Level Text Settings」→「レベルテキスト設定」
            LevelTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(LevelTextFoldoutProp.boolValue, "レベルテキスト設定", true, FoldoutStyle);

            if (LevelTextFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（元コードは "Name Text Settings" だが、ここでは正しく日本語へ）
                CustomEditorProperties.TextTitleWithDescription(
                    "レベルテキスト設定",
                    "Emerald AI の組み込み（Unityベース）UI システムを用いて、この AI の『レベル』を表示・位置調整するための設定です。",
                    true
                );

                // 表示ON/OFF
                EditorGUILayout.PropertyField(DisplayAILevelProp, new GUIContent("AIのレベルを表示"));
                CustomEditorProperties.CustomHelpLabelField(
                    "AI のレベルを表示するかどうかを切り替えます。有効にすると、ヘルスバーの左に AI のレベルが表示されます。",
                    true
                );

                if (self.DisplayAILevel == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    // レベル値
                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), AILevelProp, "AIのレベル");
                    CustomEditorProperties.CustomHelpLabelField("AI のレベル値です。組み込みUIまたはカスタムUIのどちらでも表示できます。", true);
                    EditorGUILayout.Space();

                    // フォントのカスタム
                    EditorGUILayout.PropertyField(UseCustomFontAILevelProp, new GUIContent("レベルフォントをカスタム"));
                    CustomEditorProperties.CustomHelpLabelField("レベルテキストのフォントをカスタマイズするかどうかを制御します。", true);
                    EditorGUILayout.Space();

                    if (self.UseCustomFontAILevel == YesOrNo.Yes)
                    {
                        EditorGUILayout.PropertyField(AILevelFontProp, new GUIContent("レベルフォント"));
                        CustomEditorProperties.CustomHelpLabelField("AI のレベルテキストに使用するフォントを設定します。", true);
                        EditorGUILayout.Space();
                    }

                    // 位置・色
                    EditorGUILayout.PropertyField(AILevelPosProp, new GUIContent("AIのレベル 位置"));
                    CustomEditorProperties.CustomHelpLabelField("AI のレベルテキストの表示位置を制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(LevelTextColorProp, new GUIContent("レベル色"));
                    CustomEditorProperties.CustomHelpLabelField("AI のレベルテキストの色を制御します。", true);
                    EditorGUILayout.Space();

                    // アウトライン
                    EditorGUILayout.PropertyField(UseAILevelUIOutlineEffectProp, new GUIContent("レベルテキストにアウトラインを使用"));
                    CustomEditorProperties.CustomHelpLabelField("AI のレベルUIにアウトライン効果を使用するかどうかを制御します。", true);
                    EditorGUILayout.Space();

                    if (self.UseAILevelUIOutlineEffect == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AILevelUIOutlineColorProp, new GUIContent("レベルテキスト アウトライン色"));
                        CustomEditorProperties.CustomHelpLabelField("AI のレベルテキストのアウトライン色を制御します。", true);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AILevelUIOutlineSizeProp, new GUIContent("レベルテキスト アウトライン太さ"));
                        CustomEditorProperties.CustomHelpLabelField("AI のレベルテキストのアウトラインの太さを制御します。", true);
                        EditorGUILayout.Space();

                        CustomEditorProperties.EndIndent();
                    }

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）ヘルスバー（位置/スケール/色/カスタムスプライト）の設定を編集します。
        /// </summary>
        void HealthbarSettings(EmeraldUI self)
        {
            // 「Health Bar Settings」→「HPバー設定」
            HealthBarsFoldoutProp.boolValue = CustomEditorProperties.Foldout(HealthBarsFoldoutProp.boolValue, "HPバー設定", true, FoldoutStyle);

            if (HealthBarsFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "HPバー設定",
                    "Emerald AI の組み込み（Unityベース）UI システムを用いて、この AI のヘルスバーを表示・位置調整するための設定です。",
                    true
                );

                // 自動作成
                EditorGUILayout.PropertyField(CreateHealthBarsProp, new GUIContent("HPバーを自動作成"));
                CustomEditorProperties.CustomHelpLabelField(
                    "AI に対して Emerald が自動でヘルスバーを作成する機能を有効/無効にします。有効にすると追加入力項目が表示されます。",
                    true
                );
                EditorGUILayout.Space();

                if (self.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    EditorGUILayout.PropertyField(HealthBarPosProp, new GUIContent("HPバー 位置"));
                    CustomEditorProperties.CustomHelpLabelField("作成されたヘルスバーの初期位置を制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarScaleProp, new GUIContent("HPバー スケール"));
                    CustomEditorProperties.CustomHelpLabelField("作成されたヘルスバーのスケールを制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarColorProp, new GUIContent("HPバー 色"));
                    CustomEditorProperties.CustomHelpLabelField("AI のヘルスバーの色を制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarColorDamageProp, new GUIContent("被ダメ時のHPバー色"));
                    CustomEditorProperties.CustomHelpLabelField("ダメージを受けた際に表示されるヘルスバーの色を制御します。", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarBackgroundColorProp, new GUIContent("背景色"));
                    CustomEditorProperties.CustomHelpLabelField("ヘルスバーの背景色を制御します。", true);
                    EditorGUILayout.Space();

                    // カスタムスプライト
                    EditorGUILayout.PropertyField(CustomizeHealthBarProp, new GUIContent("カスタムHPバーを使用"));
                    CustomEditorProperties.CustomHelpLabelField("AI のヘルスバーにカスタムスプライトを使用できるようにします。", true);
                    EditorGUILayout.Space();

                    if (self.UseCustomHealthBar == YesOrNo.Yes)
                    {
                        // 「Health Bar Sprites」→「HPバー用スプライト」
                        EditorGUILayout.LabelField("HPバー用スプライト", EditorStyles.boldLabel);
                        EditorGUILayout.Space();

                        CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), HealthBarImageProp, "バー", typeof(Sprite), true);
                        CustomEditorProperties.CustomHelpLabelField("AI のヘルスバー本体のスプライトを設定します。", true);

                        CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), HealthBarBackgroundImageProp, "バー背景", typeof(Sprite), true);
                        CustomEditorProperties.CustomHelpLabelField("AI のヘルスバー背景のスプライトを設定します。", true);
                    }

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）コンバットテキスト（被弾数値等）のグローバル設定画面（マネージャー）を開くショートカットです。
        /// </summary>
        void CombatTextSettings(EmeraldUI self)
        {
            // 「Combat Text Settings」→「コンバットテキスト設定」
            CombatTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(CombatTextFoldoutProp.boolValue, "コンバットテキスト設定", true, FoldoutStyle);

            if (CombatTextFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "コンバットテキスト設定",
                    "Emerald AI の組み込み（グローバル）コンバットテキストシステムへのショートカットです。",
                    true
                );

                // 説明（英語→日本語）
                CustomEditorProperties.CustomHelpLabelField(
                    "コンバットテキストシステムは Combat Text Manager から調整できます。これらの設定はグローバルに適用されます。",
                    false
                );

                var ButtonStyle = new GUIStyle(GUI.skin.button);
                if (GUILayout.Button("コンバットテキストマネージャーを開く", ButtonStyle))
                {
                    // ウィンドウタイトルも日本語に（動作に影響なし）
                    EditorWindow CTM = EditorWindow.GetWindow(typeof(EmeraldCombatTextManager), true, "コンバットテキストマネージャー");
                    CTM.minSize = new Vector2(600f, 725f);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        private void OnSceneGUI()                   // シーンビューでのガイド描画
        {
            EmeraldUI self = (EmeraldUI)target;
            DrawUIPositions(self);                  // 位置ガイドを描画
        }

        /// <summary>
        /// （日本語）名前テキストやHPバーの現在位置をシーンビューに線で可視化します。
        /// </summary>
        void DrawUIPositions(EmeraldUI self)
        {
            if (self == null) return;

            if (self.DisplayAIName == YesOrNo.Yes && self.NameTextFoldout)
            {
                Handles.color = self.NameTextColor; // 名前色でラインを描画
                Handles.DrawLine(new Vector3(self.transform.localPosition.x, self.transform.localPosition.y, self.transform.localPosition.z),
                    new Vector3(self.AINamePos.x, self.AINamePos.y, self.AINamePos.z) + self.transform.localPosition);
                Handles.color = Color.white;
            }

            if (self.AutoCreateHealthBars == YesOrNo.Yes && self.HealthBarsFoldout)
            {
                Handles.color = self.HealthBarColor; // HPバー色でラインを描画
                Handles.DrawLine(new Vector3(self.transform.localPosition.x + 0.25f, self.transform.localPosition.y, self.transform.localPosition.z),
                    new Vector3(self.HealthBarPos.x + 0.25f, self.HealthBarPos.y, self.HealthBarPos.z) + self.transform.localPosition);
                Handles.color = Color.white;
            }
        }
    }
}
