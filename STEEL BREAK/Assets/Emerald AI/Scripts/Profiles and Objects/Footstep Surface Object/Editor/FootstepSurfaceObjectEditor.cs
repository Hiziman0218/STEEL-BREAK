using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(FootstepSurfaceObject))]
    [CanEditMultipleObjects]
    public class FootstepSurfaceObjectEditor : Editor
    {
        [Header("インスペクター用：折りたたみ見出しの GUI スタイル")]
        GUIStyle FoldoutStyle;

        [Header("エディタ用アイコン（Resources/Editor Icons/EmeraldFootsteps）")]
        Texture FootstepsEditorIcon;

        [Header("インスペクター項目に対応する SerializedProperty 参照")]
        SerializedProperty HideSettingsFoldout, SurfaceSettingsFoldout, SurfaceType, SurfaceTexture, SurfaceTag, StepVolume, StepSounds, StepEffectTimeout, StepEffects, FootprintTimeout, Footprints;

        void OnEnable()
        {
            if (FootstepsEditorIcon == null) FootstepsEditorIcon = Resources.Load("Editor Icons/EmeraldFootsteps") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Variables
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            SurfaceSettingsFoldout = serializedObject.FindProperty("SurfaceSettingsFoldout");
            SurfaceType = serializedObject.FindProperty("SurfaceType");
            SurfaceTexture = serializedObject.FindProperty("SurfaceTextures");
            SurfaceTag = serializedObject.FindProperty("SurfaceTag");
            StepVolume = serializedObject.FindProperty("StepVolume");
            StepSounds = serializedObject.FindProperty("StepSounds");
            StepEffectTimeout = serializedObject.FindProperty("StepEffectTimeout");
            StepEffects = serializedObject.FindProperty("StepEffects");
            FootprintTimeout = serializedObject.FindProperty("FootprintTimeout");
            Footprints = serializedObject.FindProperty("Footprints");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            FootstepSurfaceObject self = (FootstepSurfaceObject)target;
            serializedObject.Update();

            // 見出し（日本語化）
            CustomEditorProperties.BeginScriptHeaderNew("フットステップ サーフェス設定", FootstepsEditorIcon, new GUIContent(), HideSettingsFoldout);

            EditorGUILayout.Space();
            FootstepSurfaceSettings(self);
            EditorGUILayout.Space();

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void FootstepSurfaceSettings(FootstepSurfaceObject self)
        {
            // セクション見出し（日本語化）
            SurfaceSettingsFoldout.boolValue = EditorGUILayout.Foldout(SurfaceSettingsFoldout.boolValue, "フットステップ サーフェス設定", true, FoldoutStyle);

            if (SurfaceSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                // セクション説明（日本語化）
                CustomEditorProperties.TextTitleWithDescription(
                    "フットステップ サーフェス設定",
                    "Footstep Surface Object の各種設定を制御します。各項目にカーソルを乗せるとツールチップで使い方の説明が表示されます。",
                    true
                );

                CustomEditorProperties.BeginIndent(12);

                EditorGUILayout.PropertyField(SurfaceType);

                if (self.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Tag)
                {
                    EditorGUILayout.PropertyField(SurfaceTag);
                }
                else if (self.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Texture)
                {
                    EditorGUILayout.PropertyField(SurfaceTexture);
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(StepVolume);

                EditorGUILayout.PropertyField(StepSounds);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(StepEffectTimeout);

                EditorGUILayout.PropertyField(StepEffects);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(FootprintTimeout);

                EditorGUILayout.PropertyField(Footprints);

                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}
