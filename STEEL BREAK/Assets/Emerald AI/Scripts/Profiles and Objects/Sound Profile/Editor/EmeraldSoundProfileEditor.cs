using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldSoundProfile))]
    [CanEditMultipleObjects]
    public class EmeraldSoundProfileEditor : Editor
    {
        [Header("ヘルプボタン用 GUI スタイル")]
        GUIStyle HelpButtonStyle;

        [Header("インスペクター折りたたみ見出し用 GUI スタイル")]
        GUIStyle FoldoutStyle;

        [Header("サウンドエディタ用アイコン（Resources/Editor Icons/EmeraldSounds）")]
        Texture SoundsEditorIcon;

        #region SerializedProperties
        // Ints
        [Header("待機サウンドの再生間隔（最小/最大秒）への SerializedProperty 参照")]
        SerializedProperty IdleSoundsSecondsMinProp, IdleSoundsSecondsMaxProp;

        // Floats
        [Header("各ボリューム設定（歩行/走行/ブロック/攻撃/死亡/待機/被弾/装備/納刀/警告/遠隔装備/遠隔納刀）への SerializedProperty 参照")]
        SerializedProperty WalkFootstepVolumeProp, RunFootstepVolumeProp, BlockVolumeProp, AttackVolumeProp, DeathVolumeProp, IdleVolumeProp, InjuredVolumeProp, InjuredSoundOddsProp,
            EquipVolumeProp, UnequipVolumeProp, WarningVolumeProp, RangedEquipVolumeProp, RangedUnequipVolumeProp;

        // Objects
        [Header("装備/納刀サウンド（タイプ1/タイプ2）クリップへの SerializedProperty 参照")]
        SerializedProperty SheatheWeaponProp, UnsheatheWeaponProp, RangedSheatheWeaponProp, RangedUnsheatheWeaponProp;

        // Bools
        [Header("各セクション折りたたみ状態（待機/足音/インタラクト/装備・納刀/攻撃/被弾/ブロック/警告/死亡）への SerializedProperty 参照")]
        SerializedProperty IdleSoundsFoldout, FootstepSoundsFoldout, InteractSoundsFoldout, EquipAndUnequipSoundsFoldout, AttackSoundsFoldout, InjuredSoundsFoldout, BlockSoundsFoldout, WarningSoundsFoldout, DeathSoundsFoldout;

        // List
        [Header("各サウンドリスト（攻撃/被弾/警告/死亡/足音/待機/ブロック）への SerializedProperty 参照")]
        SerializedProperty AttackSoundsProp, InjuredSoundsProp, WarningSoundsProp, DeathSoundsProp, FootStepSoundsProp, IdleSoundsProp, BlockingSoundsProp;

        // Reorderable List
        [Header("インタラクトサウンド（ID付き）用 ReorderableList 参照")]
        ReorderableList InteractSoundsList;
        #endregion

        void OnEnable()
        {
            if (SoundsEditorIcon == null) SoundsEditorIcon = Resources.Load("Editor Icons/EmeraldSounds") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            // Ints
            IdleSoundsSecondsMinProp = serializedObject.FindProperty("IdleSoundsSecondsMin");
            IdleSoundsSecondsMaxProp = serializedObject.FindProperty("IdleSoundsSecondsMax");

            // Floats
            WalkFootstepVolumeProp = serializedObject.FindProperty("WalkFootstepVolume");
            RunFootstepVolumeProp = serializedObject.FindProperty("RunFootstepVolume");
            BlockVolumeProp = serializedObject.FindProperty("BlockVolume");
            InjuredVolumeProp = serializedObject.FindProperty("InjuredVolume");
            InjuredSoundOddsProp = serializedObject.FindProperty("InjuredSoundOdds");
            AttackVolumeProp = serializedObject.FindProperty("AttackVolume");
            DeathVolumeProp = serializedObject.FindProperty("DeathVolume");
            EquipVolumeProp = serializedObject.FindProperty("EquipVolume");
            UnequipVolumeProp = serializedObject.FindProperty("UnequipVolume");
            RangedEquipVolumeProp = serializedObject.FindProperty("RangedEquipVolume");
            RangedUnequipVolumeProp = serializedObject.FindProperty("RangedUnequipVolume");
            IdleVolumeProp = serializedObject.FindProperty("IdleVolume");
            WarningVolumeProp = serializedObject.FindProperty("WarningVolume");

            // Bools
            IdleSoundsFoldout = serializedObject.FindProperty("IdleSoundsFoldout");
            FootstepSoundsFoldout = serializedObject.FindProperty("FootstepSoundsFoldout");
            InteractSoundsFoldout = serializedObject.FindProperty("InteractSoundsFoldout");
            EquipAndUnequipSoundsFoldout = serializedObject.FindProperty("EquipAndUnequipSoundsFoldout");
            AttackSoundsFoldout = serializedObject.FindProperty("AttackSoundsFoldout");
            InjuredSoundsFoldout = serializedObject.FindProperty("InjuredSoundsFoldout");
            BlockSoundsFoldout = serializedObject.FindProperty("BlockSoundsFoldout");
            WarningSoundsFoldout = serializedObject.FindProperty("WarningSoundsFoldout");
            DeathSoundsFoldout = serializedObject.FindProperty("DeathSoundsFoldout");

            // Objects
            SheatheWeaponProp = serializedObject.FindProperty("SheatheWeapon");
            UnsheatheWeaponProp = serializedObject.FindProperty("UnsheatheWeapon");
            RangedSheatheWeaponProp = serializedObject.FindProperty("RangedSheatheWeapon");
            RangedUnsheatheWeaponProp = serializedObject.FindProperty("RangedUnsheatheWeapon");

            // Lists
            AttackSoundsProp = serializedObject.FindProperty("AttackSounds");
            InjuredSoundsProp = serializedObject.FindProperty("InjuredSounds");
            WarningSoundsProp = serializedObject.FindProperty("WarningSounds");
            DeathSoundsProp = serializedObject.FindProperty("DeathSounds");
            FootStepSoundsProp = serializedObject.FindProperty("FootStepSounds");
            IdleSoundsProp = serializedObject.FindProperty("IdleSounds");
            BlockingSoundsProp = serializedObject.FindProperty("BlockingSounds");

            InitializeInteractSoundsList();
        }

        void InitializeInteractSoundsList()
        {
            // インタラクトサウンド
            InteractSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("InteractSounds"), true, true, true, true);
            InteractSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = InteractSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundEffectClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundEffectID"), GUIContent.none);
                };

            InteractSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   ID  " + "         インタラクトサウンド クリップ", EditorStyles.boldLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldSoundProfile self = (EmeraldSoundProfile)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeader("サウンドプロファイル", SoundsEditorIcon);

            EditorGUILayout.Space();
            IdleSounds(self);
            EditorGUILayout.Space();
            FootstepSounds(self);
            EditorGUILayout.Space();
            InteractSounds(self);
            EditorGUILayout.Space();
            EquipAndUnequipSounds(self);
            EditorGUILayout.Space();
            AttackSounds(self);
            EditorGUILayout.Space();
            InjuredSounds(self);
            EditorGUILayout.Space();
            BlockSounds(self);
            EditorGUILayout.Space();
            DeathSounds(self);
            EditorGUILayout.Space();
            WarningSounds(self);
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();

            CustomEditorProperties.EndScriptHeader();
        }

        void IdleSounds(EmeraldSoundProfile self)
        {
            IdleSoundsFoldout.boolValue = EditorGUILayout.Foldout(IdleSoundsFoldout.boolValue, "待機サウンド", true, FoldoutStyle);

            if (IdleSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "待機サウンド",
                    "非戦闘時に、最小／最大の待機秒数に基づいてランダムで再生されるサウンドを制御します。",
                    true
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), IdleVolumeProp, "待機サウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("待機サウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(IdleSoundsProp, "待機サウンド", "この AI が使用する待機サウンドの数を制御します。", true);

                if (self.IdleSounds.Count != 0)
                {
                    EditorGUILayout.Space();
                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), IdleSoundsSecondsMinProp, "待機サウンド最小間隔（秒）");
                    CustomEditorProperties.CustomHelpLabelField(
                        "次の待機サウンドを再生するまでに必要な最小秒数を制御します。この値は「待機サウンド最大間隔（秒）」と組み合わせてランダム化されます。",
                        true
                    );

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), IdleSoundsSecondsMaxProp, "待機サウンド最大間隔（秒）");
                    CustomEditorProperties.CustomHelpLabelField(
                        "次の待機サウンドを再生するまでに必要な最大秒数を制御します。この値は「待機サウンド最小間隔（秒）」と組み合わせてランダム化されます。",
                        true
                    );
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void FootstepSounds(EmeraldSoundProfile self)
        {
            FootstepSoundsFoldout.boolValue = EditorGUILayout.Foldout(FootstepSoundsFoldout.boolValue, "足音サウンド", true, FoldoutStyle);

            if (FootstepSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "足音サウンド",
                    "足音に使用するサウンドと設定を制御します。再生にはアニメーションイベントの追加が必要です。チュートリアルは下のボタンで確認できます。",
                    true
                );

                CustomEditorProperties.TutorialButton(
                    "注意：この機能を使用するには、WalkFootstepSound と／または RunFootstepSound のアニメーションイベントを手動で作成する必要があります。手順は『チュートリアルを見る』ボタンからご確認ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-footstep-sounds"
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WalkFootstepVolumeProp, "歩行足音の音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("歩行時の足音の音量を制御します。", false);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RunFootstepVolumeProp, "走行足音の音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("走行時の足音の音量を制御します。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomListPropertyField(FootStepSoundsProp, "足音サウンド", "この AI が使用する足音サウンドの数を制御します。", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InteractSounds(EmeraldSoundProfile self)
        {
            InteractSoundsFoldout.boolValue = EditorGUILayout.Foldout(InteractSoundsFoldout.boolValue, "インタラクトサウンド", true, FoldoutStyle);

            if (InteractSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "インタラクトサウンド",
                    "アニメーションイベント、またはサウンドエフェクトIDを用いたスクリプト呼び出しで再生できる各種サウンドです。クエスト、セリフ、アニメ効果音などに有用です。",
                    true
                );

                InteractSoundsList.DoLayoutList();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void EquipAndUnequipSounds(EmeraldSoundProfile self)
        {
            EquipAndUnequipSoundsFoldout.boolValue = EditorGUILayout.Foldout(EquipAndUnequipSoundsFoldout.boolValue, "装備／納刀サウンド", true, FoldoutStyle);

            if (EquipAndUnequipSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(
                    "装備／納刀サウンド",
                    "AI が武器を装備（抜刀）または納刀する際に再生されるサウンドを制御します。",
                    true
                );

                CustomEditorProperties.TutorialButton(
                    "注意：これらは EquipWeapon と UnequipWeapon のアニメーションイベントから自動的に呼び出されます。手順は『チュートリアルを見る』ボタンからご確認ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons"
                );

                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), EquipVolumeProp, "タイプ1 装備音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("装備サウンドの音量を制御します。", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), UnsheatheWeaponProp, "タイプ1 装備サウンド", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("AI が武器を装備（抜刀）するときに再生されるサウンドです。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), UnequipVolumeProp, "タイプ1 納刀音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("納刀サウンドの音量を制御します。", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), SheatheWeaponProp, "タイプ1 納刀サウンド", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("AI が武器を納刀するときに再生されるサウンドです。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RangedEquipVolumeProp, "タイプ2 装備音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("装備サウンドの音量を制御します。", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), RangedUnsheatheWeaponProp, "タイプ2 装備サウンド", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("AI が武器を装備（抜刀）するときに再生されるサウンドです。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RangedUnequipVolumeProp, "タイプ2 納刀音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("納刀サウンドの音量を制御します。", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), RangedSheatheWeaponProp, "タイプ2 納刀サウンド", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("AI が武器を納刀するときに再生されるサウンドです。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AttackSounds(EmeraldSoundProfile self)
        {
            AttackSoundsFoldout.boolValue = EditorGUILayout.Foldout(AttackSoundsFoldout.boolValue, "攻撃サウンド", true, FoldoutStyle);

            if (AttackSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "攻撃サウンド",
                    "AI が攻撃をトリガーした際に再生されるサウンド（咆哮・うなり・叫び 等）を制御します。アニメーションイベントの追加が必要です。チュートリアルは下記ボタンから。",
                    true
                );

                CustomEditorProperties.TutorialButton(
                    "注意：攻撃サウンドはアニメーションイベントから使用します。手順は『チュートリアルを見る』ボタンからご確認ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-attack-sounds"
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttackVolumeProp, "攻撃サウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("攻撃サウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(AttackSoundsProp, "攻撃サウンド", "この AI が使用する攻撃サウンドの数を制御します。", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InjuredSounds(EmeraldSoundProfile self)
        {
            InjuredSoundsFoldout.boolValue = EditorGUILayout.Foldout(InjuredSoundsFoldout.boolValue, "被弾サウンド", true, FoldoutStyle);

            if (InjuredSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "被弾サウンド",
                    "AI がダメージを受けた際に再生されるサウンド（うめき・うなり 等）を制御します。※エフェクト系の『Impact Sounds』とは別物で、Ability Object 側で再生される効果音ではありません。",
                    true
                );

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), InjuredSoundOddsProp, "被弾サウンドの発生確率（%）", 1, 100);
                CustomEditorProperties.CustomHelpLabelField("AI が被弾した際にサウンドを再生する確率を制御します。毎回再生させたくない場合に調整します。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), InjuredVolumeProp, "被弾サウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("被弾サウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(InjuredSoundsProp, "被弾サウンド", "この AI が使用する被弾サウンドの数を制御します。", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void BlockSounds(EmeraldSoundProfile self)
        {
            BlockSoundsFoldout.boolValue = EditorGUILayout.Foldout(BlockSoundsFoldout.boolValue, "ブロックサウンド", true, FoldoutStyle);

            if (BlockSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "ブロックサウンド",
                    "AI がブロック中にダメージを受けたときに再生されるサウンドを制御します。",
                    true
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), BlockVolumeProp, "ブロックサウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("ブロックサウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(
                    BlockingSoundsProp,
                    "ブロックサウンド",
                    "AI がブロック中に被弾した際に再生するサウンドを設定します。\n注意：本機能の動作には、ブロックの有効化と対応アニメーションの設定が必要です。",
                    true
                );
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void WarningSounds(EmeraldSoundProfile self)
        {
            WarningSoundsFoldout.boolValue = EditorGUILayout.Foldout(WarningSoundsFoldout.boolValue, "警告サウンド", true, FoldoutStyle);

            if (WarningSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "警告サウンド",
                    "AI が警告行動を行う際に再生されるサウンドを制御します。",
                    true
                );

                CustomEditorProperties.TutorialButton(
                    "注意：警告サウンドは『Cautious』ビヘイビアタイプかつ、Confidence が Coward より高い場合にのみ使用され、アニメーションイベント経由で再生されます。詳しくは『チュートリアルを見る』を参照してください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-warning-sounds"
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WarningVolumeProp, "警告サウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("警告サウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(WarningSoundsProp, "警告サウンド", "この AI が使用する警告サウンドの数を制御します。", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DeathSounds(EmeraldSoundProfile self)
        {
            DeathSoundsFoldout.boolValue = EditorGUILayout.Foldout(DeathSoundsFoldout.boolValue, "死亡サウンド", true, FoldoutStyle);

            if (DeathSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "死亡サウンド",
                    "AI が死亡したときに再生されるサウンドを制御します。",
                    true
                );

                CustomEditorProperties.TutorialButton(
                    "注意：死亡サウンドはアニメーションイベントで使用します。手順は『チュートリアルを見る』ボタンからご確認ください。",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-death-sounds"
                );

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DeathVolumeProp, "死亡サウンド音量", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("死亡サウンドの音量を制御します。", true);

                CustomEditorProperties.CustomListPropertyField(DeathSoundsProp, "死亡サウンド", "この AI が使用する死亡サウンドの数を制御します。", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 指定した ReorderableList を、与えられた表示名で描画します。
        /// </summary>
        void DrawSoundList(ReorderableList ListRef, string DisplayName)
        {
            ListRef.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, DisplayName, EditorStyles.boldLabel);
            };
            ListRef.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = ListRef.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }
    }
}
