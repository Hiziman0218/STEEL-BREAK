using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(AnimationProfile))]
    [CanEditMultipleObjects]
    public class AnimationProfileEditor : Editor
    {
        #region SerializedProperties
        [Header("インスペクター用の折りたたみスタイル（GUIStyle）")]
        GUIStyle FoldoutStyle;

        [Header("ヘルプボタンのGUIスタイル（GUIStyle）")]
        GUIStyle HelpButtonStyle;

        [Header("エディタ用アイコン（AnimationProfileEditor 用）")]
        Texture AnimationProfileEditorIcon;

        [Header("Animator の Culling Mode／Animator Controller の SerializedProperty 参照")]
        SerializedProperty AnimatorCullingModeProp, AIAnimatorProp;

        //Bool
        [Header("折りたたみ状態やフラグ類（SerializedProperty 参照）")]
        SerializedProperty AnimationListsChangedProp, AnimationsUpdatedProp, WalkFoldout, RunFoldout, TurnFoldout, Type1CombatWalkFoldout, Type1CombatRunFoldout, Type1CombatTurnFoldout, EmotesFoldout,
            Type2CombatWalkFoldout, Type2CombatRunFoldout, Type2CombatTurnFoldout, Type1StrafeFoldout, Type2StrafeFoldout, Type1DodgeFoldout, Type2DodgeFoldout, Type1CoverFoldout, Type2CoverFoldout;
        SerializedProperty Type1CombatAnimationsFoldout, Type2CombatAnimationsFoldout, Type1EquipsFoldout, Type2EquipsFoldout, Type1AttacksFoldout, Type2AttacksFoldout, Type1IdleFoldout, Type2IdleFoldout, NonCombatAnimationsFoldout, NonCombatIdleFoldout, NonCombatDeathFoldout,
           AnimatorSettingsFoldout, NonCombatHitFoldout, Type1HitFoldout, Type2HitFoldout, Type1BlockFoldout, Type2BlockFoldout, Type1DeathFoldout, Type2DeathFoldout;

        //NonCombat
        [Header("非戦闘アニメーションの ReorderableList 参照（被弾/待機/感情/死亡）")]
        ReorderableList NonCombatHitAnimationList, NonCombatIdleAnimationList, EmoteAnimationList, NonCombatDeathAnimationList;

        //Type 1
        [Header("タイプ1（近接など）アニメーションの ReorderableList 参照（被弾/攻撃/死亡）")]
        ReorderableList Type1CombatHitAnimationList, Type1AttackAnimationList, Type1DeathAnimationList;

        //Type 2
        [Header("タイプ2（遠隔など）アニメーションの ReorderableList 参照（被弾/攻撃/死亡）")]
        ReorderableList Type2CombatHitAnimationList, Type2AttackAnimationList, Type2DeathAnimationList;

        [Header("被弾アニメーションの条件/クールダウン（タイプ1/タイプ2）")]
        SerializedProperty Type1HitConditionsProp, Type2HitConditionsProp, Type1HitAnimationCooldownProp, Type2HitAnimationCooldownProp;
        #endregion

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoCallback;
            if (AnimationProfileEditorIcon == null) AnimationProfileEditorIcon = Resources.Load("Editor Icons/EmeraldAnimation") as Texture;
            InitializeProperties();
            InitializeAnimationLists();

            AnimationProfile self = (AnimationProfile)target;

            if (self.AIAnimator != null)
            {
                self.FilePath = AssetDatabase.GetAssetPath(self.AIAnimator);
            }

            // ユーザーが Animation Profile に紐づく Animator Controller を削除してしまった場合のフェイルセーフ
            if (self.AnimatorControllerGenerated && self.AIAnimator == null)
            {
                Debug.LogError("「" + self.name + "」アニメーションプロファイルの Animator Controller を見失いました。おそらく誤って削除された可能性があります。アニメーションクリップの設定は保持されています。Animator Controller を再生成してください。");
                self.AnimatorControllerGenerated = false;
            }
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoCallback;
        }

        /// <summary>
        /// Undo 登録時のコールバック。これが呼ばれたら、変更を元に戻せるようアニメーションプロファイルを更新します。
        /// </summary>
        void UndoCallback()
        {
#if UNITY_EDITOR
            AnimationProfile self = (AnimationProfile)target;
            EmeraldAnimatorGenerator.GenerateAnimatorController(self);
#endif
        }

        void InitializeProperties()
        {
            AnimationListsChangedProp = serializedObject.FindProperty("AnimationListsChanged");
            AnimationsUpdatedProp = serializedObject.FindProperty("AnimationsUpdated");

            WalkFoldout = serializedObject.FindProperty("WalkFoldout");
            RunFoldout = serializedObject.FindProperty("RunFoldout");
            TurnFoldout = serializedObject.FindProperty("TurnFoldout");
            NonCombatDeathFoldout = serializedObject.FindProperty("NonCombatDeathFoldout");
            NonCombatAnimationsFoldout = serializedObject.FindProperty("NonCombatAnimationsFoldout");
            NonCombatIdleFoldout = serializedObject.FindProperty("NonCombatIdleFoldout");

            Type1CombatWalkFoldout = serializedObject.FindProperty("Type1CombatWalkFoldout");
            Type1CombatRunFoldout = serializedObject.FindProperty("Type1CombatRunFoldout");
            Type1CombatTurnFoldout = serializedObject.FindProperty("Type1CombatTurnFoldout");
            Type2CombatWalkFoldout = serializedObject.FindProperty("Type2CombatWalkFoldout");
            Type2CombatRunFoldout = serializedObject.FindProperty("Type2CombatRunFoldout");
            Type2CombatTurnFoldout = serializedObject.FindProperty("Type2CombatTurnFoldout");
            Type1StrafeFoldout = serializedObject.FindProperty("Type1StrafeFoldout");
            Type2StrafeFoldout = serializedObject.FindProperty("Type2StrafeFoldout");
            Type1DodgeFoldout = serializedObject.FindProperty("Type1DodgeFoldout");
            Type2DodgeFoldout = serializedObject.FindProperty("Type2DodgeFoldout");
            Type1CoverFoldout = serializedObject.FindProperty("Type1CoverFoldout");
            Type2CoverFoldout = serializedObject.FindProperty("Type2CoverFoldout");
            EmotesFoldout = serializedObject.FindProperty("EmotesFoldout");
            AnimatorSettingsFoldout = serializedObject.FindProperty("AnimatorSettingsFoldout");
            NonCombatHitFoldout = serializedObject.FindProperty("NonCombatHitFoldout");
            Type1HitFoldout = serializedObject.FindProperty("Type1HitFoldout");
            Type2HitFoldout = serializedObject.FindProperty("Type2HitFoldout");
            Type1BlockFoldout = serializedObject.FindProperty("Type1BlockFoldout");
            Type2BlockFoldout = serializedObject.FindProperty("Type2BlockFoldout");
            Type1DeathFoldout = serializedObject.FindProperty("Type1DeathFoldout");
            Type2DeathFoldout = serializedObject.FindProperty("Type2DeathFoldout");
            Type1CombatAnimationsFoldout = serializedObject.FindProperty("Type1CombatAnimationsFoldout");
            Type2CombatAnimationsFoldout = serializedObject.FindProperty("Type2CombatAnimationsFoldout");
            Type1EquipsFoldout = serializedObject.FindProperty("Type1EquipsFoldout");
            Type2EquipsFoldout = serializedObject.FindProperty("Type2EquipsFoldout");
            Type1AttacksFoldout = serializedObject.FindProperty("Type1AttacksFoldout");
            Type2AttacksFoldout = serializedObject.FindProperty("Type2AttacksFoldout");
            Type1IdleFoldout = serializedObject.FindProperty("Type1IdleFoldout");
            Type2IdleFoldout = serializedObject.FindProperty("Type2IdleFoldout");

            Type1HitConditionsProp = serializedObject.FindProperty("Type1HitConditions");
            Type2HitConditionsProp = serializedObject.FindProperty("Type2HitConditions");
            Type1HitAnimationCooldownProp = serializedObject.FindProperty("Type1HitAnimationCooldown");
            Type2HitAnimationCooldownProp = serializedObject.FindProperty("Type2HitAnimationCooldown");

            AnimatorCullingModeProp = serializedObject.FindProperty("AnimatorCullingMode");
            AIAnimatorProp = serializedObject.FindProperty("AIAnimator");
        }

        void InitializeAnimationLists()
        {
            // 非戦闘時の被弾アニメーション一覧
            NonCombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.HitList"), true, true, true, true);
            DrawAnimationList(NonCombatHitAnimationList);
            NonCombatHitAnimationList.onChangedCallback = (HitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // 非戦闘時の待機アニメーション一覧
            NonCombatIdleAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.IdleList"), true, true, true, true);
            DrawAnimationList(NonCombatIdleAnimationList);
            NonCombatIdleAnimationList.onChangedCallback = (IdleAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // 非戦闘時の死亡アニメーション一覧
            NonCombatDeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.DeathList"), true, true, true, true);
            DrawAnimationList(NonCombatDeathAnimationList);
            NonCombatDeathAnimationList.onChangedCallback = (DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ1の戦闘被弾アニメーション一覧
            Type1CombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.HitList"), true, true, true, true);
            DrawAnimationList(Type1CombatHitAnimationList);
            Type1CombatHitAnimationList.onChangedCallback = (Type1CombatHitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ2の戦闘被弾アニメーション一覧
            Type2CombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.HitList"), true, true, true, true);
            DrawAnimationList(Type2CombatHitAnimationList);
            Type2CombatHitAnimationList.onChangedCallback = (Type2CombatHitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ1の攻撃アニメーション一覧
            Type1AttackAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.AttackList"), true, true, true, true);
            DrawAnimationList(Type1AttackAnimationList);
            Type1AttackAnimationList.onChangedCallback = (Type1AttackAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ2の攻撃アニメーション一覧
            Type2AttackAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.AttackList"), true, true, true, true);
            DrawAnimationList(Type2AttackAnimationList);
            Type2AttackAnimationList.onChangedCallback = (RangedAttackAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ1の死亡アニメーション一覧
            Type1DeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.DeathList"), true, true, true, true);
            DrawAnimationList(Type1DeathAnimationList);
            Type1DeathAnimationList.onChangedCallback = (Type1DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // タイプ2の死亡アニメーション一覧
            Type2DeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.DeathList"), true, true, true, true);
            DrawAnimationList(Type2DeathAnimationList);
            Type2DeathAnimationList.onChangedCallback = (Type2DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            // エモートアニメーション一覧
            EmoteAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("EmoteAnimationList"), true, true, true, true);
            EmoteAnimationList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = EmoteAnimationList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("EmoteAnimationClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationID"), GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        AnimationListsChangedProp.boolValue = true;
                    }
                };

            EmoteAnimationList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   ID  " + "         エモートアニメーションクリップ", EditorStyles.boldLabel);
            };
            EmoteAnimationList.onChangedCallback = (EmoteAnimationList) =>
            {
                AnimationListsChangedProp.boolValue = true;
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            HelpButtonStyle = CustomEditorProperties.UpdateHelpButtonStyle();
            AnimationProfile self = (AnimationProfile)target;
            serializedObject.Update();

            UpdateAnimatorController(self);
            NoGeneratedAnimatorMessage(self);

            CustomEditorProperties.BeginScriptHeader("アニメーションプロファイル", AnimationProfileEditorIcon);

            EditorGUI.BeginDisabledGroup(!self.AnimatorControllerGenerated);
            EditorGUILayout.Space();
            AnimatorControllerSettings(self);
            EditorGUILayout.Space();
            NonCombatAnimations(self);
            EditorGUILayout.Space();
            Type1CombatAnimations(self);
            EditorGUILayout.Space();
            Type2CombatAnimations(self);
            EditorGUILayout.Space();
            EmoteAnimations(self);
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            UpdateEditor(self);
            serializedObject.ApplyModifiedProperties();

            CustomEditorProperties.EndScriptHeader();
        }

        /// <summary>
        /// 非戦闘時に関わるアニメーション全般を扱います。
        /// </summary>
        void NonCombatAnimations(AnimationProfile self)
        {
            NonCombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(NonCombatAnimationsFoldout.boolValue, "非戦闘アニメーション", true, FoldoutStyle);

            if (NonCombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("非戦闘アニメーション", "AI が戦闘中でも徘徊中でもない時に使用する全アニメーションを制御します。", true);
                CustomEditorProperties.BeginIndent(20);
                NonCombatIdleAnimations(self);
                EditorGUILayout.Space();
                NonCombatMovementAnimations(self);
                EditorGUILayout.Space();
                NonCombatCombatHitAnimations(self);
                EditorGUILayout.Space();
                NonCombatDeathAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// タイプ1武器（近接など）に関わる戦闘アニメーションをすべて扱います。
        /// </summary>
        void Type1CombatAnimations(AnimationProfile self)
        {
            Type1CombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatAnimationsFoldout.boolValue, "タイプ1 戦闘アニメーション", true, FoldoutStyle);

            if (Type1CombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("タイプ1 戦闘アニメーション", "タイプ1武器タイプ用の全戦闘アニメーションを制御します。武器タイプが1つだけなら、ここが戦闘アニメーションのデフォルト一式です。非戦闘と戦闘で同じアニメを使う場合、非戦闘アニメを戦闘スロットへ自動コピーする機能が使えます。", true);
                CustomEditorProperties.BeginIndent(20);
                Type1CombatIdleAnimations(self);
                EditorGUILayout.Space();
                Type1CombatMovement();
                EditorGUILayout.Space();
                Type1StrafeAnimations(self);
                EditorGUILayout.Space();
                Type1DodgeAnimations(self);
                EditorGUILayout.Space();
                Type1CoverAnimations(self);
                EditorGUILayout.Space();
                Type1CombatHitAnimations(self);
                EditorGUILayout.Space();
                Type1BlockAnimations(self);
                EditorGUILayout.Space();
                Type1DeathAnimations(self);
                EditorGUILayout.Space();
                Type1AttackAnimations(self);
                EditorGUILayout.Space();
                Type1EquipAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// タイプ2武器（遠隔など）に関わる戦闘アニメーションをすべて扱います。
        /// </summary>
        void Type2CombatAnimations(AnimationProfile self)
        {
            Type2CombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatAnimationsFoldout.boolValue, "タイプ2 戦闘アニメーション", true, FoldoutStyle);

            if (Type2CombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("タイプ2 戦闘アニメーション", "タイプ2武器タイプ用の全戦闘アニメーションを制御します。", true);
                CustomEditorProperties.BeginIndent(20);
                Type2CombatIdleAnimations(self);
                EditorGUILayout.Space();
                Type2CombatMovement();
                EditorGUILayout.Space();
                Type2StrafeAnimations(self);
                EditorGUILayout.Space();
                Type2DodgeAnimations(self);
                EditorGUILayout.Space();
                Type2CoverAnimations(self);
                EditorGUILayout.Space();
                Type2CombatHitAnimations(self);
                EditorGUILayout.Space();
                Type2BlockAnimations(self);
                EditorGUILayout.Space();
                Type2DeathAnimations(self);
                EditorGUILayout.Space();
                Type2AttackAnimations(self);
                EditorGUILayout.Space();
                Type2EquipAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatMovementAnimations(AnimationProfile self)
        {
            // 非戦闘時の移動
            WalkFoldout.boolValue = EditorGUILayout.Foldout(WalkFoldout.boolValue, "歩行アニメーション", true, FoldoutStyle);

            if (WalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進歩行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkForward, "前進（歩行）", "非戦闘時に前方へ歩く際に再生される歩行アニメーション。", 2, false, true);

                // 左歩行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkLeft, "左（歩行）", "非戦闘時に左へ歩く際に再生されるアニメーション。左歩行がない場合は前進歩行を流用できます。", 2, true, true);

                // 右歩行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkRight, "右（歩行）", "非戦闘時に右へ歩く際に再生されるアニメーション。右歩行がない場合は前進歩行を流用できます。", 0, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            RunFoldout.boolValue = EditorGUILayout.Foldout(RunFoldout.boolValue, "走行アニメーション", true, FoldoutStyle);

            if (RunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進走行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunForward, "前進（走行）", "非戦闘時に前方へ走る際に再生されるアニメーション。", 2, false, true);

                // 左走行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunLeft, "左（走行）", "非戦闘時に左へ走る際に再生されるアニメーション。左走行がない場合は前進走行を流用できます。", 2, true, true);

                // 右走行
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunRight, "右（走行）", "非戦闘時に右へ走る際に再生されるアニメーション。右走行がない場合は前進走行を流用できます。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            TurnFoldout.boolValue = EditorGUILayout.Foldout(TurnFoldout.boolValue, "その場旋回アニメーション", true, FoldoutStyle);

            if (TurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 左旋回
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.TurnLeft, "左回転（その場）", "非戦闘時にその場で左へ回転するアニメーションクリップ。", 2, true, true);

                // 右旋回
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.TurnRight, "右回転（その場）", "非戦闘時にその場で右へ回転するアニメーションクリップ。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
            // 非戦闘時の移動
        }

        void Type1CombatMovement()
        {
            AnimationProfile self = (AnimationProfile)target;
            Type1CombatWalkFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatWalkFoldout.boolValue, "戦闘時の歩行アニメーション（タイプ1）", true, FoldoutStyle);

            // タイプ1 戦闘歩行
            if (Type1CombatWalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkForward, "戦闘歩行（前進）", "戦闘中に前へ歩く際に再生される歩行アニメーション。", 2, false, true);

                // 左
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkLeft, "戦闘歩行（左）", "戦闘中に左へ歩く際に再生されるアニメーション。左がない場合は前進歩行で代用可能。", 2, true, true);

                // 右
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkRight, "戦闘歩行（右）", "戦闘中に右へ歩く際に再生されるアニメーション。右がない場合は前進歩行で代用可能。", 2, true, true);

                // 後退
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkBack, "戦闘歩行（後退）", "戦闘中に後ろへ歩く際に再生されるアニメーション。", 1, true, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type1CombatRunFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatRunFoldout.boolValue, "戦闘時の走行アニメーション（タイプ1）", true, FoldoutStyle);

            // 戦闘走行
            if (Type1CombatRunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunForward, "戦闘走行（前進）", "戦闘中に前へ走る際に再生されるアニメーション。", 2, false, true);

                // 左
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunLeft, "戦闘走行（左）", "戦闘中に左へ走る際に再生されるアニメーション。左がない場合は前進走行で代用可能。", 2, true, true);

                // 右
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunRight, "戦闘走行（右）", "戦闘中に右へ走る際に再生されるアニメーション。右がない場合は前進走行で代用可能。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type1CombatTurnFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatTurnFoldout.boolValue, "戦闘時のその場旋回（タイプ1）", true, FoldoutStyle);

            // 戦闘時その場旋回
            if (Type1CombatTurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 左回転
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.TurnLeft, "戦闘旋回（左）", "戦闘中にその場で左へ回転するアニメーション。", 2, true, true);

                // 右回転
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.TurnRight, "戦闘旋回（右）", "戦闘中にその場で右へ回転するアニメーション。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatMovement()
        {
            AnimationProfile self = (AnimationProfile)target;

            Type2CombatWalkFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatWalkFoldout.boolValue, "戦闘時の歩行アニメーション（タイプ2）", true, FoldoutStyle);

            // タイプ2 戦闘歩行
            if (Type2CombatWalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkForward, "戦闘歩行（前進）", "戦闘中に前へ歩く際に再生されるアニメーション。", 2, false, true);

                // 左
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkLeft, "戦闘歩行（左）", "戦闘中に左へ歩く際に再生されるアニメーション。左がない場合は前進歩行で代用可能。", 2, true, true);

                // 右
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkRight, "戦闘歩行（右）", "戦闘中に右へ歩く際に再生されるアニメーション。右がない場合は前進歩行で代用可能。", 2, true, true);

                // 後退
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkBack, "戦闘歩行（後退）", "戦闘中に後ろへ歩く際に再生されるアニメーション。", 1, true, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type2CombatRunFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatRunFoldout.boolValue, "戦闘時の走行アニメーション（タイプ2）", true, FoldoutStyle);

            // タイプ2 戦闘走行
            if (Type2CombatRunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 前進
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunForward, "戦闘走行（前進）", "戦闘中に前へ走る際に再生されるアニメーション。", 2, false, true);

                // 左
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunLeft, "戦闘走行（左）", "戦闘中に左へ走る際に再生されるアニメーション。左がない場合は前進走行で代用可能。", 2, true, true);

                // 右
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunRight, "戦闘走行（右）", "戦闘中に右へ走る際に再生されるアニメーション。右がない場合は前進走行で代用可能。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type2CombatTurnFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatTurnFoldout.boolValue, "戦闘時のその場旋回（タイプ2）", true, FoldoutStyle);

            // タイプ2 戦闘その場旋回
            if (Type2CombatTurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // 左回転
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.TurnLeft, "戦闘旋回（左）", "戦闘中にその場で左へ回転するアニメーション。", 2, true, true);

                // 右回転
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.TurnRight, "戦闘旋回（右）", "戦闘中にその場で右へ回転するアニメーション。", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1EquipAnimations(AnimationProfile self)
        {
            Type1EquipsFoldout.boolValue = EditorGUILayout.Foldout(Type1EquipsFoldout.boolValue, "装備／納刀アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1EquipsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("装備／納刀アニメーション", "AI の武器オブジェクトを装備・収納する際のアニメーションを制御します。ここにアニメを設定しない場合、この機能は無視されます。", true);

                CustomEditorProperties.TutorialButton("注意：この機能を使うには、装備と納刀のアニメーションに EquipWeapon と UnequipWeapon のアニメーションイベントを設定する必要があります。未設定の場合や手順が不明な場合はドキュメントをご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons");

                // 武器を抜く
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.PullOutWeapon, "武器を装備", "AI が武器を取り出すときに再生されるアニメーション。", 0, false, false);
                // 武器をしまう
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.PutAwayWeapon, "武器を収納", "AI が武器をしまうときに再生されるアニメーション。", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2EquipAnimations(AnimationProfile self)
        {
            Type2EquipsFoldout.boolValue = EditorGUILayout.Foldout(Type2EquipsFoldout.boolValue, "装備／納刀アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2EquipsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("装備／納刀アニメーション", "AI の武器オブジェクトを装備・収納する際のアニメーションを制御します。ここにアニメを設定しない場合、この機能は無視されます。", true);

                CustomEditorProperties.TutorialButton("注意：この機能を使うには、装備と納刀のアニメーションに EquipWeapon と UnequipWeapon のアニメーションイベントを設定する必要があります。未設定の場合や手順が不明な場合はドキュメントをご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons");

                // 武器を抜く
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.PullOutWeapon, "武器を装備", "AI が武器を取り出すときに再生されるアニメーション。", 0, false, false);
                // 武器をしまう
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.PutAwayWeapon, "武器を収納", "AI が武器をしまうときに再生されるアニメーション。", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1AttackAnimations(AnimationProfile self)
        {
            Type1AttacksFoldout.boolValue = EditorGUILayout.Foldout(Type1AttacksFoldout.boolValue, "攻撃アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1AttacksFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("攻撃アニメーション", "AI が戦闘中に使用する攻撃アニメーションを制御します。最大12個まで。攻撃アニメは『Loop Time』のチェックを外してください。", true);

                CustomEditorProperties.ImportantTutorialButton("注意：各攻撃アニメーションに CreateAbility のアニメーションイベントを手動で追加する必要があります。追加しないと攻撃が発動しません。手順はドキュメントを参照してください。",
                "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-managers/animation-viewer-manager/creating-attack-animation-events");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("タイプ1 攻撃アニメーション一覧", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("AI が戦闘中に使用する攻撃アニメーションを制御します。最大12個まで。攻撃アニメは『Loop Time』のチェックを外してください。", false);
                CustomEditorProperties.NoticeTextDescription("注意：このリストの並び替えや削除は、AI の『Type 1 Attacks』に生成される攻撃アニメーション名の順序に影響します（すでに割り当て済みの場合）。", false);

                EditorGUILayout.Space();
                Type1AttackAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.Type1Animations.AttackList.Count == 12)
                {
                    Type1AttackAnimationList.displayAdd = false;
                }
                else
                {
                    Type1AttackAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2AttackAnimations(AnimationProfile self)
        {
            Type2AttacksFoldout.boolValue = EditorGUILayout.Foldout(Type2AttacksFoldout.boolValue, "攻撃アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2AttacksFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("攻撃アニメーション", "AI が戦闘中に使用する攻撃アニメーションを制御します。最大12個まで。攻撃アニメは『Loop Time』のチェックを外してください。", true);

                CustomEditorProperties.TutorialButton("注意：各攻撃アニメーションに CreateAbility のアニメーションイベントを手動で追加する必要があります。追加しないと攻撃が発動しません。手順はドキュメントを参照してください。",
                "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-managers/animation-viewer-manager/creating-attack-animation-events");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("タイプ2 攻撃アニメーション一覧", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("AI が戦闘中に使用する攻撃アニメーションを制御します。最大12個まで。攻撃アニメは『Loop Time』のチェックを外してください。", false);

                EditorGUILayout.Space();
                Type2AttackAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否（タイプ2では6個上限の仕様に合わせる）
                if (self.Type2Animations.AttackList.Count == 6)
                {
                    Type2AttackAnimationList.displayAdd = false;
                }
                else
                {
                    Type2AttackAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatIdleAnimations(AnimationProfile self)
        {
            NonCombatIdleFoldout.boolValue = EditorGUILayout.Foldout(NonCombatIdleFoldout.boolValue, "待機アニメーション（非戦闘）", true, FoldoutStyle);

            if (NonCombatIdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("待機アニメーション", "AI が徘徊中に使用する待機アニメーションを制御します。", true);
                EditorGUILayout.LabelField("待機アニメーション一覧", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("徘徊や採食時など、ランダムに再生される待機アニメーションを制御します。最大6個まで。", false);
                NonCombatIdleAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.NonCombatAnimations.IdleList.Count == 6)
                {
                    NonCombatIdleAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatIdleAnimationList.displayAdd = true;
                }

                // 非戦闘時の基本待機アニメ
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.IdleStationary, "待機（非戦闘）", "デフォルトの待機アニメーションを制御します。", 0, false, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1CombatIdleAnimations(AnimationProfile self)
        {
            Type1IdleFoldout.boolValue = EditorGUILayout.Foldout(Type1IdleFoldout.boolValue, "待機アニメーション（戦闘・タイプ1）", true, FoldoutStyle);

            if (Type1IdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.IdleStationary, "戦闘待機", "戦闘モード中に再生される待機アニメーションを制御します。", 2, false, false);

                // タイプ1 警告待機
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.IdleWarning, "タイプ1 警告待機", "ターゲットが攻撃半径から離れない場合に警告として再生されるアニメーション。", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatIdleAnimations(AnimationProfile self)
        {
            Type2IdleFoldout.boolValue = EditorGUILayout.Foldout(Type2IdleFoldout.boolValue, "待機アニメーション（戦闘・タイプ2）", true, FoldoutStyle);

            if (Type2IdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.IdleStationary, "戦闘待機", "戦闘モード中に再生される遠隔向けの待機アニメーションを制御します。", 2, false, true);

                // タイプ2 警告待機
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.IdleWarning, "タイプ2 警告待機", "ターゲットが攻撃半径から離れない場合に警告として再生されるアニメーション。", 0, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatCombatHitAnimations(AnimationProfile self)
        {
            NonCombatHitFoldout.boolValue = EditorGUILayout.Foldout(NonCombatHitFoldout.boolValue, "被弾アニメーション（非戦闘）", true, FoldoutStyle);

            if (NonCombatHitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                EditorGUILayout.LabelField("被弾アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("非戦闘時にダメージを受けた際に再生されるアニメーションを制御します。", false);
                NonCombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.NonCombatAnimations.HitList.Count == 6)
                {
                    NonCombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1CombatHitAnimations(AnimationProfile self)
        {
            Type1HitFoldout.boolValue = EditorGUILayout.Foldout(Type1HitFoldout.boolValue, "被弾アニメーション（戦闘・タイプ1）", true, FoldoutStyle);

            if (Type1HitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // スタン
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.Stunned, "スタン", "スタン効果を受けた後に再生されるアニメーションを制御します。この状態では、被弾アニメーションがスタンアニメーションへブレンドされます（下記の『被弾可能状態』から Stunned を除外すれば無効化可能）。", 2, false, false);

                CustomEditorProperties.CustomPropertyField(Type1HitConditionsProp, "被弾可能状態", "どの状態をキャンセルして被弾アニメーションを再生できるかを制御します。※回避と装備中は自動的に対象外です。", false);
                CustomEditorProperties.CustomFloatSliderPropertyField(Type1HitAnimationCooldownProp, "被弾アニメのクールダウン", "次の被弾アニメーションを再生可能になるまでの秒数。", 0f, 4f, true);

                EditorGUILayout.LabelField("戦闘時の被弾アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("戦闘時にダメージを受けた際に再生されるアニメーションを制御します。", false);
                Type1CombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.Type1Animations.HitList.Count == 6)
                {
                    Type1CombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    Type1CombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatHitAnimations(AnimationProfile self)
        {
            Type2HitFoldout.boolValue = EditorGUILayout.Foldout(Type2HitFoldout.boolValue, "被弾アニメーション（戦闘・タイプ2）", true, FoldoutStyle);

            if (Type2HitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // スタン
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.Stunned, "スタン", "スタン効果を受けた後に再生されるアニメーションを制御します。この状態では、被弾アニメーションがスタンアニメーションへブレンドされます（下記の『被弾可能状態』から Stunned を除外すれば無効化可能）。", 2, false, false);

                CustomEditorProperties.CustomPropertyField(Type2HitConditionsProp, "被弾可能状態", "どの状態をキャンセルして被弾アニメーションを再生できるかを制御します。※回避と装備中は自動的に対象外です。", false);
                CustomEditorProperties.CustomFloatSliderPropertyField(Type2HitAnimationCooldownProp, "被弾アニメのクールダウン", "次の被弾アニメーションを再生可能になるまでの秒数。", 0f, 4f, true);

                EditorGUILayout.LabelField("戦闘時の被弾アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("戦闘時にダメージを受けた際に再生されるアニメーションを制御します。", false);
                Type2CombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.Type2Animations.HitList.Count == 6)
                {
                    Type2CombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    Type2CombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1BlockAnimations(AnimationProfile self)
        {
            Type1BlockFoldout.boolValue = EditorGUILayout.Foldout(Type1BlockFoldout.boolValue, "ガード関連アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1BlockFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // ガード待機
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.BlockIdle, "ガード待機", "ガード中にループ再生されるアニメーション。", 2, false, false);
                // ガード被弾
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.BlockHit, "ガード被弾", "ガード中に攻撃を受けた際に再生されるアニメーション。", 2, false, false);
                // リコイル
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.Recoil, "攻撃側リコイル", "ブロック対象に攻撃が当たった後、攻撃側に再生されるリコイルアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2BlockAnimations(AnimationProfile self)
        {
            Type2BlockFoldout.boolValue = EditorGUILayout.Foldout(Type2BlockFoldout.boolValue, "ガード関連アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2BlockFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // ガード
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.BlockIdle, "ガード", "ガード中に再生されるアニメーション。", 2, false, false);
                // ガード被弾
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.BlockHit, "ガード被弾", "ガード中に攻撃を受けた際に再生されるアニメーション。", 2, false, false);
                // リコイル
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.Recoil, "攻撃側リコイル", "ブロック対象に攻撃が当たった後、攻撃側に再生されるリコイルアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatDeathAnimations(AnimationProfile self)
        {
            NonCombatDeathFoldout.boolValue = EditorGUILayout.Foldout(NonCombatDeathFoldout.boolValue, "死亡アニメーション（非戦闘）", true, FoldoutStyle);

            if (NonCombatDeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("死亡アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("AI が死亡した際に再生されるアニメーションを制御します。注意：死亡アニメーションが一切設定されていない場合、ラグドール死亡が使用されます。", false);
                CustomEditorProperties.NoticeTextDescription("注意：死亡アニメーションがない場合、Unity の Ragdoll Wizard または他のラグドールツールで AI にラグドール設定が必要です。", false);
                NonCombatDeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.NonCombatAnimations.DeathList.Count == 6)
                {
                    NonCombatDeathAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatDeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1DeathAnimations(AnimationProfile self)
        {
            Type1DeathFoldout.boolValue = EditorGUILayout.Foldout(Type1DeathFoldout.boolValue, "死亡アニメーション（戦闘・タイプ1）", true, FoldoutStyle);

            if (Type1DeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("死亡アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("AI が死亡した際に再生されるアニメーションを制御します。注意：死亡アニメーションが一切設定されていない場合、ラグドール死亡が使用されます。", false);
                CustomEditorProperties.NoticeTextDescription("注意：死亡アニメーションがない場合、Unity の Ragdoll Wizard または他のラグドールツールで AI にラグドール設定が必要です。", false);
                Type1DeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.Type1Animations.DeathList.Count == 6)
                {
                    Type1DeathAnimationList.displayAdd = false;
                }
                else
                {
                    Type1DeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2DeathAnimations(AnimationProfile self)
        {
            Type2DeathFoldout.boolValue = EditorGUILayout.Foldout(Type2DeathFoldout.boolValue, "死亡アニメーション（戦闘・タイプ2）", true, FoldoutStyle);

            if (Type2DeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("死亡アニメーション", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("AI が死亡した際に再生されるアニメーションを制御します。", false);
                CustomEditorProperties.NoticeTextDescription("注意：死亡アニメーションがない場合、Unity の Ragdoll Wizard または他のラグドールツールで AI にラグドール設定が必要です。", false);
                Type2DeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                // 追加可否
                if (self.Type1Animations.DeathList.Count == 6)
                {
                    Type2DeathAnimationList.displayAdd = false;
                }
                else
                {
                    Type2DeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1StrafeAnimations(AnimationProfile self)
        {
            Type1StrafeFoldout.boolValue = EditorGUILayout.Foldout(Type1StrafeFoldout.boolValue, "平行移動（ストレイフ）アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1StrafeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 左ストレイフ
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.StrafeLeft, "左ストレイフ", "左へ平行移動する際のアニメーション。", 2, false, true);

                // 右ストレイフ
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.StrafeRight, "右ストレイフ", "右へ平行移動する際のアニメーション。", 2, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2StrafeAnimations(AnimationProfile self)
        {
            Type2StrafeFoldout.boolValue = EditorGUILayout.Foldout(Type2StrafeFoldout.boolValue, "平行移動（ストレイフ）アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2StrafeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 左ストレイフ
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.StrafeLeft, "左ストレイフ", "左へ平行移動する際のアニメーション。", 2, false, true);

                // 右ストレイフ
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.StrafeRight, "右ストレイフ", "右へ平行移動する際のアニメーション。", 2, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1DodgeAnimations(AnimationProfile self)
        {
            Type1DodgeFoldout.boolValue = EditorGUILayout.Foldout(Type1DodgeFoldout.boolValue, "回避アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1DodgeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 左回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeLeft, "回避（左）", "左へ回避する際のアニメーション。", 2, false, false);

                // 後退回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeBack, "回避（後ろ）", "後ろへ回避する際のアニメーション。", 2, false, false);

                // 右回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeRight, "回避（右）", "右へ回避する際のアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2DodgeAnimations(AnimationProfile self)
        {
            Type2DodgeFoldout.boolValue = EditorGUILayout.Foldout(Type2DodgeFoldout.boolValue, "回避アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2DodgeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // 左回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeLeft, "回避（左）", "左へ回避する際のアニメーション。", 2, false, false);

                // 後退回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeBack, "回避（後ろ）", "後ろへ回避する際のアニメーション。", 2, false, false);

                // 右回避
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeRight, "回避（右）", "右へ回避する際のアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1CoverAnimations(AnimationProfile self)
        {
            Type1CoverFoldout.boolValue = EditorGUILayout.Foldout(Type1CoverFoldout.boolValue, "カバー（遮蔽物）アニメーション（タイプ1）", true, FoldoutStyle);

            if (Type1CoverFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.ImportantTutorialButton("注意：この機能を使用するには、AI に Cover コンポーネントを追加し、Cover Node を配置しておく必要があります。未設定の場合はドキュメントをご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/cover-component");

                // カバー待機
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.CoverIdle, "カバー待機", "AI がカバー中に再生される待機アニメーション。しゃがみ等の姿勢を推奨。", 2, true, false);

                // カバー被弾
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.CoverHit, "カバー被弾", "AI がカバー中に被弾した際に再生されるアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CoverAnimations(AnimationProfile self)
        {
            Type2CoverFoldout.boolValue = EditorGUILayout.Foldout(Type2CoverFoldout.boolValue, "カバー（遮蔽物）アニメーション（タイプ2）", true, FoldoutStyle);

            if (Type2CoverFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.ImportantTutorialButton("注意：この機能を使用するには、AI に Cover コンポーネントを追加し、Cover Node を配置しておく必要があります。未設定の場合はドキュメントをご覧ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/cover-component");

                // カバー待機
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.CoverIdle, "カバー待機", "AI がカバー中に再生される待機アニメーション。しゃがみ等の姿勢を推奨。", 2, true, false);

                // カバー被弾
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.CoverHit, "カバー被弾", "AI がカバー中に被弾した際に再生されるアニメーション。", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void EmoteAnimations(AnimationProfile self)
        {
            EmotesFoldout.boolValue = EditorGUILayout.Foldout(EmotesFoldout.boolValue, "エモートアニメーション", true, FoldoutStyle);

            if (EmotesFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("エモートアニメーション", "AI がエモートとして使用できるアニメーションを制御します。", true);

                EditorGUILayout.LabelField("エモートアニメーション一覧", EditorStyles.boldLabel);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("PlayEmoteAnimation 関数を呼び出し、引数にエモートIDを渡したときに再生されるアニメーション群を設定します。各アニメーションの速度は speed パラメータで調整可能。最大10個まで。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EmoteAnimationList.DoLayoutList();

                // 追加可否
                if (self.EmoteAnimationList.Count == 10)
                {
                    EmoteAnimationList.displayAdd = false;
                }
                else
                {
                    EmoteAnimationList.displayAdd = true;
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AnimatorControllerSettings(AnimationProfile self)
        {
            AnimatorSettingsFoldout.boolValue = EditorGUILayout.Foldout(AnimatorSettingsFoldout.boolValue, "Animator 設定", true, FoldoutStyle);

            if (AnimatorSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Animator 設定", "アニメーションシステムは、変更に応じて AI の Animator Controller を自動更新するため、手動での適用は不要です。", true);

                DisplayAnimatorController();
                EditorGUILayout.PropertyField(AnimatorCullingModeProp, new GUIContent("Animator Culling Mode"));
                CustomEditorProperties.CustomHelpLabelField("この AI の Animator に適用する Culling Mode を制御します。アニメ死亡アニメを使用する場合は Always Animate 推奨（オフスクリーン死亡でTポーズのまま固まることがあるため）。", true);

                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("非戦闘アニメをタイプ1戦闘アニメへコピー"), GUILayout.Height(23)))
                {
                    CopyNonCombatAnimationsToType1(self);
                }
                if (GUILayout.Button(new GUIContent("非戦闘アニメをタイプ2戦闘アニメへコピー"), GUILayout.Height(23)))
                {
                    CopyNonCombatAnimationsToType2(self);
                }
                CustomEditorProperties.CustomHelpLabelField("非戦闘アニメ（待機・旋回・移動・被弾・死亡）を一括で戦闘アニメへコピーします。※タイプ1武器タイプにのみ有効。非戦闘と戦闘で同一アニメを使うAIに実用的です。", true);

                CopyAnimationProfileButton(self);
                RegenerateAnimatorControllerButton(self);
                ClearAnimatorControllerButton(self);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CheckForMissingAnimationsButton(AnimationProfile self)
        {
            CustomEditorProperties.NoticeTextTitleWithDescription("リマインダー", "有効化されているアニメーションスロットには必ずアニメーションを割り当ててください。未設定があるとエラーの原因になります。下の『不足アニメーションのチェック』ボタンを押すと、足りないアニメを Unity Console にログ出力できます。", false);

            GUI.backgroundColor = new Color(1.5f, 0f, 0f, 0.5f);
            if (GUILayout.Button("不足アニメーションをチェック", HelpButtonStyle, GUILayout.Height(23)))
            {
                CheckForMissingAnimations();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 変更があったときに Animator Controller を自動更新します。
        /// </summary>
        /// <param name="self"></param>
        void UpdateAnimatorController(AnimationProfile self)
        {
            // Unity Editor 内でのみ自動更新（ランタイム自動更新は不可）
#if UNITY_EDITOR
            if (self.AnimatorControllerGenerated && self.AIAnimator != null)
            {
                if (self.AnimationsUpdated || self.AnimationListsChanged)
                {
                    EmeraldAnimatorGenerator.GenerateAnimatorController(self);
                }
            }
#endif
        }

        void NoGeneratedAnimatorMessage(AnimationProfile self)
        {
            if (!self.AnimatorControllerGenerated)
            {
                EditorGUILayout.Space();
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.35f);
                EditorGUILayout.HelpBox("このアニメーションプロファイルには、まだ生成済みの Animator Controller がありません。下の『Animator Controller を作成』ボタンを押してから、アニメーションを適用してください。", MessageType.Warning);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
                CreateAnimatorControllerButton(self); // 「Animator Controller を作成」ボタンの描画
            }
        }

        void CheckForMissingAnimations()
        {
            // TODO: すべてではなく、必須アニメのみをチェックするように最適化する。
            // EmeraldAI.Internal.AnimationCheck.CheckForMissingAnimations(EmeraldComp);
        }

        void CreateAnimatorControllerButton(AnimationProfile self)
        {
            if (!self.AnimatorControllerGenerated || self.MissingRuntimeController)
            {
                if (GUILayout.Button("Animator Controller を作成", GUILayout.Height(23)))
                {
                    self.FilePath = EditorUtility.SaveFilePanelInProject("OverrideController として保存", "", "overrideController", "保存するファイル名を入力してください");
                    if (self.FilePath != string.Empty)
                    {
                        string UserFilePath = self.FilePath;
                        string SourceFilePath = AssetDatabase.GetAssetPath(Resources.Load("Emerald Animator Controller"));
                        AssetDatabase.CopyAsset(SourceFilePath, UserFilePath);
                        self.AIAnimator = AssetDatabase.LoadAssetAtPath(UserFilePath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                        EmeraldAnimatorGenerator.GenerateAnimatorController(self);
                        serializedObject.Update();
                        self.AnimatorControllerGenerated = true;
                        AnimationsUpdatedProp.boolValue = false;
                        EditorUtility.SetDirty(self);
                        self.MissingRuntimeController = false;
                    }
                }
            }
        }

        void DisplayAnimatorController()
        {
            EditorGUILayout.PropertyField(AIAnimatorProp, new GUIContent("Animator Controller"));
            CustomEditorProperties.CustomHelpLabelField("このアニメーションプロファイルに紐づく Animator Controller。実行時、これを使用している全AIへ適用されます。", true);
        }

        void RegenerateAnimatorControllerButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("現在の Animator Controller を再生成します。マスターの Emerald AI コントローラに変更があった場合、新しい内容へ更新できます。\n注意：手動で加えた変更は上書きされます。", false, new Color(145f, 145f, 0f, 0.6f), MessageType.Info);

            GUI.backgroundColor = new Color(1.5f, 1.3f, 0, 1f);
            if (GUILayout.Button("Animator Controller を再生成", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("Animator Controller を再生成しますか？", "この Animator Controller を再生成してよろしいですか？\n手動で加えた変更は上書きされます。この操作は元に戻せません。", "はい", "キャンセル"))
            {
                string SourceFilePath = AssetDatabase.GetAssetPath(Resources.Load("Emerald Animator Controller"));
                string ControllerPath = self.FilePath;
                AssetDatabase.CopyAsset(SourceFilePath, ControllerPath);
                var TempRuntimeAnimator = AssetDatabase.LoadAssetAtPath(ControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                self.AIAnimator = TempRuntimeAnimator;
                EmeraldAnimatorGenerator.GenerateAnimatorController(self); // アニメーションプロファイルの設定から Animator Controller を再生成
                ApplyRuntimeAnimatorController();
            }
            GUI.backgroundColor = Color.white;
        }

        void CopyNonCombatAnimationsToType1(AnimationProfile self)
        {
            if (EditorUtility.DisplayDialog("非戦闘アニメをコピーしますか？", "非戦闘アニメーションをタイプ1の戦闘アニメーションへコピーします。実行してよろしいですか？この操作は元に戻せません。", "OK", "キャンセル"))
            {
                self.Type1Animations.IdleStationary = new AnimationClass(self.NonCombatAnimations.IdleStationary.AnimationSpeed, self.NonCombatAnimations.IdleStationary.AnimationClip, self.NonCombatAnimations.IdleStationary.Mirror);

                self.Type1Animations.WalkForward = new AnimationClass(self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, self.NonCombatAnimations.WalkForward.Mirror);
                self.Type1Animations.WalkLeft = new AnimationClass(self.NonCombatAnimations.WalkLeft.AnimationSpeed, self.NonCombatAnimations.WalkLeft.AnimationClip, self.NonCombatAnimations.WalkLeft.Mirror);
                self.Type1Animations.WalkRight = new AnimationClass(self.NonCombatAnimations.WalkRight.AnimationSpeed, self.NonCombatAnimations.WalkRight.AnimationClip, self.NonCombatAnimations.WalkRight.Mirror);
                self.Type1Animations.WalkBack = new AnimationClass(-self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, false);

                self.Type1Animations.RunForward = new AnimationClass(self.NonCombatAnimations.RunForward.AnimationSpeed, self.NonCombatAnimations.RunForward.AnimationClip, self.NonCombatAnimations.RunForward.Mirror);
                self.Type1Animations.RunLeft = new AnimationClass(self.NonCombatAnimations.RunLeft.AnimationSpeed, self.NonCombatAnimations.RunLeft.AnimationClip, self.NonCombatAnimations.RunLeft.Mirror);
                self.Type1Animations.RunRight = new AnimationClass(self.NonCombatAnimations.RunRight.AnimationSpeed, self.NonCombatAnimations.RunRight.AnimationClip, self.NonCombatAnimations.RunRight.Mirror);

                self.Type1Animations.TurnLeft = new AnimationClass(self.NonCombatAnimations.TurnLeft.AnimationSpeed, self.NonCombatAnimations.TurnLeft.AnimationClip, self.NonCombatAnimations.TurnLeft.Mirror);
                self.Type1Animations.TurnRight = new AnimationClass(self.NonCombatAnimations.TurnRight.AnimationSpeed, self.NonCombatAnimations.TurnRight.AnimationClip, self.NonCombatAnimations.TurnRight.Mirror);

                self.Type1Animations.HitList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.HitList.Count; i++)
                {
                    self.Type1Animations.HitList.Add(new AnimationClass(self.NonCombatAnimations.HitList[i].AnimationSpeed, self.NonCombatAnimations.HitList[i].AnimationClip, self.NonCombatAnimations.HitList[i].Mirror));
                }

                self.Type1Animations.DeathList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.DeathList.Count; i++)
                {
                    self.Type1Animations.DeathList.Add(new AnimationClass(self.NonCombatAnimations.DeathList[i].AnimationSpeed, self.NonCombatAnimations.DeathList[i].AnimationClip, self.NonCombatAnimations.DeathList[i].Mirror));
                }

                serializedObject.Update();
                AnimationsUpdatedProp.boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void CopyNonCombatAnimationsToType2(AnimationProfile self)
        {
            if (EditorUtility.DisplayDialog("非戦闘アニメをコピーしますか？", "非戦闘アニメーションをタイプ2の戦闘アニメーションへコピーします。実行してよろしいですか？この操作は元に戻せません。", "OK", "キャンセル"))
            {
                self.Type2Animations.IdleStationary = new AnimationClass(self.NonCombatAnimations.IdleStationary.AnimationSpeed, self.NonCombatAnimations.IdleStationary.AnimationClip, self.NonCombatAnimations.IdleStationary.Mirror);

                self.Type2Animations.WalkForward = new AnimationClass(self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, self.NonCombatAnimations.WalkForward.Mirror);
                self.Type2Animations.WalkLeft = new AnimationClass(self.NonCombatAnimations.WalkLeft.AnimationSpeed, self.NonCombatAnimations.WalkLeft.AnimationClip, self.NonCombatAnimations.WalkLeft.Mirror);
                self.Type2Animations.WalkRight = new AnimationClass(self.NonCombatAnimations.WalkRight.AnimationSpeed, self.NonCombatAnimations.WalkRight.AnimationClip, self.NonCombatAnimations.WalkRight.Mirror);
                self.Type2Animations.WalkBack = new AnimationClass(-self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, false);

                self.Type2Animations.RunForward = new AnimationClass(self.NonCombatAnimations.RunForward.AnimationSpeed, self.NonCombatAnimations.RunForward.AnimationClip, self.NonCombatAnimations.RunForward.Mirror);
                self.Type2Animations.RunLeft = new AnimationClass(self.NonCombatAnimations.RunLeft.AnimationSpeed, self.NonCombatAnimations.RunLeft.AnimationClip, self.NonCombatAnimations.RunLeft.Mirror);
                self.Type2Animations.RunRight = new AnimationClass(self.NonCombatAnimations.RunRight.AnimationSpeed, self.NonCombatAnimations.RunRight.AnimationClip, self.NonCombatAnimations.RunRight.Mirror);

                self.Type2Animations.TurnLeft = new AnimationClass(self.NonCombatAnimations.TurnLeft.AnimationSpeed, self.NonCombatAnimations.TurnLeft.AnimationClip, self.NonCombatAnimations.TurnLeft.Mirror);
                self.Type2Animations.TurnRight = new AnimationClass(self.NonCombatAnimations.TurnRight.AnimationSpeed, self.NonCombatAnimations.TurnRight.AnimationClip, self.NonCombatAnimations.TurnRight.Mirror);

                self.Type2Animations.HitList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.HitList.Count; i++)
                {
                    self.Type2Animations.HitList.Add(new AnimationClass(self.NonCombatAnimations.HitList[i].AnimationSpeed, self.NonCombatAnimations.HitList[i].AnimationClip, self.NonCombatAnimations.HitList[i].Mirror));
                }

                self.Type2Animations.DeathList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.DeathList.Count; i++)
                {
                    self.Type2Animations.DeathList.Add(new AnimationClass(self.NonCombatAnimations.DeathList[i].AnimationSpeed, self.NonCombatAnimations.DeathList[i].AnimationClip, self.NonCombatAnimations.DeathList[i].Mirror));
                }

                serializedObject.Update();
                AnimationsUpdatedProp.boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// アニメーションプロファイルの RuntimeAnimatorController を、対象AIの Animator へ適用します。
        /// </summary>
        void ApplyRuntimeAnimatorController()
        {
            AnimationProfile self = (AnimationProfile)target;

            if (self.EmeraldAnimationComponent == null || self.EmeraldAnimationComponent.m_AnimationProfile != self)
                return;

            if (self.EmeraldAnimationComponent.AIAnimator != null && self.EmeraldAnimationComponent.AIAnimator.runtimeAnimatorController == null && self.EmeraldAnimationComponent.m_AnimationProfile != null && self.EmeraldAnimationComponent.m_AnimationProfile.AIAnimator != null)
                self.EmeraldAnimationComponent.AIAnimator.runtimeAnimatorController = self.EmeraldAnimationComponent.m_AnimationProfile.AIAnimator;
        }

        void CopyAnimationProfileButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("このアニメーションプロファイルを（アニメ設定を保持したまま）複製し、Animator Controller をクリアします。新規コントローラ生成のための下準備に使えます。", false, new Color(0.25f, 2f, 0f, 0.75f), MessageType.Info);

            GUI.backgroundColor = new Color(0.1f, 1.2f, 0f, 0.5f);
            if (GUILayout.Button("アニメーションプロファイルを複製", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("アニメーションプロファイルを複製しますか？", "このアニメーションプロファイルを複製してよろしいですか？この操作は元に戻せません。", "はい", "キャンセル"))
            {
                EmeraldAnimatorGenerator.CopyAnimationProfile(self);
            }
            GUI.backgroundColor = Color.white;
        }

        void ClearAnimatorControllerButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("現在の Animator Controller をクリアし、新しく作成できる状態にします。", false, new Color(1.5f, 0f, 0f, 0.75f), MessageType.Info);

            GUI.backgroundColor = new Color(1.5f, 0f, 0f, 0.5f);
            if (GUILayout.Button("Animator Controller をクリア", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("Animator Controller をクリアしますか？", "この Animator Controller をクリアしてよろしいですか？この操作は元に戻せません。", "はい", "キャンセル"))
            {
                self.AIAnimator = null;
                self.AnimatorControllerGenerated = false;
            }
            GUI.backgroundColor = Color.white;
        }

        void DrawAnimationList(ReorderableList ListRef)
        {
            ListRef.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = ListRef.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationSpeed"), GUIContent.none);

                    if (element.FindPropertyRelative("AnimationSpeed").floatValue == 0)
                    {
                        element.FindPropertyRelative("AnimationSpeed").floatValue = 1;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        AnimationListsChangedProp.boolValue = true;
                    }
                };

            ListRef.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   速度  " + "     クリップ", EditorStyles.boldLabel);
            };
        }

        /// <summary>
        /// Animation Profile から EmeraldAnimationComponent の参照をクリアします。
        /// </summary>
        void OnDestroy()
        {
            AnimationProfile self = (AnimationProfile)target;
            self.EmeraldAnimationComponent = null;
        }

        void UpdateEditor(AnimationProfile self)
        {
#if UNITY_EDITOR
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
        }
    }
}
