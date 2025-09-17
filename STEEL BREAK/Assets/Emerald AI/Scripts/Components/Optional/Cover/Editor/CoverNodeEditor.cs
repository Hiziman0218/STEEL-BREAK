using UnityEngine;                    // Unity のランタイムAPI
using UnityEditor;                    // エディタ拡張API（Editor, SerializedProperty など）

namespace EmeraldAI.Utility           // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(CoverNode))] // このエディタは CoverNode 用のカスタムインスペクタ
    [CanEditMultipleObjects]          // 複数選択編集を許可

    // 【クラス概要】CoverNodeEditor：
    //  CoverNode コンポーネント用のカスタムインスペクタを提供するエディタ拡張クラス。
    //  ノード種別・視線補助・FOV角度・Gizmo色などの設定UIをまとめ、説明テキストも表示する。
    public class CoverNodeEditor : Editor
    {
        // --- メンバ変数（SerializedPropertyやエディタ用リソース） ---
        [Header("カバー動作の種類（CoverType）へバインドするSerializedProperty")]
        SerializedProperty CoverType;                     // CoverNode.CoverType

        [Header("（未使用）障害物のない位置を探索するフラグ（LookForUnobstructedPosition）")]
        SerializedProperty LookForUnobstructedPosition;   // ※初期化のみ。UIでは未使用（元実装のまま保持）

        [Header("ターゲットが見えない場合に視線が通る位置を探すフラグ（GetLineOfSightPosition）")]
        SerializedProperty GetLineOfSightPosition;        // CoverNode.GetLineOfSightPosition

        [Header("カバーノードの視界角リミット（CoverAngleLimit, 60～180度）")]
        SerializedProperty CoverAngleLimit;               // CoverNode.CoverAngleLimit

        [Header("垂直方向ガイド（矢印）Gizmo の色（ArrowColor）")]
        SerializedProperty ArrowColor;                    // CoverNode.ArrowColor

        [Header("カバーノード本体（球）Gizmo の色（NodeColor）")]
        SerializedProperty NodeColor;                     // CoverNode.NodeColor

        [Header("インスペクタ上部ヘッダー用のアイコンテクスチャ")]
        Texture NodeEditorIcon;                           // Resources から読み込むエディタ用アイコン

        void OnEnable()                                   // エディタ有効化時に呼ばれる
        {
            if (NodeEditorIcon == null)                  // アイコン未ロードなら
                NodeEditorIcon = Resources.Load("Editor Icons/EmeraldCover") as Texture; // 指定パスからロード
            InitializeProperties();                      // SerializedProperty の紐付けを初期化
        }

        void InitializeProperties()                      // 対象オブジェクトの各プロパティへバインド
        {
            CoverType = serializedObject.FindProperty("CoverType");                               // 種別
            LookForUnobstructedPosition = serializedObject.FindProperty("LookForUnobstructedPosition"); // 未使用（元実装通り）
            GetLineOfSightPosition = serializedObject.FindProperty("GetLineOfSightPosition");     // 視線補助フラグ
            CoverAngleLimit = serializedObject.FindProperty("CoverAngleLimit");                   // 角度制限
            ArrowColor = serializedObject.FindProperty("ArrowColor");                             // 矢印色
            NodeColor = serializedObject.FindProperty("NodeColor");                               // ノード色
        }

        public override void OnInspectorGUI()            // カスタムインスペクタの描画
        {
            serializedObject.Update();                   // 直列化オブジェクトを更新（同期）

            // 見出し（タイトルとアイコン）※英語から日本語へ差し替え
            CustomEditorProperties.BeginScriptHeader("カバーノード", NodeEditorIcon);

            EditorGUILayout.Space();                     // 余白
            CoverNodeSettings();                         // 設定UIの描画
            EditorGUILayout.Space();                     // 余白

            serializedObject.ApplyModifiedProperties();  // 変更の適用

            CustomEditorProperties.EndScriptHeader();    // 見出しの終了
        }

        void CoverNodeSettings()                         // CoverNode 設定セクション
        {
            CoverNode self = (CoverNode)target;         // 対象コンポーネント参照を取得

            CustomEditorProperties.BeginFoldoutWindowBox(); // 折りたたみボックス開始

            // セクションタイトルと説明（英語→日本語）
            CustomEditorProperties.TextTitleWithDescription(
                "カバーノード設定",
                "このカバーノード使用時のAIの振る舞いと、Gizmo（可視化）の色を設定します。",
                true);

            // カバータイプの選択
            EditorGUILayout.PropertyField(CoverType);

            // カバータイプ別の説明（英語→日本語に差し替え）
            if (self.CoverType == CoverTypes.CrouchAndPeak)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Crouch and Peak（しゃがみ＆ピーク）：生成された「隠れる秒数」の間しゃがみ、そこから立ち上がって覗き込み（ピーク）ます。ピーク回数は生成された「ピーク回数」に基づき、各ピーク中は生成された「攻撃秒数」の間攻撃します。",
                    false);
            }
            else if (self.CoverType == CoverTypes.CrouchOnce)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Crouch Once（1回だけしゃがむ）：生成された「隠れる秒数」の間だけ一度しゃがみ、そこから立ち上がります。立っている間は生成された「攻撃秒数」の間攻撃します。",
                    false);
            }
            else if (self.CoverType == CoverTypes.Stand)
            {
                CustomEditorProperties.CustomHelpLabelField(
                    "Stand（立ち）：このカバーポイントから継続的に立った状態を維持します。立っている間は生成された「攻撃秒数」の間攻撃します。",
                    false);
            }

            // 重要メッセージ（英語→日本語）
            CustomEditorProperties.DisplayImportantMessage(
                "上記の一部設定は、AI本体の Cover コンポーネント側の設定に依存します。");

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // 視線位置の補正フラグ
            EditorGUILayout.PropertyField(GetLineOfSightPosition);
            CustomEditorProperties.CustomHelpLabelField(
                "現在のカバーノードでターゲットが視認できない場合、攻撃可能な未遮蔽位置を一時的に生成して移動するかどうかを制御します。",
                false);
            if (self.GetLineOfSightPosition == YesOrNo.Yes)
                CustomEditorProperties.DisplayImportantMessage("この設定により、AIは現在のカバーノードから離れて位置調整を行う場合があります。");

            EditorGUILayout.Space();

            // 角度リミット
            EditorGUILayout.PropertyField(CoverAngleLimit);
            CustomEditorProperties.CustomHelpLabelField(
                "このカバーノードの角度制限を設定します。ターゲットはこの範囲内にいる必要があります（シーン上の緑色エリアで可視化されます）。",
                true);

            // 矢印Gizmo色
            EditorGUILayout.PropertyField(ArrowColor);
            CustomEditorProperties.CustomHelpLabelField(
                "垂直方向ガイド（矢印）Gizmo の色を設定します。",
                true);

            // ノードGizmo色
            EditorGUILayout.PropertyField(NodeColor);
            CustomEditorProperties.CustomHelpLabelField(
                "カバーノード（球体）Gizmo の色を設定します。",
                true);

            CustomEditorProperties.EndFoldoutWindowBox(); // 折りたたみボックス終了
        }
    }
}
