using UnityEditor;                            // Unity エディタ拡張 API
using UnityEngine;                            // Unity 基本 API（Handles, Texture など）
using System.Collections.Generic;             // List<T> の使用
using System.Reflection;                      // リフレクション（FieldInfo 取得に使用）
using System;                                 // 汎用

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldBehaviors), true)]   // （注）EmeraldBehaviors（および派生）用のカスタムインスペクタ
    [CanEditMultipleObjects]                         // （注）複数選択の編集に対応
    // 【クラス概要】EmeraldBehaviorsEditor：EmeraldBehaviors のインスペクタ GUI を提供し、行動タイプ別の設定（Passive/Coward/Aggressive）とカスタム拡張項目を描画する Editor クラス
    public class EmeraldBehaviorsEditor : Editor
    {
        [Header("Foldout の見た目（EditorGUI のスタイルキャッシュ）")]
        GUIStyle FoldoutStyle;                       // 折りたたみ見出しの GUIStyle

        [Header("エディタ用アイコン（Resources からロード）")]
        Texture BehaviorsEditorIcon;                 // インスペクタ見出しに表示するアイコン

        [Header("派生クラスで宣言された Public インスタンスフィールド一覧")]
        FieldInfo[] CustomFields;                    // 追加カスタム項目の自動描画に用いる

        [Header("インスペクタのシリアライズドプロパティ群（各 UI 項目の参照）")]
        SerializedProperty HideSettingsFoldout,      // 設定セクションを隠す
                         BehaviorSettingsFoldout,    // Behavior Settings 折りたたみ
                         CurrentBehaviorType,        // 現在の行動タイプ
                         CustomSettingsFoldout,      // Custom Settings 折りたたみ
                         TargetToFollow,             // 追従対象
                         CautiousSeconds,            // 警戒状態の秒数
                         ChaseSeconds,               // 追跡の秒数
                         FleeSeconds,                // 逃走の秒数
                         RequireObstruction,         // 視界遮蔽を条件にするか
                         InfititeChase,              // 無制限追跡
                         FleeOnLowHealth,            // 低体力で逃走
                         StayNearStartingArea,       // 開始地点付近に留まる
                         MaxDistanceFromStartingArea,// 開始地点からの最大距離
                         UpdateFleePositionSeconds,  // 逃走位置の更新間隔
                         PercentToFlee,              // 逃走を開始する体力％
                         FollowingStoppingDistance;  // 追従停止距離

        void OnEnable()
        {
            // アイコンのロード（未ロード時のみ）
            if (BehaviorsEditorIcon == null) BehaviorsEditorIcon = Resources.Load("Editor Icons/EmeraldBehaviors") as Texture;
            InitializeProperties(); // シリアライズドプロパティの紐づけ初期化
        }

        void InitializeProperties()
        {
            // ここで Editor の各 UI 項目に対応する SerializedProperty を解決
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            BehaviorSettingsFoldout = serializedObject.FindProperty("BehaviorSettingsFoldout");
            CustomSettingsFoldout = serializedObject.FindProperty("CustomSettingsFoldout");
            CurrentBehaviorType = serializedObject.FindProperty("CurrentBehaviorType");
            TargetToFollow = serializedObject.FindProperty("TargetToFollow");
            CautiousSeconds = serializedObject.FindProperty("CautiousSeconds");
            ChaseSeconds = serializedObject.FindProperty("ChaseSeconds");
            FleeSeconds = serializedObject.FindProperty("FleeSeconds");
            RequireObstruction = serializedObject.FindProperty("RequireObstruction");
            InfititeChase = serializedObject.FindProperty("InfititeChase");
            FleeOnLowHealth = serializedObject.FindProperty("FleeOnLowHealth");
            StayNearStartingArea = serializedObject.FindProperty("StayNearStartingArea");
            UpdateFleePositionSeconds = serializedObject.FindProperty("UpdateFleePositionSeconds");
            PercentToFlee = serializedObject.FindProperty("PercentToFlee");
            MaxDistanceFromStartingArea = serializedObject.FindProperty("MaxDistanceFromStartingArea");
            FollowingStoppingDistance = serializedObject.FindProperty("FollowingStoppingDistance");

            // 親クラスに含まれない（= 派生クラス固有の）public インスタンスフィールドを収集し、Custom Settings で自動表示する
            CustomFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles(); // 共通スタイル更新
            EmeraldBehaviors self = (EmeraldBehaviors)target;           // 対象参照
            serializedObject.Update();                                  // SP 同期

            // 見出し（タイトル：Behaviors、アイコン、隠しフラグ）
            CustomEditorProperties.BeginScriptHeaderNew("Behaviors", BehaviorsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                BehaviorSettings(self);                                 // 行動タイプ別の標準設定描画
                if (self.GetType().ToString() != "EmeraldAI.EmeraldBehaviors")
                {
                    EditorGUILayout.Space();
                    CustomSettings(self);                               // 派生クラスのカスタム項目描画
                }
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                   // 見出し終了

            serializedObject.ApplyModifiedProperties();                 // SP 反映
        }

        void BehaviorSettings(EmeraldBehaviors self)
        {
            // 「Behavior Settings」セクションの折りたたみ
            BehaviorSettingsFoldout.boolValue = EditorGUILayout.Foldout(BehaviorSettingsFoldout.boolValue, "Behavior Settings", true, FoldoutStyle);

            if (BehaviorSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 説明：3 種の基本行動タイプから選択。Target to Follow 設定で Companion/Pet 化が可能
                CustomEditorProperties.TextTitleWithDescription("Behavior Settings", "Choose from 1 of the 3 available base behavior types. Companion and Pet options are avaialble within these options by setting a Target to Follow.", true);
                EditorGUILayout.Space();

                EditorGUILayout.Space();
                // 現在の行動タイプ（オーバーライドで独自行動へ拡張可）
                CustomEditorProperties.CustomPropertyField(CurrentBehaviorType, "Current Behavior Type", "The behavior this AI will use.", true);

                PassiveSettings(self);   // Passive 用 UI
                CowardSettings(self);    // Coward 用 UI
                AggressiveSettings(self);// Aggressive 用 UI

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// （日本語）Passive（非戦闘）行動タイプ向けのオプションを表示します。
        /// </summary>
        void PassiveSettings(EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
                return;

            // 説明：Passive は攻撃/逃走を行わず、Movement の Wander Type に従って徘徊
            CustomEditorProperties.TextTitleWithDescription("Passive Settings", "Passive AI will not attack or flee from targets. They will wander according to their Wander Type set within the Movement Component.", true);

            // 追従対象の割り当てにより Pet（非戦闘コンパニオン）化。Wander Type は無視される
            CustomEditorProperties.CustomPropertyField(TargetToFollow, "Target to Follow", "Assigning a Target to Follow will turn an AI into a Pet AI (or a non-combat Componanion AI). Note: If a Target to Follow is assigned, they will ignore their Wander Type and follow their follower instead.", true);

            if (self.TargetToFollow)
            {
                // 追従停止距離（1〜15）
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FollowingStoppingDistance, "Following Stopping Distance", 1, 15);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will stop from their Target to Follow.", true); // （訳）追従時に停止する距離
            }
        }

        /// <summary>
        /// （日本語）Coward（臆病）行動タイプ向けのオプションを表示します。
        /// </summary>
        void CowardSettings(EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Coward)
                return;

            // 説明：敵対相手から逃走する
            CustomEditorProperties.TextTitleWithDescription("Coward Settings", "Coward AI will flee from targets who they have an Enemy Relation Type with.", true);

            // 警戒秒数（0 なら警戒状態を無視）
            CustomEditorProperties.CustomIntSliderPropertyField(CautiousSeconds, "Cautious Seconds", "Controls the amount of time an AI will remain in the Cautious State before fleeing from their target. " +
                "If an AI has a warning animation, this will automatically be played while in this state. If this value is set to 0, the cautious state will be ignored.", 0, 15, false);

            if (self.CautiousSeconds > 0)
            {
                CustomEditorProperties.DisplayImportantMessage("Be aware that AI who are in a cautious state will not flee from a detected target until after the duration of their Cautious Seconds (unless they're attacked)."); // （訳）警戒時間中は逃走しない点に注意
            }

            EditorGUI.BeginDisabledGroup(self.InfititeChase);
            // 逃走秒数（検知外になったときに非戦闘へ戻るまで）
            CustomEditorProperties.CustomIntSliderPropertyField(FleeSeconds, "Flee Seconds", "Controls the amount of time an AI will flee from a target for returning its non-combat state. This happens when the current target is outside of an AI's detection radius.", 1, 60, true);

            // 視界遮蔽がある場合のみ時間延長
            CustomEditorProperties.CustomPropertyField(RequireObstruction, "Require Obstruction", "Only allow the flee time to increase if the AI's current target is obstructed. This allows the AI to continuously flee the target while they are visible, " +
                "but give up if the target has been obstructed (or not visible) for the duration of the Flee Seconds.", true);

            EditorGUILayout.Space();
            // 逃走位置の更新間隔
            CustomEditorProperties.CustomFloatSliderPropertyField(UpdateFleePositionSeconds, "Update Flee Position Seconds", "Controls how often the flee position will be updated.", 0.25f, 5f, true);

            EditorGUILayout.Space();
        }

        /// <summary>
        /// （日本語）Aggressive（攻撃的）行動タイプ向けのオプションを表示します。
        /// </summary>
        void AggressiveSettings(EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive)
                return;

            // 説明：敵対相手に攻撃を行う
            CustomEditorProperties.TextTitleWithDescription("Aggressive Settings", "Aggressive AI will attack targets who they have an Enemy Relation Type with.", true);

            // 追従対象の割り当てでコンパニオン化（以降の設定は無効）
            CustomEditorProperties.CustomPropertyField(TargetToFollow, "Target to Follow", "Assigning a Target to Follow will turn an AI into a Companion AI. They will also ignore their Wander Type and follow their the specified instead. Note: AI who have currently have a Target to Follow cannot use any of the settings below.", true);

            if (self.TargetToFollow)
            {
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FollowingStoppingDistance, "Following Stopping Distance", 1, 15);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will stop from their Target to Follow.", true); // （訳）追従停止距離
            }

            EditorGUI.BeginDisabledGroup(self.TargetToFollow);
            // 警戒秒数（0 なら警戒状態を無視）
            CustomEditorProperties.CustomIntSliderPropertyField(CautiousSeconds, "Cautious Seconds", "Controls the amount of time an AI will remain in the Cautious State before attacking their target. " +
                "If an AI has a warning animation, this will automatically be played while in this state. If this value is set to 0, the cautious state will be ignored.", 0, 15, false);

            if (self.CautiousSeconds > 0)
            {
                CustomEditorProperties.DisplayImportantMessage("Be aware that AI who are in a cautious state will not attack a detected target until after the duration of their Cautious Seconds (unless they're attacked)."); // （訳）警戒時間中は攻撃しない
            }

            EditorGUILayout.Space();
            // 無制限追跡の有効化（距離/時間制限を無効化）
            CustomEditorProperties.CustomPropertyField(InfititeChase, "Infitite Chase", "Controls whether or not the AI will chase their target without any distance or time resrtictions. Note: This will disable the Chase Seconds and Stay Near Starting Area settings.", true);

            EditorGUI.BeginDisabledGroup(self.InfititeChase);
            // 追跡秒数
            CustomEditorProperties.CustomIntSliderPropertyField(ChaseSeconds, "Chase Seconds", "Controls the amount of time an AI will chase a target for before giving up and exiting its combat state. This happens when the current target is outside of an AI's detection radius.", 1, 60, true);

            // 視界遮蔽がある場合のみ追跡時間延長
            CustomEditorProperties.CustomPropertyField(RequireObstruction, "Require Obstruction", "Only allow the chase time to increase if the AI's current target is obstructed. This allows the AI to continuously chase the target while they are visible, " +
                "but give up if the target has been obstructed (or not visible) for the duration of the Chase Seconds.", true);

            // 開始地点付近に留まるか（離れすぎると諦める）
            CustomEditorProperties.CustomPropertyField(StayNearStartingArea, "Stay Near Starting Area", "Controls whether or not an AI will give up on a target if it gets too far away from its starting area.", true);

            if (self.StayNearStartingArea == YesOrNo.Yes)
            {
                CustomEditorProperties.BeginIndent();
                // 開始地点からの最大許容距離
                CustomEditorProperties.CustomIntSliderPropertyField(MaxDistanceFromStartingArea, "Max Distance From Starting Area", "Controls the maximum distance an AI is allowed to be from its starting area before giving up on a target and return to its starting position or area.", 10, 100, true);
                CustomEditorProperties.EndIndent();
            }
            EditorGUI.EndDisabledGroup();

            // 低体力時に逃走するか
            CustomEditorProperties.CustomPropertyField(FleeOnLowHealth, "Flee on Low Health", "Controls whether or not an AI will flee upon low health while in combat.", true);

            if (self.FleeOnLowHealth == YesOrNo.Yes)
            {
                CustomEditorProperties.BeginIndent();
                // 逃走を開始する体力割合
                CustomEditorProperties.CustomIntSliderPropertyField(PercentToFlee, "Percent to Flee", "Controls the percentage of low health needed to flee.", 1, 99, true);
                // 逃走位置の更新間隔
                CustomEditorProperties.CustomFloatSliderPropertyField(UpdateFleePositionSeconds, "Update Flee Position Seconds", "Controls how often the flee position will be updated.", 0.25f, 5f, true);
                CustomEditorProperties.EndIndent();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        void OnSceneGUI()
        {
            EmeraldBehaviors self = (EmeraldBehaviors)target;  // シーン上のハンドル描画用参照
            DrawStartingAreaDistance(self);                    // 開始地点半径のガイド表示
        }

        /// <summary>
        /// （日本語）Stay Near Starting Area が有効で、かつインスペクタで Behavior Settings が開いている場合に、開始地点からの許容距離を円で描画します。
        /// </summary>
        void DrawStartingAreaDistance(EmeraldBehaviors self)
        {
            if (self.StayNearStartingArea == YesOrNo.Yes && BehaviorSettingsFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = new Color(0, 0.6f, 0, 1f);     // 緑
                Handles.DrawWireDisc(self.transform.position, Vector3.up, (float)self.MaxDistanceFromStartingArea, 3f); // ワイヤーディスク
                Handles.color = Color.white;                   // 色を戻す
            }
        }

        /// <summary>
        /// （日本語）派生クラスで追加されたカスタム変数を、専用セクションに列挙表示します。
        /// </summary>
        void CustomSettings(EmeraldBehaviors self)
        {
            // 「Custom Settings」セクションの折りたたみ
            CustomSettingsFoldout.boolValue = EditorGUILayout.Foldout(CustomSettingsFoldout.boolValue, "Custom Settings", true, FoldoutStyle);

            if (CustomSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 説明：派生クラス（EmeraldBehavior の子）で追加した変数をここに表示
                CustomEditorProperties.TextTitleWithDescription("Custom Settings", "Any variables added through a child class of EmeraldBehavior will be added here.", true);

                foreach (FieldInfo field in CustomFields)
                {
                    // 配列はインデントを足して表示
                    if (field.FieldType.GetElementType() != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // List<T> もインデントして表示
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // ユーザークラス（Unity 型/文字列以外）もインデントして表示
                    else if (field.FieldType.IsClass && field.FieldType.ToString() != "System.String" && !field.FieldType.ToString().Contains("Unity"))
                    {
                        Debug.Log(field.FieldType);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // 単一変数はそのまま表示
                    else
                    {
                        if (serializedObject.FindProperty(field.Name) != null)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        }
                    }

                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
