using UnityEngine;                                   // Unity ランタイムAPI
using UnityEditor;                                   // エディタ拡張API（Editor 等）
using EmeraldAI.Utility;                             // カスタムエディタ補助

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(EmeraldSoundDetector))]     // このカスタムインスペクタは EmeraldSoundDetector 用

    // 【クラス概要】EmeraldSoundDetectorEditor：
    //  EmeraldSoundDetector（サウンド検知コンポーネント）のインスペクタを拡張し、
    //  基本係数・しきい値・Unaware/Suspicious/Aware 各リアクションやイベントを
    //  日本語UIで設定できるようにするエディタクラス。
    public class EmeraldSoundDetectorEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（EditorGUI 用）")]
        GUIStyle FoldoutStyle;                        // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture SoundDetectionEditorIcon;             // インスペクタ上部のアイコン

        [Header("サウンド検知：各種係数/しきい値（SerializedProperty）")]
        SerializedProperty TargetVelocityFactorProp;  // 対象速度の影響度
        SerializedProperty FalloffDealyProp;          // 減衰までの遅延
        SerializedProperty DistanceFactorProp;        // 対象距離の影響度
        SerializedProperty MinVelocityThresholdProp;  // 最小速度しきい値
        SerializedProperty AttentionRateProp;         // 注意上昇率
        SerializedProperty AttentionFalloffProp;      // 注意減衰
        SerializedProperty AttractModifierCooldownProp; // アトラクトモディファイアのクールダウン
        SerializedProperty DelayUnawareSecondsProp;   // 未感知に戻すまでの遅延（秒）

        [Header("リアクション（未感知/疑念/警戒）オブジェクト参照")]
        SerializedProperty UnawareReactionProp;       // 未感知リアクション
        SerializedProperty SuspiciousReactionProp;    // 疑念リアクション
        SerializedProperty AwareReactionProp;         // 警戒リアクション

        [Header("しきい値（未感知/疑念/警戒）")]
        SerializedProperty UnawareThreatLevelProp;    // 未感知しきい値
        SerializedProperty SuspiciousThreatLevelProp; // 疑念しきい値
        SerializedProperty AwareThreatLevelProp;      // 警戒しきい値

        [Header("UnityEvent（未感知/疑念/警戒）")]
        SerializedProperty UnawareEventProp;          // 未感知イベント
        SerializedProperty SuspiciousEventProp;       // 疑念イベント
        SerializedProperty AwareEventProp;            // 警戒イベント

        [Header("インスペクタ表示の折りたたみ/非表示トグル")]
        SerializedProperty HideSettingsFoldoutProp,   // 全体非表示
                           SoundDetectorFoldoutProp,  // サウンド検知セクション
                           UnawareFoldoutProp,        // 未感知セクション
                           SuspiciousFoldoutProp,     // 疑念セクション
                           AwareFoldoutProp;          // 警戒セクション

        /// <summary>
        /// （日本語）エディタ有効化時：アイコンをロードし、各 SerializedProperty を対象フィールドへバインドします。
        /// </summary>
        private void OnEnable()
        {
            if (SoundDetectionEditorIcon == null) SoundDetectionEditorIcon = Resources.Load("Editor Icons/EmeraldSoundDetector") as Texture;

            TargetVelocityFactorProp = serializedObject.FindProperty("TargetVelocityFactor");
            FalloffDealyProp = serializedObject.FindProperty("FalloffDealy");
            DistanceFactorProp = serializedObject.FindProperty("DistanceFactor");
            MinVelocityThresholdProp = serializedObject.FindProperty("MinVelocityThreshold");
            AttentionRateProp = serializedObject.FindProperty("AttentionRate");
            AttentionFalloffProp = serializedObject.FindProperty("AttentionFalloff");
            DelayUnawareSecondsProp = serializedObject.FindProperty("DelayUnawareSeconds");
            AttractModifierCooldownProp = serializedObject.FindProperty("AttractModifierCooldown");

            UnawareThreatLevelProp = serializedObject.FindProperty("UnawareThreatLevel");
            SuspiciousThreatLevelProp = serializedObject.FindProperty("SuspiciousThreatLevel");
            AwareThreatLevelProp = serializedObject.FindProperty("AwareThreatLevel");

            UnawareEventProp = serializedObject.FindProperty("UnawareEvent");
            SuspiciousEventProp = serializedObject.FindProperty("SuspiciousEvent");
            AwareEventProp = serializedObject.FindProperty("AwareEvent");

            UnawareReactionProp = serializedObject.FindProperty("UnawareReaction");
            SuspiciousReactionProp = serializedObject.FindProperty("SuspiciousReaction");
            AwareReactionProp = serializedObject.FindProperty("AwareReaction");

            HideSettingsFoldoutProp = serializedObject.FindProperty("HideSettingsFoldout");
            SoundDetectorFoldoutProp = serializedObject.FindProperty("SoundDetectorFoldout");
            UnawareFoldoutProp = serializedObject.FindProperty("UnawareFoldout");
            SuspiciousFoldoutProp = serializedObject.FindProperty("SuspiciousFoldout");
            AwareFoldoutProp = serializedObject.FindProperty("AwareFoldout");
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。ヘッダーと各セクションを日本語UIで表示します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldSoundDetector self = (EmeraldSoundDetector)target;

            serializedObject.Update();

            // ヘッダー（英語 "Sound Detector" → 日本語「サウンド検知」）
            CustomEditorProperties.BeginScriptHeaderNew("サウンド検知", SoundDetectionEditorIcon, new GUIContent(), HideSettingsFoldoutProp);

            if (!HideSettingsFoldoutProp.boolValue)
            {
                EditorGUILayout.Space();
                SoundDetectorSettings(self);       // 基本設定
                EditorGUILayout.Space();
                UnawareSettings();                 // 未感知
                EditorGUILayout.Space();
                SuspiciousSettings();              // 疑念
                EditorGUILayout.Space();
                AwareSettings();                   // 警戒
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// （日本語）サウンド検知の基本設定（係数やしきい値）を編集します。
        /// </summary>
        void SoundDetectorSettings(EmeraldSoundDetector self)
        {
            // フォールドアウト（英語 "Sound Detector Settings" → 日本語）
            SoundDetectorFoldoutProp.boolValue = EditorGUILayout.Foldout(SoundDetectorFoldoutProp.boolValue, "サウンド検知の設定", true, FoldoutStyle);

            if (SoundDetectorFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "サウンド検知の設定",
                    "Sound Detector コンポーネントは、AI に“聞く”能力を付与します。プレイヤーや外部ソースによる移動/音が発生すると、" +
                    "Reaction Object をトリガーして AI の挙動（注視・移動・警戒など）を決定します。Reaction はユーザーが自由にカスタマイズ可能です。",
                    false);

                // もとの英語 HelpBox を日本語へ
                EditorGUILayout.HelpBox(
                    "AI は『敵対関係（Enemy）』にあるプレイヤーのみを“聞きます”。この判定に用いるタグとレイヤーは、AI 本体の Emerald AI（Detection Settings）の設定に基づきます。",
                    MessageType.Info);

                GUILayout.Space(10);

                DisplayThreatLevel(self); // 現在値の可視化

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionRateProp, "注意上昇率", 0.0025f, 1.0f);
                CustomHelpLabelField("検知対象の速度が最小しきい値以上のとき、Current Threat Amount（現在の脅威量）が増える基本速度です。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), MinVelocityThresholdProp, "最小速度しきい値", 0f, 10f);
                CustomHelpLabelField("音として扱う最小移動速度。これ未満は注意減衰の対象となり、検知されません。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), TargetVelocityFactorProp, "対象速度の影響度", 0f, 1f);
                CustomHelpLabelField("検知対象の速度が注意上昇率へ与える影響度を制御します。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DistanceFactorProp, "対象距離の影響度", 0f, 1f);
                CustomHelpLabelField("対象までの距離が注意上昇率へ与える影響度を制御します。近いほど影響が大きくなります。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionFalloffProp, "注意減衰", 0.0025f, 1.0f);
                CustomHelpLabelField("検知対象の速度が最小しきい値を下回っている間に、Current Threat Amount をどれくらいの速さで減少させるか。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FalloffDealyProp, "減衰までの遅延", 0, 10f);
                CustomHelpLabelField("すべての検知対象の速度が最小しきい値を下回ってから、注意減衰が開始されるまでの秒数。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttractModifierCooldownProp, "アトラクトモディファイアのクールダウン", 1f, 25f);
                CustomHelpLabelField("一度 Attract Modifier を検知した後、再び検知できるようになるまでの待機秒数。", true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）未感知（Unaware）状態の設定。
        /// </summary>
        void UnawareSettings()
        {
            // 見出し（英語 "Unaware Settings" → 日本語）
            UnawareFoldoutProp.boolValue = EditorGUILayout.Foldout(UnawareFoldoutProp.boolValue, "未感知（Unaware）設定", true, FoldoutStyle);

            if (UnawareFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "未感知（Unaware）設定",
                    "未感知リアクションは『疑念』または『警戒』になった後でのみ発火します。ターゲットを見失ったり静かすぎる場合などに、" +
                    "Reaction Object で変更した設定を初期状態へ戻す用途に適しています。",
                    false);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), UnawareThreatLevelProp, "未感知しきい値", 0.0f, 1f);
                CustomHelpLabelField("未感知状態へ戻るために必要な Current Threat Amount の値を指定します。", false);

                GUILayout.Space(15);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DelayUnawareSecondsProp, "未感知に戻すまでの遅延（秒）", 0f, 25f);
                CustomHelpLabelField("未感知しきい値を満たした後、実際に未感知を適用するまでの待機秒数。", false);

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(UnawareReactionProp, new GUIContent("未感知リアクション"));
                CustomHelpLabelField("この状態で使用する Reaction Object。複数の AI で共有可能です。", false);

                GUILayout.Space(15);
                CustomHelpLabelField("未感知イベント：AI が未感知になったときに呼ばれるカスタムイベント群です。", false);
                EditorGUILayout.PropertyField(UnawareEventProp, new GUIContent("未感知イベント"));

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）疑念（Suspicious）状態の設定。
        /// </summary>
        void SuspiciousSettings()
        {
            // 見出し（英語 "Suspicious Settings" → 日本語）
            SuspiciousFoldoutProp.boolValue = EditorGUILayout.Foldout(SuspiciousFoldoutProp.boolValue, "疑念（Suspicious）設定", true, FoldoutStyle);

            if (SuspiciousFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "疑念（Suspicious）設定",
                    "疑念リアクションは『疑念しきい値』到達時に 1 回だけ発火します。以後は AI が交戦するか、未感知しきい値に達するまで再発火しません。",
                    false);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), SuspiciousThreatLevelProp, "疑念しきい値", 0.0f, 1f);
                CustomHelpLabelField("疑念状態に到達するために必要な Current Threat Amount。", false);

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(SuspiciousReactionProp, new GUIContent("疑念リアクション"));
                CustomHelpLabelField("この状態で使用する Reaction Object。複数の AI で共有可能です。", false);

                GUILayout.Space(15);
                CustomHelpLabelField("疑念イベント：AI が疑念状態に達したときに呼ばれるカスタムイベント群です。", false);
                EditorGUILayout.PropertyField(SuspiciousEventProp, new GUIContent("疑念イベント"));

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）警戒（Aware）状態の設定。
        /// </summary>
        void AwareSettings()
        {
            // 見出し（英語 "Aware Settings" → 日本語）
            AwareFoldoutProp.boolValue = EditorGUILayout.Foldout(AwareFoldoutProp.boolValue, "警戒（Aware）設定", true, FoldoutStyle);

            if (AwareFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "警戒（Aware）設定",
                    "警戒リアクションは『警戒しきい値』到達時に 1 回だけ発火します。以後は AI が交戦するか、未感知しきい値に達するまで再発火しません。",
                    false);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AwareThreatLevelProp, "警戒しきい値", 0.0f, 1f);
                CustomHelpLabelField("警戒状態に到達するために必要な Current Threat Amount。", false);

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(AwareReactionProp, new GUIContent("警戒リアクション"));
                CustomHelpLabelField("この状態で使用する Reaction Object。複数の AI で共有可能です。", false);

                GUILayout.Space(15);
                CustomHelpLabelField("警戒イベント：AI が警戒状態に達したときに呼ばれるカスタムイベント群です。", false);
                EditorGUILayout.PropertyField(AwareEventProp, new GUIContent("警戒イベント"));

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）現在の脅威レベルと脅威量（プログレスバー）を表示します。
        /// </summary>
        void DisplayThreatLevel(EmeraldSoundDetector self)
        {
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box

            DisplayTitle("情報"); //Title

            CustomHelpLabelField("現在の脅威レベル: " + self.CurrentThreatLevel.ToString(), false);

            Rect r = EditorGUILayout.BeginVertical();
            r.height = 25;
            EditorGUI.ProgressBar(r, self.CurrentThreatAmount, "現在の脅威量: " + (Mathf.Round(self.CurrentThreatAmount * 100f) / 100f).ToString());
            EditorGUILayout.EndVertical();
            GUILayout.Space(35);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);
        }

        /// <summary>
        /// （日本語）ヘルプボックス風の説明テキストを表示します。
        /// </summary>
        void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
            EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// （日本語）枠タイトルを太字で表示します。
        /// </summary>
        void DisplayTitle(string Title)
        {
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
    }
}
