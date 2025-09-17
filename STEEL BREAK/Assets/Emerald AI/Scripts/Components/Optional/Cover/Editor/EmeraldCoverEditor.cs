using UnityEngine;                    // Unity ランタイムAPI
using UnityEditor;                    // Unity エディタ拡張API（Editor 等）

namespace EmeraldAI.Utility           // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldCover))] // このエディタは EmeraldCover 用のカスタムインスペクタ
    [CanEditMultipleObjects]             // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldCoverEditor：
    //  EmeraldCover コンポーネントのインスペクタをカスタマイズして、
    //  カバー探索・距離・角度・時間などの各種設定を見やすく編集できるようにするエディタクラス。
    public class EmeraldCoverEditor : Editor
    {
        [Header("折りたたみ表示のスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                     // フォールドアウトの見た目を統一

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture CoverEditorIcon;                   // インスペクタ上部のアイコン

        [Header("設定の折りたたみフラグ（SettingsFoldout）")]
        SerializedProperty SettingsFoldout;        // 設定セクションの開閉状態

        [Header("全体非表示フラグ（HideSettingsFoldout）")]
        SerializedProperty HideSettingsFoldout;    // ヘッダー右側の非表示トグル

        [Header("最小カバー距離（MinCoverDistance）")]
        SerializedProperty MinCoverDistance;       // ターゲットからこれ未満のノードは不可

        [Header("最大移動距離（MaxTravelDistance）")]
        SerializedProperty MaxTravelDistance;      // カバー候補までの最長移動距離

        [Header("探索半径（CoverSearchRadius）")]
        SerializedProperty CoverSearchRadius;      // カバー候補を探す半径

        [Header("カバーノード探索のレイヤーマスク（CoverNodeLayerMask）")]
        SerializedProperty CoverNodeLayerMask;     // Overlap 用 LayerMask

        [Header("隠れる時間（最小値）HideSecondsMin")]
        SerializedProperty HideSecondsMin;         // しゃがみ/隠れの最短秒数

        [Header("隠れる時間（最大値）HideSecondsMax")]
        SerializedProperty HideSecondsMax;         // しゃがみ/隠れの最長秒数

        [Header("攻撃時間（最小値）AttackSecondsMin")]
        SerializedProperty AttackSecondsMin;       // ピーク時の攻撃最短秒数

        [Header("攻撃時間（最大値）AttackSecondsMax")]
        SerializedProperty AttackSecondsMax;       // ピーク時の攻撃最長秒数

        [Header("ピーク回数（最小値）PeakTimesMin")]
        SerializedProperty PeakTimesMin;           // 覗き込み最小回数

        [Header("ピーク回数（最大値）PeakTimesMax")]
        SerializedProperty PeakTimesMax;           // 覗き込み最大回数

        /// <summary>
        /// （日本語）エディタ有効化時の初期化処理。アイコンのロードと SerializedProperty のバインドを行う。
        /// </summary>
        void OnEnable()
        {
            // アイコンを一度だけ読み込み
            if (CoverEditorIcon == null) CoverEditorIcon = Resources.Load("Editor Icons/EmeraldCover") as Texture;
            InitializeProperties(); // 対象の SerializedProperty をフィールドへバインド
        }

        /// <summary>
        /// （日本語）対象オブジェクト（EmeraldCover）の各シリアライズ済みフィールドを探し、プロパティを紐付ける。
        /// </summary>
        void InitializeProperties()
        {
            // フラグ群
            SettingsFoldout = serializedObject.FindProperty("SettingsFoldout");           // 設定折りたたみ
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");   // 全体非表示

            // 探索/距離/レイヤー
            MinCoverDistance = serializedObject.FindProperty("MinCoverDistance");         // 最小距離
            MaxTravelDistance = serializedObject.FindProperty("MaxTravelDistance");       // 最大移動距離
            CoverSearchRadius = serializedObject.FindProperty("CoverSearchRadius");       // 探索半径
            CoverNodeLayerMask = serializedObject.FindProperty("CoverNodeLayerMask");     // レイヤーマスク

            // 時間・回数
            HideSecondsMin = serializedObject.FindProperty("HideSecondsMin");             // 隠れる最小秒
            HideSecondsMax = serializedObject.FindProperty("HideSecondsMax");             // 隠れる最大秒
            AttackSecondsMin = serializedObject.FindProperty("AttackSecondsMin");         // 攻撃最小秒
            AttackSecondsMax = serializedObject.FindProperty("AttackSecondsMax");         // 攻撃最大秒
            PeakTimesMin = serializedObject.FindProperty("PeakTimesMin");                 // ピーク回数最小
            PeakTimesMax = serializedObject.FindProperty("PeakTimesMax");                 // ピーク回数最大
        }

        /// <summary>
        /// （日本語）インスペクタGUIのメイン描画。ヘッダーと設定セクションの描画、適用を行う。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイルを更新
            serializedObject.Update();                                  // 直列化オブジェクトを最新に

            // ヘッダー開始（英語→日本語へ差し替え）
            CustomEditorProperties.BeginScriptHeaderNew("カバー", CoverEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue) // 非表示でなければ設定を描画
            {
                EditorGUILayout.Space();
                CoverSettings();                // 設定セクションの描画
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了

            serializedObject.ApplyModifiedProperties(); // 変更を適用
        }

        /// <summary>
        /// （日本語）カバー関連の全設定UIを描画する。
        /// </summary>
        void CoverSettings()
        {
            EmeraldCover self = (EmeraldCover)target; // 対象コンポーネント参照

            // セクション見出し（英語→日本語へ）
            SettingsFoldout.boolValue = EditorGUILayout.Foldout(
                SettingsFoldout.boolValue,
                "カバー設定",
                true,
                FoldoutStyle
            );

            if (SettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox(); // ボックス開始

                // タイトル + 説明（英語→日本語）
                CustomEditorProperties.TextTitleWithDescription(
                    "カバー設定",
                    "AI が戦闘中にカバーノードを探索・選択して遮蔽を取る動作を有効にします。1つのカバーノードを同時に使用できるのは1体のみです。" +
                    "カバーノードが見つからない場合、AI は現在ターゲットへの射線が通るよう、現在位置の周囲にウェイポイントを生成して移動を試みます。",
                    false
                );

                // 初回のみの重要メッセージ（英語→日本語）
                if (!self.ConfirmInfoMessage)
                {
                    CustomEditorProperties.DisplayImportantMessage(
                        "このコンポーネント使用中は、AI の『移動時（戦闘）回頭速度』が上書きされます。また、戦闘アクションの『ストレイフ』および『ランダム位置へ移動』は無視されます。" +
                        "\n\nAnimator Controller を v1.3.0 より前のバージョンで生成している場合は、Cover 用のステートを反映するために Animation Profile の Animator Controller を再生成してください。"
                    );

                    if (GUILayout.Button("了解")) // 「Okay」→「了解」
                    {
                        serializedObject.Update();
                        serializedObject.FindProperty("ConfirmInfoMessage").boolValue = true; // 了解フラグを立てる
                        serializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                // レイヤーマスク
                EditorGUILayout.PropertyField(CoverNodeLayerMask);
                CustomEditorProperties.CustomHelpLabelField("カバーノードの探索に使用するレイヤーマスクを指定します。", true);
                EditorGUILayout.Space();

                // 探索半径
                EditorGUILayout.PropertyField(CoverSearchRadius);
                CustomEditorProperties.CustomHelpLabelField("AI がカバーノードを検索する半径を指定します。", true);
                EditorGUILayout.Space();

                // 最小カバー距離
                EditorGUILayout.PropertyField(MinCoverDistance);
                CustomEditorProperties.CustomHelpLabelField("検出したターゲットから、カバーノードまで許容される最小距離です。これ未満のノードは候補になりません。", true);
                EditorGUILayout.Space();

                // 最大移動距離
                EditorGUILayout.PropertyField(MaxTravelDistance);
                CustomEditorProperties.CustomHelpLabelField("AI がカバーノードへ移動してよい最大距離です。これを超える候補は除外されます。", true);
                EditorGUILayout.Space();

                // ピーク回数（最小/最大）
                EditorGUILayout.PropertyField(PeakTimesMin);
                CustomEditorProperties.CustomHelpLabelField("現在のカバーノードで、AI が覗き込み（ピーク）ながら攻撃する最小回数を設定します。", false);

                EditorGUILayout.PropertyField(PeakTimesMax);
                CustomEditorProperties.CustomHelpLabelField("現在のカバーノードで、AI が覗き込み（ピーク）ながら攻撃する最大回数を設定します。", true);
                EditorGUILayout.Space();

                // 隠れる時間（最小/最大）
                EditorGUILayout.PropertyField(HideSecondsMin);
                CustomEditorProperties.CustomHelpLabelField("AI が現在のカバーノードで『隠れる』（しゃがむ）最小時間を設定します。", false);

                EditorGUILayout.PropertyField(HideSecondsMax);
                CustomEditorProperties.CustomHelpLabelField("AI が現在のカバーノードで『隠れる』（しゃがむ）最大時間を設定します。", true);
                EditorGUILayout.Space();

                // 攻撃時間（最小/最大）
                EditorGUILayout.PropertyField(AttackSecondsMin);
                CustomEditorProperties.CustomHelpLabelField("AI が立ち上がってから攻撃を継続する最短時間を設定します。しゃがみ姿勢からは攻撃できません。", false);

                EditorGUILayout.PropertyField(AttackSecondsMax);
                CustomEditorProperties.CustomHelpLabelField("AI が立ち上がってから攻撃を継続する最長時間を設定します。しゃがみ姿勢からは攻撃できません。", true);

                CustomEditorProperties.EndFoldoutWindowBox(); // ボックス終了
            }
        }
    }
}
