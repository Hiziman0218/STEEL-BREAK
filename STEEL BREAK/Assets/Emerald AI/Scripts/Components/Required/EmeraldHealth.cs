using System.Collections;                         // コルーチン用
using System.Collections.Generic;                 // リスト等コレクション
using UnityEngine;                                // Unity API
using System;                                     // 汎用ユーティリティ
using EmeraldAI.Utility;                          // Emerald ユーティリティ（オブジェクトプール等）

namespace EmeraldAI
{
    /// <summary>
    /// （日本語）このコンポーネントは AI の被ダメージ処理と体力（Health）の管理を行います。
    /// Damage 関数は IDamageable インターフェイス経由で呼び出されます。
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/health-component")]
    // 【クラス概要】EmeraldHealth：
    //  ・被ダメージの受付（ブロック/回避の軽減）とヘルス更新
    //  ・被弾/クリティカル/任意ダメージ/ブロック/回避/死亡/回復ティック等のイベント発火
    //  ・戦闘終了時の自然回復（HealRate）処理
    public class EmeraldHealth : MonoBehaviour, IDamageable
    {
        #region Health variables    
        [Header("現在の体力（ランタイムで減少する値）")]
        public int CurrentHealth = 50;

        [Header("開始時の体力（最大体力の基準）")]
        public int StartingHealth = 50;

        [Header("毎秒の回復量（戦闘離脱後の自然回復に使用）")]
        public int HealRate = 0;

        [Header("被弾エフェクトをAIに追従させて貼り付けるか")]
        public bool AttachHitEffects = false;

        [Header("不死（ダメージで体力を減らさない）")]
        public bool Immortal = false;

        [Header("現在適用中のステータス効果（内部用）")]
        public List<string> CurrentActiveEffects;

        [Header("インスペクタ：Hit Effect 設定の折りたたみ")]
        public bool HitEffectFoldout;

        [Header("被弾時に追加のヒットエフェクトを使用するか")]
        public YesOrNo UseHitEffect = YesOrNo.No;

        [Header("ヒットエフェクトの生成位置オフセット")]
        public Vector3 HitEffectPosOffset;

        [Header("ヒットエフェクトの消滅までの秒数")]
        public float HitEffectTimeoutSeconds = 3f;

        [Header("被弾時に再生可能なエフェクトのリスト")]
        public List<GameObject> HitEffectsList = new List<GameObject>();

        // ↓ 以下はイベント定義。インスペクタに出ないため [Header] ではなくコメントで説明します。
        // 被ダメージ（通常）時
        public delegate void DamageHandler();
        public event DamageHandler OnTakeDamage;

        // 被ダメージ（クリティカル）時
        public delegate void TakeCritDamageHandler();
        public event TakeCritDamageHandler OnTakeCritDamage;

        // いかなるダメージでも
        public delegate void AnyDamageHandler();
        public event DamageHandler OnTakeAnyDamage;

        // ブロック時
        public delegate void BlockHandler();
        public event BlockHandler OnBlock;

        // 回避時
        public delegate void DodgeHandler();
        public event DodgeHandler OnDodge;

        // 死亡時
        public delegate void DeathHandler();
        public event DeathHandler OnDeath;

        // 回復ティック（1秒ごと等）発生時
        public delegate void HealRateTickHandler();
        public event HealRateTickHandler OnHealRateTick;

        // 外部からのヒールを受けた時
        public delegate void OnHealingReceivedHandler();
        public event OnHealingReceivedHandler OnHealingReceived;

        // 体力が変化した時（最大/現在の更新など）
        public delegate void HealthChangeHandler();
        public event HealthChangeHandler OnHealthChange;

        [Header("主要コンポーネント EmeraldSystem 参照（内部で取得）")]
        EmeraldSystem EmeraldComponent;

        // ↓ こちらは C# のプロパティ（シリアライズ対象外）。[Header] は付けず用途を明記します。
        // 現在のアクティブ効果リストの公開プロパティ
        public List<string> ActiveEffects { get => CurrentActiveEffects; set => CurrentActiveEffects = value; }

        // 現在体力の公開プロパティ（IDamageable 実装）
        public int Health { get => CurrentHealth; set => CurrentHealth = value; }

        // 初期体力（最大体力）の公開プロパティ（IDamageable 実装）
        public int StartHealth { get => StartingHealth; set => StartingHealth = value; }
        #endregion

        #region Editor Variables
        [Header("インスペクタ：設定を隠す")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ：Health 設定の折りたたみ")]
        public bool HealthFoldout;
        #endregion

        void Start()
        {
            CurrentHealth = StartingHealth;                                         // 開始時に現在体力を最大へ
            EmeraldComponent = GetComponent<EmeraldSystem>();                       // 主要参照を取得
            EmeraldComponent.CombatComponent.OnExitCombat += RecoverHealth;         // 戦闘離脱時に回復を開始
        }

        /// <summary>
        /// （日本語）ダメージを適用します。ブロック/回避が有効なら軽減します。
        /// ラグドールを使う場合は AttackerTransform と RagdollForce を指定してください。
        /// </summary>
        /// <param name="DamageAmount">与えられるダメージ量。</param>
        /// <param name="AttackerTransform">攻撃者の Transform。</param>
        /// <param name="RagdollForce">死亡時に適用するラグドール力（Use Ragdoll が有効な場合）。</param>
        /// <param name="CriticalHit">クリティカルヒットかどうか。</param>
        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            // 既に死亡/極小スケール/追従対象からの攻撃/友好関係からの攻撃は無視
            if (EmeraldComponent.AnimationComponent.IsDead || transform.localScale == Vector3.one * 0.003f || AttackerTransform && AttackerTransform == EmeraldComponent.TargetToFollow || AttackerTransform && EmeraldComponent.DetectionComponent.GetTargetFactionRelation(AttackerTransform) == "Friendly") return;

            // ターゲットがいない場合は、攻撃者を追跡するための処理
            if (AttackerTransform != null) CheckForAttacker(AttackerTransform);

            // 直近の攻撃者を記録
            EmeraldComponent.CombatComponent.LastAttacker = AttackerTransform;

            // 攻撃者との相対角度（ブロック/回避判定に使用）
            float AttackerAngle = EmeraldCombatManager.TransformAngle(EmeraldComponent, AttackerTransform);

            // ブロック判定（ブロック中 かつ 角度が MitigationAngle 以内 かつ Animator 側の Blocking=true）
            bool Blocked = (EmeraldComponent.AnimationComponent.IsBlocking && AttackerAngle <= EmeraldComponent.CombatComponent.MaxMitigationAngle && EmeraldComponent.AIAnimator.GetBool("Blocking"));

            // 回避判定（回避中 かつ 角度が MitigationAngle 以内）
            bool Dodged = (EmeraldComponent.AnimationComponent.IsDodging && AttackerAngle <= EmeraldComponent.CombatComponent.MaxMitigationAngle);

            // 基本は与ダメ値。ブロック/回避で軽減
            int CalculatedDamage = DamageAmount;

            if (Blocked) CalculatedDamage = Mathf.FloorToInt(Mathf.Abs((DamageAmount * ((EmeraldComponent.CombatComponent.MitigationAmount) * 0.01f)) - DamageAmount)); // ブロック軽減
            if (Dodged) CalculatedDamage = Mathf.FloorToInt(Mathf.Abs((DamageAmount * ((EmeraldComponent.CombatComponent.MitigationAmount) * 0.01f)) - DamageAmount)); // 回避軽減

            // 不死でなければ体力を減少
            if (!Immortal)
                Health -= CalculatedDamage;

            // Combat Text の表示（有効時）
            if (CalculatedDamage > 0) CombatTextSystem.Instance.CreateCombatTextAI(CalculatedDamage, EmeraldComponent.CombatComponent.DamagePosition(), CriticalHit, false);

            // 与ダメイベントを攻撃者側へ伝搬（攻撃者が Emerald AI なら）
            if (AttackerTransform != null)
            {
                EmeraldSystem AttackEmeraldComponent = AttackerTransform.GetComponent<EmeraldSystem>();
                if (AttackEmeraldComponent != null) AttackEmeraldComponent.CombatComponent.InvokeDoDamage();
                if (AttackEmeraldComponent != null && CriticalHit) AttackEmeraldComponent.CombatComponent.InvokeDoCritDamage();
            }

            // 被ダメ系イベントの発火
            if (!CriticalHit && CalculatedDamage > 0) OnTakeDamage?.Invoke();
            else if (CriticalHit && CalculatedDamage > 0) OnTakeCritDamage?.Invoke();
            OnTakeAnyDamage?.Invoke();

            // ブロック/回避ではなく、実ダメージが発生した場合のみヒットエフェクト
            if (!Blocked && !Dodged && CalculatedDamage > 0) CreateHitEffect();

            // ブロック/回避のイベント
            if (Blocked) OnBlock?.Invoke();
            else if (Dodged) OnDodge?.Invoke();

            // 体力が 0 以下になったら死亡処理へ
            if (Health <= 0 && !EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount = RagdollForce;
                Health = 0;
                Death();
            }
        }

        /// <summary>
        /// （日本語）攻撃を受けたが現在ターゲットがいない場合に呼ばれます。
        /// 攻撃者の関係が中立以上であればターゲットに設定し、そうでなければ視界内のターゲット探索を行います。
        /// </summary>
        void CheckForAttacker(Transform AttackerTransform)
        {
            if (EmeraldComponent.CombatTarget == null && !EmeraldComponent.CombatComponent.CombatState)
            {
                StartCoroutine(DelaySetDetectedTarget(AttackerTransform));
            }
        }

        /// <summary>
        /// （日本語）非戦闘ヒット再生→戦闘状態遷移の猶予を与えるため、少し待ってから SetDetectedTarget します。
        /// </summary>
        IEnumerator DelaySetDetectedTarget(Transform AttackerTransform)
        {
            string RelationName = EmeraldComponent.DetectionComponent.GetTargetFactionRelation(AttackerTransform);

            yield return new WaitForSeconds(0.6f); // 少し待機

            if (RelationName == "Neutral" || RelationName == "Enemy")
            {
                EmeraldComponent.DetectionComponent.SetDetectedTarget(AttackerTransform);
            }
            else
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
            }
        }

        /// <summary>
        /// （日本語）AI の体力が 0 になった際に呼ばれます。死亡イベントの発火およびラグドールや各種無効化を行います。
        /// </summary>
        void Death()
        {
            OnDeath?.Invoke();                                       // 外部へ死亡イベント通知
            EmeraldComponent.AnimationComponent.IsDead = true;       // アニメ側の死亡フラグ
            EmeraldCombatManager.DisableComponents(EmeraldComponent);// コンポーネント無効化
            EmeraldCombatManager.EnableRagdoll(EmeraldComponent);    // ラグドール有効化
        }

        /// <summary>
        /// （日本語）AI の体力を即座に全回復します。
        /// </summary>
        public void InstantlyRefillAIHealth()
        {
            Health = StartHealth;
            CurrentHealth = StartHealth;
            OnHealthChange?.Invoke();
        }

        /// <summary>
        /// （日本語）AI を即死させます。
        /// </summary>
        public void KillAI()
        {
            EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount = 1;
            Health = 0;
            Death();
        }

        /// <summary>
        /// （日本語）戦闘終了時（OnExitCombat）に呼ばれ、HealRate に基づき徐々に回復します。
        /// </summary>
        void RecoverHealth()
        {
            StartCoroutine(RecoverHealthInternal());
        }

        /// <summary>
        /// （日本語）回復の内部処理。毎秒 HealRate だけ体力を回復します（戦闘再開で中断）。
        /// </summary>
        IEnumerator RecoverHealthInternal()
        {
            float t = 0;

            while (CurrentHealth < StartingHealth)
            {
                t += Time.deltaTime;

                if (t >= 1)
                {
                    CurrentHealth = CurrentHealth + HealRate; // 1秒毎に回復
                    OnHealRateTick?.Invoke();                 // ティック通知
                    t = 0;
                }

                if (EmeraldComponent.CombatComponent.CombatState) yield break; // 戦闘再開で中断

                yield return null;
            }

            CurrentHealth = StartingHealth; // 上限に合わせる
        }

        // （日本語）外部から回復ティックを通知したい場合に呼ぶ
        public void UpdateHealTick()
        {
            OnHealRateTick?.Invoke();
        }

        // （日本語）外部からヒールを受け取ったことを通知したい場合に呼ぶ
        public void UpdateHealingReceived()
        {
            OnHealingReceived?.Invoke();
        }

        /// <summary>
        /// （日本語）AI の最大体力と現在体力を更新します。
        /// </summary>
        public void UpdateHealth(int MaxHealth, int CurrentHealth)
        {
            Health = CurrentHealth;
            StartHealth = MaxHealth;
            OnHealthChange?.Invoke();
        }

        /// <summary>
        /// （日本語）AI の被弾時、任意のヒットエフェクト（Ability 以外）を生成します。
        /// </summary>
        void CreateHitEffect()
        {
            if (EmeraldComponent.HealthComponent.UseHitEffect == YesOrNo.Yes && !EmeraldComponent.LBDComponent && EmeraldComponent.HealthComponent.HitEffectsList.Count > 0 && !EmeraldComponent.AnimationComponent.IsDead)
            {
                GameObject RandomBloodEffect = EmeraldComponent.HealthComponent.HitEffectsList[UnityEngine.Random.Range(0, EmeraldComponent.HealthComponent.HitEffectsList.Count)];
                if (RandomBloodEffect != null)
                {
                    GameObject SpawnedBlood = EmeraldObjectPool.SpawnEffect(RandomBloodEffect, Vector3.zero, EmeraldComponent.transform.rotation, EmeraldComponent.HealthComponent.HitEffectTimeoutSeconds) as GameObject;
                    if (AttachHitEffects) SpawnedBlood.transform.SetParent(EmeraldComponent.transform); // 追従させる場合は親子付け
                    SpawnedBlood.transform.position = EmeraldComponent.CombatComponent.DamagePosition() + EmeraldComponent.HealthComponent.HitEffectPosOffset; // ダメージ位置＋オフセット
                }
            }
        }
    }
}
