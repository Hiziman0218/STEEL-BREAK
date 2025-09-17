using System.Collections;                           // コルーチン用
using System.Collections.Generic;                   // コレクション関連
using UnityEngine;                                  // Unity 基本API
using EmeraldAI.Utility;                            // Emerald ユーティリティ

namespace EmeraldAI
{
    /// <summary>
    /// （日本語）このスクリプトは Emerald AI のあらゆる「行動（Behaviors）」と「状態（States）」を管理します。
    /// 多くの関数は、カスタム行動や機能を作るためにオーバーライド可能です。
    /// </summary>
    [System.Serializable]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/behaviors-component")]
    // 【クラス概要】EmeraldBehaviors：
    //  AI の非戦闘/警戒/攻撃/逃走などの状態遷移、各状態での移動・攻撃・追跡・撤退のロジックを司るコンポーネント。
    public class EmeraldBehaviors : MonoBehaviour
    {
        #region Behavior Variables
        [Header("主要コンポーネント EmeraldSystem 参照（内部で取得）")]
        protected EmeraldSystem EmeraldComponent;

        // 行動タイプの列挙。Passive=非交戦、Coward=臆病（逃走）、Aggressive=攻撃的
        public enum BehaviorTypes { Passive = 0, Coward = 1, Aggressive = 2 };

        [Header("現在の行動タイプ（Passive/Coward/Aggressive）")]
        public BehaviorTypes CurrentBehaviorType = BehaviorTypes.Aggressive;

        [Header("追従対象（コンパニオン等）。指定時は Follow ロジックを使用")]
        public Transform TargetToFollow;

        [Header("警戒状態（Cautious）の継続秒数（到達でAggressive/Fleeに遷移）")]
        public int CautiousSeconds = 0;

        [Header("追跡を無限に継続するか（GiveUpを無効化）")]
        public bool InfititeChase;

        [Header("Aggressive時：検知外になってから諦めるまでの秒数（追跡打ち切り）")]
        public int ChaseSeconds = 5;

        [Header("Coward時：検知外になってから諦めるまでの秒数（逃走打ち切り）")]
        public int FleeSeconds = 5;

        [Header("『遮蔽物で遮られたときのみ諦めタイマーを進める』条件を要求するか")]
        public bool RequireObstruction;

        [Header("低体力で逃走へ移行する閾値（最大体力に対する%）")]
        public int PercentToFlee = 20;

        [Header("逃走先の更新間隔（秒）")]
        public float UpdateFleePositionSeconds = 1.5f;

        [Header("開始地点から離れて良い最大距離（超過で帰還）")]
        public int MaxDistanceFromStartingArea = 30;

        [Header("追従時の停止距離（コンパニオン追従用）")]
        public float FollowingStoppingDistance = 2f;

        [Header("エイム中フラグ（攻撃の発動制御に使用）")]
        public bool IsAiming;

        // ↓ デリゲート/イベントはフィールドではないため Header は付けず、説明コメントを付与
        public delegate void StartFleeHandler(); // 逃走開始コールバック
        public event StartFleeHandler OnFlee;    // 逃走開始イベント

        [Header("低体力時に逃走へ移行するか（Yes/No）")]
        public YesOrNo FleeOnLowHealth = YesOrNo.No;

        [Header("開始地点付近を維持するか（Yes/No）")]
        public YesOrNo StayNearStartingArea = YesOrNo.No;

        /// <summary>
        /// （日本語）AI が警戒状態にいる時間の計測に使用するタイマー。
        /// </summary>
        [Header("警戒状態の経過時間（内部タイマー）")]
        protected float CautiousTimer;

        /// <summary>
        /// （日本語）逃走先の更新頻度を制御するためのタイマー。
        /// </summary>
        [Header("逃走先の更新タイマー（内部）")]
        protected float UpdateFleePositionTimer;

        /// <summary>
        /// （日本語）ターゲットが検知半径外にいる時間を計測するタイマー。
        /// </summary>
        [Header("検知外の経過時間（ギブアップ判定用）")]
        protected float GiveUpTimer;

        /// <summary>
        /// （日本語）攻撃のクールダウン長を計測するタイマー。
        /// </summary>
        [Header("攻撃クールダウン計測タイマー")]
        protected float AttackTimer;

        /// <summary>
        /// （日本語）現在の行動状態名。複数段階のカスタム状態に拡張しやすいよう string 管理。
        /// </summary>
        [Header("現在の行動状態名（\"Non Combat\"/\"Cautious\"/\"Aggressive\"/\"Flee\"）")]
        public string BehaviorState = "Non Combat";
        #endregion

        #region Editor Variables
        [Header("インスペクタ：設定を隠す（エディタ内部用）")]
        [HideInInspector] public bool HideSettingsFoldout;

        [Header("インスペクタ：Behavior 設定の折りたたみ（エディタ内部用）")]
        [HideInInspector] public bool BehaviorSettingsFoldout;

        [Header("インスペクタ：カスタム設定の折りたたみ（エディタ内部用）")]
        [HideInInspector] public bool CustomSettingsFoldout;
        #endregion

        public virtual void Start()
        {
            InitailizeBehaviors(); // 行動コンポーネントの初期化
        }

        /// <summary>
        /// （日本語）Behavior コンポーネントを初期化します。
        /// </summary>
        public virtual void InitailizeBehaviors()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // 中核参照を取得

            ResetState(); // タイマーと状態を既定（Non Combat）へ

            if (TargetToFollow != null)
            {
                if (!TargetToFollow.gameObject.activeSelf)
                {
                    Debug.LogError("The '" + gameObject.name + "' AI's Follower Target '" + TargetToFollow.name + "' is disabled so it has been removed as the AI's follower. You can enable said gameobject or use the SetFollowerTarget(Transform) API to assign a follower through code if needed.");
                    TargetToFollow = null; // 無効な追従対象は解除
                }
                else
                {
                    StartCoroutine(SetFollowerTargetInternal()); // わずかな遅延後に追従対象を適用（他コンポーネントの初期化完了を待つ）
                }
            }

            // 各種イベントへの購読
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += OnDetectTarget; // 敵検知で警戒へ
            EmeraldComponent.DetectionComponent.OnNullTarget += ResetState;              // ターゲット喪失でリセット
            EmeraldComponent.CombatComponent.OnKilledTarget += OnKilledTarget;          // 撃破で非戦闘へ
            EmeraldComponent.HealthComponent.OnTakeDamage += OnTakeDamage;              // 被ダメ時の状態遷移

            // Passive の場合はタグ/レイヤーを中立化
            if (CurrentBehaviorType == BehaviorTypes.Passive)
            {
                if (!gameObject.CompareTag("Untagged"))
                {
                    gameObject.tag = "Untagged";
                }
                if (gameObject.layer != 0)
                {
                    gameObject.layer = 0;
                }
            }
        }

        /// <summary>
        /// （日本語）他コンポーネントの初期化完了を少し待ってから、追従対象を適用します。
        /// </summary>
        IEnumerator SetFollowerTargetInternal()
        {
            yield return new WaitForSeconds(0.1f);                        // 100ms 待機
            EmeraldComponent.DetectionComponent.SetTargetToFollow(TargetToFollow); // 追従対象をセット
        }

        /// <summary>
        /// （日本語）Behavior オブジェクトの連続更新処理。擬似 Update として動作し、EmeraldComponent の情報を参照して状態別の処理を行います。
        /// </summary>
        public virtual void BehaviorUpdate()
        {
            if (EmeraldComponent.AnimationComponent.IsDead)
                return; // 死亡時は何もしない

            switch (BehaviorState)
            {
                case "Non Combat":
                    WanderBehavior();   // 徘徊/追従
                    break;
                case "Cautious":
                    CautiousBehavior(); // 警戒
                    break;
                case "Aggressive":
                    AggressiveBehavior();// 追跡/攻撃
                    break;
                case "Flee":
                    CowardBehavior();   // 逃走
                    break;
            }

            // 検知半径を外れたターゲットの監視（必要に応じて「諦め」や「帰還」を実行）
            // カスタマイズしたい場合は DetectTargetTracker をオーバーライド
            DetectTargetTracker();
        }

        /// <summary>
        /// （日本語）警告アニメを再生し現在ターゲットを注視。CautiousSeconds に達したら行動タイプに応じて Aggressive または Flee へ遷移。
        /// </summary>
        public virtual void CautiousBehavior()
        {
            if (EmeraldComponent.CombatTarget && EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.AIAgentActive && CurrentBehaviorType != BehaviorTypes.Passive)
            {
                // 追従対象がある場合は警戒をスキップして Aggressive へ
                if (CurrentBehaviorType == BehaviorTypes.Aggressive && EmeraldComponent.TargetToFollow) BehaviorState = "Aggressive";

                CautiousTimer += Time.deltaTime; // 経過加算

                if (CautiousTimer >= CautiousSeconds)
                {
                    if (CurrentBehaviorType == BehaviorTypes.Aggressive)
                    {
                        BehaviorState = "Aggressive";
                    }
                    else if (CurrentBehaviorType == BehaviorTypes.Coward)
                    {
                        OnFlee?.Invoke(); // 逃走開始イベント発火
                        BehaviorState = "Flee";
                    }

                    CautiousTimer = 0;
                }

                if (CautiousTimer > 2)
                {
                    EmeraldComponent.AnimationComponent.PlayWarningAnimation(); // 警告アニメ
                }
            }
        }

        /// <summary>
        /// （日本語）現在ターゲットを能動的に追跡し、条件が整えば攻撃します。
        /// </summary>
        public virtual void AggressiveBehavior()
        {
            if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.AIAgentActive && EmeraldComponent.CombatTarget)
            {
                // ターゲットまで到達可能か（経路の有無）
                bool CanReachTarget = EmeraldComponent.MovementComponent.CanReachTarget;

                // 組み込みの戦闘用移動（目的地をターゲット位置に設定、近すぎる場合は後退）
                if (!EmeraldComponent.MovementComponent.DefaultMovementPaused)
                {
                    EmeraldComponent.MovementComponent.CombatMovement();
                }
                else if (!EmeraldComponent.MovementComponent.DefaultMovementPaused && CanReachTarget)
                {
                    EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position + transform.forward * 2);
                }

                // 戦闘行動の更新（有効化から2秒間は初期化待ちで抑制）
                if (Time.time - EmeraldComponent.TimeSinceEnabled > 2f) EmeraldComponent.CombatComponent.UpdateActions();

                Attack(); // 優先度高：攻撃条件を満たせば攻撃

                // 低体力で逃走へ移行
                if (FleeOnLowHealth == YesOrNo.Yes && ((float)EmeraldComponent.HealthComponent.CurrentHealth / (float)EmeraldComponent.HealthComponent.StartingHealth) < (PercentToFlee * 0.01f))
                {
                    EmeraldComponent.AnimationComponent.ResetTriggers(0);
                    OnFlee?.Invoke(); // 逃走開始イベント
                    BehaviorState = "Flee";
                }
            }
        }

        /// <summary>
        /// （日本語）組み込みの逃走移動（ターゲットから反対方向へ目的地生成）。UpdateFleePositionSeconds ごと、または到達時に更新。
        /// </summary>
        public virtual void CowardBehavior()
        {
            if (!EmeraldComponent.MovementComponent.DefaultMovementPaused)
            {
                UpdateFleePositionTimer += Time.deltaTime;
                if (UpdateFleePositionTimer > UpdateFleePositionSeconds || EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.MovementComponent.StoppingDistance)
                {
                    EmeraldComponent.MovementComponent.FleeMovement(); // 新たな逃走先を設定
                    UpdateFleePositionTimer = 0;
                }
            }
        }

        /// <summary>
        /// （日本語）非戦闘時は組み込みの徘徊（WanderType に従う）または追従（TargetToFollow 指定時）を実行。
        /// </summary>
        public virtual void WanderBehavior()
        {
            if (EmeraldComponent.MovementComponent.AIAgentActive && !EmeraldComponent.CombatComponent.CombatState && !EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                if (!EmeraldComponent.TargetToFollow)
                {
                    EmeraldComponent.MovementComponent.Wander(); // 徘徊
                }
                else
                {
                    EmeraldComponent.MovementComponent.FollowCompanionTarget(FollowingStoppingDistance); // 追従
                }
            }
        }

        /// <summary>
        /// （日本語）ターゲットが検知半径外にいる時間を追跡し、時間超過で諦めて戦闘を中止します。
        /// また、開始地点からの乖離が大きい場合は帰還処理を行います。
        /// </summary>
        public virtual void DetectTargetTracker()
        {
            if (!EmeraldComponent.CombatComponent.CombatState || EmeraldComponent.CombatComponent.DeathDelayActive || InfititeChase || EmeraldComponent.TargetToFollow)
                return; // 非戦闘/死亡遅延中/無限追跡/追従時は監視しない

            // 検知外（または遮蔽が必要条件で遮蔽あり）の経過時間を加算
            if (EmeraldComponent.CombatComponent.DistanceFromTarget > EmeraldComponent.DetectionComponent.DetectionRadius && !RequireObstruction || RequireObstruction && EmeraldComponent.DetectionComponent.TargetObstructed)
            {
                GiveUpTimer += Time.deltaTime;

                // 追跡/逃走それぞれの打ち切り秒に達したら戦闘を中止。警戒中も中止。
                if (GiveUpTimer >= ChaseSeconds && CurrentBehaviorType == BehaviorTypes.Aggressive || GiveUpTimer >= FleeSeconds && CurrentBehaviorType == BehaviorTypes.Coward || BehaviorState == "Cautious")
                {
                    CancelCombat(); // 戦闘/追跡/逃走を停止
                }
            }
            else
            {
                GiveUpTimer = 0; // 検知内に戻ったらリセット
            }

            // 開始地点からの距離超過を監視（Aggressive かつ「開始位置維持」有効時）
            if (CurrentBehaviorType == BehaviorTypes.Aggressive && StayNearStartingArea == YesOrNo.Yes && Vector3.Distance(EmeraldComponent.MovementComponent.StartingDestination, transform.position) > MaxDistanceFromStartingArea)
            {
                EmeraldComponent.MovementComponent.EnableReturnToStart(); // 開始地点へ帰還
                CancelCombat(); // 戦闘/追跡を停止
            }
        }

        /// <summary>
        /// （日本語）Aggressive 状態で攻撃条件を常時チェックし、整えば攻撃をトリガします（優先度の高い状態）。
        /// </summary>
        public virtual void Attack()
        {
            var EnterConditions = EmeraldComponent.AnimationComponent.IsIdling || EmeraldComponent.AnimationComponent.IsMoving; // 立ち/移動中に入れる
            var CooldownConditions = EmeraldComponent.AnimationComponent.IsIdling || EmeraldComponent.AnimationComponent.IsMoving || EmeraldComponent.AnimationComponent.IsBackingUp ||
                EmeraldComponent.AnimationComponent.IsTurningLeft || EmeraldComponent.AnimationComponent.IsTurningRight || EmeraldComponent.AnimationComponent.IsGettingHit; // クールダウン進行可

            if (CooldownConditions) AttackTimer += Time.deltaTime; // 攻撃CD加算

            if (EmeraldCombatManager.AllowedToAttack(EmeraldComponent) && EnterConditions && !IsAiming && AttackTimer >= EmeraldComponent.CombatComponent.CurrentAttackCooldown)
            {
                EmeraldCombatManager.CheckAttackHeight(EmeraldComponent); // 高さ条件チェック

                if (!EmeraldComponent.CombatComponent.TargetOutOfHeightRange)
                {
                    EmeraldComponent.AnimationComponent.IsMoving = false;
                    EmeraldComponent.CombatComponent.AdjustCooldowns(); // 攻撃毎のCD補正
                    EmeraldComponent.CombatComponent.AttackPosition = EmeraldComponent.CombatTarget.position - transform.position;
                    EmeraldComponent.CombatComponent.AttackPosition.y = 0;
                    EmeraldComponent.AnimationComponent.PlayAttackAnimation(); // 攻撃を再生
                    AttackTimer = 0;
                }
                else
                {
                    AttackTimer = 0; // 高さ不適合でもCDはリセット
                }
            }

            // 攻撃トリガ済みでも、ターゲットが射程外ならキャンセル
            if (AttackTimer >= EmeraldComponent.CombatComponent.CurrentAttackCooldown)
            {
                if (EmeraldComponent.m_NavMeshAgent.remainingDistance > EmeraldComponent.m_NavMeshAgent.stoppingDistance && EmeraldComponent.AIAnimator.GetBool("Attack"))
                {
                    EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                    AttackTimer = 0;
                }
            }
        }

        /// <summary>
        /// （日本語）戦闘と追跡、または逃走を停止します。
        /// </summary>
        public virtual void CancelCombat()
        {
            EmeraldComponent.CombatComponent.ClearTarget();                                             // ターゲット解除
            EmeraldComponent.CombatComponent.DeathDelayActive = true;                                   // 短期的な抑止
            EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position + EmeraldComponent.transform.forward * (EmeraldComponent.MovementComponent.StoppingDistance * 1.5f)); // 少し前進
            EmeraldComponent.AnimationComponent.ResetTriggers(0);                                       // 各種トリガをリセット
            ResetState();                                                                               // 状態初期化
        }

        /// <summary>
        /// （日本語）設定を既定値に戻します（状態=Non Combat、各タイマー=0）。
        /// </summary>
        public virtual void ResetState()
        {
            BehaviorState = "Non Combat";
            CautiousTimer = 0;
            UpdateFleePositionTimer = 0;
            GiveUpTimer = 0;
            AttackTimer = 0;
        }

        /// <summary>
        /// （日本語）ターゲットを撃破した際、状態を Non Combat に戻します（新ターゲットが見つかれば更新される）。
        /// </summary>
        public virtual void OnKilledTarget()
        {
            BehaviorState = "Non Combat";
            GiveUpTimer = 0;
            EmeraldComponent.AnimationComponent.WarningAnimationTriggered = false; // 警告アニメの再生可能化
        }

        /// <summary>
        /// （日本語）ターゲット検知時、戦闘移動が引き継ぐまでの間、目的地を現在位置に設定して静止します。
        /// </summary>
        public virtual void OnDetectTarget()
        {
            BehaviorState = "Cautious";                                   // 警戒に遷移
            if (isActiveAndEnabled) EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position); // いったん停止
        }

        /// <summary>
        /// （日本語）警戒中に被弾した場合、行動タイプに応じて Aggressive または Flee に遷移します。
        /// </summary>
        public virtual void OnTakeDamage()
        {
            if (BehaviorState == "Cautious" && CurrentBehaviorType == BehaviorTypes.Aggressive) BehaviorState = "Aggressive";
            else if (BehaviorState == "Cautious" && CurrentBehaviorType == BehaviorTypes.Coward) BehaviorState = "Flee";
        }
    }
}
