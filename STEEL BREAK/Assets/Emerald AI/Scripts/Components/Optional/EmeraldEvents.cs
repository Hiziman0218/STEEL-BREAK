using UnityEngine;            // Unity の基本APIを使用するため
using UnityEngine.Events;     // UnityEvent（イベントコールバック）を使用するため

namespace EmeraldAI             // EmeraldAI 用の名前空間
{
    /// <summary> // 既存英語コメントを保持
    /// Allows UnityEvents to work through Emerald's various usable callbacks.
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/events-component")] // Wiki へのヘルプURL（インスペクタから参照可能）
    // 【クラス概要】EmeraldEvents：Emerald AI の各種コールバック（検出/戦闘/移動/死亡など）に対して UnityEvent を紐づけ、ノーコードで外部反応を発火させるコンポーネント
    public class EmeraldEvents : MonoBehaviour   // MonoBehaviour を継承するイベント結線用クラス
    {
        #region Events Variables                  // —— イベント関連のメンバ変数（UnityEvent） ——

        [Header("有効化時（OnEnable）に発火するイベント（初期化タイミングで実行したい処理を紐づけ）")]
        public UnityEvent OnEnabledEvent;         // コンポーネントが有効化された瞬間に Invoke される

        [Header("Start() 実行時に発火するイベント（シーン開始/生成直後のフック）")]
        public UnityEvent OnStartEvent;           // Start で最初に Invoke される

        [Header("敵ターゲット（Enemy）が検出された時に発火するイベント")]
        public UnityEvent OnEnemyTargetDetectedEvent; // DetectionComponent の OnEnemyTargetDetected に対応

        [Header("戦闘開始時に発火するイベント（OnStartCombat デリゲートに対応）")]
        public UnityEvent OnStartCombatEvent;     // CombatComponent の OnStartCombat に対応

        [Header("戦闘終了時に発火するイベント（OnEndCombat デリゲートに対応）")]
        public UnityEvent OnEndCombatEvent;       // CombatComponent の OnEndCombat に対応

        [Header("攻撃アニメーション開始時に発火するイベント")]
        public UnityEvent OnAttackStartEvent;     // AnimationComponent の OnStartAttackAnimation に対応

        [Header("攻撃アニメーション終了時に発火するイベント")]
        public UnityEvent OnAttackEndEvent;       // AnimationComponent の OnEndAttackAnimation に対応

        [Header("通常ダメージを受けた時に発火するイベント")]
        public UnityEvent OnTakeDamageEvent;      // HealthComponent の OnTakeDamage に対応

        [Header("クリティカルダメージを受けた時に発火するイベント")]
        public UnityEvent OnTakeCritDamageEvent;  // HealthComponent の OnTakeCritDamage に対応

        [Header("通常ダメージを与えた時に発火するイベント")]
        public UnityEvent OnDoDamageEvent;        // CombatComponent の OnDoDamage に対応

        [Header("クリティカルダメージを与えた時に発火するイベント")]
        public UnityEvent OnDoCritDamageEvent;    // CombatComponent の OnDoCritDamage に対応

        [Header("ターゲットをキルした（撃破した）時に発火するイベント")]
        public UnityEvent OnKilledTargetEvent;    // CombatComponent の OnKilledTarget に対応

        [Header("このAIが死亡した時に発火するイベント")]
        public UnityEvent OnDeathEvent;           // HealthComponent の OnDeath に対応

        [Header("目的地に到達した時に発火するイベント（ナビ完了）")]
        public UnityEvent OnReachedDestinationEvent; // MovementComponent の OnReachedDestination に対応

        [Header("ウェイポイントに到達した時に発火するイベント（巡回等）")]
        public UnityEvent OnReachedWaypointEvent; // MovementComponent の OnReachedWaypoint に対応

        [Header("ウェイポイントが生成された時に発火するイベント（動的経路生成）")]
        public UnityEvent OnGeneratedWaypointEvent; // MovementComponent の OnGeneratedWaypoint に対応

        [Header("プレイヤー（友好/敵対問わず）を検出した時に発火するイベント")]
        public UnityEvent OnPlayerDetectedEvent;  // DetectionComponent の OnPlayerDetected に対応

        [Header("逃走挙動（Flee）を開始した時に発火するイベント")]
        public UnityEvent OnFleeEvent;            // BehaviorsComponent の OnFlee に対応

        [Header("EmeraldSystem 参照（各種コンポーネントやデリゲートへアクセスするための中核）")]
        EmeraldSystem EmeraldComponent;           // 実行時に GetComponent で取得し、各デリゲートにイベントを購読させる
        #endregion

        #region Editor Variables                   // —— インスペクタ表示制御用のフラグ ——

        [Header("インスペクタで設定群を隠す（折りたたみの表示制御）")]
        public bool HideSettingsFoldout;          // 設定セクションを非表示にするか

        [Header("一般系イベントの折りたたみ（開閉トグル）")]
        public bool GeneralEventsFoldout;         // 一般イベント（検出/移動等）セクションの開閉状態

        [Header("戦闘系イベントの折りたたみ（開閉トグル）")]
        public bool CombatEventsFoldout;          // 戦闘イベントセクションの開閉状態
        #endregion

        void Start()                               // Unity ライフサイクル：初期化（シーン開始時）
        {
            OnStartEvent.Invoke();                 // OnStartEvent を即時発火（リスナー未登録でも安全に呼べる）
            InitializeEvents();                    // 各種デリゲートと UnityEvent を紐づける初期化処理
        }

        /// <summary>
        /// Initialize the Events Component.        // 既存英語コメント：イベントコンポーネントの初期化
        /// </summary>
        void InitializeEvents()                   // デリゲート購読の一括設定
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // 同一 GameObject 上の EmeraldSystem を取得

            // —— Movement 系のデリゲートへ UnityEvent を購読 ——
            EmeraldComponent.MovementComponent.OnReachedDestination += OnReachedDestinationEvent.Invoke; // 目的地到達 → イベント発火
            EmeraldComponent.MovementComponent.OnReachedWaypoint += OnReachedWaypointEvent.Invoke;       // WP到達 → イベント発火
            EmeraldComponent.MovementComponent.OnGeneratedWaypoint += OnGeneratedWaypointEvent.Invoke;   // WP生成 → イベント発火

            // —— Detection 系のデリゲートへ UnityEvent を購読 ——
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += OnEnemyTargetDetectedEvent.Invoke; // 敵検出 → イベント発火
            EmeraldComponent.DetectionComponent.OnPlayerDetected += OnPlayerDetectedEvent.Invoke;           // プレイヤー検出 → イベント発火

            // —— Health 系のデリゲートへ UnityEvent を購読 —…
            EmeraldComponent.HealthComponent.OnDeath += OnDeathEvent.Invoke;                 // 死亡 → イベント発火
            EmeraldComponent.HealthComponent.OnTakeDamage += OnTakeDamageEvent.Invoke;       // 被ダメ → イベント発火
            EmeraldComponent.HealthComponent.OnTakeCritDamage += OnTakeCritDamageEvent.Invoke; // 被クリティカル → イベント発火

            // —— Combat / Animation 系のデリゲートへ UnityEvent を購読 ——
            EmeraldComponent.CombatComponent.OnKilledTarget += OnKilledTargetEvent.Invoke;   // 撃破 → イベント発火
            EmeraldComponent.AnimationComponent.OnStartAttackAnimation += OnAttackStartEvent.Invoke; // 攻撃開始アニメ → イベント発火
            EmeraldComponent.AnimationComponent.OnEndAttackAnimation += OnAttackEndEvent.Invoke;     // 攻撃終了アニメ → イベント発火
            EmeraldComponent.CombatComponent.OnDoDamage += OnDoDamageEvent.Invoke;           // 与ダメ → イベント発火
            EmeraldComponent.CombatComponent.OnDoCritDamage += OnDoCritDamageEvent.Invoke;   // 与クリティカル → イベント発火
            EmeraldComponent.CombatComponent.OnStartCombat += OnStartCombatEvent.Invoke;     // 戦闘開始 → イベント発火
            EmeraldComponent.CombatComponent.OnEndCombat += OnEndCombatEvent.Invoke;         // 戦闘終了 → イベント発火

            // —— Behavior 系のデリゲートへ UnityEvent を購読 ——
            EmeraldComponent.BehaviorsComponent.OnFlee += OnFleeEvent.Invoke;                // 逃走開始 → イベント発火
        }

        void OnEnable()                             // コンポーネントが有効化された際に呼ばれる
        {
            OnEnabledEvent.Invoke();                // 有効化イベントを発火（エディタ再生中の再有効化にも対応）
        }
    }
}
