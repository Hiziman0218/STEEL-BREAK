using UnityEngine;
using System.Collections;

namespace EmeraldAI
{
    /// <summary>
    /// モジュール式アクション：近接攻撃と飛翔体（プロジェクタイル）を検知し、AI に回避行動をさせます。
    /// </summary>
    [CreateAssetMenu(fileName = "回避アクション", menuName = "Emerald AI/コンバットアクション/回避アクション")]
    public class DodgeAction : EmeraldAction
    {
        [Header("近距離（近接）攻撃を検知する半径（メートル）")]
        [Range(1, 8)]
        [Tooltip("近距離（近接）攻撃を検知するための半径。")]
        public float MeleeDetectionRadius = 3f;

        [Header("回避をトリガー可能な最大角度（度）")]
        [Range(0, 360f)]
        [Tooltip("回避を発動できる最大角度。この角度より大きい場合は回避が発動しません。")]
        public float MaxDodgeAngle = 60f;

        [Header("回避中に受けるダメージの軽減率（%）")]
        [Range(0, 100)]
        [Tooltip("AI が回避中にダメージを受けた場合に軽減される割合（%）。")]
        public int MitigationAmount = 100;

        [Header("飛翔体（プロジェクタイル）を検知する半径（メートル）")]
        [Range(1, 10)]
        [Tooltip("接近中の飛翔体を検知するための半径。")]
        public float ProjectileDetectionRadius = 3.5f;

        [Header("回避をトリガーする対象のプロジェクタイルレイヤー")]
        [Tooltip("回避をトリガーするのに必要なプロジェクタイルのレイヤー。各プロジェクタイルで設定された「Projectile Layer」に基づきます。既定では Ignore Raycast。")]
        public LayerMask ProjectileLayers = 1 << 2;

        [Header("回避が発生する確率（0〜1）")]
        [Range(0, 1)]
        [Tooltip("必要条件が満たされた場合に回避が成功する確率。")]
        public float OddsToDodge = 0.5f;

        /// <summary>
        /// EmeraldAction を継続的に更新します。Update 相当の処理を、このアクションのスコープ内で行います。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            DodgeActionUpdate(EmeraldComponent, ActionClass);
        }

        void DodgeActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            ActionClass.IsActive = EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.InternalDodge;

            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && EmeraldComponent.CombatComponent.DistanceFromTarget <= MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxDodgeAngle / 2f)
                {
                    TriggerDodge(EmeraldComponent, ActionClass);
                }

                // ProjectileLayers に含まれるレイヤー上のオブジェクトから IAvoidable インターフェイス実装を探す
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CombatComponent.TargetAngle < MaxDodgeAngle / 2f)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(EmeraldComponent.transform.position, ProjectileDetectionRadius, ProjectileLayers);

                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        var IAvoidableRef = hitColliders[i].GetComponent<IAvoidable>();

                        if (IAvoidableRef != null && IAvoidableRef.AbilityTarget == EmeraldComponent.transform)
                        {
                            TriggerDodge(EmeraldComponent, ActionClass);
                            break;
                        }
                    }
                }
            }

            if (ActionClass.IsActive)
            {
                EmeraldComponent.CombatComponent.CombatActionActive = ActionClass.IsActive;

                var Conditions = (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
                if (Conditions)
                {
                    EmeraldComponent.AnimationComponent.ResetTriggers(0);
                }
            }
        }

        void TriggerDodge(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            // 回避の発生判定を行う
            float Roll = Random.Range(0f, 1f);
            ActionClass.CooldownLengthTimer = 0;
            if (Roll > OddsToDodge)
            {
                return;
            }

            if (EmeraldComponent.AnimationComponent.InternalHit) return;

            EmeraldComponent.AnimationComponent.InternalDodge = true;
            // 現在の軽減率と角度を、このアクションの設定値に合わせる
            EmeraldComponent.CombatComponent.MitigationAmount = MitigationAmount;
            EmeraldComponent.CombatComponent.MaxMitigationAngle = MaxDodgeAngle;
            EmeraldComponent.AIAnimator.SetBool("Blocking", false); // 回避と同時にブロックがトリガーされていた場合はブロックを停止
            EmeraldComponent.AnimationComponent.TriggerDodgeState();
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AnimationComponent.AttackTriggered = false;
            EmeraldComponent.MovementComponent.SetActionDirection();
            if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.BehaviorsComponent.IsAiming = true;
            EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(CancelAttack(EmeraldComponent));
        }

        /// <summary>
        /// 回避中に攻撃が（遅延やアニメーション遷移の都合で）誤ってトリガーされた場合にリセットします。
        /// </summary>
        IEnumerator CancelAttack(EmeraldSystem EmeraldComponent)
        {
            yield return new WaitForSeconds(0.4f);
            EmeraldComponent.AnimationComponent.InternalDodge = false;
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other && ActionClass.CooldownLengthTimer >= CooldownLength && Conditions &&
                !EmeraldComponent.AIAnimator.GetBool("Attack") && !EmeraldComponent.AIAnimator.GetBool("Blocking") && !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && !EmeraldComponent.AnimationComponent.InternalHit;

        }
    }
}
