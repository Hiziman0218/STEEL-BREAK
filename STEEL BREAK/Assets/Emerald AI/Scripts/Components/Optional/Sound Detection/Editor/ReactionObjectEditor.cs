using System.Collections;                                      // （保持）コルーチン関連
using System.Collections.Generic;                              // （保持）コレクション
using UnityEngine;                                             // Unity ランタイム API
using UnityEditor;                                             // エディタ拡張 API
using UnityEditorInternal;                                     // ReorderableList など
using UnityEditor.SceneManagement;                             // シーン編集状態の管理
using UnityEngine.SceneManagement;                             // シーン管理

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(ReactionObject))]                     // このカスタムインスペクタは ReactionObject 用

    // 【クラス概要】ReactionObjectEditor：
    //  ReactionObject（複数の Reaction を保持する ScriptableObject）を
    //  インスペクタ上で編集しやすくするためのエディタ拡張クラス。
    //  ReorderableList により並び替え・追加・削除、各 ReactionType ごとの入力項目とツールチップを提供する。
    public class ReactionObjectEditor : Editor
    {
        [Header("リアクション一覧（ReorderableList で並び替え/追加/削除）")]
        ReorderableList ReactionList;                           // ReactionObject.ReactionList を編集表示するためのリスト

        [Header("ツールチップ：Debug Log の説明（Unity コンソールへメッセージ出力）")]
        string DebugLogMessageInfo = "Unity コンソールへメッセージを出力します（仕組みや値のテストに便利）。";

        [Header("ツールチップ：Play Sound の説明（AI の位置で効果音を再生）")]
        string PlaySoundInfo = "この AI の位置でサウンドを再生します（聴覚的なキューに便利）。";

        [Header("ツールチップ：Emote 再生の説明（設定済み Emote ID でアニメ実行）")]
        string PlayEmoteAnimationInfo = "Emerald AI のエディタ（Animation Settings タブ）で設定した Emote Animation ID を使用してエモートアニメを再生します（視覚的なキュー）。";

        [Header("ツールチップ：最も騒がしいターゲットの方向を見る")]
        string LookAtLoudestTargetPositionInfo = "最も大きな騒音を発した方向を一定時間見つめます。";

        [Header("ツールチップ：開始位置へ戻る")]
        string ReturnToStartingPositionInfo = "AI を開始位置へ戻します。";

        [Header("ツールチップ：検知距離の一時拡張")]
        string ExpandDetectionDistanceInfo = "AI の検知距離を、現在の検知距離に加算する形で拡張します（最近攻撃してきた対象の再検知などに有用）。";

        [Header("ツールチップ：移動状態の変更（歩行/走行 など）")]
        string SetMovementStateInfo = "AI の移動状態を Walk/Run などに切り替えます。";

        [Header("ツールチップ：検知距離を初期値へリセット")]
        string ResetDetectionDistanceInfo = "AI の検知距離を既定（開始時）の値へ戻します。";

        [Header("ツールチップ：Look At 位置を初期値へリセット")]
        string ResetLookAtPositionInfo = "AI の Look At 位置を既定（開始時）の位置へ戻します。";

        [Header("ツールチップ：Attract Modifier 反応（アトラクト専用）")]
        string AttractModifierInfo = "（Attract Modifier 使用時のみ）AttractModifier コンポーネントを持つゲームオブジェクトで条件が満たされたときに呼び出されます。";

        [Header("ツールチップ：ディレイ（下にあるリアクションの実行を遅延）")]
        string DelayInfo = "このリアクションの“次にあるリアクション”の実行を、指定秒だけ遅らせます。";

        [Header("ツールチップ：全てを初期状態に戻す")]
        string ResetAllToDefaultInfo = "変更された値（Look At 位置、検知距離、移動状態、戦闘状態）をすべて既定値に戻します。";

        [Header("ツールチップ：戦闘状態へ入る")]
        string EnterCombatStateInfo = "AI を戦闘状態にし、戦闘用アニメーションを許可します。装備アニメーションを使用している場合は、移行前に装備アニメが再生されます。";

        [Header("ツールチップ：戦闘状態から出る")]
        string ExitCombatStateInfo = "AI を非戦闘状態に戻します（可視ターゲットがいない場合）。装備アニメーションを使用している場合は、移行前に収納アニメが再生されます。";

        [Header("ツールチップ：最も騒音の大きいターゲットから逃走")]
        string FleeFromLoudestTargetInfo = "最も騒音の大きい検知ターゲットを逃走対象に設定します。臆病（Coward）挙動の AI 向け（対象がいない場合は無視）。";

        [Header("ツールチップ：最も騒音の大きいターゲットへ移動")]
        string MoveToLoudestTargetInfo = "最も騒音の大きい検知ターゲットへ直接移動します（対象がいない場合は無視）。";

        [Header("ツールチップ：現在位置を基準にウェイポイント巡回")]
        string MoveAroundCurrentPositionInfo = "AI の現在位置を基準に、設定した半径と数で新規ウェイポイントを生成し巡回します。";

        [Header("ツールチップ：最も騒音の大きいターゲット周辺を巡回")]
        string MoveAroundLoudestTargetInfo = "最も騒音の大きいターゲットの周辺を基準に、設定した半径と数で新規ウェイポイントを生成し巡回します（対象がいない場合は無視）。";

        [Header("ツールチップ：最も騒音の大きいターゲットを戦闘相手に設定")]
        string SetLoudestTargetAsCombatTargetInfo = "最も騒音の大きいターゲットを現在の戦闘ターゲットに設定し、Combat モードへ移行します。視認を必要とせず、その位置へ移動して攻撃を開始します。";

        [Header("ツールチップ：何もしない（初期値）")]
        string NoneInfo = "None はデフォルトのリアクションで、呼び出されても何も起きません。";

        /// <summary>
        /// （日本語）エディタが有効化されたときに呼ばれます。ReorderableList を初期化します。
        /// </summary>
        private void OnEnable()
        {
            UpdateReactionList();                                        // リスト構築
        }

        /// <summary>
        /// （日本語）ReactionList（ReorderableList）の設定を更新・再構築します。
        /// </summary>
        void UpdateReactionList()
        {
            // ReorderableList の生成（ReactionObject.ReactionList を対象）
            ReactionList = new ReorderableList(serializedObject, serializedObject.FindProperty("ReactionList"), true, true, true, true);
            ReactionList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    CustomCallback(ReactionList, rect, index, isActive, isFocused); // 各要素の描画
                };

            // ヘッダーを日本語化
            ReactionList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "リアクション一覧", EditorStyles.boldLabel);
            };

            // 各要素の高さを、設定された項目数に応じて可変にする
            ReactionList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = ReactionList.serializedProperty.GetArrayElementAtIndex(index);
                float height = 1;

                if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.One)
                    height -= 1.35f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Two)
                    height = 1;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Three)
                    height += 1.35f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Four)
                    height += 2.7f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Five)
                    height += 4.05f;
                else if ((Reaction.ElementLineHeights)element.FindPropertyRelative("ElementLineHeight").intValue == Reaction.ElementLineHeights.Six)
                    height += 5.4f;

                return EditorGUIUtility.singleLineHeight * (2.35f + height);
            };

            // 新規要素追加時のデフォルト値を設定
            ReactionList.onAddCallback = ReactionList =>
            {
                var m_List = serializedObject.FindProperty("ReactionList");
                m_List.arraySize++;

                SerializedProperty element = ReactionList.serializedProperty.GetArrayElementAtIndex(m_List.arraySize - 1);
                element.FindPropertyRelative("ReactionType").intValue = (int)ReactionTypes.None;
                element.FindPropertyRelative("IntValue1").intValue = 5;
                element.FindPropertyRelative("IntValue2").intValue = 2;
                element.FindPropertyRelative("StringValue").stringValue = "New Message";
                element.FindPropertyRelative("FloatValue").floatValue = 1f;
                element.FindPropertyRelative("BoolValue").boolValue = true;
                element.FindPropertyRelative("SoundRef").objectReferenceValue = null;
            };
        }

        /// <summary>
        /// （日本語）インスペクタのメイン描画。説明と ReorderableList を表示し、変更時に再構築します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            ReactionObject self = (ReactionObject)target;

            serializedObject.Update();

            // タイトル枠
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
            DisplayTitle("リアクションオブジェクト");

            // 説明（英語→日本語化）
            CustomHelpLabelField("このオブジェクトに登録されたリアクションは、上から下の順に実行されます。AI がターゲットを視認した場合、このリアクションはキャンセルされ、AI の Behavior Type に従います。", false);
            EditorGUILayout.HelpBox("各『リアクション種別』や値の上にカーソルを置くと、詳細なツールチップが表示されます。", MessageType.Info);
            GUILayout.Space(5);

            // リスト本体
            EditorGUI.BeginChangeCheck();
            ReactionList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                // 項目の構成が変わったら高さ計算などを更新
                UpdateReactionList();
            }

            GUILayout.Space(15);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);

#if UNITY_EDITOR
            // エディタ編集中（Play中でない）に変更があれば Dirty マーク
            if (!Application.isPlaying)
            {
                Undo.RecordObject(self, "Undo");

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
#endif

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// （日本語）ヘルプボックス風の説明テキストを表示します。
        /// </summary>
        void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// （日本語）汎用ポップアップ（Enum っぽい UI）を描画します。
        /// </summary>
        void CustomPopup(Rect position, GUIContent label, SerializedProperty property, string nameOfLabel, string[] names)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            string[] enumNamesList = names;
            var newValue = EditorGUI.Popup(position, property.intValue, enumNamesList);

            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// （日本語）枠タイトルを太字で表示します。
        /// </summary>
        void DisplayTitle(string Title)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// （日本語）各 ReactionType ごとの入力項目を描画します。ツールチップは日本語化済み。
        /// </summary>
        void CustomCallback(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            // 種別のドロップダウン（ラベル日本語化）
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 11f, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("ReactionType"), new GUIContent("リアクション種別", ""));

            // --- 1行構成の要素 ---
            if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetDetectionDistance)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetDetectionDistanceInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetLookAtPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetLookAtPositionInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ReturnToStartingPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ReturnToStartingPositionInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.None)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", NoneInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ResetAllToDefault)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ResetAllToDefaultInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.EnterCombatState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", EnterCombatStateInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ExitCombatState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ExitCombatStateInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.FleeFromLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", FleeFromLoudestTargetInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.SetLoudestTargetAsCombatTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", SetLoudestTargetAsCombatTargetInfo));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.One;
            }

            // --- 2行構成の要素 ---
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.DebugLogMessage)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", DebugLogMessageInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("StringValue"), new GUIContent("デバッグメッセージ", "Unity コンソールに表示するメッセージ。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.PlayEmoteAnimation)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", PlayEmoteAnimationInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("エモートアニメ ID", "Animation Settings タブで設定した Emote Animation ID と同一の ID を指定します。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.LookAtLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", LookAtLoudestTargetPositionInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("秒数", "最も騒音の大きいターゲットを見つめる時間（秒）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.Delay)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", DelayInfo));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0.25f, 10f, new GUIContent("遅延秒数", "この行の下にある次のリアクションを呼び出すまでの遅延（秒）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.ExpandDetectionDistance)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", ExpandDetectionDistanceInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), new GUIContent("距離", "AI の検知半径に“加算”される距離（ユニット）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.SetMovementState)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", SetMovementStateInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("MovementState"), new GUIContent("移動状態", "AI が使用する移動状態（Walk / Run など）。『すべて初期化』や同リアクションで既定値に戻せます。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.LookAtAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("アトラクトモディファイアの反応", "Look At 機能が有効な場合、検知した誘引元（Attract Source）を見つめます。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Two;
            }

            // --- 3行構成の要素 ---
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.PlaySound)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", PlaySoundInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundRef"), new GUIContent("オーディオクリップ", "リアクション発火時に再生する AudioClip。"));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 1f, new GUIContent("音量", "オーディオクリップの再生音量。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Three;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveToLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveToLoudestTargetInfo));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("待機秒数", "最大騒音位置で待機する時間（秒）。"));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("次のリアクションを遅延", "目的地到達（またはウェイポイント巡回完了）まで次のリアクションを遅延します（推奨）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Three;
            }

            // --- 4行構成の要素 ---
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.MoveToAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("アトラクトモディファイアの反応", "誘引元（Attract Source）の位置へ移動します。"));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("待機秒数", "誘引元の位置で待機する時間（秒）。"));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("次のリアクションを遅延", "目的地到達（またはウェイポイント巡回完了）まで次のリアクションを遅延します（推奨）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Four;
            }

            // --- 5行構成の要素 ---
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveAroundCurrentPosition)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveAroundCurrentPositionInfo));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("半径", "AI の現在位置からウェイポイントを生成する半径。"));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("総ウェイポイント数", "このリアクションによって生成されるウェイポイントの数。ターゲットを視認しない限り、すべて到達するまで続行します。"));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("待機秒数", "次のウェイポイントを生成するまでの待機時間（秒）。"));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("次のリアクションを遅延", "目的地到達（またはウェイポイント巡回完了）まで次のリアクションを遅延します（推奨）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Five;
            }
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.MoveAroundLoudestTarget)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", MoveAroundLoudestTargetInfo));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("半径", "最も騒音の大きいターゲットの位置を基準にウェイポイントを生成する半径。"));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("総ウェイポイント数", "このリアクションによって生成されるウェイポイントの数。ターゲットを視認しない限り、すべて到達するまで続行します。"));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("待機秒数", "次のウェイポイントを生成するまでの待機時間（秒）。"));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("次のリアクションを遅延", "目的地到達（またはウェイポイント巡回完了）まで次のリアクションを遅延します（推奨）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Five;
            }

            // --- 6行構成の要素 ---
            else if ((ReactionTypes)element.FindPropertyRelative("ReactionType").intValue == ReactionTypes.AttractModifier && (AttractModifierReactionTypes)element.FindPropertyRelative("AttractModifierReaction").intValue == AttractModifierReactionTypes.MoveAroundAttractSource)
            {
                EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 11, rect.width, EditorGUIUtility.singleLineHeight),
                new GUIContent("           ", AttractModifierInfo));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 34, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AttractModifierReaction"), new GUIContent("アトラクトモディファイアの反応", "検知した誘引元（Attract Source）を基準に、ユーザー設定の半径・数でウェイポイントを生成します。"));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 57, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue1"), 1, 25, new GUIContent("半径", "Attract Modifier からの生成位置の半径。"));
                EditorGUI.IntSlider(new Rect(rect.x, rect.y + 80, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("IntValue2"), 1, 10, new GUIContent("総ウェイポイント数", "このリアクションによって生成されるウェイポイントの数。ターゲットを視認しない限り、すべて到達するまで続行します。"));
                EditorGUI.Slider(new Rect(rect.x, rect.y + 103, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("FloatValue"), 0f, 10f, new GUIContent("待機秒数", "次のウェイポイントを生成するまでの待機時間（秒）。"));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 126, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("BoolValue"), new GUIContent("次のリアクションを遅延", "目的地到達（またはウェイポイント巡回完了）まで次のリアクションを遅延します（推奨）。"));
                element.FindPropertyRelative("ElementLineHeight").intValue = (int)Reaction.ElementLineHeights.Six;
            }
        }
    }
}
