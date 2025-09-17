using System.Collections;                         // コレクション（IEnumerable 等）
using System.Collections.Generic;                 // List など
using UnityEngine;                                // Unity 基本 API
using UnityEditor;                                // エディタ拡張 API
using UnityEditorInternal;                        // ReorderableList など

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldMovement))]       // このエディタは EmeraldMovement に適用
    [CanEditMultipleObjects]                      // 複数選択編集を許可
    /// <summary>
    /// 【クラス説明（日本語）】
    /// EmeraldMovement のカスタムインスペクタ。移動・旋回・傾斜追従・徘徊（ウェイポイント/ダイナミック/目的地/ステーショナリ）設定を編集する UI を提供します。
    /// </summary>
    // ▼このクラスは「EmeraldMovement のインスペクタ拡張クラス」
    public class EmeraldMovementEditor : Editor
    {
        [Header("折りたたみ見出しのスタイル（注釈）")]
        GUIStyle FoldoutStyle;                     // セクション見出しの GUIStyle

        [Header("対象 AI の EmeraldAnimation 参照（注釈）")]
        EmeraldAnimation EmeraldAnimation;         // アニメーションプロファイル参照

        [Header("移動エディタの見出しアイコン（注釈）")]
        Texture MovementEditorIcon;                // ヘッダーに表示するテクスチャ

        [Header("現在選択中ウェイポイントのインデックス（注釈）")]
        int CurrentWaypointIndex = -1;             // シーン操作で使用

        #region SerializedProperties
        //Foldouts
        [Header("折りたたみ状態（注釈）")]
        SerializedProperty HideSettingsFoldout, WanderFoldout, WaypointsFoldout, WaypointsListFoldout, MovementFoldout, AlignmentFoldout, TurnFoldout;

        //Int
        [Header("Int プロパティ（注釈）")]
        SerializedProperty StationaryIdleSecondsMinProp, StationaryIdleSecondsMaxProp, WanderRadiusProp, MaxSlopeLimitProp, WalkSpeedProp, RunSpeedProp, MinimumWaitTimeProp, MaximumWaitTimeProp,
            WalkBackwardsSpeedProp, StationaryTurningSpeedCombatProp, MovingTurningSpeedCombatProp, BackupTurningSpeedProp;

        //Floats
        [Header("Float プロパティ（注釈）")]
        SerializedProperty StoppingDistanceProp, NonCombatAngleToTurnProp, CombatAngleToTurnProp, StationaryTurningSpeedNonCombatProp, MovingTurnSpeedNonCombatProp, MovementTurningSensitivityProp, MaxNormalAngleProp, NonCombatAlignSpeedProp,
            CombatAlignSpeedProp, ForceWalkDistanceProp, DecelerationDampTimeProp;

        //Enums
        [Header("Enum プロパティ（注釈）")]
        SerializedProperty WanderTypeProp, WaypointTypeProp, AlignAIWithGroundProp, CurrentMovementStateProp, AnimatorTypeProp, AlignmentQualityProp, AlignAIOnStartProp;

        //LayerMask
        [Header("LayerMask プロパティ（注釈）")]
        SerializedProperty DynamicWanderLayerMaskProp, BackupLayerMaskProp, AlignmentLayerMaskProp;

        //Bool
        [Header("Bool プロパティ（注釈）")]
        SerializedProperty UseRandomRotationOnStartProp, AnimationsUpdatedProp;

        //Objects
        [Header("オブジェクト参照（注釈）")]
        SerializedProperty WaypointObjectProp;
        #endregion

        /// <summary>
        /// 【OnEnable（日本語）】
        /// 対象参照・アイコンのロード・SerializedProperty の初期化を行います。
        /// </summary>
        void OnEnable()
        {
            EmeraldMovement self = (EmeraldMovement)target;                                      // 対象の EmeraldMovement
            EmeraldAnimation = self.GetComponent<EmeraldAnimation>();                             // アニメ参照をキャッシュ
            if (MovementEditorIcon == null) MovementEditorIcon = Resources.Load("Editor Icons/EmeraldMovement") as Texture; // アイコン取得

            InitializeProperties();                                                               // プロパティの紐付け
        }

        /// <summary>
        /// 【プロパティ初期化（日本語）】
        /// serializedObject から各フィールドの SerializedProperty を取得します。
        /// </summary>
        void InitializeProperties()
        {
            //Enums
            WanderTypeProp = serializedObject.FindProperty("WanderType");                         // 徘徊タイプ
            WaypointTypeProp = serializedObject.FindProperty("WaypointType");                     // ウェイポイント種別
            AlignAIWithGroundProp = serializedObject.FindProperty("AlignAIWithGround");           // 傾斜追従の有無
            CurrentMovementStateProp = serializedObject.FindProperty("CurrentMovementState");     // 移動アニメ種類
            AnimatorTypeProp = serializedObject.FindProperty("MovementType");                     // 移動タイプ（RootMotion/NavMesh）
            AlignmentQualityProp = serializedObject.FindProperty("AlignmentQuality");             // 追従品質
            AlignAIOnStartProp = serializedObject.FindProperty("AlignAIOnStart");                 // Start での追従計算

            //Ints
            StationaryIdleSecondsMinProp = serializedObject.FindProperty("StationaryIdleSecondsMin"); // 最小アイドル切替秒
            StationaryIdleSecondsMaxProp = serializedObject.FindProperty("StationaryIdleSecondsMax"); // 最大アイドル切替秒
            WanderRadiusProp = serializedObject.FindProperty("WanderRadius");                         // 徘徊半径
            MaxSlopeLimitProp = serializedObject.FindProperty("MaxSlopeLimit");                       // 生成許容最大傾斜
            WanderRadiusProp = serializedObject.FindProperty("WanderRadius");                         // 徘徊半径（重複行・原文準拠）
            MinimumWaitTimeProp = serializedObject.FindProperty("MinimumWaitTime");                   // 最小待機時間
            MaximumWaitTimeProp = serializedObject.FindProperty("MaximumWaitTime");                   // 最大待機時間
            WalkSpeedProp = serializedObject.FindProperty("WalkSpeed");                               // 歩行速度
            WalkBackwardsSpeedProp = serializedObject.FindProperty("WalkBackwardsSpeed");             // 後退歩行速度
            RunSpeedProp = serializedObject.FindProperty("RunSpeed");                                 // 走行速度
            BackupTurningSpeedProp = serializedObject.FindProperty("BackupTurningSpeed");             // 後退時の旋回速度

            CombatAngleToTurnProp = serializedObject.FindProperty("CombatAngleToTurn");               // 旋回開始角（戦闘）
            NonCombatAngleToTurnProp = serializedObject.FindProperty("NonCombatAngleToTurn");         // 旋回開始角（非戦闘）
            StationaryTurningSpeedNonCombatProp = serializedObject.FindProperty("StationaryTurningSpeedNonCombat"); // 静止時の旋回速度（非戦闘）
            StationaryTurningSpeedCombatProp = serializedObject.FindProperty("StationaryTurningSpeedCombat");       // 静止時の旋回速度（戦闘）          
            MovingTurnSpeedNonCombatProp = serializedObject.FindProperty("MovingTurnSpeedNonCombat");               // 移動中の旋回速度（非戦闘）
            MovingTurningSpeedCombatProp = serializedObject.FindProperty("MovingTurnSpeedCombat");                  // 移動中の旋回速度（戦闘）

            //Floats
            StoppingDistanceProp = serializedObject.FindProperty("StoppingDistance");               // 停止距離
            MovementTurningSensitivityProp = serializedObject.FindProperty("MovementTurningSensitivity"); // 旋回アニメ感度
            DecelerationDampTimeProp = serializedObject.FindProperty("DecelerationDampTime");      // 減速時ダンプ時間
            MaxNormalAngleProp = serializedObject.FindProperty("MaxNormalAngle");                  // 追従最大角度
            NonCombatAlignSpeedProp = serializedObject.FindProperty("NonCombatAlignmentSpeed");    // 追従速度（非戦闘）
            CombatAlignSpeedProp = serializedObject.FindProperty("CombatAlignmentSpeed");          // 追従速度（戦闘）
            ForceWalkDistanceProp = serializedObject.FindProperty("ForceWalkDistance");            // 歩行へ切替える距離

            //LayerMask
            DynamicWanderLayerMaskProp = serializedObject.FindProperty("DynamicWanderLayerMask");  // ダイナミック徘徊レイヤー
            BackupLayerMaskProp = serializedObject.FindProperty("BackupLayerMask");                // 後退検知レイヤー
            AlignmentLayerMaskProp = serializedObject.FindProperty("AlignmentLayerMask");          // 傾斜追従レイヤー

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");            // 全体折りたたみ
            WanderFoldout = serializedObject.FindProperty("WanderFoldout");                        // 徘徊設定
            WaypointsFoldout = serializedObject.FindProperty("WaypointsFoldout");                  // ウェイポイント設定
            WaypointsListFoldout = serializedObject.FindProperty("WaypointsListFoldout");          // ウェイポイント一覧
            MovementFoldout = serializedObject.FindProperty("MovementFoldout");                    // 移動設定
            AlignmentFoldout = serializedObject.FindProperty("AlignmentFoldout");                  // 傾斜追従設定
            TurnFoldout = serializedObject.FindProperty("TurnFoldout");                            // 旋回設定
            UseRandomRotationOnStartProp = serializedObject.FindProperty("UseRandomRotationOnStart"); // 開始時ランダム回転
            AnimationsUpdatedProp = serializedObject.FindProperty("AnimationsUpdated"); // 備考：複数スクリプトで使用中。元の挙動を尊重。

            //Objects
            WaypointObjectProp = serializedObject.FindProperty("m_WaypointObject");                // ウェイポイントオブジェクト
        }

        /// <summary>
        /// 【インスペクタ描画（日本語）】
        /// ヘッダーおよび各セクション（移動/旋回/傾斜追従/徘徊/ウェイポイント）を描画します。
        /// </summary>
        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();                             // 共通スタイル更新
            EmeraldMovement self = (EmeraldMovement)target;                                         // 対象
            serializedObject.Update();                                                              // 変更追跡開始

            // ヘッダー（"Movement" → 「移動」に日本語化）
            CustomEditorProperties.BeginScriptHeaderNew("移動", MovementEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                MovementSettings(self);     // 移動設定
                EditorGUILayout.Space();
                TurnSettings(self);         // 旋回設定
                EditorGUILayout.Space();
                AlignmentSettings(self);    // 傾斜追従設定
                EditorGUILayout.Space();
                WanderSettings(self);       // 徘徊タイプ設定
                EditorGUILayout.Space();
                WaypointSettings(self);     // ウェイポイント設定
                if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints) EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();                                              // ヘッダー終了
            serializedObject.ApplyModifiedProperties();                                            // 変更を反映
        }

        /// <summary>
        /// 【ウェイポイント設定（日本語）】
        /// ウェイポイントの追加/削除、インポート/エクスポート、リスト表示を行います。
        /// </summary>
        void WaypointSettings(EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
            {
                WaypointsFoldout.boolValue = EditorGUILayout.Foldout(WaypointsFoldout.boolValue, "ウェイポイント設定", true, FoldoutStyle);

                if (WaypointsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();

                    CustomEditorProperties.TextTitleWithDescription(
                        "ウェイポイント エディタ",
                        "AI が辿るウェイポイントを定義します。［ウェイポイントを追加］で順に作成すると、その順番で移動し、可視化のためのラインが描画されます。",
                        true);

                    if (self.WaypointsList != null && Selection.objects.Length == 1)
                    {
                        EditorGUILayout.LabelField("最後のウェイポイントへ到達したときの挙動を制御します。", EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(WaypointTypeProp, new GUIContent("ウェイポイントの種別"));
                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                        GUI.backgroundColor = Color.white;

                        if (self.WaypointType == (EmeraldMovement.WaypointTypes.Loop))
                        {
                            CustomEditorProperties.CustomHelpLabelField(
                                "ループ - 作成順に連続移動し続けます。最後に到達すると最初へ戻り、ループを形成します。",
                                false);
                        }
                        else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Reverse))
                        {
                            CustomEditorProperties.CustomHelpLabelField(
                                "リバース - 作成順に最後まで到達したのち、待機時間だけ待って逆順に辿ります。これを繰り返します。",
                                false);
                        }
                        else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Random))
                        {
                            CustomEditorProperties.CustomHelpLabelField(
                                "ランダム - すべてのウェイポイントをランダムに巡回します。各到達時に待機時間だけ停止します。",
                                false);
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        CustomEditorProperties.CustomHelpLabelField("現在のウェイポイントオブジェクトからすべてのウェイポイントを読み込みます。", false);
                        EditorGUILayout.PropertyField(WaypointObjectProp);

                        if (GUILayout.Button("ウェイポイントデータをインポート") && EditorUtility.DisplayDialog(
                                "ウェイポイントデータをインポートしますか？",
                                "この AI のウェイポイントをすべて消去し、適用されたウェイポイントオブジェクトからインポートします。元に戻すことはできません。よろしいですか？",
                                "はい", "キャンセル"))
                        {
                            if (self.m_WaypointObject == null)
                            {
                                Debug.LogError("ウェイポイントオブジェクトが未設定です。インポートするには、まず割り当ててください。");
                                return;
                            }

                            self.WaypointsList = new List<Vector3>(self.m_WaypointObject.Waypoints);
                            EditorUtility.SetDirty(self);
                        }
                        EditorGUILayout.Space();
                        CustomEditorProperties.CustomHelpLabelField("全ウェイポイントをウェイポイントオブジェクトに書き出し、他の AI と共有できます。", false);
                        if (GUILayout.Button("ウェイポイントデータを書き出し"))
                        {
                            // 現在のウェイポイントをアセットへ保存（原文ロジックのまま）
                            string SavePath = EditorUtility.SaveFilePanelInProject("ウェイポイントデータの保存", "新しいウェイポイントオブジェクト", "asset", "保存するファイル名を入力してください");
                            if (SavePath != string.Empty)
                            {
                                var m_WaypointObject = CreateInstance<EmeraldWaypointObject>();
                                m_WaypointObject.Waypoints = new List<Vector3>(self.WaypointsList);
                                AssetDatabase.CreateAsset(m_WaypointObject, SavePath);
                            }

                            // 原文にある UI 修正ハック（保持）
                            CustomEditorProperties.BeginScriptHeader("", null);
                            CustomEditorProperties.BeginFoldoutWindowBox();
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        if (GUILayout.Button("ウェイポイントを追加"))
                        {
                            Vector3 newPoint = new Vector3(0, 0, 0);

                            if (self.WaypointsList.Count == 0)
                            {
                                newPoint = self.transform.position + Vector3.forward * (self.StoppingDistance * 2);
                            }
                            else if (self.WaypointsList.Count > 0)
                            {
                                newPoint = self.WaypointsList[self.WaypointsList.Count - 1] + Vector3.forward * (self.StoppingDistance * 2);
                            }

                            Undo.RecordObject(self, "Add Waypoint");
                            self.WaypointsList.Add(newPoint);
                            EditorUtility.SetDirty(self);
                        }

                        var style = new GUIStyle(GUI.skin.button);
                        style.normal.textColor = Color.red;

                        if (GUILayout.Button("すべてのウェイポイントをクリア", style) && EditorUtility.DisplayDialog(
                                "ウェイポイントをクリアしますか？",
                                "この AI のウェイポイントをすべて削除します。元に戻すことはできません。よろしいですか？",
                                "はい", "キャンセル"))
                        {
                            self.WaypointsList.Clear();
                            EditorUtility.SetDirty(self);
                        }
                        GUI.contentColor = Color.white;
                        GUI.backgroundColor = Color.white;

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        CustomEditorProperties.BeginIndent();
                        WaypointsListFoldout.boolValue = CustomEditorProperties.Foldout(WaypointsListFoldout.boolValue, "ウェイポイント一覧", true, FoldoutStyle);

                        if (WaypointsListFoldout.boolValue)
                        {
                            CustomEditorProperties.BeginFoldoutWindowBox();
                            CustomEditorProperties.TextTitleWithDescription(
                                "ウェイポイント一覧",
                                "この AI の現在のウェイポイント一覧です。［削除］で個別に削除できます。",
                                true);
                            EditorGUILayout.Space();

                            if (self.WaypointsList.Count > 0)
                            {
                                for (int j = 0; j < self.WaypointsList.Count; ++j)
                                {
                                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                                    EditorGUILayout.LabelField("ウェイポイント " + (j + 1), EditorStyles.toolbarButton);
                                    GUI.backgroundColor = Color.white;

                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    if (j < self.WaypointsList.Count - 1)
                                    {
                                        if (GUILayout.Button(new GUIContent("挿入", "この点と次の点の間に新しい点を挿入します。"), EditorStyles.miniButton, GUILayout.Height(18)))
                                        {
                                            Undo.RecordObject(self, "Insert Waypoint Above this Point");
                                            self.WaypointsList.Insert(j + 1, (self.WaypointsList[j] + self.WaypointsList[j + 1]) / 2f);
                                            CurrentWaypointIndex = j + 1;
                                            EditorUtility.SetDirty(self);
                                            HandleUtility.Repaint();
                                        }
                                    }

                                    if (GUILayout.Button(new GUIContent("削除", "この点をウェイポイント一覧から削除します。"), EditorStyles.miniButton, GUILayout.Height(18)))
                                    {
                                        Undo.RecordObject(self, "Remove Point");
                                        self.WaypointsList.RemoveAt(j);
                                        EditorUtility.SetDirty(self);
                                        HandleUtility.Repaint();
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    GUILayout.Space(10);
                                }
                            }
                            CustomEditorProperties.EndFoldoutWindowBox();
                        }

                        CustomEditorProperties.EndIndent();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                    else if (self.WaypointsList != null && Selection.objects.Length > 1)
                    {
                        CustomEditorProperties.DisplayWarningMessage("ウェイポイントは複数オブジェクトの同時編集に対応していません。1 体のみ選択して編集してください。");
                    }

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        /// <summary>
        /// 【移動設定（日本語）】
        /// 速度・距離・移動タイプなど、移動に関する基本設定を行います。
        /// </summary>
        void MovementSettings(EmeraldMovement self)
        {
            MovementFoldout.boolValue = EditorGUILayout.Foldout(MovementFoldout.boolValue, "移動設定", true, FoldoutStyle);

            if (MovementFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("移動設定", "速度や距離など、移動に関する各種設定を制御します。", true);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(AnimatorTypeProp, new GUIContent("移動タイプ"));
                CustomEditorProperties.CustomHelpLabelField("AI の移動方法を制御します。Root Motion（アニメーション駆動）か、NavMesh（エージェント駆動）を選択します。", true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (EmeraldAnimation.m_AnimationProfile.AnimatorControllerGenerated)
                    {
                        // 備考：AnimatorTypeProp は再生成時に self 側とズレるため、直接代入（原文のまま）
                        self.MovementType = (EmeraldMovement.MovementTypes)AnimatorTypeProp.intValue;
                        EmeraldAnimatorGenerator.GenerateAnimatorController(EmeraldAnimation.m_AnimationProfile);
                    }
                }

                // Movement Type 詳細
                CustomEditorProperties.BeginIndent();

                GUI.backgroundColor = new Color(5f, 0.5f, 0.5f, 1f);
                EditorGUILayout.LabelField(
                    "Root Motion を使用する場合、AI の移動速度はアニメーションの再生速度に依存します。必要に応じて Animation Profile で調整してください。",
                    EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUI.BeginDisabledGroup(self.MovementType == EmeraldMovement.MovementTypes.RootMotion);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WalkSpeedProp, "歩行速度", 0.5f, 5);
                CustomEditorProperties.CustomHelpLabelField("AI の歩行速度を制御します。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RunSpeedProp, "走行速度", 0.5f, 10);
                CustomEditorProperties.CustomHelpLabelField("AI の走行速度を制御します。", true);

                CustomFloatAnimationField(new Rect(), new GUIContent(), WalkBackwardsSpeedProp, "後退歩行速度", 0.5f, 3f);
                CustomEditorProperties.CustomHelpLabelField("AI の後退歩行速度を制御します。", true);

                // NavMesh 速度を更新する場合のアニメータ再生成（原文ロジックを保持）
                if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.AnimatorControllerGenerated && self.MovementType == EmeraldMovement.MovementTypes.NavMeshDriven)
                {
                    if (EmeraldAnimation.m_AnimationProfile.AnimationsUpdated || EmeraldAnimation.m_AnimationProfile.AnimationListsChanged)
                    {
                        EmeraldAnimatorGenerator.GenerateAnimatorController(EmeraldAnimation.m_AnimationProfile);
                    }
                }

                EditorGUI.EndDisabledGroup();
                CustomEditorProperties.EndIndent();
                // Movement Type 詳細ここまで

                EditorGUILayout.PropertyField(CurrentMovementStateProp, new GUIContent("移動アニメーション"));
                CustomEditorProperties.CustomHelpLabelField(
                    "ウェイポイント移動・目的地移動・徘徊時に使用されるアニメーションの種類を制御します。必要に応じて実行時にスクリプトから切り替えることもできます。",
                    true);
                EditorGUILayout.Space();

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ForceWalkDistanceProp, "歩行に切り替える距離", 0.0f, 8.0f);
                CustomEditorProperties.CustomHelpLabelField("目的地やターゲットに接近する際、この距離以内に入ると走行から歩行へ切り替えます。0 にすると無効化できます。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), StoppingDistanceProp, "停止距離", 0.25f, 40);
                CustomEditorProperties.CustomHelpLabelField("非戦闘系の目的地やウェイポイント手前で停止する距離を制御します。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DecelerationDampTimeProp, "減速時のダンプ時間", 0.1f, 0.4f);
                CustomEditorProperties.CustomHelpLabelField("移動から停止へ減速する際のアニメーション補間時間。値を小さくすると切り替えが素早くなります。", true);

                EditorGUILayout.PropertyField(BackupLayerMaskProp, new GUIContent("後退検知レイヤー"));
                CustomEditorProperties.CustomHelpLabelField("AI の背後数ユニットにコライダーを検知した場合、後退処理を停止します。ここで検知対象のレイヤーを指定します。", true);

                if (BackupLayerMaskProp.intValue == 0)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("後退検知レイヤーに Nothing は指定できません。", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【旋回設定（日本語）】
        /// 非戦闘/戦闘時の旋回角・旋回速度・感度など、旋回に関する設定を行います。
        /// </summary>
        void TurnSettings(EmeraldMovement self)
        {
            TurnFoldout.boolValue = EditorGUILayout.Foldout(TurnFoldout.boolValue, "旋回設定", true, FoldoutStyle);

            if (TurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("旋回設定", "旋回に関連する各種設定と速度を制御します。", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), NonCombatAngleToTurnProp, "旋回開始角（非戦闘）", 15, 90);
                CustomEditorProperties.CustomHelpLabelField(
                    "非戦闘時に旋回アニメーションを再生するために必要な角度。左右方向は自動検出されます。AI に専用の旋回アニメが無い場合は歩行アニメを代用することもできます。",
                    true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatAngleToTurnProp, "旋回開始角（戦闘）", 20, 90);
                CustomEditorProperties.CustomHelpLabelField(
                    "戦闘時に旋回アニメーションを再生するために必要な角度。左右方向は自動検出されます。AI に専用の旋回アニメが無い場合は歩行アニメを代用することもできます。",
                    true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), StationaryTurningSpeedNonCombatProp, "静止時の旋回速度（非戦闘）", 1, 200);
                CustomEditorProperties.CustomHelpLabelField(
                    "非戦闘かつ静止時の旋回速度。Root Motion を使用する場合は、旋回アニメが支援するため低めの値でも機能します。徘徊時に旋回が遅い場合はこの値を上げてください。",
                    true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), StationaryTurningSpeedCombatProp, "静止時の旋回速度（戦闘）", 1, 200);
                CustomEditorProperties.CustomHelpLabelField("戦闘かつ静止時の旋回速度。", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MovingTurnSpeedNonCombatProp, "移動中の旋回速度（非戦闘）", 50, 750);
                CustomEditorProperties.CustomHelpLabelField(
                    "非戦闘かつ移動中の旋回速度。Root Motion 使用時は旋回アニメが支援するため、必要に応じて調整してください。",
                    true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MovingTurningSpeedCombatProp, "移動中の旋回速度（戦闘）", 50, 750);
                CustomEditorProperties.CustomHelpLabelField("戦闘かつ移動中の旋回速度。", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), BackupTurningSpeedProp, "後退時の旋回速度", 5, 750);
                CustomEditorProperties.CustomHelpLabelField("後退動作中の旋回速度。", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), MovementTurningSensitivityProp, "旋回アニメ感度", 0.5f, 3f);
                CustomEditorProperties.CustomHelpLabelField("移動用ブレンドツリーにおける旋回アニメの感度。四足モデルなどで効果が分かりやすい設定です。", true);

                EditorGUILayout.PropertyField(UseRandomRotationOnStartProp, new GUIContent("開始時のランダム回転"));
                CustomEditorProperties.CustomHelpLabelField("Start 時にランダムな回転を与えるかどうか。", true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【傾斜追従設定（日本語）】
        /// 傾斜や地形に AI を傾けて追従させる機能（Align AI）の設定を行います。
        /// </summary>
        void AlignmentSettings(EmeraldMovement self)
        {
            AlignmentFoldout.boolValue = EditorGUILayout.Foldout(AlignmentFoldout.boolValue, "傾斜追従設定", true, FoldoutStyle);

            if (AlignmentFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "傾斜追従設定",
                    "AI を地形やオブジェクトの傾斜に合わせます（Final IK などの全身 IK を使う場合は無効化推奨）。",
                    true);

                EditorGUILayout.PropertyField(AlignAIWithGroundProp, new GUIContent("傾斜に追従"));
                CustomEditorProperties.CustomHelpLabelField("地形やオブジェクトの角度に AI を合わせ、自然な見た目にします。無効化すると AI ごとのパフォーマンスが向上します。", true);

                if (self.AlignAIWithGround == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    EditorGUILayout.PropertyField(AlignmentLayerMaskProp, new GUIContent("傾斜判定レイヤー"));
                    CustomEditorProperties.CustomHelpLabelField("傾斜追従の角度計算に使用するレイヤー。指定外のレイヤーは無視されます。", true);

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(AlignmentQualityProp, new GUIContent("追従品質"));
                    CustomEditorProperties.CustomHelpLabelField("更新頻度を調整して Align AI の品質を制御します。", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), NonCombatAlignSpeedProp, "追従速度（非戦闘）", 5, 200);
                    CustomEditorProperties.CustomHelpLabelField("非戦闘時に地面へ追従する速度。", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatAlignSpeedProp, "追従速度（戦闘）", 5, 200);
                    CustomEditorProperties.CustomHelpLabelField("戦闘時に地面へ追従する速度。", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxNormalAngleProp, "最大角度", 5, 50);
                    CustomEditorProperties.CustomHelpLabelField("地面追従のために AI が回転する最大角度。", true);

                    EditorGUILayout.PropertyField(AlignAIOnStartProp, new GUIContent("開始時に追従計算"));
                    CustomEditorProperties.CustomHelpLabelField("Start タイミングで一度、傾斜追従計算を実行します。", true);

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// 【徘徊タイプ設定（日本語）】
        /// 徘徊方法（ダイナミック/ウェイポイント/ステーショナリ/目的地/カスタム）と各種パラメータを設定します。
        /// </summary>
        void WanderSettings(EmeraldMovement self)
        {
            WanderFoldout.boolValue = EditorGUILayout.Foldout(WanderFoldout.boolValue, "徘徊タイプ設定", true, FoldoutStyle);

            if (WanderFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription(
                    "徘徊タイプ設定",
                    "非戦闘時の徘徊挙動を制御します。ウェイポイント徘徊を選ぶとウェイポイントエディタが表示されます。",
                    true);
                EditorGUILayout.LabelField(
                    "この AI が使用する徘徊メカニズムを選択します。徘徊中も、視界に入ったターゲットに対して挙動タイプに従って反応します。",
                    EditorStyles.helpBox);
                EditorGUILayout.PropertyField(WanderTypeProp, new GUIContent("徘徊タイプ"));

                CustomEditorProperties.BeginIndent();
                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic)
                {
                    CustomEditorProperties.CustomHelpLabelField(
                        "ダイナミック - 徘徊半径内にウェイポイントを動的生成し、ランダムに徘徊します。",
                        true);
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
                {
                    CustomEditorProperties.CustomHelpLabelField(
                        "ウェイポイント - あなたが定義したウェイポイント間を移動します（詳細はこの下の折りたたみ）。",
                        true);
                    if (GUILayout.Button("ウェイポイント設定を開く"))
                    {
                        self.WanderFoldout = false;
                        self.WaypointsFoldout = true;
                    }
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Stationary)
                {
                    CustomEditorProperties.CustomHelpLabelField(
                        "ステーショナリ - その場から移動せず、トリガー半径にターゲットが入った場合のみ動作します。",
                        true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StationaryIdleSecondsMinProp, "最小アイドル切替秒");
                    CustomEditorProperties.CustomHelpLabelField(
                        "アイドルアニメが複数ある場合、次のアイドルへ切り替わるまでの最小秒数。最大値と組み合わせてランダム化されます。",
                        true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StationaryIdleSecondsMaxProp, "最大アイドル切替秒");
                    CustomEditorProperties.CustomHelpLabelField(
                        "アイドルアニメが複数ある場合、次のアイドルへ切り替わるまでの最大秒数。最小値と組み合わせてランダム化されます。",
                        true);
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Destination)
                {
                    CustomEditorProperties.CustomHelpLabelField(
                        "目的地 - 単一の目的地まで移動します。到達後はその場に留まります（経路探索は Unity NavMesh に依存）。",
                        true);

                    if (GUILayout.Button("目的地をリセット"))
                    {
                        self.SingleDestination = self.transform.position + self.transform.forward * 2;
                    }
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Custom)
                {
                    CustomEditorProperties.CustomHelpLabelField(
                        "カスタム - スクリプトから目的地を与える徘徊。Unity NavMesh により到達し、到達後はその場に留まります。",
                        false);
                }
                CustomEditorProperties.EndIndent();

                EditorGUILayout.Space();

                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic)
                {
                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderRadiusProp, "ダイナミック徘徊半径", ((int)self.StoppingDistance + 3), 300);
                    CustomEditorProperties.CustomHelpLabelField("徘徊に使用する半径。AI はこの半径内でランダムにウェイポイントを選びます。", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxSlopeLimitProp, "最大傾斜角", 10, 60);
                    CustomEditorProperties.CustomHelpLabelField("ウェイポイントが生成され得る地形の最大傾斜角。", true);

                    EditorGUILayout.PropertyField(DynamicWanderLayerMaskProp, new GUIContent("ダイナミック徘徊レイヤー"));
                    CustomEditorProperties.CustomHelpLabelField("ダイナミックウェイポイント生成に使用するレイヤー。", false);

                    if (DynamicWanderLayerMaskProp.intValue == 0)
                    {
                        GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                        EditorGUILayout.LabelField("ダイナミック徘徊レイヤーに Nothing は指定できません。", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                    }
                }

                EditorGUILayout.Space();

                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic || self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
                {
                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), MinimumWaitTimeProp, "最小待機時間");
                    CustomEditorProperties.CustomHelpLabelField(
                        "ダイナミックおよびランダム徘徊で次のウェイポイントを生成するまでの最小秒数。最大待機時間とランダム化されます。",
                        true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), MaximumWaitTimeProp, "最大待機時間");
                    CustomEditorProperties.CustomHelpLabelField(
                        "ダイナミックおよびランダム徘徊で次のウェイポイントを生成するまでの最大秒数。最小待機時間とランダム化されます。",
                        true);
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }

            if (self.WanderType == EmeraldMovement.WanderTypes.Destination)
            {
                if (self.SingleDestination == Vector3.zero)
                {
                    self.SingleDestination = new Vector3(self.transform.position.x, self.transform.position.y, self.transform.position.z + 5);
                }
            }
        }

        /// <summary>
        /// 【シーン描画（日本語）】
        /// 移動設定に関するガイド（ウェイポイント、徘徊半径、目的地）をシーン上に描画します。
        /// </summary>
        void OnSceneGUI()
        {
            EmeraldMovement self = (EmeraldMovement)target;
            DrawWaypoints(self);               // ウェイポイントの可視化
            DrawWanderArea(self);              // 徘徊半径の可視化
            DrawSingleDestinationPoint(self);  // 目的地の可視化
        }

        /// <summary>
        /// 【ウェイポイントの可視化（日本語）】
        /// ウェイポイント徘徊時に、ラインと操作ハンドルを描画します。
        /// </summary>
        void DrawWaypoints(EmeraldMovement self)
        {
            // Delete キーで選択中ウェイポイントを削除（原文ロジックをそのまま保持）
            if (Event.current != null && Event.current.isKey && Event.current.type.Equals(EventType.KeyDown) && Event.current.keyCode == KeyCode.Delete)
            {
                Event.current.Use();

                if (CurrentWaypointIndex != -1)
                {
                    Undo.RecordObject(self, "Deleted Waypoint");
                    self.WaypointsList.RemoveAt(CurrentWaypointIndex);
                }
            }

            if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints && WaypointsFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                if (self.WaypointsList.Count > 0 && self.WaypointsList != null)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(self.transform.position, self.WaypointsList[0]);
                    Handles.color = Color.white;

                    Handles.color = Color.green;
                    if (self.WaypointType != (EmeraldMovement.WaypointTypes.Random))
                    {
                        for (int i = 0; i < self.WaypointsList.Count - 1; i++)
                        {
                            Handles.DrawLine(self.WaypointsList[i], self.WaypointsList[i + 1]);
                        }
                    }
                    else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Random))
                    {
                        for (int i = 0; i < self.WaypointsList.Count; i++)
                        {
                            for (int j = (i + 1); j < self.WaypointsList.Count; j++)
                            {
                                Handles.DrawLine(self.WaypointsList[i], self.WaypointsList[j]);
                            }
                        }
                    }
                    Handles.color = Color.white;

                    Handles.color = Color.green;
                    if (self.WaypointType == (EmeraldMovement.WaypointTypes.Loop))
                    {
                        Handles.DrawLine(self.WaypointsList[0], self.WaypointsList[self.WaypointsList.Count - 1]);
                    }

                    // 最後に操作したウェイポイントを強調表示
                    Handles.color = new Color(0, 1, 0, 0.25f);
                    for (int i = 0; i < self.WaypointsList.Count; i++)
                    {
                        if (CurrentWaypointIndex != i)
                            Handles.color = new Color(1, 1, 1, 0.05f);
                        else
                            Handles.color = new Color(1, 1, 0, 0.05f);
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                        Handles.DrawSolidDisc(self.WaypointsList[i], Vector3.up, self.StoppingDistance);
                        Handles.color = new Color(0, 0, 0, 0.5f);
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                        Handles.DrawSolidDisc(self.WaypointsList[i], Vector3.up, 0.25f);
                    }

                    // 位置ハンドルを表示し、ドラッグで座標を更新
                    for (int i = 0; i < self.WaypointsList.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 Pos = Handles.PositionHandle(self.WaypointsList[i], Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(self, "Changed Waypoint Position");
                            self.WaypointsList[i] = Pos;
                            CurrentWaypointIndex = i;
                        }

                        Handles.color = Color.white;
                        CustomEditorProperties.DrawString("ウェイポイント " + (i + 1), self.WaypointsList[i] + Vector3.up, Color.white);
                    }
                }
            }
        }

        /// <summary>
        /// 【徘徊半径の可視化（日本語）】
        /// ダイナミック徘徊時に半径ガイドを描画します。
        /// </summary>
        void DrawWanderArea(EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic && WanderFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = new Color(0, 0.6f, 0, 1f);
                Handles.DrawWireDisc(self.transform.position, Vector3.up, (float)self.WanderRadius, 3f);
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// 【目的地の可視化（日本語）】
        /// 目的地徘徊時に、目的地点をハンドルで表示・編集します。
        /// </summary>
        void DrawSingleDestinationPoint(EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Destination && self.SingleDestination != Vector3.zero && WanderFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = Color.green;
                Handles.DrawLine(self.transform.position, self.SingleDestination);
                Handles.color = Color.white;

                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.SphereHandleCap(0, self.SingleDestination, Quaternion.identity, 0.5f, EventType.Repaint);
                CustomEditorProperties.DrawString("目的地", self.SingleDestination + Vector3.up, Color.white);

                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                self.SingleDestination = Handles.PositionHandle(self.SingleDestination, Quaternion.identity);

                EditorGUI.BeginChangeCheck();
                Vector3 Pos = Handles.PositionHandle(self.SingleDestination, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(self, "Changed Destination Position");
                    self.SingleDestination = Pos;
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(self);
#endif
            }
        }

        /// <summary>
        /// 【スライダー（アニメ更新フラグ付）ラッパー（日本語）】
        /// 値変更時に AnimationsUpdated を立てるためのユーティリティ。ロジックは原文どおりです。
        /// </summary>
        void CustomFloatAnimationField(Rect position, GUIContent label, SerializedProperty property, string Name, float Min, float Max)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Slider(Name, property.floatValue, Min, Max);

            if (newValue != property.floatValue)
            {
                AnimationsUpdatedProp.boolValue = true;
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = newValue;
            }

            EditorGUI.EndProperty();
        }
    }
}
