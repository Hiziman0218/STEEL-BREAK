using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAbilityObject), true)]
    [CanEditMultipleObjects]
    public class EmeraldAbilityObjectEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（GUIStyle）")]
        GUIStyle FoldoutStyle;

        [Header("派生クラスの公開インスタンスフィールド一覧（反射で取得）")]
        FieldInfo[] CustomFields;

        [Header("インスペクター基本情報（SerializedProperty 参照）: 能力名/アイコン/各折りたたみ表示/モジュール折りたたみ/クールダウン/条件/召喚")]
        SerializedProperty AbilityName, AbilityIcon, DerivedSettingsFoldout, InfoSettingsFoldout, HideSettingsFoldout, ModularSettingsFoldout, CooldownSettings, ConditionSettings, SummonSettings;

        [Header("各モジュール設定の SerializedProperty 参照（近接/プロジェクタイル/矢/グレネード/一般弾/銃弾/空中/地上/弾幕/テレポート/ホーミング/ターゲット種別/拡散/コライダー/生成/チャージ/AOE/ダメージ/スタン/回復/ノックバック）")]
        SerializedProperty MeleeSettings, ProjectileSettings, ArrowProjectileSettings, GrenadeSettings, GeneralProjectileSettings, BulletProjectileSettings, AerialProjectileSettings, GroundProjectileSettings, BarrageProjectileSettings, TeleportSettings, HomingSettings, TargetTypeSettings,
            SpreadSettings, ColliderSettings, CreateSettings, ChargeSettings, AreaOfEffectSettings, DamageSettings, StunnedSettings, HealingSettings, KnockbackSettings;

        [Header("ツールチップ文（日本語）: チャージ/生成/近接/プロジェクタイル/一般弾/銃弾/地上/テレポート/ホーミング/空中/ターゲット種別/拡散/コライダー/AOE/スタン/ノックバック/ダメージ/回復/グレネード/クールダウン/条件/召喚")]
        string ChargeSettingsTooltip = "アビリティのチャージ（詠唱）時に、エフェクトやサウンドを再生できるようにします。エフェクトの位置は、ChargeEffect アニメーションイベントから渡される Attack Transform 名で決まります。この設定を使うには Enabled を true にしてください。\n\n注意：発火には ChargeEffect のアニメーションイベントが必須です。AI の攻撃アニメ内で EmeraldAttack イベントより前に設定してください。";
        string CreateSettingsTooltip = "アビリティが『生成された瞬間』に、エフェクトやサウンドを再生できるようにします。エフェクトの位置は CreateAbility で渡される Attack Transform 名で決まります。この設定を使うには Enabled を true にしてください。";
        string MeleeSettingsTooltip = "指定した角度と距離の範囲内でダメージを与えられるようにします。\n\n注意：近接攻撃アニメが Weapon Collision Events を使用している場合、角度と距離の設定は無視され、武器コリジョンの衝突判定に依存します。";
        string ProjectileSettingsTooltip = "このアビリティで使用する主なエフェクトを制御します。\n\n注意：右クリックでこのモジュールをコピーし、他のプロジェクタイル系モジュールへ貼り付けて共有できます。Projectile Effect は必須で、未設定の項目は使用時に無視されます。";
        string GeneralProjectileSettingsTooltip = "プロジェクタイルを指定ターゲットへ向かって移動させます。各種魔法やロケット等に利用できます。";
        string BulletProjectileSettingsTooltip = "弾丸のようなプロジェクタイルを、指定ターゲットへ非常に高速で移動させます。";
        string GroundProjectileSettingsTooltip = "プロジェクタイルを地面へアラインし、地表に沿って移動させます。\n\n注意：この設定は本アビリティの Projectile Settings に依存します。";
        string TeleportSettingsTooltip = "所有者を、指定ターゲットの半径内へテレポートさせます。";
        string HomingSettingsTooltip = "プロジェクタイルをターゲットソースへホーミングさせます。";
        string AerialProjectileSettingsTooltip = "作成者またはターゲットの『上空』から、カスタマイズ可能な半径でプロジェクタイルをスポーンさせます。\n\n注意：この設定は本アビリティの Projectile Settings に依存します。";
        string TargetTypeSettingsTooltip = "このアビリティの想定ターゲットの取得元（現在ターゲット/ランダム/複数 等）を制御します。";
        //string BranchSettingsTooltip = "Allows projectiles the chance to branch to other nearby targets after they have collided with the ability's Target Source. The effect for this is based on this ability's Projectile Module.";
        string SpreadSettingsTooltip = "プロジェクタイルをスポーンソースから X/Y 方向に拡散させます。";
        string ColliderSettingsTooltip = "Projectile Effect に Sphere Collider を自動付与し、オブジェクトやターゲットとの衝突を有効化します。\n\n注意：Projectile Effect にコライダーが既に存在する場合は自動生成されず、既存のコライダーが使用されます。";
        string AreaOfEffectSettingsTooltip = "指定半径内の範囲（AOE）にアビリティ効果を与えます。";
        string StunSettingsTooltip = "ヒットに成功したターゲットへ、確率でスタン効果を与えます。";
        string KnockbackSettingsTooltip = "ヒットに成功したターゲットへ、確率でノックバック効果を与えます。";
        string DamageSettingsTooltip = "ヒットに成功したターゲットへダメージを与えます。";
        string HealSettingsTooltip = "味方AIターゲットを回復します。";
        string GrenadeSettingsTooltip = "グレネードアビリティの各種設定を制御します。";
        string CooldownSettingsTooltip = "アビリティにクールダウンを設定し、再使用間隔（Cooldown Length）を超えるまで使用できないようにします。";
        string ConditionSettingsTooltip = "条件モジュールを使うアビリティは、条件が満たされたときのみ発動します。High Priority の条件は AI の Pick Type を無視して優先的に選ばれます（条件が満たされている場合）。\n\n注意：条件が満たされない場合、このアビリティはスキップされます。";
        string SummonSettingsTooltip = "召喚アビリティの設定を制御します。\n\n注意：召喚アビリティは Emerald AI エージェントのみをスポーンできます。";


        void OnEnable()
        {
            EmeraldAbilityObject self = (EmeraldAbilityObject)target;
            if (self.AbilityIcon == null) self.AbilityIcon = Resources.Load("Editor Icons/EmeraldAbility") as Texture2D; // AbilityIcon が null の場合はデフォルトアイコンをロード

            DerivedSettingsFoldout = serializedObject.FindProperty("DerivedSettingsFoldout");
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            InfoSettingsFoldout = serializedObject.FindProperty("InfoSettingsFoldout");
            ModularSettingsFoldout = serializedObject.FindProperty("ModularSettingsFoldout");
            AbilityName = serializedObject.FindProperty("AbilityName");
            AbilityIcon = serializedObject.FindProperty("AbilityIcon");
            CooldownSettings = serializedObject.FindProperty("CooldownSettings");
            ConditionSettings = serializedObject.FindProperty("ConditionSettings");
            MeleeSettings = serializedObject.FindProperty("MeleeSettings");
            ProjectileSettings = serializedObject.FindProperty("ProjectileSettings");
            BulletProjectileSettings = serializedObject.FindProperty("BulletProjectileSettings");
            GeneralProjectileSettings = serializedObject.FindProperty("GeneralProjectileSettings");
            GrenadeSettings = serializedObject.FindProperty("GrenadeSettings");
            ArrowProjectileSettings = serializedObject.FindProperty("ArrowProjectileSettings");
            AerialProjectileSettings = serializedObject.FindProperty("AerialProjectileSettings");
            GroundProjectileSettings = serializedObject.FindProperty("GroundProjectileSettings");
            BarrageProjectileSettings = serializedObject.FindProperty("BarrageProjectileSettings");
            TeleportSettings = serializedObject.FindProperty("TeleportSettings");
            HomingSettings = serializedObject.FindProperty("HomingSettings");
            TargetTypeSettings = serializedObject.FindProperty("TargetTypeSettings");
            SpreadSettings = serializedObject.FindProperty("SpreadSettings");
            ColliderSettings = serializedObject.FindProperty("ColliderSettings");
            CreateSettings = serializedObject.FindProperty("CreateSettings");
            ChargeSettings = serializedObject.FindProperty("ChargeSettings");
            AreaOfEffectSettings = serializedObject.FindProperty("AreaOfEffectSettings");
            DamageSettings = serializedObject.FindProperty("DamageSettings");
            StunnedSettings = serializedObject.FindProperty("StunnedSettings");
            KnockbackSettings = serializedObject.FindProperty("KnockbackSettings");
            HealingSettings = serializedObject.FindProperty("HealingSettings");
            SummonSettings = serializedObject.FindProperty("SummonSettings");

            // 親クラスに属さない（派生クラス固有の）パブリック変数を全取得
            CustomFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            EmeraldAbilityObject self = (EmeraldAbilityObject)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();
            CustomEditorProperties.BeginScriptHeaderNew(self.AbilityName, self.AbilityIcon, new GUIContent(), HideSettingsFoldout, false);

            EditorGUILayout.Space();
            InfoSettings(self);
            EditorGUILayout.Space();
            DerivedSettings(self);
            EditorGUILayout.Space();

            CustomEditorProperties.EndScriptHeader();

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

            serializedObject.ApplyModifiedProperties();
        }

        void InfoSettings(EmeraldAbilityObject self)
        {
            InfoSettingsFoldout.boolValue = EditorGUILayout.Foldout(InfoSettingsFoldout.boolValue, "情報設定", true, FoldoutStyle);

            if (InfoSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("情報設定", "アビリティの名前・説明・アイコンを設定します。", true);

                EditorGUILayout.PropertyField(AbilityName);
                CustomEditorProperties.CustomHelpLabelField("このアビリティの名前。", true);

                self.AbilityDescription = CustomEditorProperties.CustomDescriptionField(self, "アビリティ説明", self.AbilityDescription);
                CustomEditorProperties.CustomHelpLabelField("このアビリティの説明。", true);

                EditorGUILayout.PropertyField(AbilityIcon);
                CustomEditorProperties.CustomHelpLabelField("このアビリティのアイコン。空の場合はデフォルトアイコンが使用されます。", true);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 派生クラスのカスタム変数（親クラス以外）を、専用のスタイルでまとめて表示します。
        /// </summary>
        void DerivedSettings(EmeraldAbilityObject self)
        {
            DerivedSettingsFoldout.boolValue = EditorGUILayout.Foldout(DerivedSettingsFoldout.boolValue, self.AbilityName + " 設定", true, FoldoutStyle);

            if (DerivedSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(self.AbilityName + " 設定", self.AbilityDescription, true);

                if (ChargeSettings != null) DrawModule(ChargeSettings, "チャージモジュール", ChargeSettingsTooltip);
                if (CreateSettings != null) DrawModule(CreateSettings, "生成モジュール", CreateSettingsTooltip);

                if (CooldownSettings != null) DrawModule(CooldownSettings, "クールダウンモジュール", CooldownSettingsTooltip, false);

                if (ConditionSettings != null) DrawConditionModule(ConditionSettings, "発動条件モジュール", ConditionSettingsTooltip, false);

                if (SummonSettings != null) DrawSummonModule(SummonSettings, "召喚モジュール", SummonSettingsTooltip, true);

                if (MeleeSettings != null) DrawModule(MeleeSettings, "近接モジュール", MeleeSettingsTooltip, true);

                if (ColliderSettings != null) DrawModule(ColliderSettings, "コライダーモジュール", ColliderSettingsTooltip, true);
                if (TargetTypeSettings != null) DrawModule(TargetTypeSettings, "ターゲット種別モジュール", TargetTypeSettingsTooltip, true);
                if (ProjectileSettings != null) DrawModule(ProjectileSettings, "プロジェクタイル演出モジュール", ProjectileSettingsTooltip, true);
                if (GeneralProjectileSettings != null) DrawModule(GeneralProjectileSettings, "一般プロジェクタイルモジュール", GeneralProjectileSettingsTooltip, true);
                if (GrenadeSettings != null) DrawModule(GrenadeSettings, "グレネードモジュール", GrenadeSettingsTooltip, true);
                if (BulletProjectileSettings != null) DrawModule(BulletProjectileSettings, "弾丸プロジェクタイルモジュール", BulletProjectileSettingsTooltip, true);
                if (ArrowProjectileSettings != null) DrawModule(ArrowProjectileSettings, "矢プロジェクタイルモジュール", "", true);
                if (AerialProjectileSettings != null) DrawModule(AerialProjectileSettings, "空中プロジェクタイルモジュール", AerialProjectileSettingsTooltip, true);
                if (GroundProjectileSettings != null) DrawModule(GroundProjectileSettings, "地上プロジェクタイルモジュール", GroundProjectileSettingsTooltip, true);
                if (BarrageProjectileSettings != null) DrawModule(BarrageProjectileSettings, "弾幕プロジェクタイルモジュール", "", true);
                if (TeleportSettings != null) DrawModule(TeleportSettings, "テレポートモジュール", TeleportSettingsTooltip, true);
                if (AreaOfEffectSettings != null) DrawModule(AreaOfEffectSettings, "範囲効果（AOE）モジュール", AreaOfEffectSettingsTooltip, true);

                if (HomingSettings != null) DrawModule(HomingSettings, "ホーミングモジュール", HomingSettingsTooltip);
                //if (BranchSettings != null) DrawModule(BranchSettings, "Branch Module", BranchSettingsTooltip);
                if (SpreadSettings != null) DrawModule(SpreadSettings, "拡散モジュール", SpreadSettingsTooltip);
                if (DamageSettings != null) DrawDamageModule(DamageSettings, "ダメージモジュール", DamageSettingsTooltip);
                if (StunnedSettings != null) DrawModule(StunnedSettings, "スタンモジュール", StunSettingsTooltip);
                if (KnockbackSettings != null) DrawModule(KnockbackSettings, "ノックバックモジュール", KnockbackSettingsTooltip);
                if (HealingSettings != null) DrawHealingModule(HealingSettings, "回復モジュール", HealSettingsTooltip, true);


                foreach (FieldInfo field in CustomFields)
                {
                    // すべてのフィールドを取得して Ability Object Editor スタイルで描画する。
                    // ただし EmeraldAI.AbilityData 由来のクラスは別で処理するため除外。
                    var TypeInfo = field.FieldType.GetTypeInfo();
                    string Namespace = TypeInfo.Namespace;
                    var DeclaringType = TypeInfo.DeclaringType;
                    string ClassInfo = "";

                    if (DeclaringType != null)
                    {
                        ClassInfo = DeclaringType.ToString();
                    }

                    // 配列には余白を追加してオフセット表示
                    if (field.FieldType.GetElementType() != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    // List には余白を追加してオフセット表示
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (field.FieldType.IsClass && Namespace != "UnityEngine" && Namespace != "System" && ClassInfo != "EmeraldAI.AbilityData")
                    {
                        CustomEditorProperties.BeginFoldoutWindowBox();

                        if (serializedObject.FindProperty(field.Name).FindPropertyRelative("Enabled") == null)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Toggle(true, GUILayout.Width(28));
                                EditorGUI.EndDisabledGroup();
                                EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.Space(5);
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.BeginHorizontal();
                                var Style = new GUIStyle(EditorStyles.radioButton);
                                serializedObject.FindProperty(field.Name).FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(serializedObject.FindProperty(field.Name).FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                                EditorGUILayout.EndHorizontal();

                            }
                            GUILayout.Space(5);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(2.5f);
                        CustomEditorProperties.EndFoldoutWindowBox();
                    }
                    // 単体の変数はオフセットせずに表示
                    else
                    {
                        if (ClassInfo != "EmeraldAI.AbilityData")
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                    }
                    GUILayout.Space(2.5f);
                }
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// AbilityData 由来のプロパティを、モジュール用の折りたたみUIとして描画します。
        /// </summary>
        void DrawModule(SerializedProperty property, string Name, string Tooltip, bool Required = false)
        {
            CustomEditorProperties.BeginFoldoutWindowBox();
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal();
                if (!Required)
                {
                    property.FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(property.FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                    EditorGUILayout.PropertyField(property, new GUIContent(Name, "(任意) " + Tooltip));
                }
                else if (Required)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle(true, GUILayout.Width(28));
                    property.FindPropertyRelative("Enabled").boolValue = true;
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.PropertyField(property, new GUIContent(Name, "(必須) " + Tooltip));
                }

                EditorGUILayout.EndHorizontal();

            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
            GUILayout.Space(2.5f);
            CustomEditorProperties.EndFoldoutWindowBox();
            GUILayout.Space(2.5f);
        }

        /// <summary>
        /// AbilityData 由来（ダメージ）のプロパティを、モジュール用の折りたたみUIとして描画します。
        /// </summary>
        void DrawDamageModule(SerializedProperty property, string Name, string Tooltip, bool Required = false)
        {
            CustomEditorProperties.BeginFoldoutWindowBox();
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal();
                if (!Required)
                {
                    property.FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(property.FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                    property.FindPropertyRelative("Foldout").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("Foldout").boolValue, new GUIContent(Name, "(任意) " + Tooltip), true);
                }
                else if (Required)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle(true, GUILayout.Width(28));
                    property.FindPropertyRelative("Enabled").boolValue = true;
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.PropertyField(property, new GUIContent(Name, "(必須) " + Tooltip));
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (property.FindPropertyRelative("Foldout").boolValue)
            {
                // Base Damage
                GUILayout.Space(5);
                CustomEditorProperties.BeginIndent(45);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("BaseDamageSettings"));
                CustomEditorProperties.EndIndent();
                // Base Damage

                // Critical Hits
                CustomEditorProperties.BeginIndent(45);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("UseCriticalHits"));
                EditorGUI.BeginDisabledGroup(!property.FindPropertyRelative("UseCriticalHits").boolValue);
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.BeginIndent(15);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("CriticalHitSettings"));
                CustomEditorProperties.EndIndent();
                CustomEditorProperties.EndFoldoutWindowBox();
                EditorGUI.EndDisabledGroup();
                CustomEditorProperties.EndIndent();
                // Critical Hits

                // Damage Over Time
                CustomEditorProperties.BeginIndent(45);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("UseDamageOverTime"));
                EditorGUI.BeginDisabledGroup(!property.FindPropertyRelative("UseDamageOverTime").boolValue);
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.BeginIndent(15);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DamageOverTimeSettings"));
                CustomEditorProperties.EndIndent();
                CustomEditorProperties.EndFoldoutWindowBox();
                EditorGUI.EndDisabledGroup();
                CustomEditorProperties.EndIndent();
                // Damage Over Time
            }

            GUILayout.Space(2.5f);
            CustomEditorProperties.EndFoldoutWindowBox();
            GUILayout.Space(2.5f);
        }

        /// <summary>
        /// AbilityData 由来（回復）のプロパティを、モジュール用の折りたたみUIとして描画します。
        /// </summary>
        void DrawHealingModule(SerializedProperty property, string Name, string Tooltip, bool Required = false)
        {
            CustomEditorProperties.BeginFoldoutWindowBox();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                property.FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(property.FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                property.FindPropertyRelative("Foldout").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("Foldout").boolValue, new GUIContent(Name, "(必須) " + Tooltip), true);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (property.FindPropertyRelative("Foldout").boolValue)
            {
                CustomEditorProperties.BeginIndent(45);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("TargetType"));
                GUILayout.Space(2.5f);
                CustomEditorProperties.BeginIndent(15);
                if (property.FindPropertyRelative("TargetType").intValue == 0) //Self
                {

                }
                else if (property.FindPropertyRelative("TargetType").intValue == 1) //Target
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("Radius"));
                }
                else if (property.FindPropertyRelative("TargetType").intValue == 2) //Area
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("Radius"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("Delay"));
                }
                CustomEditorProperties.EndIndent();
                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealingEffect"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealingEffectTimeoutSeconds"));
                if (property.FindPropertyRelative("TargetType").intValue == 2) //Area
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("EffectHeightOffset"));
                }

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealingType"));
                GUILayout.Space(2.5f);
                CustomEditorProperties.BeginIndent(15);
                if (property.FindPropertyRelative("HealingType").intValue == 0)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("BaseHealAmount"));
                }
                else if (property.FindPropertyRelative("HealingType").intValue == 1)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("BaseHealAmount"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("HealsPerTick"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("TickRate"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("HealOverTimeLength"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("HealTickSounds"));
                }
                CustomEditorProperties.EndIndent();
                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealTargetEffect"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealTargetEffectTimeoutSeconds"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("HealingSoundsList"));
                CustomEditorProperties.EndIndent();
            }

            GUILayout.Space(2.5f);
            CustomEditorProperties.EndFoldoutWindowBox();
            GUILayout.Space(2.5f);
        }

        /// <summary>
        /// AbilityData 由来（召喚）のプロパティを、モジュール用の折りたたみUIとして描画します。
        /// </summary>
        void DrawSummonModule(SerializedProperty property, string Name, string Tooltip, bool Required = false)
        {
            CustomEditorProperties.BeginFoldoutWindowBox();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                property.FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(property.FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                property.FindPropertyRelative("Foldout").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("Foldout").boolValue, new GUIContent(Name, "(必須) " + Tooltip), true);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (property.FindPropertyRelative("Foldout").boolValue)
            {
                CustomEditorProperties.BeginIndent(45);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("CastEffect"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("CastEffectTimeoutSeconds"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("CastSounds"));

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonEffect"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonEffectTimeoutSeconds"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonEffectHeightOffset"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonSounds"));

                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonAmount"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonPosition"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonRadius"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonDelay"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("AIPrefabs"));
                GUILayout.Space(15);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("IsTimedSummon"));
                GUILayout.Space(2.5f);
                if (property.FindPropertyRelative("IsTimedSummon").boolValue)
                {
                    CustomEditorProperties.BeginIndent(15);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("SummonLength"));
                    CustomEditorProperties.EndIndent();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5f);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.PropertyField(property.FindPropertyRelative("DespawnAfterKilled"));
                GUILayout.Space(2.5f);
                if (property.FindPropertyRelative("DespawnAfterKilled").boolValue)
                {
                    CustomEditorProperties.BeginIndent(15);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("DespawnLength"));
                    CustomEditorProperties.EndIndent();
                }
                GUILayout.Space(15);

                CustomEditorProperties.EndIndent();
            }

            GUILayout.Space(2.5f);
            CustomEditorProperties.EndFoldoutWindowBox();
            GUILayout.Space(2.5f);
        }

        /// <summary>
        /// AbilityData 由来（条件判定）のプロパティを、モジュール用の折りたたみUIとして描画します。
        /// </summary>
        void DrawConditionModule(SerializedProperty property, string Name, string Tooltip, bool Required = false)
        {
            CustomEditorProperties.BeginFoldoutWindowBox();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginHorizontal();
                property.FindPropertyRelative("Enabled").boolValue = EditorGUILayout.Toggle(property.FindPropertyRelative("Enabled").boolValue, GUILayout.Width(28));
                property.FindPropertyRelative("Foldout").boolValue = EditorGUILayout.Foldout(property.FindPropertyRelative("Foldout").boolValue, new GUIContent(Name, "(任意) " + Tooltip), true);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (property.FindPropertyRelative("Foldout").boolValue)
            {
                CustomEditorProperties.BeginIndent(45);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("HighPriority"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ConditionType"));

                GUILayout.Space(2.5f);
                if ((ConditionTypes)property.FindPropertyRelative("ConditionType").enumValueIndex == ConditionTypes.DistanceFromTarget)
                {
                    CustomEditorProperties.BeginIndent(15);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("ValueCompareType"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("DistanceFromTarget"));
                    CustomEditorProperties.EndIndent();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5f);
                    EditorGUILayout.EndVertical();
                }
                else if ((ConditionTypes)property.FindPropertyRelative("ConditionType").enumValueIndex == ConditionTypes.AllyLowHealth ||
                    (ConditionTypes)property.FindPropertyRelative("ConditionType").enumValueIndex == ConditionTypes.SelfLowHealth)
                {
                    CustomEditorProperties.BeginIndent(15);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("LowHealthPercentage"));
                    CustomEditorProperties.EndIndent();
                    EditorGUILayout.BeginVertical();
                    GUILayout.Space(5f);
                    EditorGUILayout.EndVertical();
                }

                CustomEditorProperties.EndIndent();
            }

            GUILayout.Space(2.5f);
            CustomEditorProperties.EndFoldoutWindowBox();
            GUILayout.Space(2.5f);
        }

        void SetModule(SerializedProperty property, bool State)
        {
            property.FindPropertyRelative("Enabled").boolValue = State;
        }
    }
}
