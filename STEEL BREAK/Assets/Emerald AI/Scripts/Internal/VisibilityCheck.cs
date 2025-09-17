using UnityEngine;  // Unity の基本API

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 【VisibilityCheck（可視判定による最適化制御）】
    /// AIのレンダラー可視状態（isVisible）とLOD構成に基づき、
    /// ・見えていない間はAIを「無効化（省リソース状態）」へ
    /// ・再度見えるようになったら「有効化（通常状態）」へ
    /// を自動制御するコンポーネント。
    /// Deactivate/Activate は EmeraldOptimization と EmeraldSystem の状態を切り替える。
    /// </summary>
    public class VisibilityCheck : MonoBehaviour
    {
        #region Variables  // 変数宣言

        [Header("最適化設定を保持するコンポーネント参照（可視判定・LOD数・遅延設定など）")]
        [HideInInspector] public EmeraldOptimization EmeraldOptimization;   // Emerald の最適化コンポーネント

        [Header("対象AI本体の EmeraldSystem 参照（アニメーション・移動・検知などの有効/無効を切替）")]
        [HideInInspector] public EmeraldSystem EmeraldComponent;            // Emerald のメインシステム参照

        [Header("ダメージ/HP参照用インターフェース（AIの生死判定に使用）")]
        IDamageable m_IDamageable;                                          // IDamageable（HPが0以下かどうかの判断に使う）

        [Header("初期遅延後に可視イベントの有効化を開始したか（内部フラグ）")]
        bool SystemActivated = false;                                       // InitializeDelay 後に true になる

        [Header("非表示になってからの累積時間（秒）。UseDeactivateDelay=Yes のときの判定に使用")]
        float DeactivateTimer;                                              // 連続で不可視が続いた時間

        #endregion

        void Start()
        {
            m_IDamageable = EmeraldOptimization.GetComponent<IDamageable>(); // 同一ルート上の IDamageable を取得
            Invoke("InitializeDelay", 1);                                    // 開始直後の多重イベント回避のため、1秒遅延して起動
        }

        /// <summary>
        /// 起動直後の多重レンダラーイベント発火を避けるため、遅延して有効化する。
        /// </summary>
        void InitializeDelay()
        {
            SystemActivated = true; // 以後、可視イベント（OnBecameVisible）で有効化を許可
        }

        /// <summary>
        /// LOD と最適化設定に応じ、各レンダラーの可視状態をチェックする。
        /// ・OptimizedState が Inactive（通常状態）で、全LODが不可視なら Deactivate
        /// ・OptimizedState が Active（最適化状態）で、いずれかが可視なら Activate
        /// </summary>
        public void CheckAIRenderers()
        {
            if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Inactive && EmeraldOptimization.Initialized)
            {
                if (!EmeraldOptimization.Renderer1.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.One)
                {
                    DeactivateTimer += Time.deltaTime; // 不可視経過を加算

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate(); // 遅延条件を満たすか、遅延無効なら即座に無効化
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Two)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && !EmeraldOptimization.Renderer3.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Three)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
                else if (!EmeraldOptimization.Renderer1.isVisible && !EmeraldOptimization.Renderer2.isVisible && !EmeraldOptimization.Renderer3.isVisible && !EmeraldOptimization.Renderer4.isVisible && EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Four)
                {
                    DeactivateTimer += Time.deltaTime;

                    if (EmeraldOptimization.UseDeactivateDelay == YesOrNo.Yes && DeactivateTimer >= EmeraldOptimization.DeactivateDelay || EmeraldOptimization.UseDeactivateDelay == YesOrNo.No)
                    {
                        Deactivate();
                    }
                }
            }
            else if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Active)
            {
                if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Two)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible)
                    {
                        Activate(); // どれか1つでも見えたら復帰
                    }
                }
                else if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Three)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible || EmeraldOptimization.Renderer3.isVisible)
                    {
                        Activate();
                    }
                }
                else if (EmeraldOptimization.TotalLODsRef == EmeraldOptimization.TotalLODsEnum.Four)
                {
                    if (EmeraldOptimization.Renderer1.isVisible || EmeraldOptimization.Renderer2.isVisible || EmeraldOptimization.Renderer3.isVisible || EmeraldOptimization.Renderer4.isVisible)
                    {
                        Activate();
                    }
                }
            }
        }

        /// <summary>
        /// 最適化システム使用時に、AIを「無効化（省リソース）」へ遷移させる。
        /// ・戦闘中/死亡中/帰還中/警戒中などの例外状態では無効化しない。
        /// ・コンポーネントやアニメーターを止め、コリジョン/検知も停止する。
        /// </summary>
        public void Deactivate()
        {
            if (!EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.CombatTarget == null && !EmeraldComponent.MovementComponent.ReturningToStartInProgress && EmeraldComponent.DetectionComponent.CurrentDetectionState == EmeraldDetection.DetectionStates.Unaware)
            {
                EmeraldComponent.CombatComponent.TargetDetectionActive = false;     // ターゲット検知を停止
                EmeraldComponent.AIBoxCollider.enabled = false;                     // コリジョン無効化
                EmeraldComponent.DetectionComponent.enabled = false;                // 検知処理を停止
                EmeraldComponent.enabled = false;                                   // 本体コンポーネントを停止
                EmeraldOptimization.OptimizedState = EmeraldOptimization.OptimizedStates.Active; // 「最適化状態」へ
                if (EmeraldComponent.UIComponent != null) EmeraldComponent.UIComponent.SetUI(false); // UIを非表示
                if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.DisableInverseKinematics(); // IK停止
                EmeraldComponent.m_NavMeshAgent.destination = transform.position + transform.forward * 0.5f; // 目的地を近傍に設定（停止誘導）
                EmeraldComponent.AIAnimator.SetFloat("Speed", 0);                    // スピードパラメータを0へ
                EmeraldComponent.AIAnimator.enabled = false;                         // アニメーター停止
                DeactivateTimer = 0;                                                // 遅延タイマーをリセット
            }
        }

        /// <summary>
        /// 最適化システム使用時に、AIを「有効化（通常）」へ遷移させる。
        /// ・HPが0より大きい（生存）場合のみ復帰。
        /// </summary>
        public void Activate()
        {
            if (m_IDamageable.Health > 0)
            {
                EmeraldComponent.CombatComponent.TargetDetectionActive = true;      // ターゲット検知を有効化
                EmeraldComponent.AIBoxCollider.enabled = true;                      // コリジョン有効化
                EmeraldComponent.DetectionComponent.enabled = true;                 // 検知処理を再開
                EmeraldComponent.enabled = true;                                    // 本体コンポーネントを有効化
                EmeraldComponent.AIAnimator.enabled = true;                         // アニメーターを再開
                if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.EnableInverseKinematics(); // IK再開
                EmeraldOptimization.OptimizedState = EmeraldOptimization.OptimizedStates.Inactive; // 「通常状態」へ
            }
        }

        /// <summary>
        /// このAIのレンダラーがカメラから見えなくなったときに呼ばれる。
        /// 一定の遅延（DeactivateDelay）後に Deactivate を実行する。
        /// </summary>
        void OnBecameInvisible()
        {
            if (EmeraldComponent.AnimationComponent.IsDead) return;                 // 既に死亡なら無視
            Invoke("Deactivate", EmeraldOptimization.DeactivateDelay);              // 遅延後に無効化を呼ぶ
        }

        /// <summary>
        /// 起動時に一度だけ、可視の場合に限ってAIを有効化するために呼ばれる。
        /// </summary>
        void OnWillRenderObject()
        {
            if (EmeraldOptimization.OptimizedState == EmeraldOptimization.OptimizedStates.Active)
            {
                Activate(); // 可視なら復帰
            }
        }

        /// <summary>
        /// このAIのレンダラーが可視になったときに呼ばれる。
        /// 初期遅延が完了していれば、Invokeの遅延呼び出しをキャンセルして即時有効化。
        /// </summary>
        void OnBecameVisible()
        {
            if (SystemActivated)
            {
                CancelInvoke(); // Deactivate の予約を取り消し
                Activate();     // 直ちに復帰
            }
        }
    }
}
