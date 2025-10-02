using UnityEngine;
using EmeraldAI.Utility;
using System.Collections.Generic;

namespace EmeraldAI
{
    /// <summary>
    /// モジュール式アクション：近接攻撃やプロジェクタイル攻撃を検知して、AI にブロック行動をさせます。
    /// </summary>
    [CreateAssetMenu(fileName = "ブロックアクション", menuName = "Emerald AI/コンバットアクション/ブロックアクション")]
    public class BlockAction : EmeraldAction
    {
        [Header("成功したブロックが継続する時間（秒）")]
        [Range(1, 3)]
        [Tooltip("成功したブロックが持続する時間（秒）を制御します。")]
        public float BlockLength = 1;

        [Header("近接攻撃を検知する半径（メートル）")]
        [Range(1, 8)]
        [Tooltip("近距離（近接）攻撃を検知するための半径。")]
        public float MeleeDetectionRadius = 3f;

        [Header("ブロックを発動できる最大角度（度）")]
        [Range(0, 360)]
        [Tooltip("ブロックをトリガーできる最大角度。これより大きい角度の攻撃はブロックを発動しません。")]
        public float MaxBlockAngle = 60f;

        [Header("ブロック中の被ダメージ軽減率（%）")]
        [Range(0, 100)]
        [Tooltip("AI がブロック中にダメージを受けた場合に軽減される割合（%）。")]
        public int MitigationAmount = 50;

        [Header("飛翔体（プロジェクタイル）を検知する半径（メートル）")]
        [Range(1, 10)]
        [Tooltip("飛翔体（プロジェクタイル）を検知するための半径。")]
        public float ProjectileDetectionRadius = 3.5f;

        [Header("ブロック対象とするプロジェクタイルのレイヤー")]
        [Tooltip("ブロックをトリガーするのに必要なプロジェクタイルのレイヤー。各プロジェクタイルで設定された「Projectile Layer」に基づきます。既定では Ignore Raycast。")]
        public LayerMask ProjectileLayers = 1 << 2;

        [Header("ブロックが発生する確率（0〜1）")]
        [Range(0, 1)]
        [Tooltip("必要条件が満たされた場合にブロックが成功する確率。")]
        public float OddsToBlock = 0.5f;

        /// <summary>
        /// EmeraldAction を継続的に更新します。Update 相当の処理を、このアクションのスコープで実行します。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            BlockActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// 現在のターゲットからの攻撃（近接・飛翔体）の到来を継続的にチェックします。
        /// </summary>
        void BlockActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && EmeraldComponent.CombatComponent.DistanceFromTarget <= MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxBlockAngle / 2f)
                {
                    SetBlockState(EmeraldComponent, ActionClass, true);
                }
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CombatComponent.DistanceFromTarget > MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxBlockAngle / 2f)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(EmeraldComponent.transform.position, ProjectileDetectionRadius, ProjectileLayers);

                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        var IAvoidableRef = hitColliders[i].GetComponent<IAvoidable>();

                        if (IAvoidableRef != null && IAvoidableRef.AbilityTarget == EmeraldComponent.transform)
                        {
                            SetBlockState(EmeraldComponent, ActionClass, true);
                            break;
                        }
                    }
                }
            }
            else
            {
                ActionClass.ActionLengthTimer += Time.deltaTime;

                // メモ: ブロックの終了条件は、この下の処理内で内部的に扱います。
                if (CanExit(EmeraldComponent, ActionClass))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }

                // Unity の Animator と遷移の都合上、状態遷移の合間でのヒットを確実に拾うために順序よく確認します。
                if (EmeraldComponent.AnimationComponent.IsGettingHit)
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }
                if (!EmeraldComponent.AnimationComponent.IsBlocking && EmeraldComponent.AIAnimator.GetBool("Hit"))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }
                if (!EmeraldComponent.AIAnimator.GetBool("Blocking") && EmeraldComponent.AIAnimator.GetBool("Hit"))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }
            }
        }

        /// <summary>
        /// 指定の State（true/false）に応じてブロック状態を設定します。
        /// </summary>
        void SetBlockState(EmeraldSystem EmeraldComponent, ActionsClass ActionClass, bool State)
        {
            // ブロック発動の確率判定を行う
            float Roll = Random.Range(0f, 1f);

            if (Roll > OddsToBlock && State)
            {
                EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                ActionClass.IsActive = false;
                ActionClass.ActionLengthTimer = 0;
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            if (State)
            {
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                // 現在の軽減率と角度を、このアクションの設定値に合わせる
                EmeraldComponent.CombatComponent.MitigationAmount = MitigationAmount;
                EmeraldComponent.CombatComponent.MaxMitigationAngle = MaxBlockAngle;
                EmeraldComponent.AnimationComponent.AttackTriggered = false;
                EmeraldComponent.AIAnimator.ResetTrigger("Dodge Triggered"); // ブロックと同時に回避がトリガーされた場合は回避を停止
                EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            }

            EmeraldComponent.AnimationComponent.InternalBlock = State;
            EmeraldComponent.AIAnimator.SetBool("Blocking", State);
            ActionClass.IsActive = State;
            ActionClass.ActionLengthTimer = 0;
            ActionClass.CooldownLengthTimer = 0;

        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other && ActionClass.CooldownLengthTimer >= CooldownLength && !EmeraldComponent.AIAnimator.GetBool("Attack") && Conditions &&
                !EmeraldComponent.AIAnimator.GetBool("Blocking") && !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && !EmeraldComponent.AnimationComponent.InternalHit;
        }

        /// <summary>
        /// ブロックを終了するための追加条件をチェックします。
        /// </summary>
        bool CanExit(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            return (ActionClass.ActionLengthTimer >= BlockLength || EmeraldComponent.CombatTarget == null || EmeraldComponent.AnimationComponent.IsStunned || EmeraldComponent.AIAnimator.GetBool("Stunned Active") || EmeraldComponent.CombatComponent.TargetAngle > MaxBlockAngle);
        }
    }
}
