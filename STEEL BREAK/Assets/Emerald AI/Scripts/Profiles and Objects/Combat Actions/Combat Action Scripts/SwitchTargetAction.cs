using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【モジュール式アクション】
    /// AI にターゲットを切り替える能力を付与します。
    /// </summary>
    [CreateAssetMenu(fileName = "ターゲット切り替えアクション", menuName = "Emerald AI/コンバットアクション/ターゲット切り替えアクション")]
    public class SwitchTargetAction : EmeraldAction
    {
        [Header("ターゲット切り替え方法（PickTargetTypes）")]
        [Tooltip("ターゲットを切り替える際に使用する方式。")]
        public PickTargetTypes PickTargetType = PickTargetTypes.Random;

        /// <summary>
        /// EmeraldAction を継続的に更新します。渡された EmeraldComponent と ActionClass の情報を用いて、
        /// このアクション内で Update 相当の処理を実行します。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!CanExecute(EmeraldComponent, ActionClass))
                return;

            SwitchTarget(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return ActionClass.CooldownLengthTimer >= CooldownLength && Conditions && EmeraldComponent.transform.localScale != Vector3.one * 0.003f;

        }

        /// <summary>
        /// CanExecute の条件を満たしたため、アクションを実行します。
        /// </summary>
        void SwitchTarget(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (EmeraldComponent.AnimationComponent.IsAttacking || EmeraldComponent.AIAnimator.GetBool("Attack") || EmeraldComponent.DetectionComponent.LineOfSightTargets.Count <= 1)
            {
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            // カバーポイントへ移動中は、AI にターゲット切り替えを許可しない
            if (EmeraldComponent.CoverComponent != null && EmeraldComponent.CoverComponent.CoverState == EmeraldCover.CoverStates.MovingToCover)
            {
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetType);
            ActionClass.CooldownLengthTimer = 0;
        }
    }
}
