using UnityEngine;                                  // Unity �����^�C��API
using UnityEditor;                                  // �G�f�B�^�g��API�iEditor ���j
using UnityEditorInternal;                          // ReorderableList
using EmeraldAI.Utility;                            // Emerald �̃J�X�^���G�f�B�^�⏕

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(AttractModifier))]         // ���̃J�X�^���C���X�y�N�^�� AttractModifier �p

    // �y�N���X�T�v�zAttractModifierEditor�F
    //  AttractModifier �R���|�[�l���g�i���E�ՓˁE�J�X�^���Ăяo�����ŋߗ�AI��U���j��
    //  �C���X�y�N�^�Őݒ�ł���悤�ɂ���G�f�B�^�g���B
    //  ���C���[�A���a�A�N�[���_�E���A�g���K�[��ʁA���A�N�V�����A�T�E���h�ꗗ�Ȃǂ���{��UI�ŕҏW�\�ɂ��܂��B
    public class AttractModifierEditor : Editor
    {
        [Header("�܂肽���݌��o���̃X�^�C���iEditorGUI �p�j")]
        GUIStyle FoldoutStyle;                       // ���o���̕`��X�^�C��

        [Header("�w�b�_�[�ɕ\������A�C�R���iResources ����Ǎ��j")]
        Texture AttractModifierEditorIcon;           // �C���X�y�N�^�㕔�̃A�C�R��

        [Header("�Ώۃv���p�e�B�iSerializedProperty�j�ւ̎Q��")]
        SerializedProperty PlayerFactionProp,        // �v���C���[�h���i�C���f�b�N�X�j
                         RadiusProp,                 // �U�����a
                         MinVelocityProp,            // �ŏ����x�i�Փ˃g���K���j
                         ReactionCooldownSecondsProp,// ���A�N�V�����N�[���_�E���b
                         SoundCooldownSecondsProp,   // �T�E���h�N�[���_�E���b
                         EmeraldAILayerProp,         // ���m�Ώۂ� Emerald AI ���C���[
                         TriggerTypeProp,            // �g���K�[�^�C�v
                         AttractReactionProp,        // �A�g���N�g�ɗp���� ReactionObject
                         TriggerLayersProp,          // ������Փ˃��C���[
                         EnemyRelationsOnlyProp,     // �G�Ί֌W�̂�
                         HideSettingsFoldout,        // �S�̔�\��
                         AttractModifierFoldout;     // �ݒ�Z�N�V�����J��

        [Header("�g���K�[���ɍĐ�����T�E���h�̈ꗗ�iReorderableList�j")]
        ReorderableList TriggerSoundsList;           // �g���K�[�T�E���h�̃��X�g

        [Header("�h���f�[�^�iResources ����Ǎ��j")]
        EmeraldFactionData FactionData;              // �v���C���[�h�����̕\���Ɏg�p

        /// <summary>
        /// �i���{��j�G�f�B�^�L�������F�A�C�R���̃��[�h�A�e SerializedProperty �̃o�C���h�A�T�E���h���X�g�̏��������s���܂��B
        /// </summary>
        private void OnEnable()
        {
            if (AttractModifierEditorIcon == null) AttractModifierEditorIcon = Resources.Load("AttractModifier") as Texture; // ����̃G�f�B�^�A�C�R�������[�h

            // �e�t�B�[���h��ΏۃI�u�W�F�N�g�̃V���A���C�Y�ς݃v���p�e�B�֕R�t��
            RadiusProp = serializedObject.FindProperty("Radius");
            PlayerFactionProp = serializedObject.FindProperty("PlayerFaction.FactionIndex");
            MinVelocityProp = serializedObject.FindProperty("MinVelocity");
            ReactionCooldownSecondsProp = serializedObject.FindProperty("ReactionCooldownSeconds");
            SoundCooldownSecondsProp = serializedObject.FindProperty("SoundCooldownSeconds");
            EmeraldAILayerProp = serializedObject.FindProperty("EmeraldAILayer");
            TriggerTypeProp = serializedObject.FindProperty("TriggerType");
            AttractReactionProp = serializedObject.FindProperty("AttractReaction");
            TriggerLayersProp = serializedObject.FindProperty("TriggerLayers");
            EnemyRelationsOnlyProp = serializedObject.FindProperty("EnemyRelationsOnly");
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            AttractModifierFoldout = serializedObject.FindProperty("AttractModifierFoldout");
            FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            // Trigger Sounds�i���X�g�̏������j
            TriggerSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("TriggerSounds"), true, true, true, true);
            TriggerSoundsList.drawHeaderCallback = rect =>
            {
                // �p�� "Trigger Sounds List" �� ���{��u�g���K�[�T�E���h�ꗗ�v
                EditorGUI.LabelField(rect, "�g���K�[�T�E���h�ꗗ", EditorStyles.boldLabel);
            };
            TriggerSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = TriggerSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        /// <summary>
        /// �i���{��j�C���X�y�N�^�̃��C���`��F�w�b�_�[�A�ݒ�Z�N�V�����̕`����s���܂��B
        /// </summary>
        public override void OnInspectorGUI()
        {
            AttractModifier self = (AttractModifier)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();

            // �w�b�_�[�i�p�� "Attract Modifier" �� ���{��u�A�g���N�g���f�B�t�@�C�A�v�j
            CustomEditorProperties.BeginScriptHeaderNew("�A�g���N�g���f�B�t�@�C�A", AttractModifierEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                AttractModifierSettings();          // �ݒ�Z�N�V����
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties(); // �ύX��K�p
        }

        /// <summary>
        /// �i���{��j�A�g���N�g���f�B�t�@�C�A�̐ݒ�UI��`�悵�܂��B
        /// </summary>
        void AttractModifierSettings()
        {
            // �p�� "Attract Modifier Settings" �� ���{��u�A�g���N�g���f�B�t�@�C�A�ݒ�v
            AttractModifierFoldout.boolValue = EditorGUILayout.Foldout(AttractModifierFoldout.boolValue, "�A�g���N�g���f�B�t�@�C�A�ݒ�", true, FoldoutStyle);

            if (AttractModifierFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                // �^�C�g���������i�p�ꁨ���{��j
                CustomEditorProperties.TextTitleWithDescription(
                    "�A�g���N�g���f�B�t�@�C�A�ݒ�",
                    "���̃V�X�e���́A�w��͈͓��ɂ��� AI ��U�����A�ݒ肵���w�A�g���N�g���A�N�V�����x�����s���܂��B"
                  + "Attract Modifier ���A�^�b�`����Ă���I�u�W�F�N�g���U���̔������ɂȂ�܂��B"
                  + "�T�E���h���m�R���|�[�l���g�̋@�\���g�����A����̃I�u�W�F�N�g�E�ՓˁE�J�X�^���Ăяo���ɂ���āA���͂� AI �������񂹂���悤�ɂ��܂��B",
                    true
                );

                // �`���[�g���A���i�p�ꁨ���{��^�����N�͂��̂܂܁j
                CustomEditorProperties.TutorialButton(
                    "Attract Modifier �̎g�����`���[�g���A���͈ȉ����Q�Ƃ��Ă��������B",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component/using-an-attract-modifier"
                );

                // �e�v���p�e�B�i���x���Ɛ�������{�ꉻ�j
                CustomEditorProperties.CustomPropertyField(
                    EmeraldAILayerProp,
                    "Emerald AI �̃��C���[",
                    "AI ���g�p���Ă���wEmerald AI �p���C���[�x�ł��B���̃��C���[�ɑ����A���� Sound Detection �R���|�[�l���g������ AI �̂݌��o����܂��B",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    AttractReactionProp,
                    "�A�g���N�g���A�N�V����",
                    "���̏C���q���N��/�g���K�[���ꂽ�Ƃ��ɌĂяo�� Reaction Object �ł��B"
                  + "�iProject �r���[�ŉE�N���b�N �� Create > Emerald AI > Create > Reaction Object �ō쐬�ł��܂��j",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    EnemyRelationsOnlyProp,
                    "�G�Ί֌W�̂�",
                    "�v���C���[�ɑ΂��āw�G�iEnemy�j�x�֌W�� AI �̂݁A���� Attract Modifier ���󂯎��悤�ɐ��䂵�܂��B�����ɂ���ƁA�͈͓��̂��ׂĂ� AI ���ΏۂɂȂ�܂��B",
                    false
                );

                if (EnemyRelationsOnlyProp.boolValue)
                {
                    CustomEditorProperties.BeginIndent();
                    // "Player Faction" �� �u�v���C���[�h���v
                    PlayerFactionProp.intValue = EditorGUILayout.Popup("�v���C���[�h��", PlayerFactionProp.intValue, FactionData.FactionNameList.ToArray());
                    EditorGUILayout.LabelField("�v���C���[����������h���ł��B", EditorStyles.helpBox);
                    CustomEditorProperties.EndIndent();
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(
                    RadiusProp,
                    "���a",
                    "���� Attract Modifier �̌��ʔ͈͂ł��B�͈͓��� AI �̓g���K�[���� Reaction Object ���󂯎��܂��B",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    ReactionCooldownSecondsProp,
                    "���A�N�V�����N�[���_�E���i�b�j",
                    "�A�g���N�g���A�N�V�������ēx���s�ł���悤�ɂȂ�܂ł̕b���ł��B",
                    true
                );

                CustomEditorProperties.CustomPropertyField(
                    SoundCooldownSecondsProp,
                    "�T�E���h�N�[���_�E���i�b�j",
                    "�g���K�[�T�E���h���Đ��\�ɂȂ�܂ł̕b���ł��B",
                    true
                );

                if ((TriggerTypes)TriggerTypeProp.intValue == TriggerTypes.OnCollision)
                {
                    CustomEditorProperties.CustomPropertyField(
                        MinVelocityProp,
                        "�ŏ����x",
                        "�Փ˃g���K�[�^�C�v���ɁA�A�g���N�g���A�N�V�����𔭉΂��邽�߂ɕK�v�ȍŏ����Α��x�ł��B",
                        true
                    );
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(
                    TriggerTypeProp,
                    "�g���K�[�^�C�v",
                    "Attract Modifier ���ǂ̂悤�ɋN�����邩�𐧌䂵�܂��B",
                    false
                );

                // �e�g���K�[��ʂ̐����i�p�ꁨ���{��j
                if (TriggerTypeProp.intValue == (int)TriggerTypes.OnStart)
                {
                    EditorGUILayout.LabelField("OnStart - Start ���� Reaction Object �����s���A���� GameObject ��U�����Ƃ��܂��B", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnTrigger)
                {
                    EditorGUILayout.LabelField("OnTrigger - ���̃I�u�W�F�N�g�ɑ΂��āw�g���K�[�Փˁx�����������Ƃ��� Reaction Object �����s���܂��B�U�����͂��� GameObject �ł��B", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCollision)
                {
                    EditorGUILayout.LabelField("OnCollision - ���̃I�u�W�F�N�g�ɑ΂��āw��g���K�[�Փˁi�ʏ�̏Փˁj�x�����������Ƃ��� Reaction Object �����s���܂��B�U�����͂��� GameObject �ł��B", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCustomCall)
                {
                    EditorGUILayout.LabelField("OnCustomCall - AttractModifier �X�N���v�g���� ActivateAttraction �֐����Ă΂ꂽ�Ƃ��� Reaction Object �����s���܂��B�U�����͂��� GameObject �ł��B", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }

                GUILayout.Space(5);

                // ��񃁃b�Z�[�W�i�p�ꁨ���{��j
                EditorGUILayout.LabelField("�g���K�[�����𖞂����ƁA�w�g���K�[�T�E���h�ꗗ�x���烉���_����1�Đ�����܂��B", EditorStyles.helpBox);
                TriggerSoundsList.DoLayoutList();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// �i���{��j�V�[���r���[��Ō��ʔ͈́i���a�j��Ԃ����C���[�f�B�X�N�ŉ������܂��B
        /// </summary>
        void OnSceneGUI()
        {
            AttractModifier self = (AttractModifier)target;
            Handles.color = new Color(1f, 0f, 0, 1f);
            Handles.DrawWireDisc(self.transform.position, self.transform.up, (float)self.Radius, 3);
        }

        /// <summary>
        /// �i���{��j�g���K�[�Ɏg�p�\�ȃ��C���[��I������ UI ��`�悵�܂��iNothing �͕s�j�B
        /// </summary>
        void TriggerLayerMaskDrawer()
        {
            CustomEditorProperties.BeginIndent();
            CustomEditorProperties.CustomPropertyField(
                TriggerLayersProp,
                "�g���K�[���C���[",
                "���� Attract Modifier ���g���K�[�ł���Փ˃��C���[�𐧌䂵�܂��B",
                true
            );

            if (TriggerLayersProp.intValue == 0)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("�g���K�[���C���[�� LayerMask ���wNothing�x�ɐݒ肷�邱�Ƃ͂ł��܂���B", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            CustomEditorProperties.EndIndent();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}
