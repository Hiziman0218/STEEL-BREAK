using System.Collections;                              // コルーチン用（原文保持）
using System.Collections.Generic;                      // 汎用コレクション（原文保持）
using UnityEngine;                                     // Unity ランタイム API
using UnityEngine.Events;                              // UnityEvent（原文保持）
using System.Linq;                                     // LINQ（原文保持）

namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// （日本語）AI に「音を聞く」能力を与え、目視できないプレイヤー（ターゲット）を検知できるようにします。
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component")]
    // 【クラス概要】EmeraldSoundDetector：
    //  ・検知半径内の対象（LineOfSightTargets）の移動量や距離から「騒音レベル」を推定し、脅威度を段階的に上げ下げします。
    //  ・Unaware / Suspicious / Aware の各しきい値に達した際、リアクション（ReactionObject）や UnityEvent を発火します。
    //  ・AttractModifier（誘引）からの外部トリガにも対応し、移動・注視・退避などの反応を実行します。
    public class EmeraldSoundDetector : MonoBehaviour
    {
        #region Sound Detector Variables
        [Header("AttractModifier により検知した元オブジェクト（誘引元）")]
        public GameObject DetectedAttractModifier;

        [Header("現在の脅威レベル（未感知/疑念/警戒）")]
        public ThreatLevels CurrentThreatLevel = ThreatLevels.Unaware;

        [Header("現在の脅威量（0.0〜1.0）。各しきい値と比較して状態を決定")]
        public float CurrentThreatAmount;

        [Header("注視（Attention）の上昇率ベース。移動/距離要因と合算")]
        public float AttentionRate = 0.1f;

        [Header("検知対象の最小速度しきい値（これ以下は移動と見なさない）")]
        public float MinVelocityThreshold = 0.5f;

        [Header("対象速度が脅威量へ与える寄与率")]
        public float TargetVelocityFactor = 0.3f;

        [Header("距離が脅威量へ与える寄与率（近いほど増える）")]
        public float DistanceFactor = 0.1f;

        [Header("脅威量の減衰率（休止時に 0 へ近づける）")]
        public float AttentionFalloff = 0.05f;

        [Header("減衰開始までの遅延（秒）。移動検知直後の猶予を与える")]
        public float FalloffDealy = 3f;

        [Header("Unaware（未感知）へ戻すまでの待機時間（秒）")]
        public float DelayUnawareSeconds = 5f;

        [Header("AttractModifier を再度受け付けるまでのクールダウン（秒）")]
        public float AttractModifierCooldown = 5;

        [Header("移動ターゲットを検知中かどうかのフラグ")]
        public bool MovingTargetDetected;

        //Unaware
        [Header("Unaware（未感知）しきい値（0.0〜1.0）")]
        public float UnawareThreatLevel = 0.05f;
        [Header("Unaware 反応の一度きり発火フラグ（内部用）")]
        bool UnawareTriggered;
        [SerializeField]
        [Header("Unaware 到達時に実行する ReactionObject（任意）")]
        public ReactionObject UnawareReaction;
        [Header("Unaware 到達時に発火する UnityEvent（任意）")]
        public UnityEvent UnawareEvent;

        //Suspicious
        [Header("Suspicious（疑念）しきい値（0.0〜1.0）")]
        public float SuspiciousThreatLevel = 0.5f;
        [Header("Suspicious 反応の一度きり発火フラグ（内部用）")]
        bool SuspiciousTriggered;
        [SerializeField]
        [Header("Suspicious 到達時に実行する ReactionObject（任意）")]
        public ReactionObject SuspiciousReaction;
        [Header("Suspicious 到達時に発火する UnityEvent（任意）")]
        public UnityEvent SuspiciousEvent;

        //Aware
        [Header("Aware（警戒）しきい値（0.0〜1.0）")]
        public float AwareThreatLevel = 1f;
        [Header("Aware 反応の一度きり発火フラグ（内部用）")]
        bool AwareTriggered;
        [SerializeField]
        [Header("Aware 到達時に実行する ReactionObject（任意）")]
        public ReactionObject AwareReaction;
        [Header("Aware 到達時に発火する UnityEvent（任意）")]
        public UnityEvent AwareEvent;

        //Private variables
        [Header("Unaware 復帰までの遅延タイマ（内部用）")]
        float DelayUnawareTimer = 0;
        [Header("AI のコアコンポーネント参照（EmeraldSystem）")]
        EmeraldSystem EmeraldComponent;
        [Header("検知コンポーネント参照（EmeraldDetection）")]
        EmeraldDetection EmeraldDetection;
        [Header("移動コンポーネント参照（EmeraldMovement）")]
        EmeraldMovement EmeraldMovement;
        [Header("目的地到達フラグ（内部用）")]
        bool ArrivedAtDestination;
        [Header("現在実行中のリアクション用コルーチン")]
        Coroutine CurrentReactionCoroutine;
        [Header("移動計算用コルーチン")]
        Coroutine CalculateMovementCoroutine;
        [Header("向き補正用コルーチン")]
        Coroutine RotateTowardsCoroutine;
        [Header("最後に AttractModifier を受けてからの経過時間（秒）")]
        float TimeSinceLastAttractModifier;
        [Header("サウンド検知の有効/無効（内部制御用）")]
        bool SoundDetectorEnabled = true;
        [Header("減衰待機タイマ（Falloff 遅延用）")]
        float FalloffDelayTimer;

        [SerializeField]
        [Header("現在追跡中ターゲットのデータ一覧（位置/速度/距離/騒音レベル）")]
        public List<TargetDataClass> CurrentTargetData = new List<TargetDataClass>();

        [System.Serializable]
        public class TargetDataClass
        {
            [Header("対象 Transform")]
            public Transform Target;
            [Header("最後に観測した位置")]
            public Vector3 LastPosition;
            [Header("対象の速度（推定値）")]
            public float Velocty;
            [Header("AI からの距離")]
            public float Distance;
            [Header("騒音レベル（速度などから算出）")]
            public float NoiseLevel;

            public TargetDataClass(Transform m_Target, Vector3 m_LastPosition, float m_Velocty, float m_Distance, float m_NoiseLevel)
            {
                Target = m_Target;                  // 初期化：対象
                LastPosition = m_LastPosition;      // 初期化：直前位置
                Velocty = m_Velocty;                // 初期化：速度
                Distance = m_Distance;              // 初期化：距離
                NoiseLevel = m_NoiseLevel;          // 初期化：騒音レベル
            }
        }

        #endregion

        #region Editor Variables
        [Header("インスペクタ表示：全体セクションを非表示にするか")]
        public bool HideSettingsFoldout;
        [Header("インスペクタ表示：サウンド検知セクションの折りたたみ")]
        public bool SoundDetectorFoldout;
        [Header("インスペクタ表示：Unaware セクションの折りたたみ")]
        public bool UnawareFoldout;
        [Header("インスペクタ表示：Suspicious セクションの折りたたみ")]
        public bool SuspiciousFoldout;
        [Header("インスペクタ表示：Aware セクションの折りたたみ")]
        public bool AwareFoldout;
        #endregion

        void Start()
        {
            InitializeSoundDetector();              // 初期化
        }

        /// <summary>
        /// （日本語）サウンド検知コンポーネントを初期化します。
        /// </summary>
        void InitializeSoundDetector()
        {
            CurrentThreatAmount = 0;                // 脅威量をリセット
            TimeSinceLastAttractModifier = AttractModifierCooldown; // 誘引の初期待機
            EmeraldComponent = GetComponent<EmeraldSystem>();   // 主要コンポーネントの取得
            EmeraldMovement = GetComponent<EmeraldMovement>(); // 移動
            EmeraldDetection = GetComponent<EmeraldDetection>(); // 検知
        }

        /// <summary>
        /// （日本語）DisableSoundDetector を呼んだ後、検知を再度有効化します（既定では有効）。
        /// </summary>
        public void EnableSoundDetector()
        {
            SoundDetectorEnabled = true;            // 有効化フラグ
        }

        /// <summary>
        /// （日本語）サウンド検知機能を停止します。
        /// </summary>
        public void DisableSoundDetector()
        {
            SoundDetectorEnabled = false;           // 無効化フラグ
            CancelAll();                            // 実行中の処理をキャンセル
        }

        /// <summary>
        /// （日本語）視界内に入っているが未発見（LineOfSightTargets）なターゲットの音量をチェックします。
        /// </summary>
        void CheckForSounds()
        {
            // 戦闘中、または候補がいない場合は何もしない
            if (EmeraldComponent.CombatComponent.CombatState || EmeraldDetection.LineOfSightTargets.Count == 0)
            {
                MovingTargetDetected = false;
                return;
            }

            // LineOfSightTargets のうち、PlayerTag を持つものを CurrentTargetData に追加（未登録のみ）
            for (int i = 0; i < EmeraldDetection.LineOfSightTargets.Count; i++)
            {
                if (!CurrentTargetData.Exists(x => x.Target == EmeraldDetection.LineOfSightTargets[i].transform))
                {
                    if (!EmeraldDetection.LineOfSightTargets[i].gameObject.CompareTag(EmeraldDetection.PlayerTag)) continue; // プレイヤー以外は無視
                    float DistanceFromTarget = Vector3.Distance(transform.position, EmeraldDetection.LineOfSightTargets[i].transform.position);
                    CurrentTargetData.Add(new TargetDataClass(
                        EmeraldDetection.LineOfSightTargets[i].transform,
                        EmeraldDetection.LineOfSightTargets[i].transform.position,
                        MinVelocityThreshold,
                        DistanceFromTarget,
                        0));
                }
            }

            UpdateTargetData();                     // 追跡データを更新
        }

        private void Update()
        {
            if (EmeraldComponent.TargetToFollow) return; // 追従対象がいる場合は処理しない

            if (!EmeraldComponent.AnimationComponent.IsDead && SoundDetectorEnabled)
            {
                TimeSinceLastAttractModifier += Time.deltaTime; // Attract の経過時間

                if (EmeraldDetection.LineOfSightTargets.Count > 0)
                {
                    CheckForSounds();              // 音を確認
                    CheckEvents();                 // しきい値をチェックしてイベント判定
                }

                if (CurrentThreatAmount > 0 && EmeraldDetection.LineOfSightTargets.Count == 0)
                {
                    if (CurrentTargetData.Count > 0)
                    {
                        SuspiciousTriggered = false;
                        AwareTriggered = false;
                        CurrentTargetData.Clear(); // ターゲットデータ初期化
                    }

                    // 移動検知→非検知の移行に猶予を与えた後に減衰
                    FalloffDelayTimer += Time.deltaTime;
                    if (FalloffDelayTimer > FalloffDealy)
                        CurrentThreatAmount = Mathf.MoveTowards(CurrentThreatAmount, 0, AttentionFalloff * Time.deltaTime);

                    CurrentThreatAmount = Mathf.Clamp(CurrentThreatAmount, 0f, 1f);

                    if (CurrentThreatAmount < 0.001f)
                    {
                        InvokeReactionList(UnawareReaction); // Unaware リアクション
                        UnawareEvent.Invoke();               // Unaware イベント
                        DelayUnawareTimer = 0;
                        ClearThreats();                      // 脅威をクリア
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）各ターゲットの情報（速度/距離/騒音レベル等）を更新し、CurrentTargetData に保持します。
        /// </summary>
        void UpdateTargetData()
        {
            for (int i = 0; i < CurrentTargetData.Count; i++)
            {
                if (CurrentTargetData[i].Target != null)
                {
                    // 速度推定：前回位置との差分 / Δt
                    float DistanceFromTarget = Vector3.Distance(transform.position, CurrentTargetData[i].Target.position);
                    float TargetVelocity = (CurrentTargetData[i].Target.position - CurrentTargetData[i].LastPosition).magnitude / Time.deltaTime;
                    float MitigationValue = (1f - Mathf.Clamp01(DistanceFromTarget / EmeraldComponent.DetectionComponent.DetectionRadius));

                    CurrentTargetData[i].LastPosition = CurrentTargetData[i].Target.position;
                    CurrentTargetData[i].NoiseLevel = TargetVelocity; // 騒音レベル ≒ 速度
                    CalculateThreatLevel(TargetVelocity, MitigationValue); // 脅威量を更新

                    if (TargetVelocity > MinVelocityThreshold)
                    {
                        MovingTargetDetected = true;  // 移動を検知
                    }
                    else if (CurrentThreatAmount > 0 && TargetVelocity <= MinVelocityThreshold)
                    {
                        MovingTargetDetected = false; // 停止なら移動フラグを下ろす
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）移動の有無に応じて、脅威量（CurrentThreatAmount）を増減させます。
        /// </summary>
        void CalculateThreatLevel(float CurrentTargetVelocity, float MitigationValue)
        {
            if (MovingTargetDetected)
            {
                FalloffDelayTimer = 0; // 移動検知中は減衰待機タイマをリセット
                float attentionEffect = AttentionRate * Time.deltaTime;
                attentionEffect += (TargetVelocityFactor * CurrentTargetVelocity * 0.01f) * Time.deltaTime; // 速度寄与
                attentionEffect += (DistanceFactor * MitigationValue) * Time.deltaTime;                     // 距離寄与
                CurrentThreatAmount = Mathf.MoveTowards(CurrentThreatAmount, 1, attentionEffect);
            }
            else
            {
                // 非移動時：一定の猶予後に減衰
                FalloffDelayTimer += Time.deltaTime;
                if (FalloffDelayTimer > FalloffDealy)
                    CurrentThreatAmount = Mathf.MoveTowards(CurrentThreatAmount, 0, AttentionFalloff * Time.deltaTime);
            }

            CurrentThreatAmount = Mathf.Clamp(CurrentThreatAmount, 0f, 1f); // 0〜1 に制限
        }

        /// <summary>
        /// （日本語）サウンド検知機能とリアクションをすべてキャンセルします。
        /// </summary>
        void CancelAll()
        {
            if (CurrentReactionCoroutine != null) StopCoroutine(CurrentReactionCoroutine);
            if (CalculateMovementCoroutine != null) StopCoroutine(CalculateMovementCoroutine);
            if (RotateTowardsCoroutine != null) StopCoroutine(RotateTowardsCoroutine);
            EmeraldComponent.MovementComponent.RotateTowardsTarget = false;
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
            CurrentTargetData.Clear();
            EmeraldComponent.MovementComponent.ChangeWanderType((EmeraldMovement.WanderTypes)EmeraldMovement.StartingWanderingType);
        }

        void CheckEvents()
        {
            // 戦闘突入時はサウンド検知とリアクションを停止
            if (EmeraldComponent.CombatComponent.CombatState && CurrentTargetData.Count > 0)
            {
                CancelAll();
            }
            // すでにクリア済みなら何もしない
            else if (EmeraldComponent.CombatComponent.CombatState && CurrentTargetData.Count == 0)
            {
                return;
            }

            // いずれかの脅威状態から Unaware まで低下し、規定時間経過したら未感知へ戻す
            if (SuspiciousTriggered || AwareTriggered)
            {
                if (CurrentThreatAmount <= UnawareThreatLevel)
                {
                    if (!EmeraldComponent.CombatComponent.CombatState || EmeraldComponent.CombatComponent.CombatState && EmeraldDetection.TargetObstructed)
                    {
                        DelayUnawareTimer += Time.deltaTime;

                        if (DelayUnawareTimer >= DelayUnawareSeconds)
                        {
                            InvokeReactionList(UnawareReaction);
                            UnawareEvent.Invoke();
                            ClearThreats();
                            DelayUnawareTimer = 0;
                        }
                    }
                }
            }

            if (CurrentThreatAmount > UnawareThreatLevel)
            {
                DelayUnawareTimer = 0; // 脅威を検知したら未感知タイマをリセット
            }

            if (CurrentThreatAmount >= SuspiciousThreatLevel && CurrentThreatAmount < AwareThreatLevel && !SuspiciousTriggered)
            {
                // 戦闘モードではないときのみ反応/イベントを実行
                if (!EmeraldComponent.CombatComponent.CombatState)
                {
                    InvokeReactionList(SuspiciousReaction);
                    SuspiciousEvent.Invoke();
                }

                CurrentThreatLevel = ThreatLevels.Suspicious;
                SuspiciousTriggered = true;
            }
            else if (CurrentThreatAmount >= AwareThreatLevel && !AwareTriggered)
            {
                // 戦闘モードではないときのみ反応/イベントを実行
                if (!EmeraldComponent.CombatComponent.CombatState)
                {
                    InvokeReactionList(AwareReaction);
                    AwareEvent.Invoke();
                }

                CurrentThreatLevel = ThreatLevels.Aware;
                AwareTriggered = true;
            }
        }

        /// <summary>
        /// （日本語）脅威状態とデータを初期化します。LineOfSightTargets 外の AI のみリストから削除します。
        /// </summary>
        void ClearThreats()
        {
            CurrentThreatLevel = ThreatLevels.Unaware;
            CurrentThreatAmount = 0;
            SuspiciousTriggered = false;
            AwareTriggered = false;

            // 直近の反応が依存するデータ保持のため、視界外のもののみ除去
            for (int i = 0; i < CurrentTargetData.Count; i++)
            {
                if (!EmeraldDetection.LineOfSightTargets.Exists(x => x.transform == CurrentTargetData[i].Target))
                {
                    CurrentTargetData.RemoveAt(i);
                }
            }

            if (EmeraldDetection.LineOfSightTargets.Count == 0) CurrentTargetData.Clear();
        }

        /// <summary>
        /// （日本語）外部からリアクションを発火します（戦闘中やクールダウン中は無視）。
        /// AttractModifier から渡された場合はフラグによりログ内容が変わります。
        /// </summary>
        public void InvokeReactionList(ReactionObject SentReactionObject, bool SentByAttractModifier = false)
        {
            // 戦闘中、または AttractModifier のクールダウン未経過なら無視
            if (EmeraldComponent.CombatComponent.CombatState || TimeSinceLastAttractModifier < AttractModifierCooldown)
                return;

            if (SentReactionObject == null)
            {
                if (SentByAttractModifier)
                    Debug.Log("A sent Reaction Object to the AI " + gameObject.name + " by the " + DetectedAttractModifier.name + " Attract Modifier was null. Please ensure the Reaction Object slot on this Attract Modifier object is not null.");
                return;
            }

            // AI の徘徊タイプを開始時の設定へ戻す
            EmeraldComponent.MovementComponent.ChangeWanderType((EmeraldMovement.WanderTypes)EmeraldMovement.StartingWanderingType);

            if (CurrentReactionCoroutine != null) { StopCoroutine(CurrentReactionCoroutine); }
            CurrentReactionCoroutine = StartCoroutine(InvokeReactionListInternal(SentReactionObject, SentByAttractModifier));
        }

        /// <summary>
        /// （日本語）リアクションリストを逐次実行します（微小ランダム遅延で同時実行の揺らぎを付与）。
        /// </summary>
        IEnumerator InvokeReactionListInternal(ReactionObject SentReactionObject, bool SentByAttractModifier)
        {
            // 反応が同時に重ならないよう、わずかなランダム遅延を入れる
            float RandomDelay = Random.Range(0f, 0.15f);
            yield return new WaitForSeconds(RandomDelay);

            for (int i = 0; i < SentReactionObject.ReactionList.Count; i++)
            {
                // 各反応チェック前に検知情報を更新
                yield return new WaitForSeconds(0.001f);
                EmeraldComponent.DetectionComponent.UpdateAIDetection();
                yield return new WaitForSeconds(0.001f);
                CheckForSounds();
                yield return new WaitForSeconds(0.001f);

                // Reaction Type に応じて処理分岐（元のロジックをそのまま維持）
                if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.Delay)
                {
                    yield return new WaitForSeconds(SentReactionObject.ReactionList[i].FloatValue);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.DebugLogMessage)
                {
                    DebugLogMessage(SentReactionObject.ReactionList[i].StringValue);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.PlaySound)
                {
                    EmeraldComponent.SoundComponent.m_AudioSource.volume = SentReactionObject.ReactionList[i].FloatValue;
                    EmeraldComponent.SoundComponent.m_AudioSource.PlayOneShot(SentReactionObject.ReactionList[i].SoundRef);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.PlayEmoteAnimation)
                {
                    EmeraldComponent.AnimationComponent.PlayEmoteAnimation(SentReactionObject.ReactionList[i].IntValue1);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.LookAtLoudestTarget)
                {
                    LookAtLoudestTarget();
                    yield return new WaitForSeconds(SentReactionObject.ReactionList[i].IntValue1);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.SetLoudestTargetAsCombatTarget)
                {
                    SetLoudestTargetAsCombatTarget();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ReturnToStartingPosition)
                {
                    ReturnToDefaultPosition();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ExpandDetectionDistance)
                {
                    ExpandDetectionDistance(SentReactionObject.ReactionList[i].IntValue1);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.SetMovementState)
                {
                    SetMovementState(SentReactionObject.ReactionList[i].MovementState);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ResetDetectionDistance)
                {
                    ResetDetectionDistance();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ResetLookAtPosition)
                {
                    ResetLookAtPosition();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ResetAllToDefault)
                {
                    ResetAllToDefault();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ReturnToStartingPosition)
                {
                    ReturnToDefaultPosition();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.EnterCombatState)
                {
                    SetCombatState(true);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.ExitCombatState)
                {
                    SetCombatState(false);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.FleeFromLoudestTarget)
                {
                    FleeFromLoudestTarget();
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.AttractModifier)
                {
                    TimeSinceLastAttractModifier = 0;
                    AttractModifierInternal(
                        SentReactionObject.ReactionList[i].IntValue2,
                        SentReactionObject.ReactionList[i].IntValue1,
                        SentReactionObject.ReactionList[i].FloatValue,
                        SentReactionObject.ReactionList[i].ReactionType,
                        SentReactionObject.ReactionList[i].AttractModifierReaction);

                    if (SentReactionObject.ReactionList[i].AttractModifierReaction != AttractModifierReactionTypes.LookAtAttractSource)
                    {
                        yield return new WaitForSeconds(0.1f);
                        if (SentReactionObject.ReactionList[i].BoolValue == true)
                            yield return new WaitUntil(() => ArrivedAtDestination);
                    }
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.MoveToLoudestTarget)
                {
                    CalculateMovement(1, 0, SentReactionObject.ReactionList[i].FloatValue, SentReactionObject.ReactionList[i].ReactionType, SentReactionObject.ReactionList[i].AttractModifierReaction);
                    yield return new WaitForSeconds(0.1f);
                    if (SentReactionObject.ReactionList[i].BoolValue == true)
                        yield return new WaitUntil(() => ArrivedAtDestination);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.MoveAroundCurrentPosition)
                {
                    CalculateMovement(SentReactionObject.ReactionList[i].IntValue2, SentReactionObject.ReactionList[i].IntValue1, SentReactionObject.ReactionList[i].FloatValue, SentReactionObject.ReactionList[i].ReactionType, SentReactionObject.ReactionList[i].AttractModifierReaction);
                    yield return new WaitForSeconds(0.1f);
                    if (SentReactionObject.ReactionList[i].BoolValue == true)
                        yield return new WaitUntil(() => ArrivedAtDestination);
                }
                else if (SentReactionObject.ReactionList[i].ReactionType == ReactionTypes.MoveAroundLoudestTarget)
                {
                    CalculateMovement(SentReactionObject.ReactionList[i].IntValue2, SentReactionObject.ReactionList[i].IntValue1, SentReactionObject.ReactionList[i].FloatValue, SentReactionObject.ReactionList[i].ReactionType, SentReactionObject.ReactionList[i].AttractModifierReaction);
                    yield return new WaitForSeconds(0.1f);
                    if (SentReactionObject.ReactionList[i].BoolValue == true)
                        yield return new WaitUntil(() => ArrivedAtDestination);
                }
            }
        }

        /// <summary>
        /// （日本語）Unity コンソールへメッセージを出力（テスト用）。
        /// </summary>
        public void DebugLogMessage(string DebugMessage)
        {
            Debug.Log(DebugMessage);
        }

        /// <summary>
        /// （日本語）指定 Transform を基準に、半径内のランダム位置（NavMesh上）へ目的地を設定します。
        /// 半径 0 の場合は Transform の正確な位置を目的地にします。
        /// </summary>
        public void GenerateWaypoint(int Radius, Transform DestinationTransform)
        {
            if (DestinationTransform == null)
            {
                Debug.Log("Destination Transform is null. This reaction has been canceled.");
                return;
            }

            // 半径内のランダム地点
            if (Radius > 0)
            {
                Vector3 NewDestination = DestinationTransform.transform.position + new Vector3(Random.Range(-1, 2), 0, Random.Range(-1, 2)) * Radius;
                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -transform.up, out HitDown, 10, EmeraldMovement.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(NewDestination, out hit, 5f, EmeraldComponent.m_NavMeshAgent.areaMask))
                    {
                        EmeraldComponent.m_NavMeshAgent.SetDestination(NewDestination);
                    }
                }
            }
            // 目的地をそのまま使用
            else
            {
                EmeraldComponent.m_NavMeshAgent.SetDestination(DestinationTransform.transform.position);
            }
        }

        /// <summary>
        /// （日本語）現在の Look At Target をクリアします。
        /// </summary>
        public void ClearLookAtTarget()
        {
            if (CurrentTargetData.Exists(x => x.Target == EmeraldComponent.LookAtTarget))
            {
                EmeraldComponent.LookAtTarget = null;
            }
        }

        /// <summary>
        /// （日本語）最も騒音レベルが高いターゲットの方向へ向き直します。
        /// </summary>
        public void LookAtLoudestTarget()
        {
            if (CurrentTargetData.Count == 0)
                return;

            if (RotateTowardsCoroutine != null) StopCoroutine(RotateTowardsCoroutine);
            RotateTowardsCoroutine = StartCoroutine(RotateTowardsPosition(GetLoudestTarget().position)); // 最も騒がしい位置へ回頭
        }

        /// <summary>
        /// （日本語）最も騒音レベルが高いターゲットを戦闘ターゲットに設定します。
        /// </summary>
        public void SetLoudestTargetAsCombatTarget()
        {
            EmeraldAPI.Combat.SetCombatTarget(EmeraldComponent, GetLoudestTarget());
        }

        /// <summary>
        /// （日本語）AI を初期位置へ戻します（エディタ上の設定に基づく）。
        /// </summary>
        public void ReturnToDefaultPosition()
        {
            EmeraldComponent.m_NavMeshAgent.destination = EmeraldMovement.StartingDestination;
        }

        /// <summary>
        /// （日本語）検知距離を一時的に拡張します（直近で攻撃した対象の再検知などに有用）。
        /// </summary>
        public void ExpandDetectionDistance(int Distance)
        {
            if ((EmeraldDetection.StartingDetectionRadius + Distance) != EmeraldDetection.DetectionRadius)
                EmeraldDetection.DetectionRadius = EmeraldDetection.DetectionRadius + Distance;
        }

        /// <summary>
        /// （日本語）AI の移動状態を切り替えます。
        /// </summary>
        public void SetMovementState(EmeraldMovement.MovementStates MovementState)
        {
            EmeraldComponent.MovementComponent.CurrentMovementState = MovementState;
        }

        /// <summary>
        /// （日本語）検知距離を初期値へリセットします。
        /// </summary>
        void ResetDetectionDistance()
        {
            EmeraldDetection.DetectionRadius = EmeraldDetection.StartingDetectionRadius;
        }

        /// <summary>
        /// （日本語）Look At 位置を初期値へリセットします（AI が死亡している場合は無効）。
        /// </summary>
        void ResetLookAtPosition()
        {
            // AI が死亡していたら処理しない
            if (EmeraldComponent.AnimationComponent.IsDead)
                return;

            EmeraldComponent.LookAtTarget = null;
        }

        /// <summary>
        /// （日本語）LookAt、検知距離、移動状態、戦闘状態など、変更した値をすべて初期値へ戻します。
        /// </summary>
        void ResetAllToDefault()
        {
            EmeraldDetection.DetectionRadius = EmeraldDetection.StartingDetectionRadius;
            EmeraldComponent.LookAtTarget = null;
            EmeraldMovement.CurrentMovementState = EmeraldMovement.StartingMovementState;
            SetCombatState(false);
        }

        /// <summary>
        /// （日本語）AttractModifier（衝突/トリガ/開始/カスタム呼び出し）から渡されたリアクションを実行します。
        /// </summary>
        public void AttractModifierInternal(int TotalWaypoints, int Radius, float WaitTime, ReactionTypes ReactionType, AttractModifierReactionTypes AttractModifierReaction)
        {
            if (DetectedAttractModifier == null)
                return;

            if (AttractModifierReaction == AttractModifierReactionTypes.MoveToAttractSource)
            {
                CalculateMovement(1, 0, WaitTime, ReactionType, AttractModifierReaction);
            }
            else if (AttractModifierReaction == AttractModifierReactionTypes.MoveAroundAttractSource)
            {
                CalculateMovement(TotalWaypoints, Radius, WaitTime, ReactionType, AttractModifierReaction);
            }
            else if (AttractModifierReaction == AttractModifierReactionTypes.LookAtAttractSource)
            {
                EmeraldComponent.GetComponent<EmeraldSystem>().LookAtTarget = DetectedAttractModifier.transform;
            }
        }

        /// <summary>
        /// （日本語）戦闘状態の切り替え。true で戦闘アニメ、false で非戦闘アニメへ戻します。
        /// </summary>
        public void SetCombatState(bool State)
        {
            EmeraldComponent.m_NavMeshAgent.ResetPath();
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", State);
        }

        /// <summary>
        /// （日本語）反応種別に応じ、次の目的地を生成します。
        /// </summary>
        void GenerateWaypointInternal(int Radius, ReactionTypes ReactionType, AttractModifierReactionTypes AttractModifierReaction)
        {
            if (CurrentTargetData.Count > 0 && ReactionType == ReactionTypes.MoveAroundLoudestTarget || CurrentTargetData.Count > 0 && ReactionType == ReactionTypes.MoveToLoudestTarget)
            {
                GenerateWaypoint(Radius, GetLoudestTarget());
            }
            else if (ReactionType == ReactionTypes.AttractModifier)
            {
                GenerateWaypoint(Radius, DetectedAttractModifier.transform);
            }
            else
            {
                GenerateWaypoint(Radius, transform);
            }
        }

        /// <summary>
        /// （日本語）反応に基づく移動の一連の流れを計算し、コルーチンで実行します。
        /// </summary>
        void CalculateMovement(int TotalWaypoints, int Radius, float WaitTime, ReactionTypes ReactionType, AttractModifierReactionTypes AttractModifierReaction)
        {
            if (CalculateMovementCoroutine != null) StopCoroutine(CalculateMovementCoroutine);
            CalculateMovementCoroutine = StartCoroutine(CalculateMovementInternal(TotalWaypoints, Radius, WaitTime, ReactionType, AttractModifierReaction));
        }

        /// <summary>
        /// （日本語）移動実体：Stationary にして干渉を避け、Waypoints を順次生成→到達待機→次へ、を繰り返します。
        /// </summary>
        IEnumerator CalculateMovementInternal(int TotalWaypoints, int Radius, float WaitTime, ReactionTypes ReactionType, AttractModifierReactionTypes AttractModifierReaction)
        {
            EmeraldComponent.MovementComponent.ChangeWanderType(EmeraldMovement.WanderTypes.Stationary); // 既定徘徊の干渉を防ぐ
            ArrivedAtDestination = false; // 到達確認用
            EmeraldComponent.m_NavMeshAgent.ResetPath(); // 既存パスをリセット
            int CurrentWaypoints = 1; // 生成済みウェイポイント数
            float WaitTimer = 0; // 各地点での待機用タイマ

            GenerateWaypointInternal(Radius, ReactionType, AttractModifierReaction);
            yield return new WaitForSeconds(0.1f);
            ClearTurningValues();

            while (CurrentWaypoints <= TotalWaypoints)
            {
                // 戦闘突入で中断して既定の徘徊タイプへ戻す
                if (EmeraldComponent.CombatComponent.CombatState)
                {
                    EmeraldComponent.MovementComponent.ChangeWanderType((EmeraldMovement.WanderTypes)EmeraldMovement.StartingWanderingType);
                    yield break;
                }

                // 目的地に到達したら設定秒数だけ待機し、次の目的地を生成
                if (EmeraldComponent.m_NavMeshAgent.remainingDistance < EmeraldMovement.StoppingDistance && !EmeraldComponent.m_NavMeshAgent.pathPending)
                {
                    WaitTimer += Time.deltaTime;

                    if (WaitTimer > WaitTime)
                    {
                        GenerateWaypointInternal(Radius, ReactionType, AttractModifierReaction);
                        ClearTurningValues();
                        WaitTimer = 0;

                        if (CurrentWaypoints == TotalWaypoints)
                        {
                            EmeraldComponent.m_NavMeshAgent.ResetPath();
                            break;
                        }
                        else
                        {
                            CurrentWaypoints++;
                        }
                    }
                }

                yield return null;
            }

            yield return new WaitForSeconds(WaitTime);
            ArrivedAtDestination = true;
            EmeraldMovement.WaypointTimer = 0;
            // 既定の徘徊タイプへ戻す
            EmeraldComponent.MovementComponent.ChangeWanderType((EmeraldMovement.WanderTypes)EmeraldMovement.StartingWanderingType);
        }

        /// <summary>
        /// （日本語）与えられたワールド座標へ回頭するコルーチン。
        /// </summary>
        IEnumerator RotateTowardsPosition(Vector3 TargetPosition)
        {
            if (EmeraldComponent.MovementComponent.RotateTowardsTarget) yield break; // すでに回頭中なら抜ける

            EmeraldComponent.MovementComponent.RotateTowardsTarget = true; // 回頭中は一部機能を停止
            EmeraldComponent.MovementComponent.LockTurning = false;        // ターンアニメ間でのスタック防止
            EmeraldComponent.m_NavMeshAgent.isStopped = true;              // NavMesh を一時停止
            yield return new WaitForSeconds(0.1f);                         // 変更が反映されるまで待つ

            while (!EmeraldComponent.MovementComponent.LockTurning)
            {
                Vector3 Direction = new Vector3(TargetPosition.x, 0, TargetPosition.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);
                EmeraldComponent.AnimationComponent.CalculateTurnAnimations(true);
                yield return null;
            }

            EmeraldComponent.MovementComponent.RotateTowardsTarget = false;
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// （日本語）最も騒音の大きいターゲットから逃走目標を設定します（Cautious Coward AI のみ）。
        /// </summary>
        void FleeFromLoudestTarget()
        {
            EmeraldComponent.DetectionComponent.SetDetectedTarget(GetLoudestTarget());
        }

        /// <summary>
        /// （日本語）最も騒音レベルの高いターゲットを返します。対象がいなければ null。
        /// </summary>
        Transform GetLoudestTarget()
        {
            // ターゲットがいない場合は null
            if (CurrentTargetData.Count == 0)
                return null;

            float MaxNoiseLevel = CurrentTargetData.Max(x => x.NoiseLevel); // 最大騒音レベル
            Transform LoudestTarget = CurrentTargetData.Find(x => x.NoiseLevel == MaxNoiseLevel).Target; // 最大値の対象を取得
            return LoudestTarget;
        }

        /// <summary>
        /// （日本語）回頭に関する内部状態をクリアします。
        /// </summary>
        void ClearTurningValues()
        {
            EmeraldComponent.AnimationComponent.IsTurning = false;
            EmeraldComponent.MovementComponent.LockTurning = false;
        }
    }
}
