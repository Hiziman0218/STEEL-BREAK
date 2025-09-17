using System.Collections;                          // （保持）コルーチン関連
using System.Collections.Generic;                  // （保持）汎用コレクション
using UnityEngine;                                 // Unity ランタイムAPI
using UnityEditor;                                 // エディタ拡張API（Editor など）
using UnityEditorInternal;                         // ReorderableList など（本ファイルでは直接未使用だが原文保持）

namespace EmeraldAI.Utility                        // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldFootsteps))]       // このカスタムインスペクタは EmeraldFootsteps 用
    [CanEditMultipleObjects]                       // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldFootstepsEditor：
    //  EmeraldFootsteps コンポーネント（足音の判定・効果・音再生）をインスペクタ上で編集しやすくするためのエディタ拡張。
    //  フットステップ設定・サーフェス設定（Footstep Surface Object）のUIを提供し、警告表示や自動ボーン取得も行う。
    public class EmeraldFootstepsEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                      // フォールドアウトの見た目

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture FootstepsEditorIcon;                // インスペクタ上部のアイコン

        //Bools
        [Header("エディタ表示制御フラグ（SerializedProperty）")]
        SerializedProperty HideSettingsFoldout,     // 設定全体の非表示トグル
                          FootstepsFoldout,         // 「フットステップ設定」セクションの開閉
                          SurfaceFoldout;           // 「サーフェス設定」セクションの開閉

        //Variables
        [Header("対象プロパティ（SerializedProperty）")]
        SerializedProperty FootstepSurfaces,        // Footstep Surface Object のリスト
                          IgnoreLayers,             // 無視レイヤー（計算から除外）
                          FeetTransforms;           // AI の足の Transform 群

        /// <summary>
        /// （日本語）エディタ有効化時の初期化。アイコンのロードおよび SerializedProperty のバインドを行う。
        /// </summary>
        void OnEnable()
        {
            if (FootstepsEditorIcon == null) FootstepsEditorIcon = Resources.Load("Editor Icons/EmeraldFootsteps") as Texture; // ヘッダー用アイコン
            InitializeProperties();                                                                                             // 各プロパティへ紐付け
        }

        /// <summary>
        /// （日本語）対象オブジェクト（EmeraldFootsteps）のシリアライズ済みフィールドを取得し、プロパティへバインドする。
        /// </summary>
        void InitializeProperties()
        {
            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout"); // 非表示トグル
            FootstepsFoldout = serializedObject.FindProperty("FootstepsFoldout");    // フットステップ設定の折りたたみ
            SurfaceFoldout = serializedObject.FindProperty("SurfaceFoldout");      // サーフェス設定の折りたたみ

            //Variables
            FootstepSurfaces = serializedObject.FindProperty("FootstepSurfaces");       // Surface Object リスト
            IgnoreLayers = serializedObject.FindProperty("IgnoreLayers");           // 無視するレイヤー
            FeetTransforms = serializedObject.FindProperty("FeetTransforms");         // 足ボーンの Transform 群
        }

        /// <summary>
        /// （日本語）未設定時の警告メッセージを表示する。
        /// </summary>
        void DisplayWarningMessages(EmeraldFootsteps self)
        {
            if (self.FeetTransforms.Count == 0)
            {
                // 英文→日本語に置換
                CustomEditorProperties.DisplaySetupWarning("Feet Transforms のリストが空です。AI の足の Transform を Feet Transforms リストに追加してください。");
            }
            else if (self.FootstepSurfaces.Count == 0)
            {
                // 英文→日本語に置換
                CustomEditorProperties.DisplaySetupWarning("Footstep Surfaces のリストが空です。少なくとも 1 つの Footstep Surface Object を割り当ててください。");
            }
        }

        /// <summary>
        /// （日本語）インスペクタGUIのメイン描画。ヘッダー、警告、各セクションを表示する。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            EmeraldFootsteps self = (EmeraldFootsteps)target;           // 対象コンポーネント
            serializedObject.Update();                                  // 直列化オブジェクト同期

            // 英語 "Footsteps" → 日本語「フットステップ」
            CustomEditorProperties.BeginScriptHeaderNew("フットステップ", FootstepsEditorIcon, new GUIContent(), HideSettingsFoldout);

            DisplayWarningMessages(self);                               // 必要に応じて警告表示

            if (!HideSettingsFoldout.boolValue)                         // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                FootstepSettings(self);                                 // フットステップ設定
                EditorGUILayout.Space();
                SurfaceSettings(self);                                  // サーフェス設定
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                   // ヘッダー終了

            serializedObject.ApplyModifiedProperties();                 // 変更を適用
        }

        /// <summary>
        /// （日本語）フットステップ全般の設定UIを描画する。
        /// </summary>
        void FootstepSettings(EmeraldFootsteps self)
        {
            // 英語 "Footstep Settings" → 日本語「フットステップ設定」
            FootstepsFoldout.boolValue = EditorGUILayout.Foldout(FootstepsFoldout.boolValue, "フットステップ設定", true, FoldoutStyle);

            if (FootstepsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 英語 "Foostep Settings" → 日本語「フットステップ設定」
                CustomEditorProperties.TextTitleWithDescription("フットステップ設定", "Footsteps コンポーネントの各種設定を制御します。", false);

                // 英語→日本語（チュートリアルリンクはそのまま）
                CustomEditorProperties.ImportantTutorialButton(
                    "足音を発生させるには、AI のアニメーションに Footstep 用の Animation Event を作成する必要があります。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/footsteps-component/setting-up-the-footsteps-component"
                );
                EditorGUILayout.Space();

                // ラベルとツールチップを日本語へ
                CustomEditorProperties.CustomPropertyField(
                    IgnoreLayers,
                    "無視するレイヤー",
                    "フットステップ計算時に無視するレイヤーを指定します。AI 自身のレイヤーと、LBD のコライダーレイヤーは実行時に自動で含まれます。",
                    true
                );
                EditorGUILayout.Space();

                // 英語 "Auto Grab Feet Transforms" → 日本語「足のトランスフォームを自動取得」
                if (GUILayout.Button(new GUIContent("足のトランスフォームを自動取得", "AI の足ボーン（Transform）を自動的に探索して追加します。")))
                {
                    GetFeetTransforms(self); // ボーン自動取得
                }

                EditorGUILayout.Space();
                CustomEditorProperties.BeginIndent(12);
                // 英語→日本語
                CustomEditorProperties.CustomHelpLabelField(
                    "この AI の足を表す Transform 群を制御します。これらの位置から下方向にレイキャストし、足音の接地面を判定します。Step Effects を使用している場合、" +
                    "最も地面に近い足の位置にエフェクトをスポーンします。",
                    false
                );
                EditorGUILayout.PropertyField(FeetTransforms); // Transform リスト
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）Footstep Surface Object（サーフェス別の音/エフェクト設定）関連の UI を描画する。
        /// </summary>
        void SurfaceSettings(EmeraldFootsteps self)
        {
            // 英語 "Surface Settings" → 日本語「サーフェス設定」
            SurfaceFoldout.boolValue = EditorGUILayout.Foldout(SurfaceFoldout.boolValue, "サーフェス設定", true, FoldoutStyle);

            if (SurfaceFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 英語→日本語
                CustomEditorProperties.TextTitleWithDescription(
                    "サーフェス設定",
                    "使用する Footstep Surface Object を制御します。Footstep Surface Object は Project タブで右クリックし、" +
                    "Create > Emerald AI > Footstep Surface Object から作成できます。",
                    true
                );

                // 英語→日本語
                CustomEditorProperties.CustomHelpLabelField(
                    "Footstep Surfaces のリストです。タグやテクスチャ情報から再生する足音やエフェクトを決定します。",
                    false
                );
                CustomEditorProperties.BeginIndent(12);
                EditorGUILayout.PropertyField(FootstepSurfaces); // Surface Object リスト
                EditorGUILayout.Space();
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）AI の子階層から「foot/Foot/FOOT」を含む Transform を探索し、FeetTransforms に追加する。
        /// IK などを含む名称（ik/Ik/IK）および "Foot Collider" は除外する。
        /// </summary>
        void GetFeetTransforms(EmeraldFootsteps self)
        {
            // まずは全階層を総当たりで「foot」を含む Transform を探索
            foreach (Transform t in self.GetComponentsInChildren<Transform>())
            {
                if (t.name.Contains("foot") || t.name.Contains("Foot") || t.name.Contains("FOOT")) // AI 全 Transform から "foot" を含むものを探す
                {
                    // IK や "Foot Collider" を含むものは除外（通常はボーンではないため）
                    if (!t.name.Contains("ik") && !t.name.Contains("Ik") && !t.name.Contains("IK") && !t.name.Contains("Foot Collider"))
                    {
                        if (!self.FeetTransforms.Contains(t))
                        {
                            self.FeetTransforms.Add(t); // 未登録なら追加
                        }
                    }
                }
            }

            // 追加探索：root/Root/ROOT という名前の子を 0〜3 の範囲で探索し、その配下からも "foot" を探す
            foreach (Transform root in self.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT") // 3つまでの子インデックスで root 名を探索
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            if (t.name.Contains("foot") || t.name.Contains("Foot") || t.name.Contains("FOOT")) // "foot" を含む Transform を探索
                            {
                                // IK と "Foot Collider" は除外
                                if (!t.name.Contains("ik") && !t.name.Contains("Ik") && !t.name.Contains("IK") && !t.name.Contains("Foot Collider"))
                                {
                                    if (!self.FeetTransforms.Contains(t))
                                    {
                                        self.FeetTransforms.Add(t); // 未登録なら追加
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
