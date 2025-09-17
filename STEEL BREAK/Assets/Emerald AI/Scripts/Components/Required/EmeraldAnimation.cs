using System.Collections;                               // コルーチン関連
using System.Collections.Generic;                       // コレクション関連
using UnityEngine;                                      // Unity 基本API
using EmeraldAI.Utility;                                // Emerald ユーティリティ（AnimationProfile など）

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/animation-component")]
    // 【クラス概要】EmeraldAnimation：
    //  Emerald AI のアニメーション全体（移動/待機/攻撃/被弾/回避/ブロック/スタン/死亡/エモート 等）を制御する必須コンポーネント。
    //  Animator のパラメータ設定、状態監視、イベント発火（攻撃開始・終了、被弾、リコイル）を行う。
    public class EmeraldAnimation : MonoBehaviour
    {
        #region Animation States
        [Header("現在の Animator ステート情報（Layer 0）")]
        public AnimatorStateInfo CurrentStateInfo;

        [Header("内部ダッジ検知（遷移間での取りこぼし防止）")]
        public bool InternalDodge; // 遷移の合間にダッジが見逃される/被弾中に再生されるのを避けるための内部フラグ

        [Header("内部ブロック検知（遷移間での取りこぼし防止）")]
        public bool InternalBlock; // 遷移の合間にブロックが見逃される/攻撃中に再生されるのを避けるための内部フラグ

        [Header("内部被弾検知（遷移間での取りこぼし防止）")]
        public bool InternalHit;   // 遷移の合間にヒットが見逃される/ダッジ中に再生されるのを避けるための内部フラグ

        [Header("エモート（Emote）再生中か")]
        public bool IsEmoting;

        [Header("待機（Idle）状態か")]
        public bool IsIdling;

        [Header("攻撃（Attack）アニメ再生中か")]
        public bool IsAttacking;

        [Header("ストレイフ（Strafing：横移動）中か")]
        public bool IsStrafing;

        [Header("ブロック（Blocking）中か")]
        public bool IsBlocking;

        [Header("ダッジ（Dodging：回避）中か")]
        public bool IsDodging;

        [Header("リコイル（Recoiling：弾かれ）中か")]
        public bool IsRecoiling;

        [Header("スタン（Stunned）状態か")]
        public bool IsStunned;

        [Header("被弾（Hit）状態か")]
        public bool IsGettingHit;

        [Header("装備/収納（Equip）動作中か")]
        public bool IsEquipping;

        [Header("後退（Backing Up）中か")]
        public bool IsBackingUp;

        [Header("ターン（左右回転）中か")]
        public bool IsTurning;

        [Header("左/右のターン中フラグ")]
        public bool IsTurningLeft, IsTurningRight; // 同一行宣言：Header は先頭変数に適用（仕様）

        [Header("武器切替（Switching Weapons）中か")]
        public bool IsSwitchingWeapons;

        [Header("警告（Warning）アニメ中か")]
        public bool IsWarning;

        [Header("移動（Moving）状態か")]
        public bool IsMoving;

        [Header("死亡（Dead）状態か")]
        public bool IsDead;

        [Header("Idle インデックスの手動上書きを許可するか")]
        public bool m_IdleAnimaionIndexOverride = false;
        #endregion

        #region Animation Variables
        [Header("アニメーション・プロファイル（各種クリップ/設定の束）")]
        public EmeraldAI.Utility.AnimationProfile m_AnimationProfile;

        [Header("Animator Controller が生成済みか（内部状態）")]
        public bool AnimatorControllerGenerated = false;

        [Header("アニメーションリストに変更あり（内部状態）")]
        public bool AnimationListsChanged = false;

        [Header("Runtime Animator Controller 欠落（内部状態）")]
        public bool MissingRuntimeController = false;

        [Header("アニメーション設定が更新済み（内部状態）")]
        public bool AnimationsUpdated = false;

        [Header("この AI の Animator 参照")]
        public Animator AIAnimator;

        [Header("攻撃生成直後フラグ（攻撃開始検知用）")]
        public bool AttackingTracker; // 攻撃が生成された瞬間に true

        [Header("攻撃トリガ発動中フラグ（短時間）")]
        public bool AttackTriggered; // 攻撃再生中の短時間だけ true

        [Header("警告アニメが既に起動済みか")]
        public bool WarningAnimationTriggered = false;

        [Header("戦闘/非戦闘の中間遷移中（Busy）か")]
        public bool BusyBetweenStates = false;

        [Header("現在のアニメーション状態（列挙）")]
        public AnimationStateTypes CurrentAnimationState = AnimationStateTypes.Idling;

        // ↓ デリゲート/イベントはフィールドではないため Header は付けず、説明のみ
        public delegate void GetHitHandler();                // 被弾時コールバック
        public event GetHitHandler OnGetHit;                 // 被弾イベント

        public delegate void RecoilHandler();                // リコイル時コールバック
        public event RecoilHandler OnRecoil;                 // リコイルイベント

        public delegate void StartAttackAnimationHandler();  // 攻撃アニメ開始コールバック
        public event StartAttackAnimationHandler OnStartAttackAnimation; // 攻撃開始イベント

        public delegate void EndAttackAnimationHandler();    // 攻撃アニメ終了コールバック
        public event StartAttackAnimationHandler OnEndAttackAnimation;   // ※型は元コードのまま

        [Header("直近の被弾時刻（クールダウン判定用）")]
        float LastHitTime;

        [Header("スタン制御用のコルーチン参照")]
        Coroutine StunnedCoroutine;

        [Header("主要コンポーネント EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;
        #endregion

        #region Editor Variables
        [Header("Type1 攻撃アニメ列挙名（エディタ表示用）")]
        public string[] Type1AttackEnumAnimations;

        [Header("Type2 攻撃アニメ列挙名（エディタ表示用）")]
        public string[] Type2AttackEnumAnimations;

        [Header("Type1 攻撃アニメ未設定時の表示（エディタ用）")]
        public string[] Type1AttackBlankOptions = { "No Type 1 Attack Animations" };

        [Header("Type2 攻撃アニメ未設定時の表示（エディタ用）")]
        public string[] Type2AttackBlankOptions = { "No Type 2 Attack Animations" };

        [Header("インスペクタ：設定の折りたたみを隠すか")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ：Animation Profile 折りたたみ")]
        public bool AnimationProfileFoldout;
        #endregion

        void Start()
        {
            InitailizeAnimations(); // 初期化（イベント購読・Animator 設定など）
            SetupAnimator();        // Animator の諸設定適用
        }

        /// <summary>
        /// （日本語）アニメーション・コンポーネントを初期化します。
        /// </summary>
        void InitailizeAnimations()
        {
            AIAnimator = GetComponent<Animator>();
            AIAnimator.runtimeAnimatorController = m_AnimationProfile.AIAnimator; // プロファイルの RuntimeAnimatorController を適用
            EmeraldComponent = GetComponent<EmeraldSystem>();

            EmeraldComponent.HealthComponent.OnTakeDamage += PlayHitAnimation;      // 被弾時アニメ（通常）
            EmeraldComponent.HealthComponent.OnTakeCritDamage += PlayHitAnimation;  // 被弾時アニメ（クリティカル）
            EmeraldComponent.HealthComponent.OnBlock += PlayHitAnimation;           // ブロック被弾時アニメ
            EmeraldComponent.HealthComponent.OnDeath += PlayDeathAnimation;         // 死亡アニメ
            EmeraldComponent.MovementComponent.OnReachedWaypoint += PlayIdleAnimation; // ウェイポイント到達で Idle
            EmeraldComponent.CombatComponent.OnExitCombat += ReturnToDefaultState;  // 戦闘終了で非戦闘へ戻す
            AIAnimator.cullingMode = m_AnimationProfile.AnimatorCullingMode;        // カリング設定

            InitializeWeaponTypeAnimationAndSettings();                              // 武器タイプに応じた設定
            AIAnimator.updateMode = AnimatorUpdateMode.Normal;                      // Update モード
            AIAnimator.SetFloat("Offset", Random.Range(0.0f, 1.0f));                // 同期回避のためのランダムオフセット
        }

        public void AnimationUpdate()
        {
            EmeraldComponent.AnimationComponent.CurrentStateInfo = AIAnimator.GetCurrentAnimatorStateInfo(0); // 現在ステートの更新（状態判定に使用）
            CheckAnimationStates(); // 現在のアニメーション状態を反映

            // 攻撃アニメの開始/終了を検出し、必要なイベント/次攻撃生成を行う
            if (IsAttacking && !AttackingTracker)
            {
                OnStartAttackAnimation?.Invoke();
                AttackingTracker = true;
                AttackTriggered = true;
                Invoke(nameof(StopAttackTrigger), 0.5f);
            }
            else if (!IsAttacking && AttackingTracker)
            {
                OnEndAttackAnimation?.Invoke();
                EmeraldCombatManager.GenerateNextAttack(EmeraldComponent); // 現攻撃終了後に次攻撃を生成
            }

            // 優先度の高い状態へ遷移した場合は AttackTriggered を解除（Animator のトリガ取りこぼし対策）
            if (AttackTriggered)
            {
                if (IsMoving || IsTurning || IsStunned || IsStrafing || IsBackingUp || IsBlocking || IsDodging || InternalHit || !AttackingTracker) AttackTriggered = false;
            }
        }

        void StopAttackTrigger()
        {
            AttackTriggered = false; // 攻撃トリガ短期フラグを解除
        }

        /// <summary>
        /// （日本語）現在のアニメーション状態を監視・反映します。
        /// </summary>
        public void CheckAnimationStates()
        {
            // 非戦闘時の移動/待機
            if (!EmeraldComponent.CombatComponent.CombatState) IsIdling = CurrentStateInfo.IsName("Movement") && AIAnimator.GetFloat("Speed") < 0.1f && !IsBackingUp || CurrentStateInfo.IsTag("Idle");
            if (!EmeraldComponent.CombatComponent.CombatState) IsMoving = CurrentStateInfo.IsName("Movement") && AIAnimator.GetFloat("Speed") >= 0.1f && !IsBackingUp;

            // 戦闘時（武器タイプ別）の移動/待機
            if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                IsIdling = CurrentStateInfo.IsName("Combat Movement (Type 1)") && AIAnimator.GetFloat("Speed") < 0.1f;
                IsMoving = CurrentStateInfo.IsName("Combat Movement (Type 1)") && AIAnimator.GetFloat("Speed") >= 0.1f && !IsBackingUp && !IsAttacking;
            }
            else if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                IsIdling = CurrentStateInfo.IsName("Combat Movement (Type 2)") && AIAnimator.GetFloat("Speed") < 0.1f;
                IsMoving = CurrentStateInfo.IsName("Combat Movement (Type 2)") && AIAnimator.GetFloat("Speed") >= 0.1f && !IsBackingUp && !IsAttacking;
            }

            // 各タグで状態フラグ更新
            IsEquipping = CurrentStateInfo.IsTag("Equip");
            IsBlocking = CurrentStateInfo.IsTag("Block");
            IsRecoiling = CurrentStateInfo.IsTag("Recoil");
            IsStunned = CurrentStateInfo.IsTag("Stunned");
            IsStrafing = CurrentStateInfo.IsTag("Strafing");
            IsDodging = CurrentStateInfo.IsTag("Dodging") || InternalDodge;
            IsBackingUp = CurrentStateInfo.IsTag("Backing Up") || AIAnimator.GetBool("Walk Backwards");
            IsAttacking = CurrentStateInfo.IsTag("Attack");
            IsGettingHit = CurrentStateInfo.IsTag("Hit");
            IsWarning = CurrentStateInfo.IsTag("Warning");
            IsEmoting = CurrentStateInfo.IsTag("Emote");

            // 戦闘/非戦闘の遷移中か（この間の望ましくない回転を抑止）
            BusyBetweenStates = AIAnimator.GetAnimatorTransitionInfo(0).IsName("Combat Movement (Type 1) -> Movement") || AIAnimator.GetAnimatorTransitionInfo(0).IsName("Combat Movement (Type 2) -> Movement") ||
                                AIAnimator.GetAnimatorTransitionInfo(0).IsName("Movement -> Combat Movement (Type 1)") || AIAnimator.GetAnimatorTransitionInfo(0).IsName("Movement -> Combat Movement (Type 2)") ||
                                AIAnimator.GetAnimatorTransitionInfo(0).IsName("Combat Movement (Type 1) -> Put Away Weapon (Type 1)") || AIAnimator.GetAnimatorTransitionInfo(0).IsName("Combat Movement (Type 2) -> Put Away Weapon (Type 2)") ||
                                AIAnimator.GetAnimatorTransitionInfo(0).IsName("Put Away Weapon (Type 1) -> Movement") || AIAnimator.GetAnimatorTransitionInfo(0).IsName("Put Away Weapon (Type 2) -> Movement");

            // 列挙状態へ同期
            if (IsIdling) CurrentAnimationState = AnimationStateTypes.Idling;
            if (IsMoving) CurrentAnimationState = AnimationStateTypes.Moving;
            if (IsTurningLeft) CurrentAnimationState = AnimationStateTypes.TurningLeft;
            if (IsTurningRight) CurrentAnimationState = AnimationStateTypes.TurningRight;
            if (IsEquipping) CurrentAnimationState = AnimationStateTypes.Equipping;
            if (IsBlocking) CurrentAnimationState = AnimationStateTypes.Blocking;
            if (IsRecoiling) CurrentAnimationState = AnimationStateTypes.Recoiling;
            if (IsStunned) CurrentAnimationState = AnimationStateTypes.Stunned;
            if (IsStrafing) CurrentAnimationState = AnimationStateTypes.Strafing;
            if (IsDodging) CurrentAnimationState = AnimationStateTypes.Dodging;
            if (IsBackingUp) CurrentAnimationState = AnimationStateTypes.BackingUp;
            if (IsAttacking) CurrentAnimationState = AnimationStateTypes.Attacking;
            if (IsGettingHit) CurrentAnimationState = AnimationStateTypes.GettingHit;
            if (IsDead) CurrentAnimationState = AnimationStateTypes.Dead;
            if (IsEmoting) CurrentAnimationState = AnimationStateTypes.Emoting;
            if (IsSwitchingWeapons) CurrentAnimationState = AnimationStateTypes.SwitchingWeapons;
        }

        /// <summary>
        /// （日本語）Animator の既定値へ戻し（Start 時の設定を再適用）、内部フラグをリセットします。
        /// </summary>
        public void ResetSettings()
        {
            // Animator を無効化すると既定値へ戻るため、Start で適用した設定を再適用
            SetWeaponTypeAnimationState();
            AIAnimator.SetBool("Idle Active", false);
            InitializeWeaponTypeAnimationAndSettings();
            InternalDodge = false;
            InternalBlock = false;
            InternalHit = false;
        }

        /// <summary>
        /// （日本語）Animator 関連の各種設定を行います。
        /// </summary>
        public void SetupAnimator()
        {
            AIAnimator = GetComponent<Animator>();
            EmeraldComponent = GetComponent<EmeraldSystem>(); ;

            if (AIAnimator.layerCount >= 2)
                AIAnimator.SetLayerWeight(1, 1); // サブレイヤーを有効化

            if (GetComponent<EmeraldMovement>().MovementType == EmeraldMovement.MovementTypes.RootMotion)
            {
                EmeraldComponent.m_NavMeshAgent.speed = 0; // RootMotion 使用時は NavMeshAgent は移動させない
                AIAnimator.applyRootMotion = true;
            }
            else
            {
                AIAnimator.applyRootMotion = false;
            }

            if (AIAnimator.layerCount >= 2)
            {
                AIAnimator.SetLayerWeight(1, 1);
            }

            SetWeaponTypeAnimationState(); // 武器タイプに応じた状態設定

            AIAnimator.SetInteger("Idle Index", Random.Range(0, m_AnimationProfile.NonCombatAnimations.IdleList.Count)); // Idle 開始インデックス
        }

        /// <summary>
        /// （日本語）武器タイプ（1/2）に応じた Animator/検知設定を初期化します。
        /// </summary>
        public void InitializeWeaponTypeAnimationAndSettings()
        {
            if (EmeraldComponent.CombatComponent.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                if (EmeraldComponent.CombatComponent.StartingWeaponType == EmeraldCombat.WeaponTypes.Type1)
                {
                    AIAnimator.SetInteger("Weapon Type State", 1);
                    EmeraldComponent.CombatComponent.CurrentWeaponType = EmeraldCombat.WeaponTypes.Type1;
                    EmeraldComponent.DetectionComponent.PickTargetType = EmeraldComponent.CombatComponent.Type1PickTargetType;
                }
                else if (EmeraldComponent.CombatComponent.StartingWeaponType == EmeraldCombat.WeaponTypes.Type2)
                {
                    AIAnimator.SetInteger("Weapon Type State", 2);
                    EmeraldComponent.CombatComponent.CurrentWeaponType = EmeraldCombat.WeaponTypes.Type2;
                    EmeraldComponent.DetectionComponent.PickTargetType = EmeraldComponent.CombatComponent.Type2PickTargetType;
                }
            }
            else
            {
                AIAnimator.SetInteger("Weapon Type State", 1);
                EmeraldComponent.CombatComponent.CurrentWeaponType = EmeraldCombat.WeaponTypes.Type1;
                EmeraldComponent.DetectionComponent.PickTargetType = EmeraldComponent.CombatComponent.Type1PickTargetType;
            }
        }

        /// <summary>
        /// （日本語）装備/収納アニメの有無に応じて「武器状態の自動アニメ」を有効/無効にします。
        /// </summary>
        void SetWeaponTypeAnimationState()
        {
            if (EmeraldComponent.CombatComponent.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.One)
            {
                if (m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip == null || m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip == null)
                    AIAnimator.SetBool("Animate Weapon State", false);
                else if (m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip != null && m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip != null)
                    AIAnimator.SetBool("Animate Weapon State", true);
            }
            else if (EmeraldComponent.CombatComponent.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                if (m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip == null && m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip == null &&
                    m_AnimationProfile.Type2Animations.PutAwayWeapon.AnimationClip == null && m_AnimationProfile.Type2Animations.PullOutWeapon.AnimationClip == null)
                    AIAnimator.SetBool("Animate Weapon State", false);
                else if (m_AnimationProfile.Type1Animations.PutAwayWeapon.AnimationClip != null && m_AnimationProfile.Type1Animations.PullOutWeapon.AnimationClip != null &&
                    m_AnimationProfile.Type2Animations.PutAwayWeapon.AnimationClip != null && m_AnimationProfile.Type2Animations.PullOutWeapon.AnimationClip != null)
                    AIAnimator.SetBool("Animate Weapon State", true);
            }
        }

        /// <summary>
        /// （日本語）ランダムな Idle インデックスを生成して Idle アニメを再生します。
        /// </summary>
        public void PlayIdleAnimation()
        {
            if (!EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride && m_AnimationProfile.NonCombatAnimations.IdleList.Count > 0 &&
                EmeraldComponent.MovementComponent.WaypointType != EmeraldMovement.WaypointTypes.Loop)
            {
                AIAnimator.SetInteger("Idle Index", Random.Range(1, m_AnimationProfile.NonCombatAnimations.IdleList.Count + 1));
                AIAnimator.SetBool("Idle Active", true);
            }
        }

        /// <summary>
        /// （日本語）現在の武器タイプ用の警告（Warning）アニメを再生します。
        /// </summary>
        public void PlayWarningAnimation()
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && m_AnimationProfile.Type1Animations.IdleWarning.AnimationClip == null ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && m_AnimationProfile.Type2Animations.IdleWarning.AnimationClip == null ||
                WarningAnimationTriggered)
                return;

            AIAnimator.SetTrigger("Warning");
            WarningAnimationTriggered = true;
        }

        /// <summary>
        /// （日本語）スタン時間（秒数）に応じてスタンアニメを再生します。
        /// </summary>
        public void PlayStunnedAnimation(float StunnedLength)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && m_AnimationProfile.Type1Animations.Stunned.AnimationClip == null ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && m_AnimationProfile.Type2Animations.Stunned.AnimationClip == null) return;

            if (!IsStunned && !AIAnimator.GetBool("Blocking") && !AIAnimator.GetBool("Dodge Triggered") && !IsDodging && transform.localScale != Vector3.one * 0.003f)
            {
                if (StunnedCoroutine != null) StopCoroutine(StunnedCoroutine);
                StunnedCoroutine = StartCoroutine(SetStunned(StunnedLength));
            }
        }

        IEnumerator SetStunned(float StunnedLength)
        {
            yield return new WaitForSeconds(0.5f);
            if (IsDodging || IsDead || IsBlocking || IsStunned)
            {
                AIAnimator.SetBool("Stunned Active", false);
                yield break; // ダッジ中/死亡/ブロック中/既にスタンの場合は起動しない
            }
            AIAnimator.SetBool("Stunned Active", true);
            yield return new WaitForSeconds(StunnedLength);
            AIAnimator.SetBool("Stunned Active", false);
            EmeraldComponent.BehaviorsComponent.IsAiming = false;
        }

        /// <summary>
        /// （日本語）DeathList からランダムに死亡アニメを再生します（空の場合はラグドール前提）。
        /// </summary>
        public void PlayDeathAnimation()
        {
            // 現在の武器タイプの DeathList が空なら再生しない（ラグドール死亡想定）
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && m_AnimationProfile.Type1Animations.DeathList.Count == 0 ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && m_AnimationProfile.Type2Animations.DeathList.Count == 0)
                return;

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                AIAnimator.SetInteger("Death Index", Random.Range(1, m_AnimationProfile.Type1Animations.DeathList.Count + 1));
                int DeathIndex = AIAnimator.GetInteger("Death Index");
                StartCoroutine(DisableAnimator(m_AnimationProfile.Type1Animations.DeathList[DeathIndex - 1].AnimationClip.length / m_AnimationProfile.Type1Animations.DeathList[DeathIndex - 1].AnimationSpeed));
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                AIAnimator.SetInteger("Death Index", Random.Range(1, m_AnimationProfile.Type2Animations.DeathList.Count + 1));
                int DeathIndex = AIAnimator.GetInteger("Death Index");
                StartCoroutine(DisableAnimator(m_AnimationProfile.Type2Animations.DeathList[DeathIndex - 1].AnimationClip.length / m_AnimationProfile.Type2Animations.DeathList[DeathIndex - 1].AnimationSpeed));
            }

            AIAnimator.SetTrigger("Dead");
        }

        /// <summary>
        /// （日本語）設定した条件に応じて被弾（Hit）アニメを再生します。
        /// </summary>
        public void PlayHitAnimation()
        {
            // 武器タイプに応じた被弾アニメのクールダウン値を取得
            float CurrentHitAnimationCooldown = EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 ? m_AnimationProfile.Type1HitAnimationCooldown : m_AnimationProfile.Type2HitAnimationCooldown;

            // 死亡/クールダウン中なら再生しない
            if (EmeraldComponent.HealthComponent.CurrentHealth <= 0 || Time.time < (LastHitTime + CurrentHitAnimationCooldown))
                return;

            LastHitTime = Time.time; // タイムスタンプ更新

            if (!EmeraldComponent.CombatComponent.CombatState)
            {
                if (m_AnimationProfile.NonCombatAnimations.HitList.Count == 0 && !IsBlocking)
                    return;

                int CurrentIndex = AIAnimator.GetInteger("Hit Index");
                AIAnimator.SetInteger("Hit Index", CurrentIndex);
                CurrentIndex++;
                if (CurrentIndex == m_AnimationProfile.NonCombatAnimations.HitList.Count + 1) CurrentIndex = 1;
                AIAnimator.SetInteger("Hit Index", CurrentIndex);
            }
            else if (EmeraldComponent.CombatComponent.CombatState && !IsBlocking)
            {
                if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
                {
                    if (m_AnimationProfile.Type1Animations.HitList.Count == 0 && !IsBlocking)
                        return;

                    int CurrentIndex = AIAnimator.GetInteger("Hit Index");
                    AIAnimator.SetInteger("Hit Index", CurrentIndex);
                    CurrentIndex++;
                    if (CurrentIndex >= m_AnimationProfile.Type1Animations.HitList.Count + 1) CurrentIndex = 1;
                    AIAnimator.SetInteger("Hit Index", CurrentIndex);
                }
                else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                {
                    if (m_AnimationProfile.Type2Animations.HitList.Count == 0 && !IsBlocking)
                        return;

                    int CurrentIndex = AIAnimator.GetInteger("Hit Index");
                    AIAnimator.SetInteger("Hit Index", CurrentIndex);
                    CurrentIndex++;
                    if (CurrentIndex >= m_AnimationProfile.Type2Animations.HitList.Count + 1) CurrentIndex = 1;
                    AIAnimator.SetInteger("Hit Index", CurrentIndex);
                }
            }

            // ブロックが間に合わなかった場合にキャンセル支援
            if (!IsBlocking && AIAnimator.GetBool("Blocking"))
            {
                AIAnimator.SetBool("Blocking", false);
            }

            // 条件が整った時のみ被弾アニメ再生（いくつかの状態は除外）
            if (!IsDodging && !IsSwitchingWeapons && !IsEquipping && !AIAnimator.GetBool("Dodge Triggered") && EmeraldComponent.HealthComponent.CurrentActiveEffects.Count == 0)
            {
                var Type1Conditions = (((int)m_AnimationProfile.Type1HitConditions) & ((int)CurrentAnimationState)) != 0;
                var Type2Conditions = (((int)m_AnimationProfile.Type2HitConditions) & ((int)CurrentAnimationState)) != 0;

                if (Type1Conditions && EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 || Type2Conditions && EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                {
                    // フレーム間/トリガ競合回避のため内部フラグでヒットを記録（0.5秒はダッジを無視）
                    InternalHit = true;
                    Invoke(nameof(ResetInternalHit), 0.5f);
                    AttackTriggered = false;

                    AIAnimator.SetTrigger("Hit");
                    OnGetHit?.Invoke();
                }
            }

            AIAnimator.ResetTrigger("Attack");
        }

        void ResetInternalHit()
        {
            InternalHit = false;
            AttackTriggered = false;
        }

        /// <summary>
        /// （日本語）現在の攻撃データ（CurrentAnimationIndex 等）に基づいて攻撃アニメを再生します。
        /// </summary>
        public void PlayAttackAnimation()
        {
            if (!EmeraldComponent.CombatComponent.CurrentAttackData.CooldownIgnored) EmeraldComponent.CombatComponent.CurrentAttackData.CooldownTimeStamp = Time.time;
            if (!EmeraldComponent.CombatComponent.CurrentAttackData.AbilityObject.CooldownSettings.Enabled) EmeraldComponent.CombatComponent.CurrentAttackData.CooldownTimeStamp = 0; // 無効なクールダウンは 0 扱い

            AIAnimator.SetInteger("Attack Index", EmeraldComponent.CombatComponent.CurrentAnimationIndex + 1);
            AIAnimator.SetTrigger("Attack");
            AttackTriggered = true;
        }

        /// <summary>
        /// （日本語）停止時のターンアニメ（左/右）を条件に応じて計算・再生します。
        /// </summary>
        public void CalculateTurnAnimations(bool ByPassConditions = false)
        {
            // しきい値到達時に 1 秒ロック（左右切替で固まるのを防止）
            if (!EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.DestinationAdjustedAngle <= EmeraldComponent.MovementComponent.AngleToTurn && !EmeraldComponent.MovementComponent.LockTurning)
            {
                EmeraldComponent.MovementComponent.LockTurning = true;
                StartCoroutine(LockTurns());
                DisableTurning();
            }

            Vector3 DestinationDirection = EmeraldComponent.MovementComponent.DestinationDirection;

            if (ByPassConditions || CanPlayTurningAnimation(DestinationDirection))
            {
                if (Time.timeSinceLevelLoad < 1f || EmeraldComponent.MovementComponent.LockTurning && !EmeraldComponent.CombatComponent.CombatState || IsBackingUp)
                    return;

                Vector3 cross = Vector3.Cross(transform.forward, Quaternion.LookRotation(DestinationDirection, Vector3.up) * Vector3.forward);

                if (cross.y > 0.0f) // 右回転
                {
                    EmeraldComponent.AnimationComponent.IsTurning = true;
                    EmeraldComponent.AnimationComponent.IsTurningRight = true;
                    EmeraldComponent.AnimationComponent.IsTurningLeft = false;
                    AIAnimator.SetBool("Idle Active", false);
                    AIAnimator.SetBool("Turn Right", true);
                    AIAnimator.SetBool("Turn Left", false);
                }
                else if (cross.y < 0.0f) // 左回転
                {
                    EmeraldComponent.AnimationComponent.IsTurning = true;
                    EmeraldComponent.AnimationComponent.IsTurningLeft = true;
                    EmeraldComponent.AnimationComponent.IsTurningRight = false;
                    AIAnimator.SetBool("Idle Active", false);
                    AIAnimator.SetBool("Turn Left", true);
                    AIAnimator.SetBool("Turn Right", false);
                }

            }
            else if (EmeraldComponent.CombatComponent.CombatState)
            {
                EmeraldComponent.AnimationComponent.IsTurning = false;
                EmeraldComponent.AnimationComponent.IsTurningLeft = false;
                EmeraldComponent.AnimationComponent.IsTurningRight = false;
                AIAnimator.SetBool("Turn Left", false);
                AIAnimator.SetBool("Turn Right", false);
            }
        }

        /// <summary>
        /// （日本語）ターンアニメ再生の可否を返します。
        /// </summary>
        bool CanPlayTurningAnimation(Vector3 DestinationDirection)
        {
            if (!EmeraldComponent.CombatComponent.CombatState)
            {
                return EmeraldComponent.MovementComponent.DestinationAdjustedAngle >= EmeraldComponent.MovementComponent.AngleToTurn && DestinationDirection != Vector3.zero &&
                       EmeraldComponent.MovementComponent.AIAgentActive && EmeraldComponent.m_NavMeshAgent.remainingDistance > EmeraldComponent.m_NavMeshAgent.stoppingDistance;
            }
            else
            {
                return !EmeraldComponent.CombatComponent.DeathDelayActive && EmeraldComponent.MovementComponent.DestinationAdjustedAngle >= EmeraldComponent.MovementComponent.AngleToTurn && DestinationDirection != Vector3.zero &&
                       EmeraldComponent.MovementComponent.AIAgentActive && !IsAttacking && !IsBlocking && !IsGettingHit && !IsRecoiling && !IsStrafing && !IsDodging && !IsStunned && !IsSwitchingWeapons && !IsEquipping;
            }
        }

        /// <summary>
        /// （日本語）角度しきい値到達後 1 秒だけターンをロック（左右切替によるスタック防止）。
        /// </summary>
        IEnumerator LockTurns()
        {
            yield return new WaitForSeconds(1f);
            EmeraldComponent.MovementComponent.LockTurning = false;
        }

        void DisableTurning()
        {
            IsTurning = false;
            IsTurningLeft = false;
            IsTurningRight = false;
            AIAnimator.SetBool("Turn Right", false);
            AIAnimator.SetBool("Turn Left", false);
        }

        /// <summary>
        /// （日本語）攻撃中に相手がブロックした場合、リコイル（Recoil）アニメを再生します。
        /// </summary>
        public void PlayRecoilAnimation()
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && m_AnimationProfile.Type1Animations.Recoil.AnimationClip == null ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && m_AnimationProfile.Type2Animations.Recoil.AnimationClip == null) return;

            if (EmeraldComponent.CombatTarget != null && EmeraldComponent.CurrentTargetInfo != null && EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsBlocking())
            {
                AIAnimator.ResetTrigger("Attack");
                AIAnimator.SetTrigger("Recoil");
                OnRecoil?.Invoke();
            }
        }

        /// <summary>
        /// （日本語）ストレイフ状態の切替（必要に応じて方向をランダム/維持）。
        /// </summary>
        public void SetStrafeState(bool State)
        {
            int Direction = AIAnimator.GetInteger("Strafe Direction");
            if (State) Direction = Random.Range(0, 2); // 有効化時のみ方向を更新

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                if (Direction == 0 && m_AnimationProfile.Type1Animations.StrafeLeft.AnimationClip == null) return;
                if (Direction == 1 && m_AnimationProfile.Type1Animations.StrafeRight.AnimationClip == null) return;
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1) // 元コード仕様そのまま
            {
                if (Direction == 0 && m_AnimationProfile.Type2Animations.StrafeLeft.AnimationClip == null) return;
                if (Direction == 1 && m_AnimationProfile.Type2Animations.StrafeRight.AnimationClip == null) return;
            }

            AIAnimator.SetBool("Strafe Active", State);
            if (State) AIAnimator.SetInteger("Strafe Direction", Direction);
            if (State) AIAnimator.SetTrigger("Strafing Triggered");
        }

        /// <summary>
        /// （日本語）ダッジ（回避）をトリガします。ストレイフ/後退時は方向を補正。
        /// </summary>
        public void TriggerDodgeState()
        {
            int Direction = Random.Range(0, 3);

            // ストレイフ中は回避方向をストレイフ方向に合わせる
            if (IsStrafing || AIAnimator.GetBool("Strafe Active"))
            {
                int StrafeDirection = AIAnimator.GetInteger("Strafe Direction");
                if (StrafeDirection == 0) Direction = 0;
                if (StrafeDirection == 1) Direction = 2;
            }
            /*
            // 後退中なら後方回避に固定（必要なら有効化）
            else if (IsBackingUp)
            {
                Direction = 1;
            }
            */

            // 選ばれたダッジアニメが未設定なら中断
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                if (Direction == 0 && m_AnimationProfile.Type1Animations.DodgeLeft.AnimationClip == null) return;
                if (Direction == 1 && m_AnimationProfile.Type1Animations.DodgeBack.AnimationClip == null) return;
                if (Direction == 2 && m_AnimationProfile.Type1Animations.DodgeRight.AnimationClip == null) return;
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                if (Direction == 0 && m_AnimationProfile.Type2Animations.DodgeLeft.AnimationClip == null) return;
                if (Direction == 1 && m_AnimationProfile.Type2Animations.DodgeBack.AnimationClip == null) return;
                if (Direction == 2 && m_AnimationProfile.Type2Animations.DodgeRight.AnimationClip == null) return;
            }

            AIAnimator.SetInteger("Dodge Direction", Direction);
            AIAnimator.SetTrigger("Dodge Triggered");
            AIAnimator.SetBool("Walk Backwards", false);
        }

        /// <summary>
        /// （日本語）ブロック状態の切替。
        /// </summary>
        public void PlayBlockAnimation(bool State)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && m_AnimationProfile.Type1Animations.BlockIdle.AnimationClip == null ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && m_AnimationProfile.Type2Animations.BlockIdle.AnimationClip == null) return;
            AIAnimator.SetBool("Blocking", State);
        }

        /// <summary>
        /// （日本語）複数トリガの競合を避けるため、指定秒後に各トリガをリセットします。
        /// </summary>
        public void ResetTriggers(float Delay)
        {
            StartCoroutine(ResetTriggersInternal(Delay));
        }

        IEnumerator ResetTriggersInternal(float Delay)
        {
            yield return new WaitForSeconds(Delay);
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetBool("Blocking", false);
            EmeraldComponent.AIAnimator.ResetTrigger("Dodge Triggered");
            EmeraldComponent.AIAnimator.ResetTrigger("Strafing Triggered");
            EmeraldComponent.AIAnimator.SetBool("Strafe Active", false);
            EmeraldComponent.AIAnimator.ResetTrigger("Attack Cancelled");
        }

        /// <summary>
        /// （日本語）戦闘終了コールバック：Animator を非戦闘状態へ戻します。
        /// </summary>
        void ReturnToDefaultState()
        {
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", false);
            EmeraldComponent.AnimationComponent.WarningAnimationTriggered = false;
        }

        /// <summary>
        /// （日本語）エモートアニメを ID 指定で 1 回再生します。
        /// </summary>
        public void PlayEmoteAnimation(int EmoteAnimationID)
        {
            // EmoteAnimationList を走査して該当 ID を再生
            for (int i = 0; i < m_AnimationProfile.EmoteAnimationList.Count; i++)
            {
                if (m_AnimationProfile.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                    AIAnimator.SetTrigger("Emote Trigger");
                    IsMoving = false;
                }
            }
        }

        /// <summary>
        /// （日本語）エモートアニメをループ再生します（停止が呼ばれるまで継続）。戦闘中に使用する場合はキャンセル制御が必要です。
        /// </summary>
        public void LoopEmoteAnimation(int EmoteAnimationID)
        {
            for (int i = 0; i < m_AnimationProfile.EmoteAnimationList.Count; i++)
            {
                if (m_AnimationProfile.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                    AIAnimator.SetBool("Emote Loop", true);
                    IsMoving = false;
                }
            }
        }

        /// <summary>
        /// （日本語）ループ中のエモートアニメを停止します。
        /// </summary>
        public void StopLoopEmoteAnimation(int EmoteAnimationID)
        {
            for (int i = 0; i < m_AnimationProfile.EmoteAnimationList.Count; i++)
            {
                if (m_AnimationProfile.EmoteAnimationList[i].AnimationID == EmoteAnimationID)
                {
                    AIAnimator.SetInteger("Emote Index", EmoteAnimationID);
                    AIAnimator.SetBool("Emote Loop", false);
                    IsMoving = false;
                }
            }
        }

        /// <summary>
        /// （日本語）死亡アニメの再生完了まで待ってから Animator を無効化します。
        /// </summary>
        IEnumerator DisableAnimator(float AnimationLength)
        {
            yield return new WaitForSeconds(AnimationLength);
            EmeraldComponent.AIAnimator.enabled = false;
        }
    }
}
