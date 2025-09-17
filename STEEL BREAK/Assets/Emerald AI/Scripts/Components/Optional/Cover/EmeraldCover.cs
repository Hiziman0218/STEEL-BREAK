using System.Collections;                          // コルーチン制御
using System.Collections.Generic;                  // List などのコレクション
using UnityEngine;                                 // Unity 基本API
using UnityEngine.AI;                              // NavMeshAgent などのナビメッシュ
using System.Linq;                                 // Linq（Select/ToList/Sort 等）

namespace EmeraldAI                                 // EmeraldAI 名前空間
{
    /// <summary>
    /// （日本語）AI に戦闘中のカバー（Cover Node）の使用能力を付与するコンポーネント。
    /// カバーノードの探索・選択・移動・隠れる/覗き（ピーク）・攻撃状態の遷移を管理します。
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/cover-component")] // 公式ヘルプ
    // 【クラス概要】EmeraldCover：
    //  カバー探索半径内の CoverNode を条件に基づいて評価し、最適ノードへ移動。
    //  到着後は「隠れる→覗く（攻撃）」のループ（または一度だけ隠れて攻撃）を行い、
    //  条件に応じて別ノードを再探索します。
    public class EmeraldCover : MonoBehaviour
    {
        #region Cover Variables                     // —— カバー挙動に関する変数群 ——

        [Header("ターゲットからの最小距離（この距離未満のノードは無効）")]
        [Range(3f, 15f)]
        public float MinCoverDistance = 5f;        // カバー最小距離

        [Header("カバーノードまでの最大移動距離")]
        [Range(5f, 30f)]
        public float MaxTravelDistance = 15f;      // 最大移動距離

        [Header("カバーノード探索半径")]
        [Range(8f, 40f)]
        public float CoverSearchRadius = 20f;      // 探索半径

        [Header("カバーノード検出用のレイヤーマスク")]
        public LayerMask CoverNodeLayerMask;       // OverlapSphere 用レイヤー

        [Header("現在カバー中かどうか（内部状態）")]
        public bool HasCover;                      // カバー取得フラグ

        //How long the AI stays hiding
        [Header("隠れる秒数（最小）")]
        [Range(1, 10)]
        public float HideSecondsMin = 1.5f;        // 隠れる時間の下限

        [Header("隠れる秒数（最大）")]
        [Range(1, 10)]
        public float HideSecondsMax = 3f;          // 隠れる時間の上限

        //How long the AI will attack when standing
        [Header("覗き（立ち）時の攻撃継続秒数（最小）")]
        [Range(1, 15)]
        public float AttackSecondsMin = 2f;        // 攻撃時間の下限

        [Header("覗き（立ち）時の攻撃継続秒数（最大）")]
        [Range(1, 15)]
        public float AttackSecondsMax = 4f;        // 攻撃時間の上限

        //How many times the AI will peak. After reaching 0, the AI will find another Cover Node.
        [Header("ピーク（覗き）回数（最小）※回数消化後に別ノード探索")]
        [Range(1, 10)]
        public int PeakTimesMin = 1;               // ピーク回数下限

        [Header("ピーク（覗き）回数（最大）※回数消化後に別ノード探索")]
        [Range(1, 10)]
        public int PeakTimesMax = 3;               // ピーク回数上限

        public enum CoverStates { Inactive, MovingToCover, Hiding, Peaking }; // カバー状態の列挙

        [Header("現在のカバー状態")]
        public CoverStates CoverState;             // 状態管理

        [Header("EmeraldSystem 参照（AI本体）")]
        EmeraldSystem EmeraldComponent;            // AI本体への参照

        [Header("カバーに移動中のコルーチン参照")]
        Coroutine MovingToCoverCoroutine;          // カバー移動

        [Header("カバー状態（隠れる/ピーク等）のコルーチン参照")]
        Coroutine CoverStateCoroutine;             // 状態遷移

        [Header("カバー探索の経過タイマー（秒）")]
        float CheckForCoverTimer;                  // 探索用タイマー

        [Header("カバー探索のチェック間隔（秒）")]
        float CheckForCoverSeconds;                // 探索間隔

        [Header("距離でソートした近傍ノードから考慮する最大数")]
        int maxConsideredCoverNodes = 6;           // 候補上限

        [Header("現在使用中（最後に選択した）カバーノード")]
        public CoverNode CurrentCoverNode = null;  // 直近ノード

        [Header("インフォメッセージ確認フラグ（未使用/拡張向け）")]
        public bool ConfirmInfoMessage = false;    // 情報メッセージ確認

        [Header("占有中のカバーノード一覧（最終条件判定用）")]
        List<CoverNode> OccupiedCoverNodes = new List<CoverNode>(); // 占有ノード

        [Header("有効なカバーノード候補の一時リスト（シリアライズ表示用）")]
        [SerializeField]
        List<CoverPointData> ValidCoverNodes = new List<CoverPointData>(); // 候補

        // 候補データの内部クラス（シリアライズ対象ではない）
        private class CoverPointData
        {
            [Header("候補となるカバーノード参照")]
            public CoverNode coverNode;            // ノード参照

            [Header("エージェントから候補ノードまでの距離")]
            public float distanceToAgent;          // 距離
        }
        #endregion

        #region Editor Variables                    // —— エディタ表示の折りたたみ制御 ——
        [Header("【Editor表示】設定群を隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;           // セクション非表示

        [Header("【Editor表示】設定セクションの折りたたみ")]
        public bool SettingsFoldout;               // セクション開閉
        #endregion

        void Start()
        {
            InitializeCover();                     // カバー機能の初期化
        }

        ///<summary>
        /// （日本語）カバーコンポーネントを初期化する。
        /// イベント購読・探索間隔の初期化・回頭速度の下限調整等を行う。
        ///</summary>
        void InitializeCover()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();                              // 本体参照
            CheckForCoverSeconds = Random.Range(0.9f, 1.15f);                              // 探索間隔をランダム化
            EmeraldComponent.MovementComponent.OnBackup += CancelCover;                    // バック移動開始でカバー中断
            EmeraldComponent.CombatComponent.OnKilledTarget += CancelCover;                // ターゲット撃破でカバー中断
            if (EmeraldComponent.MovementComponent.MovingTurnSpeedCombat < 500)            // 戦闘時の移動回頭速度の下限保証
                EmeraldComponent.MovementComponent.MovingTurnSpeedCombat = 500;
        }

        ///<summary>
        /// （日本語）現在のカバー状態をキャンセルする（移動/状態コルーチン停止、値のリセット等）。
        ///</summary>
        void CancelCover ()
        {
            if (CoverStateCoroutine != null) StopCoroutine(CoverStateCoroutine);           // 状態コルーチン停止
            if (MovingToCoverCoroutine != null) StopCoroutine(MovingToCoverCoroutine);     // 移動コルーチン停止
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;              // デフォルト移動の一時停止解除
            EmeraldComponent.MovementComponent.LockTurning = false;                        // 回頭ロック解除
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance; // 停止距離を攻撃距離へ戻す
            EmeraldComponent.AIAnimator.SetBool("Cover Active", false);                    // アニメ状態を解除
            HasCover = false;                                                              // カバーフラグ解除
            CoverState = CoverStates.Inactive;                                             // 状態初期化
            if (CurrentCoverNode != null)                                                  // 現ノードの占有解除
            {
                CurrentCoverNode.ClearOccupant();
                CurrentCoverNode = null;
            }
        }

        void Update()
        {
            if (EmeraldComponent.CombatComponent.CombatState)                              // 戦闘中のみ動作
            {
                if (CoverState == CoverStates.Inactive)                                    // カバー非アクティブ時
                {
                    if (!HasCover && !EmeraldComponent.MovementComponent.DefaultMovementPaused) // かつ通常移動が停止していないとき
                    {
                        CheckForCoverTimer += Time.deltaTime;                               // タイマー加算

                        if (CheckForCoverTimer >= CheckForCoverSeconds)                     // 規定間隔で探索
                        {
                            FindCover();                                                    // カバー探索
                        }
                    }
                }

                //If at any point the AI dies, cancel cover.
                if (!EmeraldComponent.m_NavMeshAgent.enabled || EmeraldComponent.AnimationComponent.IsDead) // NavMesh無効または死亡
                {
                    CancelCover();                                                          // カバー中断
                }
            }
        }

        /// <summary>
        /// （日本語）新しいカバーノードを探索して移動を開始する。
        /// 事前に状態・各フラグをリセットし、見つからなければ代替挙動を実行する。
        /// </summary>
        public void FindCover()
        {
            // CombatTarget が null の瞬間があり得るため、その場合は復帰
            if (EmeraldComponent.CombatTarget == null) return;

            CoverState = CoverStates.Inactive;                                             // 状態初期化
            HasCover = false;                                                              // カバー解除
            CheckForCoverSeconds = Random.Range(0.9f, 1.15f);                              // 次探索間隔を再設定
            CheckForCoverTimer = 0;                                                        // タイマーリセット

            Transform coverNode = FindCoverNode();                                         // 候補の取得
            if (coverNode != null)
            {
                InitializeCoverNode(coverNode);                                            // ノードへ移動開始
            } 
            else
            {
                // 空きノードなし：視界が開ける位置へ移動するか、現ノードを再利用
                if (CurrentCoverNode != null)
                {
                    if (CurrentCoverNode.GetLineOfSightPosition == YesOrNo.No || 
                        CurrentCoverNode.GetLineOfSightPosition == YesOrNo.Yes && !EmeraldComponent.DetectionComponent.TargetObstructed)
                    {
                        InitializeCoverNode(CurrentCoverNode.transform);                   // 現ノード再利用
                    }
                    else if (CurrentCoverNode.GetLineOfSightPosition == YesOrNo.Yes && EmeraldComponent.DetectionComponent.TargetObstructed)
                    {
                        Vector3 Destination = EmeraldAPI.Internal.FindUnobstructedPosition(EmeraldComponent); // 視界が開ける位置
                        StartCoroutine(Moving(Destination));                                                  // そこへ移動
                    }
                }
            }
        }

        void InitializeCoverNode (Transform coverNode)                                      // 内部：カバーノードへの移動セットアップ
        {
            EmeraldComponent.CombatComponent.CancelAllCombatActions();                      // 戦闘アクションをキャンセル
            EmeraldComponent.MovementComponent.StopBackingUp();                             // バック移動停止
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.25f;                       // 到達しやすい停止距離

            if (MovingToCoverCoroutine != null) StopCoroutine(MovingToCoverCoroutine);      // 既存移動の停止
            EmeraldComponent.m_NavMeshAgent.ResetPath();                                    // 経路リセット
            EmeraldComponent.m_NavMeshAgent.SetDestination(coverNode.position);             // 目的地設定
            MovingToCoverCoroutine = StartCoroutine(MoveToCoverNode(coverNode.position));   // コルーチン開始
        }

        /// <summary>
        /// （日本語）選択されたカバーノードへ移動する。到着・遮蔽条件・占有競合を監視しつつ進行。
        /// 条件によりカバー状態（隠れる/ピーク/攻撃）へ遷移する。
        /// </summary>
        IEnumerator MoveToCoverNode(Vector3 Destination)
        {
            CoverState = CoverStates.MovingToCover;                                         // 状態更新
            EmeraldComponent.MovementComponent.DefaultMovementPaused = true;                // デフォルト移動一時停止
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;                        // 停止距離
            EmeraldComponent.MovementComponent.LockTurning = false;                         // 回頭ロック解除
            yield return new WaitForSeconds(0.01f);                                         // 短時間待機（遷移を滑らかに）

            // 目的地が十分離れている場合のみ hasPath を待つ
            if (Vector3.Distance(Destination, transform.position) > 0.25f) 
                yield return new WaitUntil(() => EmeraldComponent.m_NavMeshAgent.hasPath);

            // カバーノードへ向かって移動
            while (EmeraldComponent.m_NavMeshAgent.enabled && !EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.m_NavMeshAgent.remainingDistance >= 0.5f)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;                    // 常に同停止距離
                Vector3 Direction = new Vector3(EmeraldComponent.m_NavMeshAgent.steeringTarget.x, 0, EmeraldComponent.m_NavMeshAgent.steeringTarget.z) 
                                  - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);               // 進行方向へ回頭

                // 競合：同ノードが他AIに占有されたらキャンセル
                if (CurrentCoverNode != null && CurrentCoverNode.IsOccupied && CurrentCoverNode.Occupant != transform)
                {
                    CancelCover();                                                          // カバー中断
                }

                yield return null;
            }

            //yield return new WaitForSeconds(0.01f); //These brief pauses are needed to smooth out transitions

            // 到着時に目的地点近傍へワープ補間（オーバーシュート防止）
            StartCoroutine(LerpToDestination(EmeraldComponent.m_NavMeshAgent.destination));

            yield return new WaitForSeconds(0.5f);                                          // ほんの少し待機

            bool AngleLimitMet = false;                                                     // 角度条件達成フラグ

            // 新カバーポジションからターゲット方向へ回頭
            float t = 0;
            while (t < 2.5f && EmeraldComponent.CombatTarget != null && !AngleLimitMet)
            {
                t += Time.deltaTime;

                Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) 
                                   - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);              // ターゲット方向へ回頭

                if (EmeraldComponent.CombatComponent.TargetAngle <= EmeraldComponent.MovementComponent.CombatAngleToTurn)
                {
                    AngleLimitMet = true;                                                  // 許容角度内に入った
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.3f);                                          // 遷移のため待機

            EmeraldComponent.MovementComponent.LockTurning = false;                         // 回頭ロック解除

            if (CoverStateCoroutine != null) StopCoroutine(CoverStateCoroutine);            // 既存ステート停止

            // カバータイプに応じて「隠れて覗く」or「隠れて攻撃」へ
            if (CurrentCoverNode.CoverType == CoverTypes.CrouchAndPeak)
            {
                CoverStateCoroutine = StartCoroutine(HideAndPeak());
            }
            else
            {
                CoverStateCoroutine = StartCoroutine(HideAndAttack());
            }
        }

        /// <summary>
        /// （日本語）到着時、目的地点へなめらかに寄せる補正（オーバーシュート対策）。
        /// </summary>
        IEnumerator LerpToDestination(Vector3 destination)
        {
            while (!EmeraldComponent.AnimationComponent.IsDead && Vector3.Distance(transform.position, destination) >= 0.15f)
            {
                EmeraldComponent.m_NavMeshAgent.Warp(Vector3.Lerp(transform.position, destination, 3 * Time.deltaTime)); // Warpで補正
                yield return null;
            }
        }

        /// <summary>
        /// （日本語）現在ノードで「隠れる ↔ 覗く（ピーク）」を一定回数繰り返す。
        /// ピーク中はターゲット視界が通れば攻撃可能。
        /// </summary>
        IEnumerator HideAndPeak ()
        {
            // ローカル関数：ピーク状態へ遷移
            void SetPeakState()
            {
                EmeraldComponent.AIAnimator.SetBool("Cover Active", false);                // カバーアニメ解除
                EmeraldComponent.MovementComponent.LockTurning = false;                    // 回頭ロック解除
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;          // 自由移動可
                CoverState = CoverStates.Peaking;                                          // 状態：ピーク
            }

            // ローカル関数：カバー状態へ遷移
            void SetCoverState()
            {
                EmeraldComponent.AIAnimator.SetBool("Cover Active", true);                 // カバーアニメON
                HasCover = true;                                                           // カバー中
                CoverState = CoverStates.Hiding;                                           // 状態：隠れる
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;           // 通常移動停止
                EmeraldComponent.MovementComponent.LockTurning = true;                     // 回頭ロック
                EmeraldComponent.DetectionComponent.TargetObstructed = true;               // ターゲットは遮蔽されている扱い
            }

            int PeakTimes = Random.Range(PeakTimesMin, PeakTimesMax + 1);                  // ピーク回数を乱数決定

            // 規定回数だけ「隠れる→ピーク」を繰り返す
            for (int i = 0; i < PeakTimes; i++)
            {
                // カバー状態を有効化
                SetCoverState();

                // 隠れる時間分待機
                float HideSeconds = Random.Range(HideSecondsMin, HideSecondsMax);
                yield return new WaitForSeconds(HideSeconds);

                // ピーク状態へ
                SetPeakState();

                yield return new WaitForSeconds(0.5f);                                     // 軽いクールタイム

                // 視界が遮蔽中かつノードが「視線位置補正」を許可している場合は、射線が通る位置へ少し移動
                if (CurrentCoverNode.GetLineOfSightPosition == YesOrNo.Yes && EmeraldComponent.DetectionComponent.TargetObstructed)
                {
                    Vector3 Destination = EmeraldAPI.Internal.FindUnobstructedPosition(EmeraldComponent); // 視界確保地点
                    yield return StartCoroutine(Moving(Destination));                                     // そこへ移動
                    yield return new WaitForSeconds(0.25f);                                                // ほんの少し待機
                }

                // ピーク（攻撃）時間分待機（この間に視界が通れば攻撃可能）
                float PeakSeconds = Random.Range(AttackSecondsMin, AttackSecondsMax);
                yield return new WaitForSeconds(PeakSeconds);

                // ループ継続：回数消化後に別ノード探索へ
            }

            // 同期ずらし用のランダムオフセット（複数AIが同時に切替しないように）
            float RandomOffset = Random.Range(0, 0.5f);
            yield return new WaitForSeconds(RandomOffset);

            HasCover = false;                                                              // カバー解除
            CoverState = CoverStates.Inactive;                                             // 次ノード探索へ
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance; // 停止距離戻し
        }

        /// <summary>
        /// （日本語）「一度だけ隠れてから攻撃」フローを実行。
        /// 隠れている間は動作を停止し、その後ピーク状態で攻撃可。のちに別ノード探索。
        /// </summary>
        IEnumerator HideAndAttack()
        {
            // ローカル関数：攻撃状態（ピーク移行前の準備）へ
            void SetAttackState()
            {
                EmeraldComponent.AIAnimator.SetBool("Cover Active", false);                // カバーアニメ解除
                EmeraldComponent.MovementComponent.LockTurning = false;                    // 回頭ロック解除
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;           // 通常移動は止める（射撃姿勢維持）
                CoverState = CoverStates.MovingToCover;                                    // 移動扱い（内部仕様）
                HasCover = false;                                                          // カバー解除
            }

            // ローカル関数：カバー状態へ
            void SetCoverState()
            {
                EmeraldComponent.AIAnimator.SetBool("Cover Active", true);                 // カバーアニメON
                HasCover = true;                                                           // カバー中
                CoverState = CoverStates.Hiding;                                           // 状態：隠れる
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;           // 通常移動停止
                EmeraldComponent.MovementComponent.LockTurning = true;                     // 回頭ロック
            }

            if (CurrentCoverNode != null && CurrentCoverNode.CoverType == CoverTypes.CrouchOnce)
            {
                // 隠れている間の状態セット
                SetCoverState();

                // 規定の隠れる時間を待つ
                float HideSeconds = Random.Range(HideSecondsMin, HideSecondsMax);
                yield return new WaitForSeconds(HideSeconds);
            }
            
            SetAttackState();                                                              // 攻撃状態へ切替

            // カバーアニメ遷移のための短い待機
            yield return new WaitForSeconds(0.5f);

            // 視界が遮蔽中かつノードが「視線位置補正」を許可している場合は、射線が通る位置へ少し移動
            if (CurrentCoverNode.GetLineOfSightPosition == YesOrNo.Yes && EmeraldComponent.DetectionComponent.TargetObstructed)
            {
                Vector3 Destination = EmeraldAPI.Internal.FindUnobstructedPosition(EmeraldComponent);
                yield return StartCoroutine(Moving(Destination));
                yield return new WaitForSeconds(0.25f);
            }
            
            HasCover = false;                                                              // カバー解除
            CoverState = CoverStates.Peaking;                                              // ピーク状態へ

            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;              // 自由移動可
            EmeraldComponent.MovementComponent.LockTurning = true;                         // 回頭ロック（射撃姿勢維持想定）
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance; // 停止距離戻し

            // ピーク（攻撃）時間分待機
            float PeakSeconds = Random.Range(AttackSecondsMin, AttackSecondsMax);
            yield return new WaitForSeconds(PeakSeconds);

            // 同期ずらし
            float RandomOffset = Random.Range(0, 0.5f);
            yield return new WaitForSeconds(RandomOffset);

            // 次のカバーノード探索のために非アクティブへ
            CoverState = CoverStates.Inactive;
        }

        IEnumerator Moving(Vector3 Destination)                                            // 内部：指定地点へ小移動（視界確保など）
        {
            // CombatTarget が null の瞬間があり得るため、その場合は終了
            if (EmeraldComponent.CombatTarget == null) yield break;

            EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;                        // 停止距離
            EmeraldComponent.CombatComponent.CancelAllCombatActions();                      // 戦闘行動を中断
            EmeraldComponent.MovementComponent.StopBackingUp();                             // バック移動停止
            EmeraldComponent.MovementComponent.LockTurning = true;                          // 回頭ロック
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;               // 一時的に移動許可
            EmeraldComponent.m_NavMeshAgent.ResetPath();                                    // 経路リセット
            EmeraldComponent.m_NavMeshAgent.destination = Destination;                      // 目的地設定
            yield return new WaitForSeconds(0.01f);                                         // 短い待機
            EmeraldComponent.MovementComponent.LockTurning = false;                         // 回頭ロック解除
            EmeraldComponent.MovementComponent.DefaultMovementPaused = true;                // 再び移動停止（到着処理向け）

            // 目的地が十分離れている場合のみ hasPath を待つ
            if (Vector3.Distance(Destination, transform.position) > 0.25f) 
                yield return new WaitUntil(() => EmeraldComponent.m_NavMeshAgent.hasPath);

            while (EmeraldComponent.m_NavMeshAgent.enabled && !EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.m_NavMeshAgent.remainingDistance >= 0.5f)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;                    // 停止距離維持
                Vector3 Direction = new Vector3(EmeraldComponent.m_NavMeshAgent.steeringTarget.x, 0, EmeraldComponent.m_NavMeshAgent.steeringTarget.z) 
                                  - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);               // 進行方向へ回頭
                yield return null;
            }

            // 最終位置へ寄せる補正
            StartCoroutine(LerpToDestination(EmeraldComponent.m_NavMeshAgent.destination));
            yield return new WaitForSeconds(0.33f);

            float t = 0;

            // 到着後、一定時間ターゲット方向へ回頭を継続
            while (t < 2.5f && EmeraldComponent.CombatTarget != null)
            {
                t += Time.deltaTime;

                Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) 
                                   - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);

                yield return null;
            }

            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;               // 自由移動可
            EmeraldComponent.MovementComponent.LockTurning = true;                          // 回頭ロック
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance; // 停止距離戻す
        }

        ///<summary>
        /// （日本語）条件に基づき最適なカバーノードを探索して返す。
        /// 条件：未占有／ターゲット前方110°以内／ターゲットから最小距離以上／ノード正面とターゲット方向の角度内／最大移動距離内。
        /// 最後に使用したノードは除外し、近傍上位からランダム選択。敵に占有されたノード近傍も除外する。
        ///</summary>
        public Transform FindCoverNode()
        {
            List<Transform> targets = EmeraldComponent.DetectionComponent.LineOfSightTargets.Select(collider => collider.transform).ToList(); // 視界内ターゲット一覧
            float maxAngleFromTargetForward = 110f; //Max angle from any target's forward direction（ターゲットの前方方向に対する最大許容角）

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, CoverSearchRadius, CoverNodeLayerMask); // 半径内のノード候補
            ValidCoverNodes.Clear(); // 候補リストをクリア
            OccupiedCoverNodes.Clear(); // 占有中ノードリストをクリア

            foreach (var hitCollider in hitColliders)
            {
                CoverNode coverPoint = hitCollider.transform.GetComponent<CoverNode>(); // CoverNode 取得

                if (!coverPoint) 
                    continue;

                // 条件1：未占有であること
                if (coverPoint.IsOccupied)
                {
                    OccupiedCoverNodes.Add(coverPoint); // 後の最終条件判定用に保持
                    continue;
                }              

                // 条件2：任意ターゲットの前方ベクトルから maxAngle 以内
                bool withinAngleOfAnyTarget = false;
                foreach (var target in targets)
                {
                    Vector3 directionToCover = (coverPoint.transform.position - target.position).normalized; // ターゲット→ノード方向
                    float angleToCover = Vector3.Angle(target.forward, directionToCover);                     // 前方との角度
                    if (angleToCover <= maxAngleFromTargetForward)
                    {
                        withinAngleOfAnyTarget = true;
                        break;
                    }
                }
                if (!withinAngleOfAnyTarget)
                    continue;

                // 条件3：すべてのターゲットから MinCoverDistance 以上離れている
                bool tooCloseToAnyTarget = false;
                foreach (var target in targets)
                {
                    float distanceCoverToTarget = Vector3.Distance(coverPoint.transform.position, target.position);
                    if (distanceCoverToTarget < MinCoverDistance)
                    {
                        tooCloseToAnyTarget = true;
                        break;
                    }
                }
                if (tooCloseToAnyTarget)
                    continue;

                // 条件4：ノード正面（forward）とターゲット方向の角度が CoverAngleLimit/2 以内
                bool exceededAngleLimitToAnyTarget = false;
                foreach (var target in targets)
                {
                    Vector3 directionFromCoverToTarget = (target.position - coverPoint.transform.position).normalized; // ノード→ターゲット方向
                    float angleBetweenCoverForwardAndTarget = Vector3.Angle(coverPoint.transform.forward, directionFromCoverToTarget); // 正面との角度
                    if (angleBetweenCoverForwardAndTarget > coverPoint.CoverAngleLimit / 2f)
                    {
                        exceededAngleLimitToAnyTarget = true;
                        break;
                    }
                }
                if (exceededAngleLimitToAnyTarget)
                    continue;

                // 条件5：自分からノードまでの距離が MaxTravelDistance 以下
                float distanceToTarget = Vector3.Distance(transform.position, coverPoint.transform.position);
                if (distanceToTarget > MaxTravelDistance)
                    continue;

                // すべての条件を満たしたので有効候補として追加
                float distanceToCover = Vector3.Distance(transform.position, coverPoint.transform.position);

                ValidCoverNodes.Add(new CoverPointData
                {
                    coverNode = coverPoint,
                    distanceToAgent = distanceToCover
                });
            }

            // 直前に使用したノードは除外（反復回避）
            if (CurrentCoverNode != null)
            {
                ValidCoverNodes.RemoveAll(cp => cp.coverNode == CurrentCoverNode);
            }

            // 最終条件：
            // 占有中ノード（Occupant が敵勢力）の近くにある候補を距離チェックで除外（MinCoverDistance 未満は無効）
            for (int i = ValidCoverNodes.Count - 1; i >= 0; i--)
            {
                Vector3 nodePosition = ValidCoverNodes[i].coverNode.transform.position;

                for (int j = 0; j < OccupiedCoverNodes.Count; j++)
                {
                    if (OccupiedCoverNodes[j].Occupant && EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, OccupiedCoverNodes[j].Occupant) == "Enemy")
                    {
                        float Distance = Vector3.Distance(OccupiedCoverNodes[j].transform.position, nodePosition);
                        if (Distance < MinCoverDistance)
                        {
                            Debug.DrawRay(ValidCoverNodes[i].coverNode.transform.position, transform.up * 6, Color.black, 5); // デバッグ表示
                            ValidCoverNodes.RemoveAt(i); // 候補から除外
                            break;
                        }
                    }
                }
            }

            if (ValidCoverNodes.Count > 0)
            {
                // 候補を自分からの距離でソート
                ValidCoverNodes.Sort((a, b) => a.distanceToAgent.CompareTo(b.distanceToAgent));

                // 近傍上位のみを考慮
                int count = Mathf.Min(maxConsideredCoverNodes, ValidCoverNodes.Count);
                List<CoverPointData> closestCoverPoints = ValidCoverNodes.GetRange(0, count);

                // 上位からランダムに1つ選択
                int index = Random.Range(0, closestCoverPoints.Count);
                CoverNode selectedCoverPoint = closestCoverPoints[index].coverNode;

                // デバッガ設定があれば、検出カバーノードを可視化（選択は緑、その他は黄）
                if (EmeraldComponent.DebuggerComponent != null && EmeraldComponent.DebuggerComponent.DrawDetectedCoverNodes == YesOrNo.Yes)
                {
                    for (int i = 0; i < closestCoverPoints.Count; i++)
                    {
                        if (closestCoverPoints[i].coverNode != selectedCoverPoint)
                            DrawCircle(closestCoverPoints[i].coverNode.transform.position - Vector3.up * 0.5f, 0.5f, Color.yellow, 3f);
                        else
                            DrawCircle(selectedCoverPoint.transform.position - Vector3.up * 0.5f, 0.5f, Color.green, 3f);
                    }
                }

                // 選択ノードを記録（以前の占有を解除してから占有）
                if (CurrentCoverNode != null) CurrentCoverNode.ClearOccupant();
                CurrentCoverNode = selectedCoverPoint;
                CurrentCoverNode.SetOccupant(transform);

                return selectedCoverPoint.transform; // トランスフォームを返す
            }
            else
            {
                return null; // 有効候補なし
            }
        }

        /// <summary>
        /// （日本語）検出した各カバーノード位置の周囲に円を描画するデバッグ表示。
        /// </summary>
        void DrawCircle(Vector3 center, float radius, Color color, float DrawTime)
        {
            Vector3 prevPos = center + new Vector3(radius, 0, 0);                         // 円周上の初期点
            for (int i = 0; i < 30; i++)
            {
                float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;                    // 次角度
                Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 新点
                Debug.DrawLine(prevPos, newPos, color, DrawTime);                          // 線で結ぶ
                prevPos = newPos;                                                          // 前回点更新
            }
        }
    }
}
