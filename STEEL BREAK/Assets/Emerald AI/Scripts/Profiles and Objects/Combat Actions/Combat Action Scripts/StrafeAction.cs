using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【モジュール式アクション】
    /// AI にターゲットの周囲をストレイフ（横移動）させる能力を付与します。
    /// </summary>
    [CreateAssetMenu(fileName = "ストレイフアクション", menuName = "Emerald AI/コンバットアクション/ストレイフアクション")]
    public class StrafeAction : EmeraldAction
    {
        [Header("ストレイフの最小持続時間（秒）")]
        [Range(0.5f, 8)]
        [Tooltip("ストレイフ（横移動）が継続する最小時間（秒）。")]
        public float StrafingLengthMin = 1;

        [Header("ストレイフの最大持続時間（秒）")]
        [Range(1, 8)]
        [Tooltip("ストレイフ（横移動）が継続する最大時間（秒）。")]
        public float StrafingLengthMax = 2f;

        [Header("ストレイフ発生の確率（0〜1）")]
        [Range(0, 1)]
        [Tooltip("必要条件が満たされた場合にストレイフを行う確率。")]
        public float OddsToStrafe = 0.5f;

        /// <summary>
        /// EmeraldAction を継続的に更新します。
        /// 渡された EmeraldComponent と ActionClass の情報を使って、このアクション内で Update 相当の処理を行います。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            StrafeActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// ストレイフアクションを Update 処理で更新します。
        /// </summary>
        void StrafeActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            // Cover Component が存在する場合、このコンバットアクションは機能に干渉する可能性があるため終了
            if (EmeraldComponent.CoverComponent) return;

            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass))
                {
                    SetStrafeState(EmeraldComponent, ActionClass, true);
                }
            }
            else
            {
                ActionClass.ActionLengthTimer += Time.deltaTime;
                var Conditions = (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;

                // ストレイフ終了の条件をチェック
                if (Conditions || CanExit(EmeraldComponent, ActionClass))
                {
                    SetStrafeState(EmeraldComponent, ActionClass, false);
                }
            }
        }

        /// <summary>
        /// AI がストレイフしている方向へレイキャストを飛ばし、回避すべき障害物があるかを返します。
        /// </summary>
        bool StrafeAvoidance(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            int Direction = EmeraldComponent.AIAnimator.GetInteger("Strafe Direction");
            return Direction == 0 && Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, -EmeraldComponent.transform.right * 3, 1, EmeraldComponent.MovementComponent.BackupLayerMask) ||
                Direction == 1 && Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, EmeraldComponent.transform.right * 3, 1, EmeraldComponent.MovementComponent.BackupLayerMask);
        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return (Mathf.Round(EmeraldComponent.CombatComponent.DistanceFromTarget * 10) / 10) <= (Mathf.Round(EmeraldComponent.m_NavMeshAgent.stoppingDistance * 10) / 10) + 0.1f && ActionClass.CooldownLengthTimer >= CooldownLength &&
                (Mathf.Round(EmeraldComponent.m_NavMeshAgent.remainingDistance * 10) / 10) >= (Mathf.Round(EmeraldComponent.CombatComponent.TooCloseDistance * 10) / 10) / 2f &&
                Conditions && !EmeraldComponent.AIAnimator.GetBool("Attack") && !EmeraldComponent.AIAnimator.GetBool("Walk Backwards") && !EmeraldComponent.AIAnimator.GetBool("Blocking") &&
                !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other &&
                EmeraldComponent.CombatComponent.TargetAngle < 60 && EmeraldComponent.CombatTarget.localScale != Vector3.one * 0.003f && EmeraldComponent.transform.localScale != Vector3.one * 0.003f;
        }

        /// <summary>
        /// ストレイフを終了する追加条件をチェックします。
        /// </summary>
        bool CanExit(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            return (ActionClass.ActionLengthTimer >= ActionClass.ActionLength || EmeraldComponent.AIAnimator.GetBool("Hit") || EmeraldComponent.CombatTarget == null || StrafeAvoidance(EmeraldComponent, ActionClass) || EmeraldComponent.CombatTarget.localScale == Vector3.one * 0.003f ||
                EmeraldComponent.transform.localScale == Vector3.one * 0.003f || EmeraldComponent.AnimationComponent.IsBackingUp || EmeraldComponent.AnimationComponent.IsEquipping || EmeraldComponent.CombatComponent.TargetAngle > 60);
        }

        /// <summary>
        /// 渡された真偽値に応じてストレイフ状態を設定します。
        /// </summary>
        void SetStrafeState(EmeraldSystem EmeraldComponent, ActionsClass ActionClass, bool State)
        {
            // ストレイフを行う確率判定
            float Roll = Random.Range(0f, 1f);

            if (Roll <= OddsToStrafe && State)
            {
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                EmeraldComponent.AnimationComponent.SetStrafeState(State);
                ActionClass.IsActive = State;
            }

            if (!State) EmeraldComponent.AnimationComponent.SetStrafeState(State);
            if (!State) ActionClass.IsActive = State;
            ActionClass.ActionLength = Random.Range(StrafingLengthMin, StrafingLengthMax);
            ActionClass.ActionLengthTimer = 0;
            ActionClass.CooldownLengthTimer = 0;
        }
    }
}
