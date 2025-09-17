using System.Collections;                           // コルーチン関連（原文保持）
using System.Collections.Generic;                   // 汎用コレクション（原文保持）
using UnityEngine;                                  // Unity ランタイムAPI
using UnityEditor;                                  // エディタ拡張API
using UnityEditorInternal;                          // ReorderableList 等（原文保持）

namespace EmeraldAI.Utility                         // EmeraldAI のユーティリティ名前空間
{
    [CustomEditor(typeof(EmeraldEvents))]           // このカスタムインスペクタは EmeraldEvents 用
    [CanEditMultipleObjects]                        // 複数オブジェクト同時編集を許可

    // 【クラス概要】EmeraldEventsEditor：
    //  EmeraldEvents コンポーネントに用意された UnityEvent 群（汎用イベント/戦闘イベント）を、
    //  見出し・説明付きで編集しやすく表示するためのエディタ拡張。
    public class EmeraldEventsEditor : Editor
    {
        [Header("フォールドアウトの見た目（EditorGUI 用スタイル）")]
        GUIStyle FoldoutStyle;                      // 見出しの描画スタイル

        [Header("ヘッダーに表示するアイコン（Resources から読込）")]
        Texture EventsEditorIcon;                   // インスペクタ上部のアイコン

        //Bools
        [Header("エディタ表示制御用のフラグ群（SerializedProperty）")]
        SerializedProperty HideSettingsFoldout,     // 設定全体の非表示トグル
                          GeneralEventsFoldout,     // 「汎用イベント」セクションの開閉
                          CombatEventsFoldout;      // 「戦闘イベント」セクションの開閉

        //Events
        [Header("UnityEvent への参照（SerializedProperty）")]
        SerializedProperty OnDeathEventProp,        // 死亡時
                          OnTakeDamageEventProp,    // 被ダメージ時
                          OnTakeCritDamageEventProp,// クリティカル被弾時
                          OnReachedDestinationEventProp, // 目的地到達時
                          OnReachedWaypointEventProp,    // ウェイポイント到達時
                          OnGeneratedWaypointEventProp,  // ウェイポイント生成時
                          OnStartEventProp,         // Start 時
                          OnAttackStartEventProp,   // 攻撃開始時
                          OnFleeEventProp,          // 逃走時
                          OnStartCombatEventProp,   // 戦闘開始時
                          OnEndCombatEventProp,     // 戦闘終了時
                          OnEnabledEventProp,       // OnEnable 時
                          OnPlayerDetectedEventProp,// プレイヤー検知時
                          OnKilledTargetEventProp,  // ターゲット撃破時
                          OnDoDamageEventProp,      // ダメージ与え時
                          OnDoCritDamageEventProp,  // クリティカル与え時
                          OnAttackEndEventProp,     // 攻撃終了時
                          OnEnemyTargetDetectedEventProp; // 敵ターゲット検知時

        void OnEnable()                             // エディタ有効化時（アイコン読込とプロパティ初期化）
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldEvents") as Texture; // ヘッダー用アイコンをロード
            InitializeProperties();                 // 対象フィールドを SerializedProperty にバインド
        }

        void InitializeProperties()                 // 各 SerializedProperty の紐付け
        {
            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");  // 非表示トグル
            GeneralEventsFoldout = serializedObject.FindProperty("GeneralEventsFoldout"); // 汎用イベント折りたたみ
            CombatEventsFoldout = serializedObject.FindProperty("CombatEventsFoldout");  // 戦闘イベント折りたたみ

            //Events
            OnDeathEventProp = serializedObject.FindProperty("OnDeathEvent");              // 死亡時
            OnTakeDamageEventProp = serializedObject.FindProperty("OnTakeDamageEvent");         // 被ダメージ時
            OnTakeCritDamageEventProp = serializedObject.FindProperty("OnTakeCritDamageEvent");     // クリティカル被弾時
            OnDoDamageEventProp = serializedObject.FindProperty("OnDoDamageEvent");           // ダメージ与え時
            OnReachedDestinationEventProp = serializedObject.FindProperty("OnReachedDestinationEvent"); // 目的地到達時
            OnReachedWaypointEventProp = serializedObject.FindProperty("OnReachedWaypointEvent");    // ウェイポイント到達時
            OnGeneratedWaypointEventProp = serializedObject.FindProperty("OnGeneratedWaypointEvent");  // ウェイポイント生成時
            OnStartEventProp = serializedObject.FindProperty("OnStartEvent");               // Start 時
            OnPlayerDetectedEventProp = serializedObject.FindProperty("OnPlayerDetectedEvent");      // プレイヤー検知時
            OnEnemyTargetDetectedEventProp = serializedObject.FindProperty("OnEnemyTargetDetectedEvent"); // 敵ターゲット検知時
            OnEnabledEventProp = serializedObject.FindProperty("OnEnabledEvent");             // OnEnable 時
            OnAttackStartEventProp = serializedObject.FindProperty("OnAttackStartEvent");         // 攻撃開始時
            OnAttackEndEventProp = serializedObject.FindProperty("OnAttackEndEvent");           // 攻撃終了時
            OnFleeEventProp = serializedObject.FindProperty("OnFleeEvent");                // 逃走時
            OnStartCombatEventProp = serializedObject.FindProperty("OnStartCombatEvent");         // 戦闘開始時
            OnEndCombatEventProp = serializedObject.FindProperty("OnEndCombatEvent");           // 戦闘終了時
            OnKilledTargetEventProp = serializedObject.FindProperty("OnKilledTargetEvent");        // ターゲット撃破時
            OnDoCritDamageEventProp = serializedObject.FindProperty("OnDoCritDamageEvent");        // クリティカル与え時
        }

        public override void OnInspectorGUI()       // インスペクタのメイン描画
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // カスタムスタイル更新
            EmeraldEvents self = (EmeraldEvents)target;                 // 対象コンポーネント参照
            serializedObject.Update();                                  // 直列化オブジェクトを最新化

            // ヘッダー：英語 "Events" → 日本語「イベント」
            CustomEditorProperties.BeginScriptHeaderNew("イベント", EventsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)      // 非表示でなければ内容を描画
            {
                EditorGUILayout.Space();
                GeneralEvents(self);                  // 汎用イベント
                EditorGUILayout.Space();
                CombatEvents(self);                   // 戦闘イベント
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader(); // ヘッダー終了

            serializedObject.ApplyModifiedProperties(); // 変更の適用
        }

        void GeneralEvents(EmeraldEvents self)      // 「汎用イベント」セクションの描画
        {
            // 見出し：英語 "General Events" → 日本語「汎用イベント」
            GeneralEventsFoldout.boolValue = EditorGUILayout.Foldout(GeneralEventsFoldout.boolValue, "汎用イベント", true, FoldoutStyle);

            if (GeneralEventsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox(); // 枠開始

                // タイトル＆説明：英語→日本語
                CustomEditorProperties.TextTitleWithDescription("汎用イベント", "汎用系のイベントをまとめて編集できます。", true);

                // OnEnabledEvent
                CustomEditorProperties.CustomHelpLabelField("この AI が有効化されたとき（OnEnable）にイベントを発火します。リスポーン時など、再有効化のタイミングで処理を行いたい場合に便利です。", false);
                EditorGUILayout.PropertyField(OnEnabledEventProp);

                // OnStartEvent
                CustomEditorProperties.CustomHelpLabelField("Start タイミングでイベントを発火します。クエストや独自処理の初期化、スポーン演出の再生などに利用できます。", false);
                EditorGUILayout.PropertyField(OnStartEventProp);

                EditorGUILayout.Space();

                // OnReachedDestinationEvent
                CustomEditorProperties.CustomHelpLabelField("目的地徘徊（Destination Wander Type）使用時、目的地へ到達した際にイベントを発火します。", false);
                EditorGUILayout.PropertyField(OnReachedDestinationEventProp, new GUIContent("目的地到達時イベント"));

                // OnReachedWaypointEvent
                CustomEditorProperties.CustomHelpLabelField("動的/ウェイポイント徘徊の両方で、各ウェイポイントに到達するたびにイベントを発火します。", false);
                EditorGUILayout.PropertyField(OnReachedWaypointEventProp, new GUIContent("ウェイポイント到達時イベント"));

                // OnGeneratedWaypointEvent
                CustomEditorProperties.CustomHelpLabelField("動的/ウェイポイント徘徊の両方で、ウェイポイントが生成されたタイミングでイベントを発火します。", false);
                EditorGUILayout.PropertyField(OnGeneratedWaypointEventProp, new GUIContent("ウェイポイント生成時イベント"));

                EditorGUILayout.Space();

                // OnPlayerDetectedEvent
                CustomEditorProperties.CustomHelpLabelField(
                    "戦闘モード外でプレイヤーを検出したときにイベントを発火します。挨拶・会話開始・クエスト進行などに利用できます。" +
                    "検出は AI の Detection Radius（検知半径）に依存し、プレイヤーがこの範囲へ入った時点で発火します。",
                    false);
                EditorGUILayout.PropertyField(OnPlayerDetectedEventProp);

                CustomEditorProperties.EndFoldoutWindowBox(); // 枠終了
            }
        }

        void CombatEvents(EmeraldEvents self)       // 「戦闘イベント」セクションの描画
        {
            // 見出し：英語 "Combat Events" → 日本語「戦闘イベント」
            CombatEventsFoldout.boolValue = EditorGUILayout.Foldout(CombatEventsFoldout.boolValue, "戦闘イベント", true, FoldoutStyle);

            if (CombatEventsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox(); // 枠開始

                // タイトル＆説明：英語→日本語
                CustomEditorProperties.TextTitleWithDescription("戦闘イベント", "戦闘に関連するイベントをまとめて編集できます。", true);

                // OnStartCombatEvent
                CustomEditorProperties.CustomHelpLabelField("AI が初めて戦闘状態へ入った瞬間に発火します。AI が戦闘を離脱して再度戦闘に入るまでは再度発火しません。", false);
                EditorGUILayout.PropertyField(OnStartCombatEventProp);

                // OnEndCombatEvent
                CustomEditorProperties.CustomHelpLabelField("検出可能な敵ターゲットが近くにいなくなり、AI が戦闘を終了したときに発火します。", false);
                EditorGUILayout.PropertyField(OnEndCombatEventProp);

                EditorGUILayout.Space();

                // OnEnemyTargetDetectedEvent
                CustomEditorProperties.CustomHelpLabelField("戦闘中に AI がターゲットを新規に検出したときに発火します。", false);
                EditorGUILayout.PropertyField(OnEnemyTargetDetectedEventProp, new GUIContent("ターゲット検出時イベント"));

                EditorGUILayout.Space();

                // OnAttackStartEvent
                CustomEditorProperties.CustomHelpLabelField("AI の攻撃が開始された瞬間に発火します。注：攻撃が外れてもこのイベントは発火します。", false);
                EditorGUILayout.PropertyField(OnAttackStartEventProp, new GUIContent("攻撃開始時イベント"));

                EditorGUILayout.Space();

                // OnAttackEndEvent
                CustomEditorProperties.CustomHelpLabelField("AI の攻撃が終了した瞬間に発火します。注：攻撃が外れてもこのイベントは発火します。", false);
                EditorGUILayout.PropertyField(OnAttackEndEventProp);

                EditorGUILayout.Space();

                // OnTakeDamageEvent
                CustomEditorProperties.CustomHelpLabelField("AI がダメージを受けたときに発火します。", false);
                EditorGUILayout.PropertyField(OnTakeDamageEventProp, new GUIContent("被ダメージ時イベント"));

                EditorGUILayout.Space();

                // OnTakeCritDamageEvent
                CustomEditorProperties.CustomHelpLabelField("AI がクリティカルダメージを受けたときに発火します。", false);
                EditorGUILayout.PropertyField(OnTakeCritDamageEventProp, new GUIContent("クリティカル被ダメージ時イベント"));

                EditorGUILayout.Space();

                // OnDoDamageEvent
                CustomEditorProperties.CustomHelpLabelField("AI がいかなる種類のダメージでも与えることに成功したときに発火します。", false);
                EditorGUILayout.PropertyField(OnDoDamageEventProp);

                EditorGUILayout.Space();

                // OnDoCritDamageEvent
                CustomEditorProperties.CustomHelpLabelField("AI が与えたダメージがクリティカルだった場合に発火します。", false);
                EditorGUILayout.PropertyField(OnDoCritDamageEventProp);

                EditorGUILayout.Space();

                // OnFleeEvent
                CustomEditorProperties.CustomHelpLabelField("AI が逃走状態になったときに発火します。逃走SE の再生や補助処理などに利用できます。", false);
                EditorGUILayout.PropertyField(OnFleeEventProp);

                EditorGUILayout.Space();

                // OnKilledTargetEvent
                CustomEditorProperties.CustomHelpLabelField("AI がターゲットを撃破したときに発火します。", false);
                EditorGUILayout.PropertyField(OnKilledTargetEventProp);

                EditorGUILayout.Space();

                // OnDeathEvent
                CustomEditorProperties.CustomHelpLabelField("AI が死亡したときに発火します。戦利品生成・クエスト進行・死亡演出などのトリガに利用できます。", false);
                EditorGUILayout.PropertyField(OnDeathEventProp, new GUIContent("死亡時イベント"));

                CustomEditorProperties.EndFoldoutWindowBox(); // 枠終了
            }
        }
    }
}
