using UnityEngine;                               // Unity の基本 API
using System.Collections.Generic;                 // List などのコレクション
using UnityEngine.AI;                             // NavMeshAgent 等
using EmeraldAI.Utility;                          // Emerald のユーティリティ
using EmeraldAI.SoundDetection;                   // サウンド検知名前空間

namespace EmeraldAI
{
    /// <summary>
    /// （日本語）ほぼすべての Emerald コンポーネントを、この 1 つの Update から更新します。
    /// これにより、各コンポーネントが 1 箇所から容易にアクセスでき、Update の分散を防ぎます。
    /// </summary>
    #region Required Components
    [RequireComponent(typeof(EmeraldAnimation))]   // 必須：アニメーション
    [RequireComponent(typeof(EmeraldDetection))]   // 必須：検知（視界/派閥）
    [RequireComponent(typeof(EmeraldSounds))]      // 必須：サウンド
    [RequireComponent(typeof(EmeraldCombat))]      // 必須：戦闘
    [RequireComponent(typeof(EmeraldBehaviors))]   // 必須：行動
    [RequireComponent(typeof(EmeraldMovement))]    // 必須：移動
    [RequireComponent(typeof(EmeraldHealth))]      // 必須：体力
    [RequireComponent(typeof(BoxCollider))]        // 必須：コライダー
    [RequireComponent(typeof(NavMeshAgent))]       // 必須：NavMeshAgent
    [RequireComponent(typeof(AudioSource))]        // 必須：AudioSource
    [SelectionBase]                                // シーン上での選択基点
    #endregion
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/")]
    // 【クラス概要】EmeraldSystem：
    //  Emerald AI の“中枢”コンポーネント。各必須コンポーネントの参照を保持し、
    //  Awake/OnEnable/Update で初期化や一括更新を行う。
    public class EmeraldSystem : MonoBehaviour
    {
        #region Target Info
        // （注）これらは複数のコンポーネントで共通利用されるため、メインの EmeraldSystem に保持します。

        [Header("現在の戦闘ターゲット（Transform）")]
        [HideInInspector] public Transform CombatTarget;

        [Header("追従（フォロー）対象の Transform")]
        [HideInInspector] public Transform TargetToFollow;

        [Header("注視（LookAt）対象の Transform")]
        [HideInInspector] public Transform LookAtTarget;

        [Header("現在のターゲットに関する総合情報（IDamageable/ICombat 等）")]
        [HideInInspector][SerializeField] public CurrentTargetInfoClass CurrentTargetInfo = null;

        [System.Serializable]
        public class CurrentTargetInfoClass
        {
            [Header("ターゲットそのものの Transform（起点）")]
            public Transform TargetSource;

            [Header("ターゲットの IDamageable 参照（体力等）")]
            public IDamageable CurrentIDamageable;

            [Header("ターゲットの ICombat 参照（ダメージ位置等）")]
            public ICombat CurrentICombat;
        }
        #endregion

        #region Internal Components
        [Header("共有オブジェクトプール（静的・一度だけ生成）")]
        public static GameObject ObjectPool;

        [Header("Combat Text の Canvas 参照（静的・一度だけ生成）")]
        public static GameObject CombatTextSystemObject;

        [Header("この AI の NavMeshAgent 参照")]
        [HideInInspector] public NavMeshAgent m_NavMeshAgent;

        [Header("この AI の BoxCollider 参照")]
        [HideInInspector] public BoxCollider AIBoxCollider;

        [Header("この AI の Animator 参照")]
        [HideInInspector] public Animator AIAnimator;

        [Header("有効化されてからの経過時刻（Time.time を記録）")]
        [HideInInspector] public float TimeSinceEnabled;
        #endregion

        #region AI Components
        [Header("検知（EmeraldDetection）コンポーネント参照")]
        [HideInInspector] public EmeraldDetection DetectionComponent;

        [Header("行動（EmeraldBehaviors）コンポーネント参照")]
        [HideInInspector] public EmeraldBehaviors BehaviorsComponent;

        [Header("移動（EmeraldMovement）コンポーネント参照")]
        [HideInInspector] public EmeraldMovement MovementComponent;

        [Header("アニメ（EmeraldAnimation）コンポーネント参照")]
        [HideInInspector] public EmeraldAnimation AnimationComponent;

        [Header("戦闘（EmeraldCombat）コンポーネント参照")]
        [HideInInspector] public EmeraldCombat CombatComponent;

        [Header("サウンド（EmeraldSounds）コンポーネント参照")]
        [HideInInspector] public EmeraldSounds SoundComponent;

        [Header("体力（EmeraldHealth）コンポーネント参照")]
        [HideInInspector] public EmeraldHealth HealthComponent;

        [Header("最適化（EmeraldOptimization）コンポーネント参照")]
        [HideInInspector] public EmeraldOptimization OptimizationComponent;

        [Header("IK（EmeraldInverseKinematics）コンポーネント参照")]
        [HideInInspector] public EmeraldInverseKinematics InverseKinematicsComponent;

        [Header("UnityEvent ラッパー（EmeraldEvents）参照")]
        [HideInInspector] public EmeraldEvents EventsComponent;

        [Header("デバッガ（EmeraldDebugger）コンポーネント参照")]
        [HideInInspector] public EmeraldDebugger DebuggerComponent;

        [Header("UI（EmeraldUI）コンポーネント参照")]
        [HideInInspector] public EmeraldUI UIComponent;

        [Header("装備/アイテム（EmeraldItems）コンポーネント参照")]
        [HideInInspector] public EmeraldItems ItemsComponent;

        [Header("サウンド検知（EmeraldSoundDetector）参照")]
        [HideInInspector] public EmeraldSoundDetector SoundDetectorComponent;

        [Header("ターゲット位置補正（TargetPositionModifier）参照")]
        [HideInInspector] public TargetPositionModifier TPMComponent;

        [Header("部位ダメージ（LocationBasedDamage）参照")]
        [HideInInspector] public LocationBasedDamage LBDComponent;

        [Header("遮蔽（EmeraldCover）コンポーネント参照")]
        [HideInInspector] public EmeraldCover CoverComponent;
        #endregion

        // Initialize Emerald AI and its components
        void Awake()
        {
            // --- 主要コンポーネントの取得と静的システムの初期化 ---
            MovementComponent = GetComponent<EmeraldMovement>();                     // 移動
            AnimationComponent = GetComponent<EmeraldAnimation>();                   // アニメ
            SoundComponent = GetComponent<EmeraldSounds>();                          // サウンド
            DetectionComponent = GetComponent<EmeraldDetection>();                   // 検知
            BehaviorsComponent = GetComponent<EmeraldBehaviors>();                   // 行動
            CombatComponent = GetComponent<EmeraldCombat>();                         // 戦闘
            HealthComponent = GetComponent<EmeraldHealth>();                         // 体力
            OptimizationComponent = GetComponent<EmeraldOptimization>();             // 最適化
            EventsComponent = GetComponent<EmeraldEvents>();                         // イベント
            DebuggerComponent = GetComponent<EmeraldDebugger>();                     // デバッガ
            UIComponent = GetComponent<EmeraldUI>();                                 // UI
            ItemsComponent = GetComponent<EmeraldItems>();                           // アイテム
            SoundDetectorComponent = GetComponent<EmeraldSoundDetector>();           // 音検知
            InverseKinematicsComponent = GetComponent<EmeraldInverseKinematics>();   // IK
            CoverComponent = GetComponent<EmeraldCover>();                           // 遮蔽
            TPMComponent = GetComponent<TargetPositionModifier>();                   // 位置補正
            m_NavMeshAgent = GetComponent<NavMeshAgent>();                           // NavMeshAgent
            AIBoxCollider = GetComponent<BoxCollider>();                             // BoxCollider
            AIAnimator = GetComponent<Animator>();                                    // Animator
            InitializeEmeraldObjectPool();                                           // オブジェクトプール初期化（静的・一度だけ）
            InitializeCombatText();                                                  // コンバットテキスト初期化（静的・一度だけ）
        }

        void OnEnable()
        {
            TimeSinceEnabled = Time.time; // 有効化された時刻を記録

            // AI が既に死亡状態で有効化された場合、デフォルト状態へリセット。
            // （オブジェクトプーリングやスポーンシステム利用時を想定）
            if (AnimationComponent.IsDead)
            {
                ResetAI();
            }
        }

        /// <summary>
        /// （日本語）Emerald のオブジェクトプールを初期化します。
        /// ObjectPool は静的変数のため、最初の 1 回のみ生成します。
        /// </summary>
        void InitializeEmeraldObjectPool()
        {
            if (EmeraldSystem.ObjectPool == null)
            {
                EmeraldSystem.ObjectPool = new GameObject();
                EmeraldSystem.ObjectPool.name = "Emerald AI Pool"; // 名前は固定
                EmeraldObjectPool.Clear();                         // 既存プールをクリア
            }
        }

        /// <summary>
        /// （日本語）Emerald のコンバットテキストシステムを初期化します。
        /// CombatTextSystemObject は静的変数のため、最初の 1 回のみ生成します。
        /// </summary>
        void InitializeCombatText()
        {
            if (EmeraldSystem.CombatTextSystemObject == null)
            {
                GameObject m_CombatTextSystem = Instantiate((GameObject)Resources.Load("Combat Text System") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextSystem.name = "Combat Text System"; // システム本体

                GameObject m_CombatTextCanvas = Instantiate((GameObject)Resources.Load("Combat Text Canvas") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextCanvas.name = "Combat Text Canvas"; // 表示キャンバス

                EmeraldSystem.CombatTextSystemObject = m_CombatTextCanvas;
                CombatTextSystem.Instance.CombatTextCanvas = m_CombatTextCanvas;
                CombatTextSystem.Instance.Initialize();         // シングルトンの初期化
            }
        }

        /// <summary>
        /// （日本語）すべての主要スクリプトを EmeraldSystem から更新します。
        /// Health が 0 以下の場合は更新をスキップします。
        /// </summary>
        void Update()
        {
            if (HealthComponent.CurrentHealth <= 0) return;     // 死亡時は何もしない

            AnimationComponent.AnimationUpdate();               // アニメーションのカスタム Update
            MovementComponent.MovementUpdate();                 // 移動のカスタム Update
            BehaviorsComponent.BehaviorUpdate();                // 行動のカスタム Update
            DetectionComponent.DetectionUpdate();               // 検知のカスタム Update
            CombatComponent.CombatUpdate();                     // 戦闘のカスタム Update
            if (DebuggerComponent) DebuggerComponent.DebuggerUpdate(); // デバッガのカスタム Update（存在時のみ）
        }

        /// <summary>
        /// （日本語）AI を初期状態へリセットします（リスポーン等で有用）。
        /// </summary>
        public void ResetAI()
        {
            EmeraldAPI.Combat.ResetAI(this);                    // API を通じて一括リセット
        }
    }
}
