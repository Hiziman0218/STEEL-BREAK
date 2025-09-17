using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EmeraldAI.Utility
{
    public class CustomEditorProperties
    {
        public static void DisplaySetupWarning(string WarningMessage)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-12);
            GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
            EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        public static void DisplayWarningMessage(string WarningMessage)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.HelpBox(WarningMessage, MessageType.Warning);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            GUI.backgroundColor = Color.white;
        }

        public static void DisplayImportantHeaderMessage(string WarningMessage)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-12);
            GUI.backgroundColor = new Color(10f, 10f, 0.0f, 0.5f);
            EditorGUILayout.HelpBox(WarningMessage, MessageType.Info);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        public static void DisplayImportantMessage(string WarningMessage)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(10f, 10f, 0.0f, 0.5f);
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.HelpBox(WarningMessage, MessageType.Info);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// AI の「頭部（Head）」に該当する Transform を自動的に探索します。
        /// </summary>
        public static void AutoFindHeadTransform(Rect position, GUIContent label, SerializedProperty property, Transform AIRef)
        {
            CustomHelpLabelField("AI の Head（頭）に該当する Transform を自動的に探索します。見つかった場合は正しいか確認してください。適切な Transform が見つからない場合は何も適用されないため、手動で割り当ててください。", false);
            if (GUILayout.Button("ヘッドの自動検出"))
            {
                // AI 内のすべての Transform を調べ、「root」という語を含むボーンを探す
                foreach (Transform root in AIRef.GetComponentsInChildren<Transform>())
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // root と名の付くボーン直下（子インデックス0〜3まで）だけを見る
                        if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT")
                        {
                            // その直下から「head」を含む Transform を検索
                            foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                            {
                                if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD"))
                                {
                                    property.objectReferenceValue = t;
                                }
                            }
                        }
                    }
                }

                // 頭部が見つからなかった場合：モデルに root ボーンがない可能性があるため、条件を緩めて再探索
                if (property.objectReferenceValue == null)
                {
                    foreach (Transform t in AIRef.GetComponentsInChildren<Transform>())
                    {
                        // Animation Rigging の MultiAimConstraint が付いた Transform は無視（拘束用の可能性があるため）
                        if (t.GetComponent<UnityEngine.Animations.Rigging.MultiAimConstraint>() == null)
                        {
                            // 「head」を含む Transform 名を探索
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD"))
                            {
                                property.objectReferenceValue = t;
                            }
                        }
                    }
                }
            }
        }

        public static void CustomListFloatSlider(Rect position, GUIContent label, SerializedProperty property, float MinValue, float MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Slider(position, label, property.floatValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomListIntSlider(Rect position, GUIContent label, SerializedProperty property, int MinValue, int MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.IntSlider(position, label, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomListPopup(Rect position, GUIContent label, SerializedProperty property, string nameOfLabel, string[] names)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            string[] enumNamesList = names;
            var newValue = EditorGUI.Popup(position, property.intValue, enumNamesList);

            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static string CustomDescriptionField(UnityEngine.Object self, string DescriptionName, string Description)
        {
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(DescriptionName);
            string Value = EditorGUILayout.TextArea(Description, style);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(self, "アクションの説明を変更");
                Description = Value;
            }
            EditorGUILayout.Space();

            return Description;
        }

        /// <summary>
        /// 説明文付きの PropertyField を描画します（必要に応じてスペースを追加）。
        /// </summary>
        public static void CustomPropertyField(SerializedProperty Property, string Name, string TextInfo, bool UseSpace)
        {
            EditorGUILayout.PropertyField(Property, new GUIContent(Name));
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            if (TextInfo != "") EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace) EditorGUILayout.Space();
        }

        /// <summary>
        /// 説明文付きのリスト PropertyField を描画します（必要に応じてスペースを追加）。
        /// </summary>
        public static void CustomListPropertyField(SerializedProperty Property, string Name, string TextInfo, bool UseSpace)
        {
            GUILayout.Space(5);
            GUILayout.Box(new GUIContent("これは何？", TextInfo + "\n\n注: 下のリストはフォールドアウトをクリックすると開きます。"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(Property, new GUIContent(Name));
            GUILayout.Space(2);
            EditorGUILayout.EndHorizontal();
            if (UseSpace) EditorGUILayout.Space();
        }

        /// <summary>
        /// 説明文付きのカスタム文字列フィールドを描画します（必要に応じてスペースを追加）。
        /// </summary>
        public static void CustomStringPropertyField(SerializedProperty property, string Name, string Description, bool UseSpace)
        {
            GUIContent label = EditorGUI.BeginProperty(new Rect(), GUIContent.none, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.TextField(Name, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = newValue;

            EditorGUI.EndProperty();

            if (Description != string.Empty)
            {
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField(Description, EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            if (UseSpace) EditorGUILayout.Space();
        }

        /// <summary>
        /// 説明文付きのカスタム float スライダーを描画します（必要に応じてスペースを追加）。
        /// </summary>
        public static void CustomFloatSliderPropertyField(SerializedProperty property, string Name, string Description, float MinValue, float MaxValue, bool UseSpace)
        {
            GUIContent label = EditorGUI.BeginProperty(new Rect(), GUIContent.none, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Slider(Name, property.floatValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newValue;

            EditorGUI.EndProperty();

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField(Description, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace) EditorGUILayout.Space();
        }

        /// <summary>
        /// 説明文付きのカスタム int スライダーを描画します（必要に応じてスペースを追加）。
        /// </summary>
        public static void CustomIntSliderPropertyField(SerializedProperty property, string Name, string Description, int MinValue, int MaxValue, bool UseSpace)
        {
            GUIContent label = EditorGUI.BeginProperty(new Rect(), GUIContent.none, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntSlider(Name, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.LabelField(Description, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace) EditorGUILayout.Space();
        }

        public static void CustomHelpLabelFieldWithType(string TextInfo, bool UseSpace, Color color, MessageType messageType)
        {
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            GUI.backgroundColor = color;
            EditorGUILayout.HelpBox(TextInfo, messageType);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
        }

        public static void TextTitle(string TitleText)
        {
            GUI.backgroundColor = new Color(1f, 155f, 1f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TitleText, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }

        public static void BoldTextTitle(string TitleText)
        {
            GUIStyle TextStyle = EditorStyles.boldLabel;
            TextStyle.fontStyle = FontStyle.Bold;
            TextStyle.fontSize = 12;
            TextStyle.normal.textColor = Color.white;
            EditorGUILayout.LabelField(TitleText, TextStyle);
        }

        public static void TextTitleWithDescription(string TitleText, string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(1f, 155f, 1f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TitleText, EditorStyles.boldLabel);

            EditorGUILayout.LabelField(TextInfo, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static GUIStyle UpdateEditorStyles()
        {
            GUIStyle FoldoutStyle = EditorStyles.foldout;
            FoldoutStyle.fontStyle = FontStyle.Bold;
            FoldoutStyle.fontSize = 12;
            if (EditorGUIUtility.isProSkin)
            {
                FoldoutStyle.active.textColor = Color.white;
                FoldoutStyle.focused.textColor = Color.white;
                FoldoutStyle.onHover.textColor = Color.white;
                FoldoutStyle.normal.textColor = Color.white;
                FoldoutStyle.onNormal.textColor = Color.white;
                FoldoutStyle.onActive.textColor = Color.white;
                FoldoutStyle.onFocused.textColor = Color.white;
            }
            else
            {
                FoldoutStyle.active.textColor = Color.black;
                FoldoutStyle.focused.textColor = Color.black;
                FoldoutStyle.onHover.textColor = Color.black;
                FoldoutStyle.normal.textColor = Color.black;
                FoldoutStyle.onNormal.textColor = Color.black;
                FoldoutStyle.onActive.textColor = Color.black;
                FoldoutStyle.onFocused.textColor = Color.black;
            }
            Color myStyleColor = Color.white;
            return FoldoutStyle;
        }

        public static GUIStyle UpdateTitleStyle()
        {
            GUIStyle TitleStyle = EditorStyles.wordWrappedLabel;
            TitleStyle.fontStyle = FontStyle.Bold;
            TitleStyle.fontSize = 16;
            TitleStyle.alignment = TextAnchor.MiddleLeft;
            TitleStyle.padding.left = 10;
            TitleStyle.padding.top = -16;
            TitleStyle.normal.textColor = Color.white;
            return TitleStyle;
        }

        public static GUIStyle UpdateHelpButtonStyle()
        {
            GUIStyle HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;
            return HelpButtonStyle;
        }

        public static void BeginScriptHeader(string TitleText, Texture Icon)
        {
            GUILayout.Space(8); // 上部に余白を追加

            GUIStyle Style = EditorStyles.wordWrappedLabel;
            Style.fontStyle = FontStyle.Bold;
            Style.fontSize = 16;
            Style.alignment = TextAnchor.MiddleLeft;
            Style.padding.left = 10;
            Style.padding.top = -15;
            Style.normal.textColor = Color.white;

            BeginIndent(-7); // ヘッダボックスのオフセット

            // ヘッダー
            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);

            EditorGUILayout.BeginVertical("Window");

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + TitleText, Icon), Style, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = Color.white;
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // 全体ボックス
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-22);
            GUI.backgroundColor = Color.white;
            BeginIndent(15); // 要素のオフセット
        }

        public static bool BeginScriptHeaderNew(string TitleText, Texture Icon, GUIContent label, SerializedProperty property, bool DisplayHideButton = true)
        {
            GUILayout.Space(8); // 上部に余白を追加

            string ButtonText = " 隠す ";
            GUIStyle Style = EditorStyles.wordWrappedLabel;
            Style.fontStyle = FontStyle.Bold;
            Style.fontSize = 16;
            Style.alignment = TextAnchor.MiddleLeft;
            Style.padding.left = 10;
            Style.padding.top = -20;
            Style.normal.textColor = Color.white;

            BeginIndent(-7); // ヘッダボックスのオフセット

            // ヘッダー
            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);

            EditorGUILayout.BeginVertical("Window");

            EditorGUILayout.BeginHorizontal(); // 追加
            //EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + TitleText, Icon), Style, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = Color.white;
            //EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            // ボタンスタイル
            var ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.fontStyle = FontStyle.Bold;

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-13f);
            if (property.boolValue)
            {
                ButtonText = " 表示 ";
            }

            if (property.boolValue)
                GUI.backgroundColor = new Color(0.7f, 1.25f, 0.7f, 0.75f);
            else
                GUI.backgroundColor = Color.white;

            if (DisplayHideButton)
            {
                if (GUILayout.Button(ButtonText, ButtonStyle, GUILayout.Height(25), GUILayout.ExpandWidth(false)))
                {
                    property.boolValue = !property.boolValue;
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            // 全体ボックス
            if (property.boolValue)
                GUI.backgroundColor = new Color(1f, 0.85f, 0.85f, 1f);
            else
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-22);

            // 現在のスクリプトウィンドウが最小化されている旨の説明を表示
            if (property.boolValue)
            {
                GUILayout.Space(3);
                EditorGUILayout.LabelField("  設定は非表示  ");
            }

            BeginIndent(15); // 要素のオフセット
            return property.boolValue;
        }

        public static void EndScriptHeader()
        {
            EndIndent();
            EndIndent();
            GUILayout.Space(7); // 右側に余白を追加
            EditorGUILayout.EndVertical();

            GUILayout.Space(6);  // 下部に余白を追加
        }

        public static void BeginFoldoutWindowBox()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-19);
        }

        public static void EndFoldoutWindowBox()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        public static void TextTitleWithDescriptionAndColor(string TitleText, string TextInfo, bool UseSpace, Color color)
        {
            GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TitleText, EditorStyles.boldLabel);

            EditorGUILayout.LabelField(TextInfo, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static void NoticeTextTitleWithDescription(string TitleText, string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(155f, 125f, 1f, 0.45f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TitleText, EditorStyles.boldLabel);

            EditorGUILayout.LabelField(TextInfo, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static void NoticeTextDescription(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(155f, 125f, 1f, 0.45f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TextInfo, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static void WarningTextTitleWithDescription(string TitleText, string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(155f, 1f, 1f, 0.35f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(TitleText, EditorStyles.boldLabel);

            EditorGUILayout.LabelField(TextInfo, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        public static void BeginIndent(int Distance = 10)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Distance);
            EditorGUILayout.BeginVertical();
        }

        public static void EndIndent()
        {
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public static void TutorialButton(string Description, string LinkText)
        {
            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;

            GUI.backgroundColor = new Color(1f, 155f, 1f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("チュートリアルを見る", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL(LinkText);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public static void ImportantTutorialButton(string Description, string LinkText)
        {
            var HelpButtonStyle = new GUIStyle(GUI.skin.button);
            HelpButtonStyle.normal.textColor = Color.white;
            HelpButtonStyle.fontStyle = FontStyle.Bold;

            GUI.backgroundColor = new Color(155f, 125f, 1f, 0.45f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = new Color(0, 0.65f, 0, 0.8f);
            if (GUILayout.Button("チュートリアルを見る", HelpButtonStyle, GUILayout.Height(20)))
            {
                Application.OpenURL(LinkText);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// AnimationProfile エディタ内で、AnimationClass を「個別の値」として描画します。
        /// </summary>
        public static void DrawAnimationClassVariables(AnimationProfile self, AnimationClass AnimationClassValue, string ValueName, string AnimationDescription, int NumberOfSpaces, bool DisplayMirror, bool DisplayLoopMessage, bool BackwardsAnimation = false)
        {
            EditorGUI.BeginChangeCheck();
            AnimationClassValue.AnimationClip = (AnimationClip)EditorGUILayout.ObjectField(ValueName + " アニメーション", AnimationClassValue.AnimationClip, typeof(AnimationClip), false);

            CustomHelpLabelField(AnimationDescription, false);
            AnimationClassValue.AnimationSpeed = EditorGUILayout.Slider(ValueName + " 速度", AnimationClassValue.AnimationSpeed, -8f, 8f);
            CustomHelpLabelField("このアニメーションの再生速度を制御します。", false);

            if (DisplayMirror)
            {
                AnimationClassValue.Mirror = EditorGUILayout.Toggle(ValueName + " ミラー", AnimationClassValue.Mirror);
                CustomHelpLabelField("ミラーを有効にすると、アニメーションを左右反転して再生します。", false);
            }

            if (AnimationClassValue.AnimationClip != null && DisplayLoopMessage)
            {
                var settings = AnimationUtility.GetAnimationClipSettings(AnimationClassValue.AnimationClip);
                if (!settings.loopTime)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("「" + ValueName + "」アニメーションはループに設定する必要があります。アニメーションの設定で「Loop Time」を有効にしてください。", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
            }

            GUILayout.Space(NumberOfSpaces * 8);

            // Animator Controller 生成時の警告を避けるため、AnimationSpeed が 0 の場合は 1 に補正
            if (AnimationClassValue.AnimationSpeed == 0)
                AnimationClassValue.AnimationSpeed = 1;

            if (EditorGUI.EndChangeCheck())
            {
                self.AnimationsUpdated = true;
            }
        }

        public static void CustomFoldout(SerializedProperty FoldoutValue, string TitleText, bool ToggleOnLabelClick, GUIStyle FoldoutStyle)
        {
            GUIContent label = new GUIContent();
            //EditorGUILayout.BeginVertical("Box");
            label = EditorGUI.BeginProperty(new Rect(), label, FoldoutValue);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Foldout(FoldoutValue.boolValue, TitleText, ToggleOnLabelClick);
            if (EditorGUI.EndChangeCheck())
                FoldoutValue.boolValue = newValue;
            EditorGUI.EndProperty();
            //EditorGUILayout.EndVertical();
        }

        public static void CustomIntField(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntField(Name, property.intValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomIntSlider(Rect position, GUIContent label, SerializedProperty property, string Name, int MinValue, int MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.IntSlider(Name, property.intValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomFloatSlider(Rect position, GUIContent label, SerializedProperty property, string Name, float MinValue, float MaxValue)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Slider(Name, property.floatValue, MinValue, MaxValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomFloatField(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.FloatField(Name, property.floatValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomColorField(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.ColorField(Name, property.colorValue);
            if (EditorGUI.EndChangeCheck())
                property.colorValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomPopupColor(Rect position, GUIContent label, SerializedProperty property, string nameOfLabel, Type typeOfEnum, Color TextColor)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            string[] enumNamesList = System.Enum.GetNames(typeOfEnum);

            var Style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            Style.normal.textColor = TextColor;

            EditorGUILayout.LabelField(new GUIContent(nameOfLabel), Style, GUILayout.Width(75));
            var newValue = EditorGUILayout.Popup("", property.intValue, enumNamesList, GUILayout.Width(65));

            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;

            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }

            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text, style);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
        }

        public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style)
        {
            Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 22f, style);
            return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, style);
        }
        public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style)
        {
            return Foldout(foldout, new GUIContent(content), toggleOnLabelClick, style);
        }

        public static void CustomTagField(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.TagField(Name, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomEnumColor(Rect position, GUIContent label, SerializedProperty property, string Name, Color TextColor)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            var Style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            Style.normal.textColor = TextColor;

            EditorGUILayout.LabelField(new GUIContent(Name), Style, GUILayout.Width(50));
            var newValue = EditorGUILayout.Popup("", property.intValue, EmeraldDetection.StringFactionList.ToArray(), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));

            if (property.intValue == EmeraldDetection.StringFactionList.Count)
            {
                property.intValue -= 1;
            }
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomEnum(Rect position, GUIContent label, SerializedProperty property, string Name)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Popup(Name, property.intValue, EmeraldDetection.StringFactionList.ToArray());
            if (property.intValue == EmeraldDetection.StringFactionList.Count)
            {
                property.intValue -= 1;
            }
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void FactionListEnum(Rect position, GUIContent label, SerializedProperty property, string Name, List<string> FationNames)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Popup(Name, property.intValue, FationNames.ToArray());
            if (property.intValue == FationNames.Count)
            {
                property.intValue -= 1;
            }
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;

            EditorGUI.EndProperty();
        }

        public static void CustomObjectField(Rect position, GUIContent label, SerializedProperty property, string Name, Type typeOfObject, bool IsEssential)
        {
            if (IsEssential && property.objectReferenceValue == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("このフィールドは空欄にできません", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.ObjectField(Name, property.objectReferenceValue, typeOfObject, true);

            if (EditorGUI.EndChangeCheck())
                property.objectReferenceValue = newValue;

            EditorGUI.EndProperty();
        }
    }
}
