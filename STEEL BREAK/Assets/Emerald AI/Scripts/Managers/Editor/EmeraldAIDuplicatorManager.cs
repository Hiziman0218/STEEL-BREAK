using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Threading;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【EmeraldAIDuplicatorManager】
    /// 参照AI（Reference AI）の各種コンポーネント設定を、指定した複数のAIに複製するためのエディタウィンドウ。
    /// ・必須／オプション各コンポーネントの有無をトグルで選択
    /// ・武器オブジェクト、IK、ラグドール、LBD（部位ダメージ）など複雑な設定も名前一致でコピー可能
    /// ・進捗バー表示、確認ダイアログなどのUI整備
    /// </summary>
    public class EmeraldAIDuplicatorManager : EditorWindow
    {
        [Header("複製先のAI配列（ここにドラッグ＆ドロップで対象AIを追加）")]
        public GameObject[] AIToDuplicateTo;

        [Header("↑ AIToDuplicateTo の SerializedProperty 参照（内部用）")]
        SerializedProperty AIToDuplicateToProp;

        // Main Components（必須コンポーネント）
        [Header("必須: EmeraldSystem を複製するか")]
        public bool m_EmeraldSystem = true;

        [Header("必須: EmeraldAnimation を複製するか")]
        public bool m_EmeraldAnimation = true;

        [Header("必須: EmeraldCombat を複製するか")]
        public bool m_EmeraldCombat = true;

        [Header("必須: EmeraldSounds を複製するか")]
        public bool m_EmeraldSounds = true;

        [Header("必須: EmeraldMovement を複製するか")]
        public bool m_EmeraldMovement = true;

        [Header("必須: EmeraldHealth を複製するか")]
        public bool m_EmeraldHealth = true;

        [Header("必須: EmeraldBehaviors を複製するか")]
        public bool m_EmeraldBehaviors = true;

        [Header("必須: EmeraldDetection を複製するか")]
        public bool m_EmeraldDetection = true;

        // Optional Components（任意コンポーネント）
        [Header("任意: EmeraldEvents を複製するか")]
        public bool m_EmeraldEvents = false;

        [Header("任意: EmeraldItems（アイテム/武器）を複製するか")]
        public bool m_EmeraldItems = false;

        [Header("任意: EmeraldInverseKinematics（アニメーションリギング）を複製するか")]
        public bool m_EmeraldInverseKinematics = false;

        [Header("任意: EmeraldOptimization（可視性による最適化）を複製するか")]
        public bool m_EmeraldOptimization = false;

        [Header("任意: EmeraldSoundDetector（音検知）を複製するか")]
        public bool m_EmeraldSoundDetector = false;

        [Header("任意: EmeraldUI（ヘルスバー/名前表示）を複製するか")]
        public bool m_EmeraldUI = false;

        [Header("任意: EmeraldFootsteps（フットステップ）を複製するか")]
        public bool m_EmeraldFootsteps = false;

        [Header("任意: EmeraldDebugger（デバッグ表示）を複製するか")]
        public bool m_EmeraldDebugger = false;

        [Header("任意: ラグドール関連コンポーネントを複製するか")]
        public bool m_RagdollComponents = false;

        [Header("任意: LBD（部位ダメージ）コンポーネントを複製するか")]
        public bool m_LBDComponents = false;

        [Header("任意: TargetPositionModifier（狙い位置補正）を複製するか")]
        public bool m_TargetPositionModifier = false;

        [Header("ウィンドウタイトル等のGUIスタイル（内部用）")]
        GUIStyle TitleStyle;

        [Header("ウィンドウ左上アイコン（内部用）")]
        Texture AIDuplicatorIcon;

        [Header("スクロール位置（内部用）")]
        Vector2 scrollPos;

        [Header("参照AI（コピー元）")]
        GameObject ReferenceAI;

        [Header("このウィンドウ自身の SerializedObject（内部用）")]
        SerializedObject serializedObject;

        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Window/Emerald AI/AI 複製マネージャ #%d", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldAIDuplicatorManager), false, "AI 複製マネージャ");
            APS.minSize = new Vector2(300, 250f); //500
        }

        void OnEnable()
        {
            if (AIDuplicatorIcon == null) AIDuplicatorIcon = Resources.Load("Editor Icons/EmeraldDuplicatorManager") as Texture;
            serializedObject = new SerializedObject(this);
            AIToDuplicateToProp = serializedObject.FindProperty("AIToDuplicateTo");
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 上部左余白
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "AI 複製マネージャ", AIDuplicatorIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  // 上部右余白
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); // 下部左余白
            EditorGUILayout.BeginVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "AI 複製 設定",
                "AI Duplicator Manager は、参照AI（Reference AI）から他のAIへ設定を複製できる強力なツールです。既に設定済みのコンポーネントを使い回せるため、開発を大幅に効率化できます。武器アイテム、アニメーションリギング（IK）、ラグドール、LBD（部位ダメージ）など複雑な情報もコピー可能です。",
                true);

            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("このフィールドは空にできません。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }
            ReferenceAI = (GameObject)EditorGUILayout.ObjectField("参照AI", ReferenceAI, typeof(GameObject), true);
            DescriptionElement("他のAIへ複製する際の基準となるAIを指定します。");
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            DisplayToggleOptions();

            //GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription(
                "AI リスト",
                "下のリストに複製先AIをドラッグ＆ドロップで追加してください。ここに含まれるすべてのオブジェクトへ、参照AIの設定が適用されます。",
                true);
            EditorGUILayout.PropertyField(AIToDuplicateToProp);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            DisplayDuplicateAIButton();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); // 下部右余白
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// トグル群（必須／任意コンポーネント）のレイアウト表示
        /// </summary>
        void DisplayToggleOptions()
        {
            if (position.width > 500)
            {
                // 必須コンポーネント
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                // 任意コンポーネント
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

        /// <summary>
        /// 必須コンポーネントの説明とトグル
        /// </summary>
        void RequiredSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "(必須コンポーネント)",
                "Emerald AI を動作させるために必要なコンポーネント群です。オンになっている項目は、セットアップ時に自動で追加されます。各コンポーネントには専用エディタがあり、詳細設定が可能です。各項目の ? アイコンにマウスオーバーすると説明ツールチップが表示されます。",
                true);

            m_EmeraldSystem = ToggleComponentElement(true, "Emerald System（基盤）", "すべての必須スクリプトの管理と更新を行う、Emerald AI の基盤スクリプトです。");
            m_EmeraldAnimation = ToggleComponentElement(m_EmeraldAnimation, "Emerald Animation（アニメ管理）", "Animation Profile を介してAIのアニメーション管理を行います。", false);
            m_EmeraldCombat = ToggleComponentElement(m_EmeraldCombat, "Emerald Combat（戦闘）", "戦闘機能、アビリティ設定、各種戦闘関連のオプションを提供します。", false);
            m_EmeraldSounds = ToggleComponentElement(m_EmeraldSounds, "Emerald Sounds（サウンド）", "Sound Profile を介してAIのサウンド管理を行います。", false);
            m_EmeraldMovement = ToggleComponentElement(m_EmeraldMovement, "Emerald Movement（移動）", "移動、回転、アラインメントに関する設定を管理します。", false);
            m_EmeraldHealth = ToggleComponentElement(m_EmeraldHealth, "Emerald Health（ダメージ）", "AI に被ダメージ機能を付与します。外部からは IDamageable インターフェイス経由で利用します。", false);
            m_EmeraldBehaviors = ToggleComponentElement(m_EmeraldBehaviors, "Emerald Behaviors（挙動）", "プリセットの挙動（ビヘイビア）を使用可能にします。必要に応じて EmeraldBehavior を継承してカスタム実装も可能です。", false);
            m_EmeraldDetection = ToggleComponentElement(m_EmeraldDetection, "Emerald Detection（検知）", "AI にターゲットの視認・検知機能を付与します。", false);
        }

        /// <summary>
        /// 任意コンポーネントの説明とトグル
        /// </summary>
        void OptionalSettings()
        {
            CustomEditorProperties.TextTitleWithDescription(
                "(任意コンポーネント)",
                "Emerald AI に追加機能を提供するコンポーネント群です。オンになっている項目はセットアップ時に自動で追加されます。各コンポーネントには専用エディタがあり、詳細設定が可能です。各項目の ? アイコンにマウスオーバーすると説明ツールチップが表示されます。",
                true);

            m_EmeraldEvents = ToggleComponentElement(m_EmeraldEvents, "Emerald Events（イベント）", "AI の多くのアクションに対してカスタムイベントを設定可能にします。", false);
            m_EmeraldItems = ToggleComponentElement(m_EmeraldItems, "Emerald Items（アイテム/装備）", "ドロップアイテム、装備アイテムなどをAIに設定できるようにします。", false);
            m_EmeraldInverseKinematics = ToggleComponentElement(m_EmeraldInverseKinematics, "Emerald Inverse Kinematics（IK）", "Unity Animation Rigging を用いたIKをAIで使用可能にします。", false);
            m_EmeraldOptimization = ToggleComponentElement(m_EmeraldOptimization, "Emerald Optimization（最適化）", "カメラ視野外でAIを無効化し、見えていないAIのコストを削減してパフォーマンスを向上させます。", false);
            m_EmeraldUI = ToggleComponentElement(m_EmeraldUI, "Emerald UI（UI表示）", "AIの頭上にヘルスバーや名前を表示する内蔵UIを使用可能にします。", false);
            m_EmeraldFootsteps = ToggleComponentElement(m_EmeraldFootsteps, "Emerald Footsteps（足音）", "検出した地面に応じてフットステップのサウンド／エフェクトを再生します。地形のテクスチャやオブジェクトのタグを使用可能です。\n\n注: フットステップのアニメーションイベント設定が必要です。移動アニメにイベントを設定してください。", false);
            m_EmeraldSoundDetector = ToggleComponentElement(m_EmeraldSoundDetector, "Emerald Sound Detector（音検知）", "プレイヤーや外部ソースが発する音をAIが聴き取れるようにします。", false);
            m_EmeraldDebugger = ToggleComponentElement(m_EmeraldDebugger, "Emerald Debugger（デバッグ）", "デバッグログ、ライン、経路情報などのデバッグ表示を有効化します。問題の特定や開発に役立ちます。", false);
            m_RagdollComponents = ToggleComponentElement(m_RagdollComponents, "Ragdoll Components（ラグドール）", "コライダー、ジョイント、リジッドボディを含むラグドール構成をコピーします。\n\n注: ボーン名が同一である必要があります。一致しない場合、この設定は無視されます。", false);
            m_LBDComponents = ToggleComponentElement(m_LBDComponents, "Location Based Damage Components（部位ダメージ）", "LBD コンポーネントを割り当て初期化します。一致するコライダーに対し、参照AIのダメージ倍率をコピーします。\n\n注: ダメージ倍率のコピーにはボーン名の一致が必要です。一致しないボーンはコピーされません。", false);
            m_TargetPositionModifier = ToggleComponentElement(m_TargetPositionModifier, "Target Position Modifier（狙い位置補正）", "ターゲットの狙い位置の高さを補正し、照準精度を向上させます。\n\n注: ボーン名が同一である必要があります。一致しない場合、この設定は無視されます。", false);
        }

        /// <summary>
        /// 「AI を複製」ボタンと確認／エラーメッセージ表示
        /// </summary>
        void DisplayDuplicateAIButton()
        {
            GUILayout.Space(15);
            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("複製プロセスを実行する前に、参照AIを指定してください。", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.BeginDisabledGroup(ReferenceAI == null);
            if (GUILayout.Button("AI を複製", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("AI 複製マネージャ", "AI リスト内の全AIに、選択した設定を複製します。よろしいですか？この操作は元に戻せません。", "はい", "キャンセル"))
                {
                    DuplicateAI();
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(25);
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 実際の複製処理本体
        /// </summary>
        void DuplicateAI()
        {
            for (int i = 0; i < AIToDuplicateTo.Length; i++)
            {
                GameObject G = AIToDuplicateTo[i];
                // 進捗バーを表示して複製の進行状況をユーザーに示す
                EditorUtility.DisplayProgressBar("AI を複製中...", (i + 1) + " 件目 / 全 " + (AIToDuplicateTo.Length) + " 件", (float)i / (float)AIToDuplicateTo.Length);

                if (AIToDuplicateTo[i].GetComponent<Animator>() != null)
                {
                    UnpackPrefab(AIToDuplicateTo[i]);

                    CopyAnimationComponent(G);
                    CopySoundsComponent(G);
                    CopyItemsComponent(G);
                    CopySoundDetectorComponent(G);
                    CopyUIComponent(G);
                    CopyFootstepsComponent(G);
                    CopyDebuggerComponent(G);
                    CopyTPMComponent(G);
                    CopyBehaviorsComponent(G);
                    CopyDetectionComponent(G);
                    CopyMovementComponent(G);
                    CopyHealthComponent(G);
                    CopyEmeraldBaseComponent(G);
                    CopyEventsComponent(G);
                    CopyCombatComponent(G);
                    CopyInverseKinematicsComponent(G);
                    CopyRagdollComponents(G);
                    CopyLBDComponent(G);
                    CopyOptimizationComponent(G);

                    CopyBoxCollider(G);
                    CopyAudioSource(G);
                    CopyTagAndLayer(G);

                    G.GetComponent<BoxCollider>().size = ReferenceAI.GetComponent<BoxCollider>().size;
                    G.GetComponent<BoxCollider>().center = ReferenceAI.GetComponent<BoxCollider>().center;

                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<EmeraldSystem>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<Animator>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<BoxCollider>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<UnityEngine.AI.NavMeshAgent>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<AudioSource>());
                }
                else
                {
                    Debug.Log("オブジェクト '" + G.name + "' には Animator Controller が無いため、処理をスキップしました。");
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 渡されたコンポーネントをインスペクタ下部へ移動
        /// </summary>
        void MoveToBottom(GameObject AIToDuplicateTo, Component ComponentToMove)
        {
            Component[] AllComponents = AIToDuplicateTo.GetComponents<Component>();

            for (int i = 0; i < AllComponents.Length; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(ComponentToMove);
            }
        }

        /// <summary>
        /// 引数の GameObject がプレハブなら Unpack（分解）する
        /// </summary>
        void UnpackPrefab(GameObject ObjectToUnpack)
        {
            PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(ObjectToUnpack);

            // プレハブの場合のみ Unpack を実施
            if (m_AssetType != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(ObjectToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// BoxCollider をコピー
        /// </summary>
        void CopyBoxCollider(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<BoxCollider>());

            if (G.GetComponent<BoxCollider>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
            else
            {
                G.AddComponent<BoxCollider>();
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
        }

        /// <summary>
        /// AudioSource をコピー
        /// </summary>
        void CopyAudioSource(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<AudioSource>());

            if (G.GetComponent<AudioSource>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
            else
            {
                G.AddComponent<AudioSource>();
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
        }

        /// <summary>
        /// Tag と Layer をコピー
        /// </summary>
        void CopyTagAndLayer(GameObject G)
        {
            G.tag = ReferenceAI.tag;
            G.layer = ReferenceAI.layer;
        }

        /// <summary>
        /// EmeraldSystem（基盤）をコピー
        /// </summary>
        void CopyEmeraldBaseComponent(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSystem>());

            if (G.GetComponent<EmeraldSystem>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
            else
            {
                G.AddComponent<EmeraldSystem>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
        }

        /// <summary>
        /// Movement をコピー
        /// </summary>
        void CopyMovementComponent(GameObject G)
        {
            if (!m_EmeraldMovement)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldMovement>());

            if (G.GetComponent<EmeraldMovement>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
            else
            {
                G.AddComponent<EmeraldMovement>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
        }

        /// <summary>
        /// Behaviors をコピー
        /// </summary>
        void CopyBehaviorsComponent(GameObject G)
        {
            if (!m_EmeraldBehaviors)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldBehaviors>());

            if (G.GetComponent<EmeraldBehaviors>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
            else
            {
                G.AddComponent<EmeraldBehaviors>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
        }

        /// <summary>
        /// Detection をコピー（HeadTransform の自動検出付き）
        /// </summary>
        void CopyDetectionComponent(GameObject G)
        {
            if (!m_EmeraldDetection)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDetection>());

            if (G.GetComponent<EmeraldDetection>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }
            else
            {
                G.AddComponent<EmeraldDetection>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }

            AutoFindHeadTransform(G);
        }

        /// <summary>
        /// Animation をコピー（Avatar を保持）
        /// </summary>
        void CopyAnimationComponent(GameObject G)
        {
            if (!m_EmeraldAnimation)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldAnimation>());

            if (G.GetComponent<EmeraldAnimation>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }
            else
            {
                G.AddComponent<EmeraldAnimation>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }

            // 参照AIの Animator 値をコピーするが、Avatar は元のものを維持
            Avatar m_Avatar = G.GetComponent<Animator>().avatar;
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<Animator>());
            ComponentUtility.PasteComponentValues(G.GetComponent<Animator>());
            G.GetComponent<Animator>().avatar = m_Avatar;
        }

        /// <summary>
        /// Combat をコピー（Attack Transform の調整付き）
        /// </summary>
        void CopyCombatComponent(GameObject G)
        {
            if (!m_EmeraldCombat)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldCombat>());

            if (G.GetComponent<EmeraldCombat>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }
            else
            {
                G.AddComponent<EmeraldCombat>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }

            CopyAttackTransforms(G);
        }

        /// <summary>
        /// Sounds をコピー
        /// </summary>
        void CopySoundsComponent(GameObject G)
        {
            if (!m_EmeraldSounds)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSounds>());

            if (G.GetComponent<EmeraldSounds>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
            else
            {
                G.AddComponent<EmeraldSounds>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
        }

        /// <summary>
        /// Health をコピー
        /// </summary>
        void CopyHealthComponent(GameObject G)
        {
            if (!m_EmeraldHealth)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldHealth>());

            if (G.GetComponent<EmeraldHealth>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
            else
            {
                G.AddComponent<EmeraldHealth>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
        }

        /// <summary>
        /// Events をコピー
        /// </summary>
        void CopyEventsComponent(GameObject G)
        {
            if (!m_EmeraldEvents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldEvents>());

            if (G.GetComponent<EmeraldEvents>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
            else
            {
                G.AddComponent<EmeraldEvents>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
        }

        /// <summary>
        /// Items をコピー（武器オブジェクトの複製/配置も行う）
        /// </summary>
        void CopyItemsComponent(GameObject G)
        {
            if (!m_EmeraldItems)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldItems>());

            if (G.GetComponent<EmeraldItems>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }
            else
            {
                G.AddComponent<EmeraldItems>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }

            CopyWeaponObjects(G);
        }

        /// <summary>
        /// SoundDetector をコピー
        /// </summary>
        void CopySoundDetectorComponent(GameObject G)
        {
            if (!m_EmeraldSoundDetector)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<SoundDetection.EmeraldSoundDetector>());

            if (G.GetComponent<SoundDetection.EmeraldSoundDetector>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
            else
            {
                G.AddComponent<SoundDetection.EmeraldSoundDetector>();
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
        }

        /// <summary>
        /// UI をコピー
        /// </summary>
        void CopyUIComponent(GameObject G)
        {
            if (!m_EmeraldUI)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldUI>());

            if (G.GetComponent<EmeraldUI>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
            else
            {
                G.AddComponent<EmeraldUI>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
        }

        /// <summary>
        /// Footsteps をコピー（足トランスフォームの割り当ても行う）
        /// </summary>
        void CopyFootstepsComponent(GameObject G)
        {
            if (!m_EmeraldFootsteps)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldFootsteps>());

            if (G.GetComponent<EmeraldFootsteps>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }
            else
            {
                G.AddComponent<EmeraldFootsteps>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }

            CopyFeetTransforms(G);
        }

        /// <summary>
        /// 足の Transform を名前一致で割り当て
        /// </summary>
        void CopyFeetTransforms(GameObject G)
        {
            var m_FootstepsComponent = G.GetComponent<EmeraldFootsteps>();
            m_FootstepsComponent.FeetTransforms.Clear();
            var m_FootstepsReference = ReferenceAI.GetComponent<EmeraldFootsteps>();

            if (m_FootstepsReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    for (int i = 0; i < m_FootstepsReference.FeetTransforms.Count; i++)
                    {
                        if (m_FootstepsReference.FeetTransforms[i].name == t.name)
                        {
                            m_FootstepsComponent.FeetTransforms.Add(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Debugger をコピー
        /// </summary>
        void CopyDebuggerComponent(GameObject G)
        {
            if (!m_EmeraldDebugger)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDebugger>());

            if (G.GetComponent<EmeraldDebugger>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
            else
            {
                G.AddComponent<EmeraldDebugger>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
        }

        /// <summary>
        /// TargetPositionModifier をコピー（Transform の一致割り当て付き）
        /// </summary>
        void CopyTPMComponent(GameObject G)
        {
            if (!m_TargetPositionModifier)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<TargetPositionModifier>());

            if (G.GetComponent<TargetPositionModifier>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }
            else
            {
                G.AddComponent<TargetPositionModifier>();
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }

            CopyTPM(G);
        }

        /// <summary>
        /// TPM の TransformSource を名前一致で割り当て
        /// </summary>
        void CopyTPM(GameObject G)
        {
            var m_TMPComponent = G.GetComponent<TargetPositionModifier>();
            var TMPReference = ReferenceAI.GetComponent<TargetPositionModifier>();

            if (TMPReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    if (t.name == TMPReference.TransformSource.name)
                    {
                        m_TMPComponent.TransformSource = t;
                    }
                }
            }
        }

        /// <summary>
        /// IK をコピー（RigBuilder層の再構築／参照差し替え含む）
        /// </summary>
        void CopyInverseKinematicsComponent(GameObject G)
        {
            if (!m_EmeraldInverseKinematics)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldInverseKinematics>());

            if (G.GetComponent<EmeraldInverseKinematics>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }
            else
            {
                G.AddComponent<EmeraldInverseKinematics>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }

            CopyInverseKinematicsComponents(G);
        }

        /// <summary>
        /// ラグドール（ConfigurableJoint/CharacterJoint）構成をコピー
        /// </summary>
        void CopyRagdollComponents(GameObject G)
        {
            if (!m_RagdollComponents)
                return;

            CopyRagdollConfigurableJointComponents(G);
            CopyRagdollCharacterJointComponents(G);
        }

        /// <summary>
        /// LBD（部位ダメージ）をコピーし、コライダーを初期化
        /// </summary>
        void CopyLBDComponent(GameObject G)
        {
            if (!m_LBDComponents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<LocationBasedDamage>());

            if (G.GetComponent<LocationBasedDamage>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }
            else
            {
                G.AddComponent<LocationBasedDamage>();
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }

            IntializeLBDComponent(G);
        }

        /// <summary>
        /// Optimization（最適化）をコピーし、Renderer を名前一致で割り当て
        /// </summary>
        void CopyOptimizationComponent(GameObject G)
        {
            if (!m_EmeraldOptimization)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldOptimization>());

            if (G.GetComponent<EmeraldOptimization>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }
            else
            {
                G.AddComponent<EmeraldOptimization>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }

            FindSingleRenderer(G);
        }

        /// <summary>
        /// 単一 Renderer 型の最適化対象を名前一致で検索・割り当て
        /// </summary>
        void FindSingleRenderer(GameObject G)
        {
            EmeraldOptimization LocalOptimization = G.GetComponent<EmeraldOptimization>();
            EmeraldOptimization RefOptimization = ReferenceAI.GetComponent<EmeraldOptimization>();

            // 以下の条件では処理しない
            if (RefOptimization == null || RefOptimization.AIRenderer == null || RefOptimization.MeshType == EmeraldOptimization.MeshTypes.LODGroup)
                return;

            LocalOptimization.AIRenderer = null;

            // 子階層を列挙して名前一致を探す
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            for (int i = 0; i < AllChildTransforms.Count; i++)
            {
                if (AllChildTransforms[i].gameObject.name == RefOptimization.AIRenderer.gameObject.name)
                {
                    if (AllChildTransforms[i].gameObject.GetComponent<Renderer>() != null)
                        LocalOptimization.AIRenderer = AllChildTransforms[i].gameObject.GetComponent<Renderer>();
                }
            }

            if (LocalOptimization.AIRenderer == null)
            {
                Debug.Log("オブジェクト '" + G.name + "' と参照AIで一致する Renderer を見つけられませんでした。Optimization コンポーネントから手動で AI Renderer を割り当ててください。");
            }
        }

        /// <summary>
        /// ConfigurableJoint を用いたラグドール構成のコピー
        /// </summary>
        void CopyRagdollConfigurableJointComponents(GameObject G)
        {
            // 参照側の収集
            List<ConfigurableJoint> ConfigurableJointList = new List<ConfigurableJoint>(ReferenceAI.GetComponentsInChildren<ConfigurableJoint>());

            // 複製先の取得
            var m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();

            // 子階層を列挙
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_ConfigurableJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (ConfigurableJointList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name) && AllChildTransforms[i].gameObject.GetComponent<ConfigurableJoint>() == null)
                    {
                        AllChildTransforms[i].gameObject.AddComponent<ConfigurableJoint>();
                    }
                }

                m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();
            }

            foreach (ConfigurableJoint C in m_ConfigurableJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && ConfigurableJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = ConfigurableJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_ConfigurableJoints.Find(go => go.name == RefJoint.name);

                        // ConfigurableJoint の値をコピー
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        // 参照側のコライダーがあればコピー
                        if (RefJoint.GetComponent<Collider>() != null)
                        {
                            UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        // ConnectedBody を名前一致で割り当て
                        if (RefJoint != null && RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Find(go => go.name == RefJoint.connectedBody.transform.name);
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        // Rigidbody の値をコピー
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// CharacterJoint を用いたラグドール構成のコピー
        /// </summary>
        void CopyRagdollCharacterJointComponents(GameObject G)
        {
            List<Rigidbody> RigidbodyList = new List<Rigidbody>();
            var m_RigidbodysAIToCopy = ReferenceAI.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody C in m_RigidbodysAIToCopy)
            {
                if (C != null)
                {
                    if (!RigidbodyList.Contains(C) && C.gameObject != ReferenceAI)
                    {
                        RigidbodyList.Add(C);
                    }
                }
            }

            List<CharacterJoint> CharacterJointList = new List<CharacterJoint>(ReferenceAI.GetComponentsInChildren<CharacterJoint>());

            // 複製先
            var m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();

            // 子階層を列挙
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_CharacterJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() != null))
                    {
                        AllChildTransforms[i].gameObject.AddComponent<CharacterJoint>();
                    }
                    else if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() == null))
                    {
                        // ルートオブジェクトの Rigidbody/Collider を複製
                        if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>() != null &&
                            RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>() != null)
                        {
                            // Rigidbody
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Rigidbody>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Rigidbody>());
                            }

                            // Collider
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Collider>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Collider>());
                            }
                        }
                    }
                }

                m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();
            }

            foreach (CharacterJoint C in m_CharacterJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && CharacterJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = CharacterJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_CharacterJoints.Find(go => go.name == RefJoint.name);

                        // CharacterJoint の値をコピー
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        // コライダーをコピー
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());

                        // なければ新規追加、あれば値のみ反映
                        if (LocalJoint.GetComponent<Collider>() == null)
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(LocalJoint.gameObject);
                        }
                        else
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        // ConnectedBody を名前一致で割り当て
                        if (RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Where(go => go.name == RefJoint.connectedBody.transform.name).Single();
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        // Rigidbody の値をコピー
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// HeadTransform を自動検出（"root/Root/ROOT" 直下から "head" 名を探索）
        /// </summary>
        void AutoFindHeadTransform(GameObject G)
        {
            EmeraldDetection TempDetectionComponent = G.GetComponent<EmeraldDetection>();
            TempDetectionComponent.HeadTransform = null;

            foreach (Transform root in G.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 3; i++)
                {
                    // ルートの子（最大3）から "root"/"Root"/"ROOT" を探す
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT")
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            // "head" を含む名前の Transform を検出
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD"))
                            {
                                TempDetectionComponent.HeadTransform = t;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 武器オブジェクト（ホルスター／手持ち）をコピーし、同名のボーンへ配置
        /// ※ 同一リグ前提
        /// </summary>
        void CopyWeaponObjects(GameObject G)
        {
            var m_ItemComponent = G.GetComponent<EmeraldItems>();

            for (int i = 0; i < m_ItemComponent.Type1EquippableWeapons.Count; i++)
            {
                GameObject HolsteredReference = m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject;

                if (HolsteredReference != null)
                {
                    GameObject HolsteredCopy = Instantiate(HolsteredReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HolsteredReference.transform.parent.name)
                        {
                            HolsteredCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HolsteredReference.transform);
                            ComponentUtility.PasteComponentValues(HolsteredCopy.transform);
                            HolsteredCopy.name = HolsteredReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject = HolsteredCopy;
                        }
                    }
                }

                GameObject HeldReference = m_ItemComponent.Type1EquippableWeapons[i].HeldObject;

                if (HeldReference != null)
                {
                    GameObject HeldCopy = Instantiate(HeldReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HeldReference.transform.parent.name)
                        {
                            HeldCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HeldReference.transform);
                            ComponentUtility.PasteComponentValues(HeldCopy.transform);
                            HeldCopy.name = HeldReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HeldObject = HeldCopy;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attack Transform（攻撃起点）をコピーし、同名ボーンへ配置
        /// </summary>
        void CopyAttackTransforms(GameObject G)
        {
            var m_EmeraldCombat = G.GetComponent<EmeraldCombat>();

            // Weapon Type 1 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType1AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType1AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == TransformReference.parent.name)
                        {
                            TransformCopy.SetParent(t);
                            ComponentUtility.CopyComponent(TransformReference);
                            ComponentUtility.PasteComponentValues(TransformCopy.transform);
                            m_EmeraldCombat.WeaponType1AttackTransforms[i] = TransformCopy;
                        }
                    }
                }
            }

            // Weapon Type 2 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType2AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType2AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);
                    ComponentUtility.CopyComponent(TransformReference);
                    ComponentUtility.PasteComponentValues(TransformCopy.transform);
                    m_EmeraldCombat.WeaponType2AttackTransforms[i] = TransformCopy;
                }
            }
        }

        /// <summary>
        /// LBD の初期化（複製先AIのコライダー列挙とダメージ倍率のコピー）
        /// </summary>
        void IntializeLBDComponent(GameObject G)
        {
            var m_LocationBasedDamageComponent = G.GetComponent<LocationBasedDamage>();
            m_LocationBasedDamageComponent.ColliderList.Clear();
            var m_ReferenceLBD = G.GetComponent<LocationBasedDamage>();
            var m_Colliders = G.GetComponentsInChildren<Collider>();
            m_LocationBasedDamageComponent.LBDComponentsLayer = m_ReferenceLBD.LBDComponentsLayer;

            foreach (Collider C in m_Colliders)
            {
                if (C != null)
                {
                    LocationBasedDamage.LocationBasedDamageClass lbdc = new LocationBasedDamage.LocationBasedDamageClass(C, 1);
                    if (!LocationBasedDamage.LocationBasedDamageClass.Contains(m_LocationBasedDamageComponent.ColliderList, lbdc) && C.gameObject != G)
                    {
                        m_LocationBasedDamageComponent.ColliderList.Add(lbdc);
                    }
                }
            }

            for (int i = 0; i < m_ReferenceLBD.ColliderList.Count; i++)
            {
                for (int j = 0; j < m_LocationBasedDamageComponent.ColliderList.Count; j++)
                {
                    if (m_ReferenceLBD.ColliderList[i].ColliderObject.name == m_LocationBasedDamageComponent.ColliderList[j].ColliderObject.name)
                    {
                        m_LocationBasedDamageComponent.ColliderList[j].DamageMultiplier = m_ReferenceLBD.ColliderList[i].DamageMultiplier;
                    }
                }
            }
        }

        /// <summary>
        /// IK 用 Rig の複製と参照差し替え（RigBuilder 層再構築、MultiAim/TwoBoneIK の割当）
        /// </summary>
        void CopyInverseKinematicsComponents(GameObject G)
        {
            var m_InverseKinematicsComponent = G.GetComponent<EmeraldInverseKinematics>();
            var m_RigBuilderComponent = G.GetComponent<UnityEngine.Animations.Rigging.RigBuilder>();

            if (m_RigBuilderComponent != null)
                m_RigBuilderComponent.layers.Clear();

            for (int i = 0; i < m_InverseKinematicsComponent.UpperBodyRigsList.Count; i++)
            {
                if (m_InverseKinematicsComponent.UpperBodyRigsList[i] != null)
                {
                    var RigReference = m_InverseKinematicsComponent.UpperBodyRigsList[i];

                    if (RigReference != null)
                    {
                        var RigCopy = Instantiate(RigReference, Vector3.zero, Quaternion.identity);
                        RigCopy.name = RigReference.name;
                        RigCopy.transform.SetParent(G.transform);
                        ComponentUtility.CopyComponent(RigReference.transform);
                        ComponentUtility.PasteComponentValues(RigCopy.transform);
                        RigCopy.name = RigReference.name;
                        m_InverseKinematicsComponent.UpperBodyRigsList[i] = RigCopy;

                        if (m_RigBuilderComponent != null)
                        {
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }
                        else
                        {
                            m_RigBuilderComponent = G.AddComponent<UnityEngine.Animations.Rigging.RigBuilder>();
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }

                        // MultiAimConstraint の参照差し替え
                        var m_MultiAimConstraints = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.MultiAimConstraint>();

                        for (int j = 0; j < m_MultiAimConstraints.Length; j++)
                        {
                            var m_SourceData = m_MultiAimConstraints[j].data;
                            Transform ConstrainedObjectReference = m_SourceData.constrainedObject;

                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == ConstrainedObjectReference.name)
                                {
                                    m_SourceData.constrainedObject = t;
                                    m_MultiAimConstraints[j].data = m_SourceData;
                                }
                            }
                        }

                        // TwoBoneIKConstraint の参照差し替え（ターゲット/ヒントは複製して同名親へ配置）
                        var m_TwoBoneIKConstraint = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.TwoBoneIKConstraint>();

                        for (int j = 0; j < m_TwoBoneIKConstraint.Length; j++)
                        {
                            var m_SourceData = m_TwoBoneIKConstraint[j].data;

                            // ヒントオブジェクト
                            Transform HintObjectReference = m_SourceData.hint;

                            if (HintObjectReference != null)
                            {
                                Transform HintObjectCopy = Instantiate(m_SourceData.hint, Vector3.zero, Quaternion.identity);
                                HintObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == HintObjectReference.parent.name)
                                    {
                                        HintObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(HintObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(HintObjectCopy.transform);
                                        m_SourceData.hint = HintObjectCopy;
                                    }
                                }
                            }

                            // ターゲットオブジェクト
                            Transform TargetObjectReference = m_SourceData.target;

                            if (TargetObjectReference != null)
                            {
                                Transform TargetObjectCopy = Instantiate(m_SourceData.target, Vector3.zero, Quaternion.identity);
                                TargetObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == TargetObjectReference.parent.name)
                                    {
                                        TargetObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(TargetObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(TargetObjectCopy.transform);
                                        m_SourceData.target = TargetObjectCopy;
                                    }
                                }
                            }

                            // ボーン参照（root/mid/tip）を名前一致で差し替え
                            Transform RootObjectReference = m_SourceData.root;
                            Transform MidObjectReference = m_SourceData.mid;
                            Transform TipObjectReference = m_SourceData.tip;

                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == RootObjectReference.name)
                                {
                                    m_SourceData.root = t;
                                }
                                else if (t.name == MidObjectReference.name)
                                {
                                    m_SourceData.mid = t;
                                }
                                else if (t.name == TipObjectReference.name)
                                {
                                    m_SourceData.tip = t;
                                }
                            }

                            // 変更を適用
                            m_TwoBoneIKConstraint[j].data = m_SourceData;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// トグル要素（名称と説明）を表示
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
        /// 下部説明ブロック
        /// </summary>
        void DescriptionElement(string DescriptionText)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.HelpBox(DescriptionText, MessageType.None, true);
            GUI.backgroundColor = Color.white;
        }
    }
}
