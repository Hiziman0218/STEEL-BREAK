using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static EmeraldAI.EmeraldMovement;

namespace EmeraldAI
{
    /// <summary>
    /// 【モジュール式アクション(レイキャストプロテスト)】
    /// 戦闘中、ターゲットの周囲にランダムなウェイポイントを生成し、そこへ移動する能力をAIに与えます。
    /// </summary>
    [CreateAssetMenu(fileName = "ランダム移動アクション", menuName = "Emerald AI/コンバットアクション/Rayテストランダム移動")]
    public class TestRandomMovementAction : EmeraldAction
    {
        [Header("最小ウェイポイント半径（メートル）")]
        [Range(4, 20)]
        [Tooltip("ランダム移動のウェイポイントを生成できる半径の最小値。")]
        public int MinWaypointRadius = 4;

        [Header("最大ウェイポイント半径（メートル）")]
        [Range(4, 20)]
        [Tooltip("ランダム移動のウェイポイントを生成できる半径の最大値。")]
        public int MaxWaypointRadius = 7;

        [Header("攻撃再開までの最短待機秒数")]
        [Range(0f, 6)]
        [Tooltip("ランダム移動後にAIが攻撃を再開するまでの最短待機時間（秒）。")]
        public float MinWaitSeconds = 0.5f;

        [Header("攻撃再開までの最長待機秒数")]
        [Range(0f, 6)]
        [Tooltip("ランダム移動後にAIが攻撃を再開するまでの最長待機時間（秒）。")]
        public float MaxWaitSeconds = 2f;

        [Header("ランダム移動が発生する確率（0〜1）")]
        [Range(0, 1)]
        [Tooltip("必要条件が満たされた場合にランダム移動が発生する確率。")]
        public float OddsToMove = 0.5f;

        //レイキャストプロ用のダミー
        private Transform m_randomMoveDummy;


        /// <summary>
        /// EmeraldAction を継続的に更新します。
        /// 渡された EmeraldComponent と ActionClass の情報を使い、このアクションの範囲内で Update 相当の処理を行います。
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            RandomMovementActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// UpdateAction を用いてランダム移動アクションを更新します。
        /// </summary>
        void RandomMovementActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            // Cover Component が存在する場合は、このコンバットアクションが干渉する可能性があるため処理を終了
            if (EmeraldComponent.CoverComponent) return;

            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass))
                {
                    GenerateRandomPositionWithinRadius(EmeraldComponent, ActionClass);
                }
            }
            else
            {
                if (CanCancel(EmeraldComponent))
                {
                    if (ActionClass.ActionCoroutine != null)
                        EmeraldComponent.GetComponent<MonoBehaviour>().StopCoroutine(ActionClass.ActionCoroutine);

                    ActionClass.IsActive = false;
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                }
            }
        }

        void GenerateRandomPositionWithinRadius(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            float Roll = Random.Range(0f, 1f);
            if (Roll > OddsToMove)
            {
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            EmeraldComponent.CombatComponent.CancelAllCombatActions();
            ActionClass.IsActive = true;
            ActionClass.CooldownLengthTimer = 0;
            EmeraldComponent.MovementComponent.StopBackingUp();
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
            int Radius = Random.Range(MinWaypointRadius, MaxWaypointRadius + 1);

            Vector3 Dir = (EmeraldComponent.CombatTarget.position - EmeraldComponent.transform.position).normalized;
            int OffsetAmount = Random.Range(1, 3);
            var DirectionOffset = Quaternion.Euler(0, OffsetAmount == 1 ? -50 : 50, 0) * Dir;
            Vector3 GeneratedDestination = EmeraldComponent.CombatTarget.position + (DirectionOffset * Radius);

            RaycastHit HitDown;
            if (Physics.Raycast(GeneratedDestination + Vector3.up * 2, -Vector3.up, out HitDown, 5f))
            {
                GeneratedDestination.y = HitDown.point.y;
            }

            //レイキャストプロ型かナビメッシュ型か判定（個人改造）
            if (EmeraldComponent.MovementComponent.MovementType == MovementTypes.RayCastPro)
            {
                //ダミーがなければ生成
                if (m_randomMoveDummy == null)
                {
                    GameObject go = new GameObject("RandomMoveDummy");
                    go.hideFlags = HideFlags.HideInHierarchy;
                    m_randomMoveDummy = go.transform;
                }
                m_randomMoveDummy.position = GeneratedDestination;
                //コントローラーに位置を渡す
                EmeraldComponent.MovementComponent.m_RCController.detector.destination = m_randomMoveDummy;
            }
            else
            {
                EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
            }


            Coroutine MoveCoroutine = ActionClass.ActionCoroutine;

            if (MoveCoroutine != null)
                EmeraldComponent.GetComponent<MonoBehaviour>().StopCoroutine(MoveCoroutine);
            MoveCoroutine = EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(Moving(EmeraldComponent, ActionClass));

            ActionClass.ActionCoroutine = MoveCoroutine;
        }

        IEnumerator Moving(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            Transform targetTransform = (EmeraldComponent.MovementComponent.MovementType == MovementTypes.RayCastPro)
                ? m_randomMoveDummy
                : null;

            // --- RayCastPro/共通の到達待ち ---
            while (!CanCancel(EmeraldComponent) && !EmeraldComponent.AnimationComponent.IsDead)
            {
                float dist;
                if (EmeraldComponent.MovementComponent.MovementType == MovementTypes.RayCastPro)
                {
                    dist = Vector3.Distance(EmeraldComponent.transform.position, targetTransform.position);
                    if (dist < 0.75f) break;

                    Vector3 dir = targetTransform.position - EmeraldComponent.transform.position;
                    EmeraldComponent.MovementComponent.UpdateRotations(dir);
                }
                else // NavMesh
                {
                    if (!EmeraldComponent.m_NavMeshAgent.enabled || !EmeraldComponent.m_NavMeshAgent.isOnNavMesh) break;
                    dist = EmeraldComponent.m_NavMeshAgent.remainingDistance;
                    if (dist < 0.75f) break;

                    Vector3 dir = EmeraldComponent.m_NavMeshAgent.steeringTarget - EmeraldComponent.transform.position;
                    EmeraldComponent.MovementComponent.UpdateRotations(dir);
                }

                yield return null;
            }

            // --- 待機処理 ---
            float wait = Random.Range(MinWaitSeconds, MaxWaitSeconds);
            float t = 0;
            while (t < wait && EmeraldComponent.CombatTarget != null)
            {
                t += Time.deltaTime;
                Vector3 dir = EmeraldComponent.CombatTarget.position - EmeraldComponent.transform.position;
                EmeraldComponent.MovementComponent.UpdateRotations(dir);
                yield return null;
            }

            ActionClass.IsActive = false;
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;

            if (EmeraldComponent.MovementComponent.MovementType == MovementTypes.NavMeshDriven)
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
        }

        bool CanCancel(EmeraldSystem EmeraldComponent)
        {
            return (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
        }

        /// <summary>
        /// この EmeraldAction を実行するために必要な条件。
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return (Conditions && ActionClass.CooldownLengthTimer >= CooldownLength && EmeraldComponent.CombatComponent.DistanceFromTarget < 15 && !EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && !EmeraldComponent.AnimationComponent.IsBlocking && !EmeraldComponent.AIAnimator.GetBool("Attack"));
        }
    }
}
