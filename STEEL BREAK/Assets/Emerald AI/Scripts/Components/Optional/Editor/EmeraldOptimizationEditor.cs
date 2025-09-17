using System.Collections;                               // （保持）コルーチン関連
using System.Collections.Generic;                       // （保持）汎用コレクション
using UnityEngine;                                      // Unity ランタイムAPI
using UnityEditor;                                      // エディタ拡張API
using System.Linq;                                      // （保持）LINQ
using UnityEditorInternal;                              // ReorderableList 等（本ファイルでは未使用だが原文保持）

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldOptimization))]         // このエディタは EmeraldOptimization 用
    [CanEditMultipleObjects]                            // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldOptimizationEditor：
    //  EmeraldOptimization コンポーネントのインスペクタをカスタマイズし、
    //  画面外/カリング時の最適化、ディレイ、メッシュタイプ（単一Skinned Mesh or LODGroup）、
    //  レンダラー参照などを日本語UIで設定できるようにするエディタ拡張クラス。
    public class EmeraldOptimizationEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                           // フォールドアウトの外観

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture OptimizationEditorIcon;                  // インスペクタ上部のアイコン

        #region SerializedProperties
        //Bool
        [Header("エディタ表示制御フラグ（SerializedProperty）")]
        SerializedProperty HideSettingsFoldout,          // 全体非表示トグル
                          OptimizationFoldout;           // 「最適化設定」セクションの開閉

        //Int
        [Header("無効化ディレイ秒（Int）")]
        SerializedProperty DeactivateDelayProp;          // 非表示/カリング後に無効化するまでの秒数

        //Object
        [Header("AI メインレンダラー参照（Skinned Mesh Renderer）")]
        SerializedProperty AIRendererProp;               // 単一メッシュ時に必要

        //Enum
        [Header("最適化に関する列挙型プロパティ")]
        SerializedProperty OptimizeAIProp,               // 画面外/カリング時に最適化を行うか
                          UseDeactivateDelayProp,        // 無効化までのディレイを使うか
                          TotalLODsProp,                 // 参照（内部用：Total LODs）
                          MeshTypeProp;                  // メッシュタイプ（SingleMesh / LODGroup）
        #endregion

        /// <summary>
        /// （日本語）エディタ有効化時：アイコンをロードし、各 SerializedProperty を対象フィールドへバインドする。
        /// </summary>
        void OnEnable()
        {
            if (OptimizationEditorIcon == null) OptimizationEditorIcon = Resources.Load("Editor Icons/EmeraldOptimization") as Texture; // ヘッダー用アイコン

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");   // 非表示トグル
            OptimizationFoldout = serializedObject.FindProperty("OptimizationFoldout");   // セクション折りたたみ

            //Int
            DeactivateDelayProp = serializedObject.FindProperty("DeactivateDelay");       // 無効化ディレイ

            //Object
            AIRendererProp = serializedObject.FindProperty("AIRenderer");                 // メインレンダラー

            //Enum
            OptimizeAIProp = serializedObject.FindProperty("OptimizeAI");                 // 最適化ON/OFF
            UseDeactivateDelayProp = serializedObject.FindProperty("UseDeactivateDelay"); // ディレイ使用
            TotalLODsProp = serializedObject.FindProperty("TotalLODsRef");               // LOD合計（参照用）
            MeshTypeProp = serializedObject.FindProperty("MeshType");                     // メッシュタイプ
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。ヘッダー、注意喚起、セクションUIを表示する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();                   // カスタムスタイル更新
            EmeraldOptimization self = (EmeraldOptimization)target;                       // 対象コンポーネント参照
            serializedObject.Update();                                                    // 直列化オブジェクトを最新化

            // ヘッダー：英語 "Optimization" → 日本語「最適化」
            CustomEditorProperties.BeginScriptHeaderNew("最適化", OptimizationEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingRendererMessage(self);                                                // 必須参照の不足を警告

            if (!HideSettingsFoldout.boolValue)                                          // 非表示でなければ中身を描画
            {
                EditorGUILayout.Space();
                OptimizationSettings(self);                                              // 最適化設定
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();                                   // 変更適用
            CustomEditorProperties.EndScriptHeader();                                     // ヘッダー終了
        }

        /// <summary>
        /// （日本語）Single Mesh タイプで最適化が有効なのに AIRenderer が未割当の場合に警告を表示する。
        /// </summary>
        void MissingRendererMessage(EmeraldOptimization self)
        {
            if (self.OptimizeAI == YesOrNo.Yes && self.MeshType == EmeraldOptimization.MeshTypes.SingleMesh && !self.AIRenderer)
            {
                // 英文→日本語に差し替え
                CustomEditorProperties.DisplayWarningMessage(
                    "Single Mesh タイプを使用する場合、AI には Renderer の割り当てが必要です。最適化設定の『AI メインレンダラー』に AI の Skinned Mesh Renderer を割り当ててください。"
                );
            }
        }

        /// <summary>
        /// （日本語）最適化設定セクションの UI を描画する。
        /// </summary>
        void OptimizationSettings(EmeraldOptimization self)
        {
            // 見出し：英語 "Optimization Settings" → 日本語「最適化設定」
            OptimizationFoldout.boolValue = EditorGUILayout.Foldout(OptimizationFoldout.boolValue, "最適化設定", true, FoldoutStyle);

            if (OptimizationFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // タイトル＆説明：英語→日本語
                CustomEditorProperties.TextTitleWithDescription(
                    "最適化設定",
                    "最適化コンポーネントは、AI のモデルがカリング（または画面外）になった際に、特定のスクリプト・機能・アニメーションを無効化して処理負荷を削減します。",
                    true
                );

                // ラベルを日本語に（挙動は同じ）
                EditorGUILayout.PropertyField(OptimizeAIProp, new GUIContent("AI を最適化"));
                CustomEditorProperties.CustomHelpLabelField("画面外またはカリング時に、この AI を最適化するかどうかを制御します。", true);

                if (self.OptimizeAI == YesOrNo.Yes)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    // ディレイ使用切替（日本語ラベル）
                    EditorGUILayout.PropertyField(UseDeactivateDelayProp, new GUIContent("無効化ディレイを使用"));
                    CustomEditorProperties.CustomHelpLabelField("『画面外で無効化』機能にディレイを設けるかを制御します。No の場合、AI は即座に無効化されます。", true);

                    if (self.UseDeactivateDelay == YesOrNo.Yes)
                    {
                        // スライダー名を日本語に
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), DeactivateDelayProp, "無効化までの遅延", 1, 30);
                        CustomEditorProperties.CustomHelpLabelField("AI がカリングまたは画面外になってから無効化されるまでの秒数を制御します。", true);
                    }

                    // メッシュタイプ（日本語ラベル/説明）
                    CustomEditorProperties.CustomPropertyField(MeshTypeProp, "メッシュタイプ", "AI が単一の Skinned Mesh Renderer を使用するか、LOD Group を使用するかを制御します。", true);

                    if (self.MeshType == EmeraldOptimization.MeshTypes.LODGroup)
                    {
                        // 情報メッセージ（英語→日本語）
                        CustomEditorProperties.DisplayImportantMessage(
                            "情報：LOD Group オプションを使用するには、AI に LOD Group コンポーネントがアタッチされ、少なくとも 1 つ以上の LOD レベルが必要です。" +
                            "各レベルには 1 つ以上のメッシュが割り当てられている必要があります。最適化コンポーネントは Start 時に LOD Group から必要な情報を自動取得します。" +
                            "要件を満たしていない場合、最適化コンポーネントは無効化されます。"
                        );
                        EditorGUILayout.Space();
                    }

                    if (self.MeshType == EmeraldOptimization.MeshTypes.SingleMesh)
                    {
                        CustomEditorProperties.BeginIndent();
                        // ラベルを日本語に
                        EditorGUILayout.PropertyField(AIRendererProp, new GUIContent("AI メインレンダラー"));
                        CustomEditorProperties.CustomHelpLabelField(
                            "AI のメインレンダラーは、AI が使用する単一の Skinned Mesh Renderer を指します。複数の Skinned Mesh Renderer を持つ場合は、Single Mesh ではなく LOD Group を使用してください。",
                            true
                        );
                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
