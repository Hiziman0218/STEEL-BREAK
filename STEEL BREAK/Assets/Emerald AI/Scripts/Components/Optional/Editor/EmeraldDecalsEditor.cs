using System.Collections;                       // コルーチン関連（本ファイルでは直接未使用、原文保持）
using System.Collections.Generic;               // 汎用コレクション（原文保持）
using UnityEngine;                              // Unity ランタイムAPI
using UnityEditor;                              // エディタ拡張API
using UnityEditorInternal;                      // ReorderableList 等（本ファイルでは未使用、原文保持）

namespace EmeraldAI.Utility                      // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldDecals))]        // EmeraldDecals 用のカスタムインスペクタ
    [CanEditMultipleObjects]                     // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldDecalsEditor：
    //  EmeraldDecals コンポーネント（デカール生成・配置の補助）のインスペクタを拡張し、
    //  生成高さ・半径・遅延・寿命・候補リストなどの設定を見やすく編集できるようにするエディタクラス。
    public class EmeraldDecalsEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI用）")]
        GUIStyle FoldoutStyle;                   // フォールドアウトのスタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture EventsEditorIcon;                // インスペクタ上部アイコン

        [Header("対象フィールドの SerializedProperty 群（インスペクタと同期）")]
        SerializedProperty HideSettingsFoldout,  // 設定全体の非表示トグル
                          DecalsFoldout,         // 「デカール設定」セクションの折りたたみ
                          BloodEffects,          // デカール候補リスト
                          BloodSpawnHeight,      // 生成高さ
                          BloodSpawnDelay,       // 生成遅延（秒）
                          BloodSpawnRadius,      // 生成半径
                          BloodDespawnTime,      // デカールの寿命（秒）
                          OddsForBlood;          // 生成確率（%）

        /// <summary>
        /// （日本語）エディタ有効化時の初期化。アイコン読込と SerializedProperty のバインドを行う。
        /// </summary>
        void OnEnable()
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldDecals") as Texture; // ヘッダー用アイコンをロード
            InitializeProperties();                                                                                     // 対象プロパティを紐付け
        }

        /// <summary>
        /// （日本語）対象オブジェクト（EmeraldDecals）のシリアライズ済みフィールドを探し、プロパティを紐付ける。
        /// </summary>
        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout"); // 非表示トグル
            DecalsFoldout = serializedObject.FindProperty("DecalsFoldout");       // セクション折りたたみ
            BloodEffects = serializedObject.FindProperty("BloodEffects");        // 候補リスト
            BloodSpawnHeight = serializedObject.FindProperty("BloodSpawnHeight");    // 生成高さ
            BloodSpawnDelay = serializedObject.FindProperty("BloodSpawnDelay");     // 生成遅延
            BloodSpawnRadius = serializedObject.FindProperty("BloodSpawnRadius");    // 生成半径
            BloodDespawnTime = serializedObject.FindProperty("BloodDespawnTime");    // 寿命
            OddsForBlood = serializedObject.FindProperty("OddsForBlood");        // 生成確率
        }

        /// <summary>
        /// （日本語）インスペクタGUIの描画。ヘッダーと「デカール設定」セクションを表示する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();  // カスタムスタイル更新
            EmeraldDecals self = (EmeraldDecals)target;                  // 対象コンポーネント参照
            serializedObject.Update();                                   // 直列化オブジェクトを最新化

            // 見出し（英語 "Decals" → 日本語「デカール」へ差し替え）
            CustomEditorProperties.BeginScriptHeaderNew("デカール", EventsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)                          // 非表示でない場合のみ内容を描画
            {
                EditorGUILayout.Space();
                DecalSettings(self);                                      // デカール設定セクション
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                     // ヘッダー終了

            serializedObject.ApplyModifiedProperties();                   // 変更を適用
        }

        /// <summary>
        /// （日本語）デカール（血痕など）生成・配置の各種設定UIを描画する。
        /// </summary>
        void DecalSettings(EmeraldDecals self)
        {
            // セクション見出し（英語 "Decal Settings" → 日本語「デカール設定」）
            DecalsFoldout.boolValue = EditorGUILayout.Foldout(DecalsFoldout.boolValue, "デカール設定", true, FoldoutStyle);

            if (DecalsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox(); // 枠開始

                // タイトル＋説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "デカール設定",
                    "このセクションでは、AI がダメージを受けた際にスポーン・配置されるデカール（プレハブ）の挙動を制御します。",
                    true
                );

                // レンダーパイプラインに関する注意（英語→日本語）
                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null && !self.MessageDismissed)
                {
                    CustomEditorProperties.DisplayImportantMessage(
                        "このコンポーネントは URP または HDRP のデカール機能（または独自のデカール実装）と併用することを想定しています。本コンポーネント自体はデカールを生成しません。" +
                        "プレハブのデカールをスポーンし、位置合わせを行います。使用中の Render Pipeline Asset でデカールが有効になっていることを確認してください。"
                    );
                    // ボタン（英語 "Dismiss Message" → 日本語「メッセージを非表示」）
                    if (GUILayout.Button(new GUIContent("メッセージを非表示", "このメッセージを今後表示しません。"), GUILayout.Height(20)))
                    {
                        self.MessageDismissed = true; // 一度承認したら再表示しない
                    }
                    GUILayout.Space(15);
                }
                //if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name == )   // 原文のまま保持（将来の型判定の備忘）

                // 生成高さ
                EditorGUILayout.PropertyField(BloodSpawnHeight);
                CustomEditorProperties.CustomHelpLabelField(
                    "デカールのスポーン高さを制御します。傾斜地形でデカールが埋まる場合、ここを上げると改善することがあります。",
                    true
                );

                // 生成半径
                EditorGUILayout.PropertyField(BloodSpawnRadius);
                CustomEditorProperties.CustomHelpLabelField(
                    "AI の位置から、この半径内のランダム位置にデカールをスポーンします。",
                    true
                );

                // 生成遅延
                EditorGUILayout.PropertyField(BloodSpawnDelay);
                CustomEditorProperties.CustomHelpLabelField(
                    "ダメージが適用されてからデカールをスポーンするまでの遅延（秒）を設定します。",
                    true
                );

                // デスポーン時間
                EditorGUILayout.PropertyField(BloodDespawnTime);
                CustomEditorProperties.CustomHelpLabelField(
                    "スポーンされたデカールが消滅するまでの時間（秒）を設定します。",
                    true
                );

                GUILayout.Space(15);

                // デカール候補リスト
                CustomEditorProperties.CustomHelpLabelField(
                    "スポーン可能なデカール・プレハブの候補リストです。",
                    false
                );
                CustomEditorProperties.BeginIndent(15);
                EditorGUILayout.PropertyField(BloodEffects); // ReorderableList で並べ替え可能（Unity 標準の挙動）
                CustomEditorProperties.EndIndent();

                CustomEditorProperties.EndFoldoutWindowBox(); // 枠終了
            }
        }
    }
}
