using System.Collections;                                 // コルーチン関連
using System.Collections.Generic;                         // コレクション操作
using UnityEngine;                                        // Unity 基本API
using EmeraldAI.Utility;                                  // Emerald ユーティリティ

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/combat-component")]
    // 【クラス概要】EmeraldCombat：
    //  Emerald AI の戦闘全般を管理するコンポーネント（ICombat 実装）。
    //  ・武器タイプ（Type1/Type2）とアクション更新
    //  ・攻撃クールダウン、ターゲット角度/距離判定
    //  ・武器切替（距離/時間/なし）
    //  ・OnStart/End/ExitCombat、与ダメ/クリティカル/撃破などのイベントを提供
    public class EmeraldCombat : MonoBehaviour, ICombat
    {
        #region Combat Variables
        [Header("使用する武器コライダーのリスト")]
        public List<EmeraldWeaponCollision> WeaponColliders = new List<EmeraldWeaponCollision>();

        [Header("現在有効な武器コライダー")]
        public EmeraldWeaponCollision CurrentWeaponCollision;

        [Header("戦闘終了後に徘徊へ戻る最短秒")]
        public int MinResumeWander = 2;

        [Header("戦闘終了後に徘徊へ戻る最長秒")]
        public int MaxResumeWander = 4;

        [Header("現在の攻撃クールダウン（秒）")]
        public float CurrentAttackCooldown;

        [Header("武器タイプ1の攻撃クールダウン（秒）")]
        public float Type1AttackCooldown = 0.35f;

        [Header("武器タイプ2の攻撃クールダウン（秒）")]
        public float Type2AttackCooldown = 0.35f;

        [Header("武器切替の経過タイマー（秒）")]
        public float SwitchWeaponTimer = 0;

        [Header("武器切替がトリガー済みか")]
        public bool SwitchWeaponTypeTriggered = false;

        [Header("いずれかの戦闘アクションが稼働中か")]
        public bool CombatActionActive;

        [Header("現在の武器タイプに応じた戦闘アクション一覧")]
        [SerializeField] public List<ActionsClass> CombatActions = new List<ActionsClass>();

        [Header("武器タイプ1用の戦闘アクション一覧")]
        [SerializeField] public List<ActionsClass> Type1CombatActions = new List<ActionsClass>();

        [Header("武器タイプ2用の戦闘アクション一覧")]
        [SerializeField] public List<ActionsClass> Type2CombatActions = new List<ActionsClass>();

        [Header("攻撃開始時に記録する位置ベクトル（Yは0に正規化）")]
        public Vector3 AttackPosition;

        [Header("行動（ブロック/回避など）で軽減可能な被ダメ率（%）")]
        // Action Object 経由で設定され、ブロックや回避の軽減率に使用される
        public int MitigationAmount = 50;

        [Header("ダメージ軽減が有効な最大角度（度）")]
        public float MaxMitigationAngle = 75;

        // ↓ イベントはフィールドではないため Header は付けず、用途をコメントで説明
        public delegate void KilledTargetHandler();  // ターゲット撃破時コールバック
        public event KilledTargetHandler OnKilledTarget;
        public delegate void DoDamageHandler();      // 与ダメ時コールバック
        public event DoDamageHandler OnDoDamage;
        public delegate void DoCritDamageHandler();  // クリティカル与ダメ時コールバック
        public event DoCritDamageHandler OnDoCritDamage;
        public delegate void StartCombatHandler();   // 初回交戦開始時コールバック
        public event StartCombatHandler OnStartCombat;
        public delegate void EndCombatHandler();     // 交戦終了時（敵が近辺にいなくなった）コールバック
        public event EndCombatHandler OnEndCombat;

        // 下記は OnEndCombat と異なり、「実際に戦闘状態フラグを抜けた瞬間」に呼ばれる。
        // Movement/Detection/Animation 等が非戦闘へ戻るために使用。
        public delegate void ExitCombatHandler();
        public event ExitCombatHandler OnExitCombat;

        [Header("現在、戦闘状態か")]
        public bool CombatState;

        // 武器タイプ（Type1/Type2）
        public enum WeaponTypes { Type1 = 0, Type2 = 1 };

        [Header("開始時の武器タイプ")]
        public WeaponTypes StartingWeaponType = WeaponTypes.Type1;

        [Header("現在の武器タイプ")]
        public WeaponTypes CurrentWeaponType = WeaponTypes.Type1;

        [Header("タイプ1時のターゲット選択方式")]
        public PickTargetTypes Type1PickTargetType = PickTargetTypes.Closest;

        [Header("タイプ2時のターゲット選択方式")]
        public PickTargetTypes Type2PickTargetType = PickTargetTypes.Closest;

        // 武器タイプの数
        public enum WeaponTypeAmounts { One, Two };

        [Header("武器タイプの数（1/2）")]
        public WeaponTypeAmounts WeaponTypeAmount = WeaponTypeAmounts.One;

        [Header("武器タイプ1の攻撃セット（AttackClass）")]
        [SerializeField]
        public AttackClass Type1Attacks;

        [Header("武器タイプ2の攻撃セット（AttackClass）")]
        [SerializeField]
        public AttackClass Type2Attacks;

        [Header("武器切替後の再切替クールダウン（秒）")]
        public int SwitchWeaponTypesCooldown = 10;

        [Header("距離基準で切替える際の閾値（m）")]
        public int SwitchWeaponTypesDistance = 8;

        [Header("現在の攻撃に使用されるトランスフォーム")]
        public Transform CurrentAttackTransform;

        [Header("タイプ1武器の攻撃用トランスフォーム群")]
        public List<Transform> WeaponType1AttackTransforms = new List<Transform>();

        [Header("タイプ2武器の攻撃用トランスフォーム群")]
        public List<Transform> WeaponType2AttackTransforms = new List<Transform>();

        // 武器切替の方式（距離/時間/なし）
        public enum SwitchWeaponTypes { Distance, Timed, None };

        [Header("武器切替の方式（Distance/Timed/None）")]
        public SwitchWeaponTypes SwitchWeaponType = SwitchWeaponTypes.Timed;

        [Header("時間基準切替の最小秒")]
        public int SwitchWeaponTimeMin = 10;

        [Header("時間基準切替の最大秒")]
        public int SwitchWeaponTimeMax = 20;

        [Header("次回切替までの目標秒（内部でランダム設定）")]
        public float SwitchWeaponTime = 0;

        [Header("ターゲットまでの距離（毎フレーム更新）")]
        public float DistanceFromTarget;

        [Header("ターゲットとの相対角度（度）")]
        public float TargetAngle;

        [Header("受けたラグドール力の大きさ")]
        public int ReceivedRagdollForceAmount;

        [Header("ラグドール用のトランスフォーム参照")]
        public Transform RagdollTransform;

        [Header("ターゲットへ向かう現在の目的地")]
        public Vector3 TargetDestination;

        [Header("今回の交戦で初回かどうか")]
        public bool FirstTimeInCombat = true;

        [Header("撃破後、非戦闘へ戻るまでの遅延秒（乱数で決定）")]
        public float DeathDelay;

        [Header("撃破後の遅延が進行中か")]
        public bool DeathDelayActive;

        [Header("撃破後遅延の経過タイマー")]
        public float DeathDelayTimer;

        [Header("現在の攻撃アニメインデックス")]
        public int CurrentAnimationIndex = 0;

        [Header("ターゲット検出を有効化しているか")]
        public bool TargetDetectionActive;

        [Header("高さ差により攻撃不可（アウトオブレンジ）か")]
        public bool TargetOutOfHeightRange;

        [Header("近すぎとみなす距離（m）")]
        public float TooCloseDistance = 1;

        [Header("攻撃開始距離（m）")]
        public float AttackDistance = 2.5f;

        [Header("現在使用するアビリティオブジェクト")]
        public EmeraldAbilityObject CurrentEmeraldAIAbility;

        [Header("条件を満たし使用可能なアビリティ一覧（内部生成）")]
        public List<AttackClass.AttackData> AvailableConditionAbilities = new List<AttackClass.AttackData>();

        [Header("主要コンポーネント EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;

        [Header("現在の攻撃データ（選択済みアタック）")]
        public AttackClass.AttackData CurrentAttackData;

        [Header("最後にこのAIへダメージを与えた相手")]
        public Transform LastAttacker;
        #endregion

        #region Private Variables
        [Header("武器切替のクールダウン中フラグ（内部）")]
        bool m_WeaponTypeSwitchDelay;

        [Header("武器切替処理のコルーチン参照")]
        Coroutine SwitchWeaponCoroutine;
        #endregion

        #region Editor Variable
        [Header("インスペクタ：設定を隠す")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ：ダメージ設定の折りたたみ")]
        public bool DamageSettingsFoldout;

        [Header("インスペクタ：コンバットアクション設定の折りたたみ")]
        public bool CombatActionSettingsFoldout;

        [Header("インスペクタ：武器切替設定の折りたたみ")]
        public bool SwitchWeaponSettingsFoldout;

        [Header("インスペクタ：武器タイプ1設定の折りたたみ")]
        public bool WeaponType1SettingsFoldout;

        [Header("インスペクタ：武器タイプ2設定の折りたたみ")]
        public bool WeaponType2SettingsFoldout;
        #endregion

        void Start()
        {
            InitializeCombat(); // 戦闘コンポーネントの初期化
        }

        /// <summary>
        /// （日本語）Combat コンポーネントを初期化します。
        /// </summary>
        void InitializeCombat()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();                                   // 主要参照を取得
            EmeraldComponent.HealthComponent.OnDeath += CancelAllCombatActions;                 // 死亡時：全戦闘アクションをキャンセル
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += EnterCombat;           // 敵検知時：戦闘開始イベント
            EmeraldComponent.DetectionComponent.OnNullTarget += NullCombatTarget;               // ターゲット喪失時：処理
            OnKilledTarget += CancelAllCombatActions;                                           // 撃破時：全戦闘アクションをキャンセル
            TargetDetectionActive = true;                                                       // 検出機能ON
            FirstTimeInCombat = true;                                                           // 初回交戦フラグを立てる
            SwitchWeaponTime = Random.Range((float)SwitchWeaponTimeMin, SwitchWeaponTimeMax + 1); // 次回切替時刻を乱数で設定
            DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);                    // 撃破後の帰還遅延を乱数で設定
            Invoke(nameof(InitializeAttacks), 0.1f);                                            // 少し遅らせて攻撃初期化（他要素の初期化完了待ち）
        }

        void InitializeAttacks()
        {
            // 現在の武器タイプに基づき、攻撃を生成
            if (CurrentWeaponType == WeaponTypes.Type1)
            {
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type1Attacks);
                EmeraldComponent.DetectionComponent.PickTargetType = Type1PickTargetType;
                CurrentAttackCooldown = Type1AttackCooldown;

            }
            else if (CurrentWeaponType == WeaponTypes.Type2)
            {
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type2Attacks);
                EmeraldComponent.DetectionComponent.PickTargetType = Type2PickTargetType;
                CurrentAttackCooldown = Type2AttackCooldown;
            }
        }

        /// <summary>
        /// （日本語）EmeraldAISystem から呼ばれるカスタム Update。距離/角度の更新、ターゲット死亡監視、武器切替、撃破遅延を処理します。
        /// </summary>
        public void CombatUpdate()
        {
            if (CombatState)
            {
                DistanceFromTarget = EmeraldCombatManager.GetDistanceFromTarget(EmeraldComponent); // ターゲットまでの距離を更新
                TargetAngle = EmeraldCombatManager.TargetAngle(EmeraldComponent);                   // ターゲット相対角度を更新
            }
            else if (!CombatState)
            {
                DistanceFromTarget = EmeraldCombatManager.GetDistanceFromLookTarget(EmeraldComponent); // 注視ターゲットまでの距離
                TargetAngle = EmeraldCombatManager.TransformAngle(EmeraldComponent, EmeraldComponent.LookAtTarget); // 注視ターゲット相対角度
            }

            CheckForTargetDeath();   // 現在ターゲットの死亡を監視
            UpdateWeaponTypeState(); // 武器切替の判定
            UpdateDeathDelay();      // 撃破後の非戦闘復帰タイミング
        }

        /// <summary>
        /// （日本語）撃破後の遅延（DeathDelay）が経過したかを監視し、経過で ExitCombat を呼びます。
        /// </summary>
        void UpdateDeathDelay()
        {
            if (DeathDelayActive)
            {
                DeathDelayTimer += Time.deltaTime;

                if (DeathDelayTimer > DeathDelay)
                {
                    ExitCombat();
                }
            }
        }

        /// <summary>
        /// （日本語）現在のターゲットが攻撃できる角度範囲内にいるか返します。
        /// </summary>
        public bool TargetWithinAngleLimit()
        {
            return TargetAngle <= EmeraldComponent.MovementComponent.CombatAngleToTurn;
        }

        /// <summary>
        /// （日本語）今回の交戦で「初めて」敵を検知したタイミングで OnStartCombat を発火します。
        /// </summary>
        public void EnterCombat()
        {
            if (FirstTimeInCombat) OnStartCombat?.Invoke();
            FirstTimeInCombat = false;
        }

        /// <summary>
        /// （日本語）戦闘終了時に各種設定をリセットします。
        /// </summary>
        public void ExitCombat()
        {
            CombatState = false;
            SwitchWeaponTimer = 0;
            ClearTarget();
            FirstTimeInCombat = true;
            DeathDelayTimer = 0;
            DeathDelayActive = false;
            OnExitCombat?.Invoke(); // Movement/Detection/Animation へ非戦闘復帰を通知
        }

        /// <summary>
        /// （日本語）戦闘中のアクション一覧を更新します（現在の武器タイプに応じて切替）。
        /// </summary>
        public void UpdateActions()
        {
            if (CurrentWeaponType == WeaponTypes.Type1) CombatActions = Type1CombatActions;
            else if (CurrentWeaponType == WeaponTypes.Type2) CombatActions = Type2CombatActions;

            if (EmeraldComponent.CombatTarget != null && !EmeraldComponent.AnimationComponent.IsDead && !EmeraldComponent.AnimationComponent.IsStunned && !EmeraldComponent.AIAnimator.GetBool("Stunned Active"))
            {
                for (int i = 0; i < CombatActions.Count; i++)
                {
                    if (CombatActions[i].Enabled)
                    {
                        CombatActions[i].emeraldAction.UpdateAction(EmeraldComponent, CombatActions[i]); // 各アクションの Update
                        var Conditions = (((int)CombatActions[i].emeraldAction.CooldownConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;

                        // 条件成立時のみ、クールダウンタイマーを進める（攻撃トリガ/アクティブ中は除外）
                        if (Conditions && !EmeraldComponent.AIAnimator.GetBool("Attack") && !CombatActions[i].IsActive)
                            CombatActions[i].CooldownLengthTimer += Time.deltaTime;
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）現在アクティブな全戦闘アクションをキャンセルします。
        /// </summary>
        public void CancelAllCombatActions()
        {
            if (CombatActions.Count == 0)
                return;

            for (int i = 0; i < CombatActions.Count; i++)
            {
                if (CombatActions[i].IsActive)
                {
                    CombatActions[i].emeraldAction.CancelAction(EmeraldComponent, CombatActions[i]);
                }
            }
        }

        /// <summary>
        /// （日本語）アクションのクールダウンを調整し、アニメ遷移の都合で同時発火しないようにします。
        /// </summary>
        public void AdjustCooldowns()
        {
            for (int i = 0; i < CombatActions.Count; i++)
            {
                if (CombatActions[i].CooldownLengthTimer >= CombatActions[i].emeraldAction.CooldownLength - 0.25f)
                    CombatActions[i].CooldownLengthTimer = 0;
            }
        }

        /// <summary>
        /// （日本語）攻撃アニメ再生中に、アタックリストの Ability Object を発動します（AnimationEvent から呼び出し）。
        /// </summary>
        public void CreateAbility(AnimationEvent AttackEventParameters)
        {
            // AnimationEvent の objectReferenceParameter が設定されていれば、スロットの能力を上書き
            if (AttackEventParameters.objectReferenceParameter != null)
            {
                CurrentEmeraldAIAbility = (EmeraldAbilityObject)AttackEventParameters.objectReferenceParameter;
            }

            // 送られてきた AttackTransformName（stringParameter）を元に、攻撃/武器トランスフォームを更新
            EmeraldCombatManager.UpdateAttackTransforms(EmeraldComponent, AttackEventParameters.stringParameter);
            // Ability が設定されていれば発動
            if (CurrentEmeraldAIAbility != null) CurrentEmeraldAIAbility.InvokeAbility(gameObject, CurrentAttackTransform);
        }

        /// <summary>
        /// （日本語）アタックの溜め（Charge）効果を発動します（AnimationEvent から呼び出し）。
        /// </summary>
        public void ChargeEffect(AnimationEvent AttackEventParameters)
        {
            // AttackTransformName から該当トランスフォームを取得
            Transform AttackTransform = EmeraldCombatManager.GetAttackTransform(EmeraldComponent, AttackEventParameters.stringParameter);
            if (CurrentEmeraldAIAbility != null && AttackTransform != null) CurrentEmeraldAIAbility.ChargeAbility(gameObject, AttackTransform);
        }

        /// <summary>
        /// （日本語）OnNullTarget 時に呼ばれます。現在ターゲットをクリアし、新規ターゲットを探索。
        /// 見つからなければ撃破遅延（DeathDelay）を待って非戦闘へ戻ります。
        /// </summary>
        void NullCombatTarget()
        {
            if (EmeraldComponent.CombatTarget == null && CombatState && !EmeraldComponent.MovementComponent.ReturningToStartInProgress && !DeathDelayActive)
            {
                DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);
                DeathDelayActive = true;
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                ClearTarget();
            }
        }

        /// <summary>
        /// （日本語）現在の IDamageable を監視し、体力が 0 になったときの処理を行います。
        /// </summary>
        void CheckForTargetDeath()
        {
            if (EmeraldComponent.CurrentTargetInfo.CurrentIDamageable != null)
            {
                if (EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0 && !DeathDelayActive)
                {
                    OnKilledTarget?.Invoke();
                    DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);
                    DeathDelayActive = true;
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    Invoke(nameof(ClearTarget), 0.01f); // 以前は 0.75 秒
                }
            }
        }

        /// <summary>
        /// （日本語）現在のターゲット参照をクリアします。残敵がいなければ OnEndCombat を発火。
        /// </summary>
        public void ClearTarget()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                // LineOfSightTargets から CurrentTarget を除外
                if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Contains(EmeraldComponent.CombatTarget.GetComponent<Collider>()))
                    EmeraldComponent.DetectionComponent.LineOfSightTargets.Remove(EmeraldComponent.CombatTarget.GetComponent<Collider>());
            }
            else
            {
                // CurrentTarget が null の場合、null 要素を全て除去
                for (int i = 0; i < EmeraldComponent.DetectionComponent.LineOfSightTargets.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.LineOfSightTargets[i] == null)
                    {
                        EmeraldComponent.DetectionComponent.LineOfSightTargets.RemoveAt(i);
                    }
                }

            }

            // 現在ターゲット関連参照をクリア
            EmeraldComponent.CombatTarget = null;
            EmeraldComponent.CurrentTargetInfo.TargetSource = null;
            EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
            EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;

            // 現在の武器タイプに応じて新たなターゲット探索
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == WeaponTypes.Type1) EmeraldComponent.DetectionComponent.SearchForTarget(Type1PickTargetType);
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == WeaponTypes.Type2) EmeraldComponent.DetectionComponent.SearchForTarget(Type2PickTargetType);

            // 周辺に検出可能な敵がいなければ OnEndCombat を発火
            if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Count == 0) OnEndCombat?.Invoke();
        }

        /// <summary>
        /// （日本語）武器切替の連続実行を防ぐためのクールダウン解除を行います（Invoke 用）。
        /// </summary>
        void WeaponSwitchCooldown()
        {
            m_WeaponTypeSwitchDelay = false;
        }

        /// <summary>
        /// （日本語）武器タイプの更新ロジック（距離/時間/なし）を処理します。
        /// </summary>
        void UpdateWeaponTypeState()
        {
            if (!CombatState || DeathDelayActive)
                return;

            if (WeaponTypeAmount == WeaponTypeAmounts.Two)
            {
                if (SwitchWeaponTypeTriggered && !m_WeaponTypeSwitchDelay)
                {
                    SwitchWeaponTypeTriggered = false;
                    m_WeaponTypeSwitchDelay = true;
                    Invoke(nameof(WeaponSwitchCooldown), SwitchWeaponTypesCooldown); // 一定時間、再切替を抑止
                }

                // 距離基準での切替
                if (SwitchWeaponType == SwitchWeaponTypes.Distance && EmeraldComponent.CombatTarget != null && !m_WeaponTypeSwitchDelay &&
                    !EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsAttacking && !EmeraldComponent.AnimationComponent.IsMoving && !EmeraldComponent.AnimationComponent.IsBackingUp && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AIAnimator.GetBool("Strafe Active"))
                {
                    if (DistanceFromTarget > SwitchWeaponTypesDistance && CurrentWeaponType != StartingWeaponType && !SwitchWeaponTypeTriggered)
                    {
                        SwapWeaponType();
                        SwitchWeaponTypeTriggered = true;
                    }
                    if (DistanceFromTarget < SwitchWeaponTypesDistance && CurrentWeaponType == StartingWeaponType && !SwitchWeaponTypeTriggered)
                    {
                        SwapWeaponType();
                        SwitchWeaponTypeTriggered = true;
                    }
                }
                // 時間基準での切替
                else if (SwitchWeaponType == SwitchWeaponTypes.Timed)
                {
                    if (!EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsEquipping && !EmeraldComponent.AnimationComponent.IsMoving)
                        SwitchWeaponTimer += Time.deltaTime;

                    if (EmeraldComponent.CombatTarget != null && SwitchWeaponTimer >= SwitchWeaponTime &&
                        !EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsEquipping && !EmeraldComponent.AnimationComponent.IsGettingHit && !EmeraldComponent.AnimationComponent.IsAttacking && !EmeraldComponent.AnimationComponent.IsMoving && !EmeraldComponent.AnimationComponent.IsBackingUp && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AIAnimator.GetBool("Strafe Active"))
                    {
                        SwapWeaponType();
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）現在の武器タイプを入れ替えます。
        /// </summary>
        public void SwapWeaponType()
        {
            if (CurrentWeaponType == WeaponTypes.Type1)
            {
                if (SwitchWeaponCoroutine != null) StopCoroutine(SwitchWeaponCoroutine);
                SwitchWeaponCoroutine = StartCoroutine(ChangeWeaponType("Type2")); // タイプ2へ切替
            }
            else if (CurrentWeaponType == WeaponTypes.Type2)
            {
                if (SwitchWeaponCoroutine != null) StopCoroutine(SwitchWeaponCoroutine);
                SwitchWeaponCoroutine = StartCoroutine(ChangeWeaponType("Type1")); // タイプ1へ切替
            }
        }

        IEnumerator ChangeWeaponType(string WeaponTypeName)
        {
            EmeraldComponent.AnimationComponent.IsSwitchingWeapons = true; // 切替中フラグ
            EmeraldCombatManager.ResetWeaponSwapTime(EmeraldComponent);
            CurrentAnimationIndex = 1;
            EmeraldComponent.AIAnimator.SetInteger("Attack Index", 1);
            EmeraldComponent.AIAnimator.SetInteger("Hit Index", 1);
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            yield return new WaitForSeconds(0.1f);

            if (WeaponTypeName == "Type1")
            {
                yield return new WaitUntil(() => EmeraldComponent.AnimationComponent.IsIdling);
                EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 1);
                CurrentAttackCooldown = Type1AttackCooldown;
                EmeraldComponent.DetectionComponent.PickTargetType = Type1PickTargetType;
            }
            else if (WeaponTypeName == "Type2")
            {
                yield return new WaitUntil(() => EmeraldComponent.AnimationComponent.IsIdling);
                EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 2);
                CurrentAttackCooldown = Type2AttackCooldown;
                EmeraldComponent.DetectionComponent.PickTargetType = Type2PickTargetType;
            }

            CurrentWeaponType = (WeaponTypes)System.Enum.Parse(typeof(WeaponTypes), WeaponTypeName);

            if (EmeraldComponent.AIAnimator.GetBool("Animate Weapon State"))
            {
                while (!EmeraldComponent.AnimationComponent.IsEquipping)
                {
                    yield return null; // 装備アニメの開始を待機
                }
            }
            else
            {
                // 装備アニメがない場合は、ここで装備/収納を切替（バイパス）
                EmeraldItems m_EmeraldItems = GetComponent<EmeraldItems>();

                if (m_EmeraldItems != null)
                {
                    yield return new WaitForSeconds(0.5f);

                    if (WeaponTypeName == "Type1")
                    {
                        m_EmeraldItems.UnequipWeapon("Weapon Type 2");
                        m_EmeraldItems.EquipWeapon("Weapon Type 1");
                    }
                    else if (WeaponTypeName == "Type2")
                    {
                        m_EmeraldItems.UnequipWeapon("Weapon Type 1");
                        m_EmeraldItems.EquipWeapon("Weapon Type 2");
                    }
                }
            }

            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            UpdateWeaponTypeValues();                          // 攻撃生成を更新
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.AnimationComponent.IsSwitchingWeapons = false; // 切替完了
        }

        /// <summary>
        /// （日本語）現在の武器タイプに合わせて攻撃を再生成し、必要な設定を更新します。
        /// </summary>
        void UpdateWeaponTypeValues()
        {
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetInteger("Attack Index", 1);
            if (CurrentWeaponType == WeaponTypes.Type1)
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type1Attacks);
            else if (CurrentWeaponType == WeaponTypes.Type2)
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type2Attacks);
        }

        public void InvokeDoDamage()
        {
            OnDoDamage?.Invoke(); // 与ダメージイベントを通知
        }

        public void InvokeDoCritDamage()
        {
            OnDoCritDamage?.Invoke(); // クリティカル与ダメージイベントを通知
        }

        /// <summary>
        /// （日本語）ICombat：この AI 自身のトランスフォームを返します（ターゲット位置参照用）。
        /// </summary>
        public Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// （日本語）ICombat：このターゲットが攻撃中かを返します。
        /// </summary>
        public bool IsAttacking()
        {
            return EmeraldComponent.AnimationComponent.AttackTriggered;
        }

        /// <summary>
        /// （日本語）ICombat：このターゲットがブロック中かを返します。
        /// </summary>
        public bool IsBlocking()
        {
            return EmeraldComponent.AnimationComponent.IsBlocking && EmeraldComponent.AIAnimator.GetBool("Blocking");
        }

        /// <summary>
        /// （日本語）ICombat：このターゲットが回避中かを返します。
        /// </summary>
        public bool IsDodging()
        {
            return EmeraldComponent.AnimationComponent.IsDodging;
        }

        /// <summary>
        /// （日本語）ICombat：外部から被ダメを受ける際の参照位置を返します。
        /// </summary>
        public Vector3 DamagePosition()
        {
            if (EmeraldComponent.TPMComponent != null)
                return new Vector3(EmeraldComponent.TPMComponent.TransformSource.position.x, EmeraldComponent.TPMComponent.TransformSource.position.y + EmeraldComponent.TPMComponent.PositionModifier, EmeraldComponent.TPMComponent.TransformSource.position.z);
            else
                return transform.position + new Vector3(0, transform.localScale.y, 0);
        }

        /// <summary>
        /// （日本語）ICombat：アビリティによるスタンが生成された際に呼ばれます。
        /// </summary>
        public void TriggerStun(float StunLength)
        {
            EmeraldComponent.AnimationComponent.PlayStunnedAnimation(StunLength);
        }
    }
}
