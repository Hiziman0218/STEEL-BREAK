using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldSetupManager】
    /// シーン内のオブジェクトへ Emerald AI の各コンポーネントを一括で追加・初期設定するエディタウィンドウ。
    /// ・必須／任意コンポーネントの選択追加
    /// ・タグ／レイヤーの適用
    /// ・アニメーション／サウンドプロファイルの割り当て
    /// ・Root Motion / NavMesh 駆動の選択 など
    /// </summary>
    public class EmeraldSetupManager : EditorWindow
    {
        [Header("セットアップ対象のオブジェクト（シーン内の GameObject を指定）")]
        public GameObject ObjectToSetup;

        [Header("AI に適用するレイヤー（Unity の Layer）")]
        public LayerMask AILayer = 4;

        [Header("AI に適用するタグ（Unity の Tag）")]
        public string AITag = "Untagged";

        [Header("一括セットアップ対象のオブジェクト群（任意）")]
        public List<GameObject> ObjectsToSetup = new List<GameObject>();

        //Required
        [Header("必須: Emerald System を追加するか")]
        public bool m_EmeraldSystem = true;

        [Header("必須: Emerald Animation を追加するか")]
        public bool m_EmeraldAnimation = true;

        [Header("必須: Emerald Combat を追加するか")]
        public bool m_EmeraldCombat = true;

        [Header("必須: Emerald Sounds を追加するか")]
        public bool m_EmeraldSounds = true;

        [Header("必須: Emerald Movement を追加するか")]
        public bool m_EmeraldMovement = true;

        [Header("必須: Emerald Health を追加するか")]
        public bool m_EmeraldHealth = true;

        [Header("必須: Emerald Behaviors を追加するか")]
        public bool m_EmeraldBehaviors = true;

        [Header("必須: Emerald Detection を追加するか")]
        public bool m_EmeraldDetection = true;

        //Optional
        [Header("任意: Emerald Events を追加するか")]
        public bool m_EmeraldEvents = false;

        [Header("任意: Emerald Items（アイテム/装備）を追加するか")]
        public bool m_EmeraldItems = false;

        [Header("任意: Emerald Inverse Kinematics（IK）を追加するか")]
        public bool m_EmeraldInverseKinematics = false;

        [Header("任意: Emerald Optimization（視界外で無効化）を追加するか")]
        public bool m_EmeraldOptimization = false;

        [Header("任意: Emerald Sound Detector（音検知）を追加するか")]
        public bool m_EmeraldSoundDetector = false;

        [Header("任意: Emerald UI（ヘルスバー/名前）を追加するか")]
        public bool m_EmeraldUI = false;

        [Header("任意: Emerald Footsteps（足音/足跡）を追加するか")]
        public bool m_EmeraldFootsteps = false;

        [Header("任意: Emerald Debugger（デバッグ表示）を追加するか")]
        public bool m_EmeraldDebugger = false;

        [Header("任意: Target Position Modifier（狙い位置補正）を追加するか")]
        public bool m_TargetPositionModifier = false;

        [Header("（任意）割り当てる Animation Profile")]
        public AnimationProfile m_AnimationProfile;

        [Header("（任意）割り当てる Sound Profile")]
        public EmeraldSoundProfile m_SoundProfile;

        [Header("アニメータの駆動方式（RootMotion / NavMeshDriven）")]
        public AnimatorTypeState AnimatorType = AnimatorTypeState.RootMotion;
        public enum AnimatorTypeState { RootMotion, NavMeshDriven }

        [Header("ウィンドウタイトル用スタイル（内部用）")]
        GUIStyle TitleStyle;

        [Header("スクロール位置（内部用）")]
        Vector2 scrollPos;

        [Header("設定アイコン（内部用）")]
        Texture SettingsIcon;

        [Header("セットアップ完了ダイアログを表示するか（内部用）")]
        bool DisplayConfirmation = false;

        [Header("対象がシーン内にあるか（内部用）")]
        bool IsInScene;

        [Header("完了ダイアログを今後表示しない（内部用）")]
        static bool DontShowDisplayConfirmation = false;

        void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Window/Emerald AI/セットアップ マネージャ #%e", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldSetupManager), false, "セットアップ マネージャ");
            APS.minSize = new Vector2(300, 250f); //500
        }

        void OnEnable()
        {
            if (SettingsIcon == null) SettingsIcon = Resources.Load("Editor Icons/EmeraldSetupManager") as Texture;
        }

        void OnGUI()
        {
            if (ObjectToSetup != null) IsInScene = ObjectToSetup.scene.IsValid();
            else IsInScene = true;

            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 上部 左余白
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "セットアップ マネージャ", SettingsIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  // 上部 右余白
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 下部 左余白
            EditorGUILayout.BeginVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "セットアップ設定",
                "このウィンドウで設定した内容で AI をセットアップします。ここで Animation Profile や Sound Profile を割り当てない場合は、セットアップ完了後に各エディタから手動で割り当ててください。",
                true);

            if (ObjectToSetup == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("このフィールドを空のままにはできません。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            ObjectToSetup = (GameObject)EditorGUILayout.ObjectField("セットアップ対象", ObjectToSetup, typeof(GameObject), true);
            if (!IsInScene) CustomEditorProperties.DisplayImportantMessage("セットアップ マネージャは Project タブ内のオブジェクトには使用できません。対象のオブジェクトがシーン内にあることを確認してから割り当ててください。");
            DescriptionElement("セットアップ マネージャが AI を構成する対象のオブジェクトです。");
            GUILayout.Space(10);


            AITag = EditorGUILayout.TagField("AI のタグ", AITag);
            DescriptionElement("AI に適用される Unity のタグです。※ Untagged は使用できません。");
            GUILayout.Space(10);

            AILayer = EditorGUILayout.LayerField("AI のレイヤー", AILayer);
            DescriptionElement("AI に適用される Unity のレイヤーです。※ Default は使用できません。");
            GUILayout.Space(10);


            m_AnimationProfile = (AnimationProfile)EditorGUILayout.ObjectField("アニメーション プロファイル", m_AnimationProfile, typeof(AnimationProfile), false);
            DescriptionElement("（任意）セットアップ時に適用する Animation Profile を割り当てます。");
            GUILayout.Space(10);

            m_SoundProfile = (EmeraldSoundProfile)EditorGUILayout.ObjectField("サウンド プロファイル", m_SoundProfile, typeof(EmeraldSoundProfile), false);
            DescriptionElement("（任意）セットアップ時に適用する Sound Profile を割り当てます。");
            GUILayout.Space(10);


            AnimatorType = (AnimatorTypeState)EditorGUILayout.EnumPopup("アニメータの種類", AnimatorType);
            DescriptionElement("この AI の移動と速度を Root Motion で駆動するか、NavMesh で駆動するかを選択します。");
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // どのコンポーネントを追加するかのトグル群を表示
            DisplayToggleOptions();

            if (ObjectToSetup == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("セットアップ処理を行うには、まず「セットアップ対象」にオブジェクトを割り当ててください。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            if (!IsInScene)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("セットアップ マネージャは Project タブ内のオブジェクトには使用できません。対象のオブジェクトがシーン内にあることを確認してから「セットアップ対象」に割り当ててください。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.BeginDisabledGroup(ObjectToSetup == null || !IsInScene);
            if (GUILayout.Button("AI をセットアップ", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Emerald Setup Manager", "このオブジェクトに AI をセットアップしてよろしいですか？", "セットアップ", "キャンセル"))
                {
                    UnpackPrefab(ObjectToSetup);
                    InitializeSetup();
                }
            }
            GUILayout.Space(25);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); // 下部 右余白
            GUILayout.EndHorizontal();

            if (DisplayConfirmation && !DontShowDisplayConfirmation)
            {
                if (EditorUtility.DisplayDialog("セットアップ マネージャ", "AI の作成が完了しました。いくつかのコンポーネントには、インスペクタ上部に警告として表示される追加設定が必要です。これらの警告に従って各コンポーネントを構成してください。", "OK", "OK（今後表示しない）"))
                {
                    DisplayConfirmation = false;
                }
                else
                {
                    DisplayConfirmation = false;
                    DontShowDisplayConfirmation = true;
                }
            }
        }

        /// <summary>
        /// 渡された GameObject がプレハブの場合は Unpack（分解）します。
        /// </summary>
        void UnpackPrefab(GameObject ObjectToUnpack)
        {
            PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(ObjectToUnpack);

            // オブジェクトがプレハブである場合のみ Unpack を実施
            if (m_AssetType != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(ObjectToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// どのコンポーネントを追加するかを制御するトグル群を表示します。
        /// </summary>
        void DisplayToggleOptions()
        {
            if (position.width > 500)
            {
                // 必須
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                // 任意
                GUILayout.Space(10);
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
            else
            {
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
        }

        void RequiredSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "（必須コンポーネント）",
                "Emerald AI の動作に必須なコンポーネント群です。オンになっている項目はセットアップ時に自動で追加されます。各コンポーネントには専用エディタがあり、機能に関連する様々な設定が用意されています。各項目の「？」アイコンにマウスオーバーすると説明ツールチップが表示されます。",
                true);
            m_EmeraldSystem = ToggleComponentElement(true, "Emerald System", "すべての必須スクリプトの管理と更新を行う、Emerald AI の基盤スクリプトです。");
            m_EmeraldAnimation = ToggleComponentElement(true, "Emerald Animation", "Animation Profile を通じて AI のアニメーション管理を行います。");
            m_EmeraldCombat = ToggleComponentElement(true, "Emerald Combat", "AI の戦闘機能、有効化するアビリティ、その他戦闘関連の設定を提供します。");
            m_EmeraldSounds = ToggleComponentElement(true, "Emerald Sounds", "Sound Profile を通じて AI のサウンド管理を行います。");
            m_EmeraldMovement = ToggleComponentElement(true, "Emerald Movement", "移動、回転、アラインメントに関連する設定を制御します。");
            m_EmeraldHealth = ToggleComponentElement(true, "Emerald Health", "AI に被ダメージ機能を付与します。外部からは IDamageable インターフェイス経由でアクセスします。");
            m_EmeraldBehaviors = ToggleComponentElement(true, "Emerald Behaviors", "プリセットの挙動（ビヘイビア）を使用可能にします。必要であれば EmeraldBehavior を継承したカスタム挙動も作成できます。");
            m_EmeraldDetection = ToggleComponentElement(true, "Emerald Detection", "AI にターゲットの視認・検知機能を付与します。");
        }

        void OptionalSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "（任意コンポーネント）",
                "Emerald AI に追加機能を提供する任意コンポーネント群です。オンになっている項目はセットアップ時に自動で追加されます。各コンポーネントには専用エディタがあり、機能に関連する様々な設定が用意されています。各項目の「？」アイコンにマウスオーバーすると説明ツールチップが表示されます。",
                true);
            m_EmeraldEvents = ToggleComponentElement(m_EmeraldEvents, "Emerald Events", "AI の多くのアクションに対してカスタムイベントを作成できるようにします。", false);
            m_EmeraldItems = ToggleComponentElement(m_EmeraldItems, "Emerald Items", "ドロップアイテムや装備アイテムなどを AI に設定できるようにします。", false);
            m_EmeraldInverseKinematics = ToggleComponentElement(m_EmeraldInverseKinematics, "Emerald Inverse Kinematics", "Unity の Animation Rigging を使用して IK を利用可能にします。", false);
            m_EmeraldOptimization = ToggleComponentElement(m_EmeraldOptimization, "Emerald Optimization", "カメラ視野外で AI を無効化し、表示されていない AI の負荷を削減してパフォーマンスを向上させます。", false);
            m_EmeraldUI = ToggleComponentElement(m_EmeraldUI, "Emerald UI", "AI の頭上にヘルスバーと名前を表示する内蔵 UI を使用可能にします。", false);
            m_EmeraldFootsteps = ToggleComponentElement(m_EmeraldFootsteps, "Emerald Footsteps", "検出した地面に応じて足音やエフェクトを再生します。Footstep Surface Objects でカスタマイズ可能。Unity Terrain のテクスチャと GameObject のタグの両方に対応。\n\n注: 足音を発火させるにはアニメーションイベントの設定が必要です。移動アニメーションへイベントを設定してください。", false);
            m_EmeraldSoundDetector = ToggleComponentElement(m_EmeraldSoundDetector, "Emerald Sound Detector", "プレイヤーや外部ソースの音を AI が聴き取れるようにします。", false);
            m_EmeraldDebugger = ToggleComponentElement(m_EmeraldDebugger, "Emerald Debugger", "デバッグログ、デバッグライン、経路情報などのデバッグ表示を有効化します。開発や問題の特定に役立ちます。", false);
            m_TargetPositionModifier = ToggleComponentElement(m_TargetPositionModifier, "Target Position Modifier", "ターゲットの狙い位置の高さを補正し、照準精度を向上させます。", false);
        }

        /// <summary>
        /// セットアップの初期化（前提条件を確認し、各種処理を実行）
        /// </summary>
        void InitializeSetup()
        {
            if (ObjectToSetup != null && ObjectToSetup.GetComponent<EmeraldSystem>() == null && ObjectToSetup.GetComponent<Animator>() != null)
            {
                AssignComponents();
                SetupBoxCollider();
                SetupAudio();
                SetupTagsAndLayers();
                SetupMovement();
                AutoFindHeadTransform();

                if (!DontShowDisplayConfirmation)
                {
                    DisplayConfirmation = true;
                }

                ObjectToSetup = null;
            }
            else if (ObjectToSetup == null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "AI オブジェクトのスロットに対象が割り当てられていません。割り当ててから再実行してください。", "OK");
                return;
            }
            else if (ObjectToSetup.GetComponent<EmeraldSystem>() != null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "このオブジェクトには既に Emerald AI コンポーネントが存在します。別のオブジェクトを選択して適用してください。", "OK");
                return;
            }
            else if (ObjectToSetup.GetComponent<Animator>() == null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "対象の AI に Animator コンポーネントがありません。追加してから再実行してください。", "OK");
                return;
            }
        }

        /// <summary>
        /// 選択されたコンポーネントを追加し、順序を整えます。
        /// </summary>
        void AssignComponents()
        {
            ObjectToSetup.AddComponent<EmeraldSystem>();
            if (m_EmeraldEvents) ObjectToSetup.AddComponent<EmeraldEvents>();
            if (m_EmeraldItems) ObjectToSetup.AddComponent<EmeraldItems>();
            if (m_EmeraldInverseKinematics) ObjectToSetup.AddComponent<EmeraldInverseKinematics>();
            if (m_EmeraldOptimization) ObjectToSetup.AddComponent<EmeraldOptimization>();
            if (m_EmeraldUI) ObjectToSetup.AddComponent<EmeraldUI>();
            if (m_EmeraldFootsteps) ObjectToSetup.AddComponent<EmeraldFootsteps>();
            if (m_EmeraldSoundDetector) ObjectToSetup.AddComponent<SoundDetection.EmeraldSoundDetector>();
            if (m_EmeraldDebugger) ObjectToSetup.AddComponent<EmeraldDebugger>();
            if (m_TargetPositionModifier) ObjectToSetup.AddComponent<TargetPositionModifier>();

            if (m_AnimationProfile != null) ObjectToSetup.GetComponent<EmeraldAnimation>().m_AnimationProfile = m_AnimationProfile;
            if (m_SoundProfile != null) ObjectToSetup.GetComponent<EmeraldSounds>().SoundProfile = m_SoundProfile;

            MoveToBottom(ObjectToSetup.GetComponent<EmeraldSystem>());
            MoveToBottom(ObjectToSetup.GetComponent<Animator>());
            MoveToBottom(ObjectToSetup.GetComponent<BoxCollider>());
            MoveToBottom(ObjectToSetup.GetComponent<UnityEngine.AI.NavMeshAgent>());
            MoveToBottom(ObjectToSetup.GetComponent<AudioSource>());
        }

        void MoveToBottom(Component ComponentToMove)
        {
            Component[] AllComponents = ObjectToSetup.GetComponents<Component>();

            for (int i = 0; i < AllComponents.Length; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(ComponentToMove);
            }
        }

        /// <summary>
        /// Root Motion の有無に依存する設定を含め、AI の移動設定を行います。
        /// </summary>
        void SetupMovement()
        {
            EmeraldMovement EmeraldMovement = ObjectToSetup.GetComponent<EmeraldMovement>();
            EmeraldMovement.MovementType = (EmeraldMovement.MovementTypes)AnimatorType;
            if (EmeraldMovement.MovementType == EmeraldMovement.MovementTypes.RootMotion)
            {
                EmeraldMovement.StationaryTurningSpeedCombat = 30;
                EmeraldMovement.StationaryTurningSpeedNonCombat = 30;
            }
        }

        /// <summary>
        /// ユーザー指定のタグとレイヤーを AI に設定します。
        /// </summary>
        void SetupTagsAndLayers()
        {
            ObjectToSetup.tag = AITag;
            ObjectToSetup.layer = AILayer;
            ObjectToSetup.GetComponent<EmeraldDetection>().DetectionLayerMask = (1 << LayerMask.NameToLayer("Water"));
        }

        /// <summary>
        /// AI の HeadTransform を自動検出します。
        /// </summary>
        void AutoFindHeadTransform()
        {
            foreach (Transform root in ObjectToSetup.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT") // ルート（最大3子）のみを探索
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD")) // ルート配下の Transform 名から "head" を含むものを探索
                            {
                                ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform = t;
                            }
                        }
                    }
                }
            }

            // ルート名のボーンが無く見つからない場合は条件を緩めて再探索
            if (ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform == null)
            {
                foreach (Transform t in ObjectToSetup.GetComponentsInChildren<Transform>())
                {
                    // Animation Rigging 用の MultiAimConstraint がある Transform は除外
                    if (t.GetComponent<UnityEngine.Animations.Rigging.MultiAimConstraint>() == null)
                    {
                        if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD")) // すべての Transform から "head" を含むものを探索
                        {
                            ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform = t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// AI の AudioSource のデフォルト値を設定します。
        /// </summary>
        void SetupAudio()
        {
            ObjectToSetup.GetComponent<AudioSource>().spatialBlend = 1;
            ObjectToSetup.GetComponent<AudioSource>().dopplerLevel = 0;
            ObjectToSetup.GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Linear;
            ObjectToSetup.GetComponent<AudioSource>().minDistance = 1;
            ObjectToSetup.GetComponent<AudioSource>().maxDistance = 50;
        }

        /// <summary>
        /// メイン Renderer の Bounds から AI のデフォルト BoxCollider を設定します。
        /// </summary>
        void SetupBoxCollider()
        {
            List<SkinnedMeshRenderer> TempSkinnedMeshes = new List<SkinnedMeshRenderer>();
            List<float> TempSkinnedMeshBounds = new List<float>();

            foreach (SkinnedMeshRenderer SMR in ObjectToSetup.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!TempSkinnedMeshes.Contains(SMR))
                {
                    TempSkinnedMeshes.Add(SMR);
                    TempSkinnedMeshBounds.Add(SMR.bounds.size.sqrMagnitude);
                }
            }

            float m_LargestBounds = TempSkinnedMeshBounds.Max();
            var AIRenderer = TempSkinnedMeshes[TempSkinnedMeshBounds.IndexOf(m_LargestBounds)];

            ObjectToSetup.GetComponent<BoxCollider>().size = new Vector3(AIRenderer.bounds.size.x / 3 / ObjectToSetup.transform.localScale.y, AIRenderer.bounds.size.y / ObjectToSetup.transform.localScale.y, AIRenderer.bounds.size.z / 3 / ObjectToSetup.transform.localScale.y);
            ObjectToSetup.GetComponent<BoxCollider>().center = new Vector3(ObjectToSetup.GetComponent<BoxCollider>().center.x, ObjectToSetup.GetComponent<BoxCollider>().size.y / 2, ObjectToSetup.GetComponent<BoxCollider>().center.z);
        }

        /// <summary>
        /// トグルUIの要素（名称と説明）を表示します。
        /// </summary>
        bool ToggleComponentElement(bool Setting, string Name, string Description, bool RequiredComponent = true)
        {
            EditorGUI.BeginDisabledGroup(RequiredComponent);
            EditorGUILayout.BeginHorizontal();
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(Name));
            Setting = EditorGUILayout.ToggleLeft(new GUIContent(Name), Setting, GUILayout.ExpandWidth(false), GUILayout.Width(textDimensions.x + 13.5f));
            EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("_Help").image, Description), GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(5);
            return Setting;
        }

        /// <summary>
        /// 下部の説明ボックスを表示します。
        /// </summary>
        void DescriptionElement(string DescriptionText)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.HelpBox(DescriptionText, MessageType.None, true);
            GUI.backgroundColor = Color.white;
        }
    }
}
