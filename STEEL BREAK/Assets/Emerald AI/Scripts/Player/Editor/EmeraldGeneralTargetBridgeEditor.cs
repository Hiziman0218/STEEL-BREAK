using UnityEngine;
using UnityEditor;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldGeneralTargetBridgeEditor】
    /// Non-AI（プレイヤー等を含む）ターゲット用ブリッジコンポーネントの
    /// インスペクタをカスタマイズするエディタ。
    /// ・開始体力や不死フラグ、ダメージ/死亡イベントを編集
    /// ・簡易ヘルスバーを表示
    /// </summary>
    [CustomEditor(typeof(EmeraldGeneralTargetBridge))]
    [CanEditMultipleObjects]
    public class EmeraldGeneralTargetBridgeEditor : Editor
    {
        [Header("Foldout（折りたたみ）タイトルのスタイル（内部用）")]
        GUIStyle FoldoutStyle;

        [Header("ヘルス関連のエディタ用アイコン（内部用）")]
        Texture HealthEditorIcon;

        [Header("開始体力の SerializedProperty（StartingHealth）")]
        SerializedProperty StartHealthProp;

        [Header("不死フラグの SerializedProperty（Immortal）")]
        SerializedProperty ImmortalProp;

        [Header("被ダメージ時イベントの SerializedProperty（OnTakeDamage）")]
        SerializedProperty OnTakeDamageProp;

        [Header("死亡時イベントの SerializedProperty（OnDeath）")]
        SerializedProperty OnDeathProp;

        [Header("設定全体の折りたたみを隠すフラグ（HideSettingsFoldout）")]
        SerializedProperty HideSettingsFoldout;

        [Header("ヘルス設定セクションの折りたたみ（HealthSettingsFoldout）")]
        SerializedProperty HealthSettingsFoldout;

        void OnEnable()
        {
            if (HealthEditorIcon == null) HealthEditorIcon = Resources.Load("Editor Icons/EmeraldHealth") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            StartHealthProp = serializedObject.FindProperty("StartingHealth");
            ImmortalProp = serializedObject.FindProperty("Immortal");
            OnTakeDamageProp = serializedObject.FindProperty("OnTakeDamage");
            OnDeathProp = serializedObject.FindProperty("OnDeath");
            HealthSettingsFoldout = serializedObject.FindProperty("HealthSettingsFoldout");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldGeneralTargetBridge self = (EmeraldGeneralTargetBridge)target;
            serializedObject.Update();

            // ヘッダ
            CustomEditorProperties.BeginScriptHeaderNew("ターゲット ブリッジ", HealthEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                HealthSetting(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        /// <summary>
        /// ヘルス設定セクションの描画
        /// </summary>
        void HealthSetting(EmeraldGeneralTargetBridge self)
        {
            HealthSettingsFoldout.boolValue = EditorGUILayout.Foldout(HealthSettingsFoldout.boolValue, "ヘルス設定", true, FoldoutStyle);

            if (HealthSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 見出しと説明
                CustomEditorProperties.TextTitleWithDescription(
                    "ヘルス設定",
                    "Target Bridge コンポーネントを使うと、プレイヤーを含む任意の GameObject を AI が正しく検知し、ダメージ呼び出しを受けられるようにできます。",
                    true
                );

                // 不死
                CustomEditorProperties.CustomPropertyField(
                    ImmortalProp,
                    "不死",
                    "このターゲットがダメージ無効で、倒されない（キル不可）状態かどうかを制御します。",
                    true
                );

                // 不死のときは開始体力やイベントを編集不可に
                EditorGUI.BeginDisabledGroup(self.Immortal);

                // 開始体力
                CustomEditorProperties.CustomPropertyField(
                    StartHealthProp,
                    "開始体力",
                    "このターゲットが開始時に持つ体力値を設定します。",
                    true
                );

                // 被ダメージイベント
                CustomEditorProperties.CustomHelpLabelField("このターゲットがダメージを受けたときにイベントを発火します。", false);
                EditorGUILayout.PropertyField(OnTakeDamageProp, new GUIContent("ダメージ時イベント"));
                EditorGUILayout.Space();

                // 死亡イベント
                CustomEditorProperties.CustomHelpLabelField("このターゲットが死亡したときにイベントを発火します。", false);
                EditorGUILayout.PropertyField(OnDeathProp, new GUIContent("死亡時イベント"));

                EditorGUI.EndDisabledGroup();

                // 簡易ヘルスバー
                DrawHealthBar(self);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 簡易ヘルスバーの描画
        /// </summary>
        void DrawHealthBar(EmeraldGeneralTargetBridge self)
        {
            GUILayout.Space(45);
            GUIStyle LabelStyle = new GUIStyle();
            LabelStyle.alignment = TextAnchor.MiddleCenter;
            LabelStyle.padding.bottom = 4;
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.normal.textColor = Color.white;

            Rect r = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;
            float CurrentHealth = ((float)self.Health / (float)self.StartHealth);

            if (!Application.isPlaying)
            {
                self.Health = self.StartHealth;
            }

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
            }

            // バー背景
            EditorGUI.DrawRect(new Rect(r.x, r.position.y - 39f, ((r.width)), 32), new Color(0.05f, 0.05f, 0.05f, 0.5f)); // アウトライン
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8)), 24), new Color(0.16f, 0.16f, 0.16f, 1f)); // 背景
            // メインバー
            Color HealthBarColor = Color.Lerp(new Color(0.6f, 0.1f, 0.1f, 1f), new Color(0.15f, 0.42f, 0.15f, 1f), CurrentHealth);
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8) * CurrentHealth), 24), HealthBarColor);

            // ラベル
            if (CurrentHealth > 0)
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "現在の体力: " + self.Health + "/" + self.StartHealth, LabelStyle);
            }
            else
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "現在の体力: " + 0 + "/" + self.StartHealth + "（死亡）", LabelStyle);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
