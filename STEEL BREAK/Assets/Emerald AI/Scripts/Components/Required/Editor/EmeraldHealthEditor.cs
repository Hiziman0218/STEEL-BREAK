using UnityEngine;                 // Unity の基本 API
using UnityEditor;                 // エディタ拡張 API
using UnityEditorInternal;         // ReorderableList などのエディタユーティリティ

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldHealth))]     // このエディタは EmeraldHealth コンポーネントに対応
    [CanEditMultipleObjects]                  // 複数オブジェクト同時編集を許可
    /// <summary>
    /// 【クラスの説明（日本語）】
    /// EmeraldHealth 用のカスタムインスペクタ。
    /// 体力（不死身、初期体力、自然回復）や被弾エフェクト（使用有無、追従、表示時間、位置オフセット、エフェクト一覧）などの設定 UI を提供します。
    /// </summary>
    // ▼このクラスは「EmeraldHealth のインスペクタ拡張クラス」
    public class EmeraldHealthEditor : Editor
    {
        [Header("折りたたみ見出しの GUIStyle（注釈）")]
        GUIStyle FoldoutStyle;                 // セクション見出しのスタイル

        [Header("ヘルスエディタの見出しアイコン（注釈）")]
        Texture HealthEditorIcon;              // ヘッダーに表示するテクスチャ

        //Int
        [Header("SerializedProperty（Int）：初期体力/回復レート（注釈）")]
        SerializedProperty StartingHealthProp, // 初期体力
                          HealRateProp;        // 回復レート（秒間）

        //Enum
        [Header("SerializedProperty（Enum）：被弾エフェクトの使用有無（注釈）")]
        SerializedProperty UseHitEffectProp;   // Yes/No

        //Bool
        [Header("SerializedProperty（Bool）：折りたたみ/不死身/エフェクト追従（注釈）")]
        SerializedProperty HideSettingsFoldout,    // 全体を折りたたむ
                          HealthFoldout,           // 体力設定の折りたたみ
                          HitEffectFoldout,        // 被弾エフェクト設定の折りたたみ
                          ImmortalProp,            // 不死身
                          AttachHitEffectsProp;    // エフェクトを AI に追従させる

        //Float
        [Header("SerializedProperty（Float）：エフェクト消滅秒数（注釈）")]
        SerializedProperty HitEffectTimeoutSecondsProp; // エフェクトが消えるまでの秒数

        //Vector
        [Header("SerializedProperty（Vector3）：エフェクト位置オフセット（注釈）")]
        SerializedProperty HitEffectPosOffsetProp; // 被弾エフェクトの表示位置オフセット

        [Header("ReorderableList：被弾エフェクト一覧（注釈）")]
        ReorderableList HitEffectsList;        // 被弾エフェクトのリスト

        /// <summary>
        /// 【OnEnable（日本語）】
        /// アイコンのロード、SerializedProperty の取得、リスト初期化を行います。
        /// </summary>
        void OnEnable()
        {
            if (HealthEditorIcon == null) HealthEditorIcon = Resources.Load("Editor Icons/EmeraldHealth") as Texture; // リソースからアイコンを取得
            InitializeProperties();  // プロパティの紐付け
            InitializeList();        // 被弾エフェクトのリスト初期化
        }

        /// <summary>
        /// 【プロパティ初期化（日本語）】
        /// serializedObject から対象フィールドの SerializedProperty を取得します。
        /// </summary>
        void InitializeProperties()
        {
            // Ints
            StartingHealthProp = serializedObject.FindProperty("StartingHealth"); // 初期体力
            HealRateProp = serializedObject.FindProperty("HealRate");       // 回復レート

            // Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout"); // 見出しの折りたたみ
            HealthFoldout = serializedObject.FindProperty("HealthFoldout");       // 体力セクション
            HitEffectFoldout = serializedObject.FindProperty("HitEffectFoldout");    // 被弾エフェクトセクション
            ImmortalProp = serializedObject.FindProperty("Immortal");            // 不死身フラグ
            AttachHitEffectsProp = serializedObject.FindProperty("AttachHitEffects");    // エフェクト追従

            // Float
            HitEffectTimeoutSecondsProp = serializedObject.FindProperty("HitEffectTimeoutSeconds"); // エフェクトのタイムアウト

            // Vector
            HitEffectPosOffsetProp = serializedObject.FindProperty("HitEffectPosOffset"); // エフェクト位置オフセット

            // Enum
            UseHitEffectProp = serializedObject.FindProperty("UseHitEffect"); // 被弾エフェクト使用可否
        }

        /// <summary>
        /// 【リスト初期化（日本語）】
        /// 被弾エフェクトの ReorderableList を設定します。
        /// </summary>
        void InitializeList()
        {
            // Hit Effects List
            HitEffectsList = new ReorderableList(serializedObject, serializedObject.FindProperty("HitEffectsList"), true, true, true, true);
            HitEffectsList.drawHeaderCallback = rect =>
            {
                // 見出しテキストを日本語化
                EditorGUI.LabelField(rect, "被弾エフェクト一覧", EditorStyles.boldLabel);
            };
            HitEffectsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = HitEffectsList.serializedProperty.GetArrayElementAtIndex(index);
                    // 各要素（エフェクトの参照）を ObjectField で表示
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        /// <summary>
        /// 【インスペクタ描画（日本語）】
        /// 見出し（ヘッダー）と各セクション（体力/被弾エフェクト）を順に描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // 共通スタイル更新
            EmeraldHealth self = (EmeraldHealth)target;                 // 対象の EmeraldHealth
            serializedObject.Update();                                  // 変更追跡開始

            // ヘッダー（"Health" → 日本語「体力」）
            CustomEditorProperties.BeginScriptHeaderNew("体力", HealthEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                HealthSettings(self);       // 体力設定
                EditorGUILayout.Space();
                HitEffectSettings(self);    // 被弾エフェクト設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了

            serializedObject.ApplyModifiedProperties(); // 変更を反映
        }

        /// <summary>
        /// 【体力設定（日本語）】
        /// 不死身、初期体力、回復レート、プレビュー用の体力バーを設定/表示します。
        /// </summary>
        void HealthSettings(EmeraldHealth self)
        {
            // フォールドアウト（"Health Settings" → 「体力設定」）
            HealthFoldout.boolValue = EditorGUILayout.Foldout(HealthFoldout.boolValue, "体力設定", true, FoldoutStyle);

            if (HealthFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // タイトルと説明（日本語化）
                CustomEditorProperties.TextTitleWithDescription("体力設定", "体力に関連する各種設定を制御します。", true);

                // 不死身（"Immortal" → 「不死身」）
                EditorGUILayout.PropertyField(ImmortalProp, new GUIContent("不死身"));
                CustomEditorProperties.CustomHelpLabelField(
                    "AI をダメージ無効・不死身にするかを制御します。これを有効にすると、他の体力関連設定は無効化されます。",
                    true);

                // 不死身中は以下のフィールドを編集不可にする
                EditorGUI.BeginDisabledGroup(self.Immortal);

                // 初期体力（"Starting Health" → 「初期体力」）
                CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StartingHealthProp, "初期体力");
                CustomEditorProperties.CustomHelpLabelField("AI の開始時の体力を制御します。", true);

                // 回復レート（"Heal Rate" → 「回復レート」）
                CustomEditorProperties.CustomPropertyField(
                    HealRateProp,
                    "回復レート",
                    "戦闘中でないとき、最大体力未満であれば 1 秒あたりに回復する量を制御します。",
                    true);

                EditorGUI.EndDisabledGroup();

                // 体力バーのプレビュー
                DrawHealthBar(self);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【体力バー描画（日本語）】
        /// 現在体力/最大体力の割合を簡易ゲージとして表示します。エディタ停止中は常に最大体力にリセットします。
        /// </summary>
        void DrawHealthBar(EmeraldHealth self)
        {
            GUILayout.Space(45);                         // 視認性のための余白
            GUIStyle LabelStyle = new GUIStyle();        // ラベルの見た目
            LabelStyle.alignment = TextAnchor.MiddleCenter;
            LabelStyle.padding.bottom = 4;
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.normal.textColor = Color.white;

            Rect r = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;
            float CurrentHealth = ((float)self.CurrentHealth / (float)self.StartingHealth); // 体力比率

            // 背景（外枠）
            EditorGUI.DrawRect(new Rect(r.x, r.position.y - 39f, ((r.width)), 32), new Color(0.05f, 0.05f, 0.05f, 0.5f));
            // 背景（内側）
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8)), 24), new Color(0.16f, 0.16f, 0.16f, 1f));
            // メインゲージ（赤→緑の線形補間）
            Color HealthBarColor = Color.Lerp(new Color(0.6f, 0.1f, 0.1f, 1f), new Color(0.15f, 0.42f, 0.15f, 1f), CurrentHealth);
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8) * CurrentHealth), 24), HealthBarColor);

            // テキスト表記（"Current Health" → 「現在の体力」, "(Dead)" → 「(死亡)」）
            if (self.CurrentHealth > 0)
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "現在の体力: " + self.CurrentHealth + "/" + self.StartingHealth, LabelStyle);
            }
            else
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "現在の体力: " + self.CurrentHealth + "/" + self.StartingHealth + " (死亡)", LabelStyle);
            }

            EditorGUILayout.EndVertical();

            // エディタ停止中は常に最大体力に戻す（プレビューをリセット）
            if (!Application.isPlaying)
            {
                self.CurrentHealth = self.StartingHealth;
            }
        }

        /// <summary>
        /// 【被弾エフェクト設定（日本語）】
        /// 被ダメージ時のヒットエフェクト（使用有無、追従、一覧、表示時間、位置オフセット）を設定します。
        /// </summary>
        void HitEffectSettings(EmeraldHealth self)
        {
            // フォールドアウト（"Hit Effect Settings" → 「被弾エフェクト設定」）
            HitEffectFoldout.boolValue = EditorGUILayout.Foldout(HitEffectFoldout.boolValue, "被弾エフェクト設定", true, FoldoutStyle);

            if (HitEffectFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // タイトルと説明（"Allows an AI to display..." を日本語化）
                CustomEditorProperties.TextTitleWithDescription(
                    "被弾エフェクト設定",
                    "AI がダメージを受けた際に、ランダムなヒットエフェクトを表示できるようにします。",
                    true);

                // 使用有無（"Use Hit Effect" → 「被弾エフェクトを使用」）
                EditorGUILayout.PropertyField(UseHitEffectProp, new GUIContent("被弾エフェクトを使用"));
                CustomEditorProperties.CustomHelpLabelField(
                    "この AI が近接ダメージを受けたとき、ヒットエフェクトを使用するかを制御します。",
                    true);

                if (self.UseHitEffect == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    // 追従（"Attach Hit Effects" → 「ヒットエフェクトを追従させる」）
                    CustomEditorProperties.CustomPropertyField(
                        AttachHitEffectsProp,
                        "ヒットエフェクトを追従させる",
                        "被弾時に生成されたヒットエフェクトを AI に追従させるかを制御します。",
                        true);
                    EditorGUILayout.Space();

                    // エフェクト一覧（日本語ヘルプ）
                    CustomEditorProperties.CustomHelpLabelField("この AI が被弾したときに表示されるヒットエフェクトの候補です（ランダムに選択されます）。", true);
                    HitEffectsList.DoLayoutList();  // ReorderableList を描画
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // 表示時間（"Hit Effect Timeout Seconds" → 「ヒットエフェクト消滅秒数」）
                    EditorGUILayout.PropertyField(HitEffectTimeoutSecondsProp, new GUIContent("ヒットエフェクト消滅秒数"));
                    CustomEditorProperties.CustomHelpLabelField("ヒットエフェクトが無効化（非表示化）されるまでの秒数を制御します。", true);
                    EditorGUILayout.Space();

                    // 位置オフセット（"Hit Effect Position Offset" → 「ヒットエフェクト位置オフセット」）
                    EditorGUILayout.PropertyField(HitEffectPosOffsetProp, new GUIContent("ヒットエフェクト位置オフセット"));
                    CustomEditorProperties.CustomHelpLabelField("AI の Hit Transform を基準に、ヒットエフェクトの表示位置をオフセットします。", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
