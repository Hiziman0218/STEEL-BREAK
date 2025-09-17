using System.Collections;                                   // コルーチン
using System.Collections.Generic;                           // List 等のコレクション
using UnityEngine;                                          // Unity 基本API
using UnityEngine.AI;                                       // NavMeshAgent 等
using EmeraldAI.Utility;                                    // Emerald のユーティリティ

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/movement-component")]
    // 【クラス概要】EmeraldMovement：
    //  Emerald AI の「移動」を司るコンポーネント。
    //  ・徘徊/目的地/ウェイポイント/追従/戦闘移動/逃走移動
    //  ・RootMotion と NavMesh 駆動の両対応
    //  ・地面への整列（傾斜追従）や回転制御、バック歩行、各種イベント通知を提供
    public class EmeraldMovement : MonoBehaviour
    {
        #region Movement Variables
        [Header("現在ターゲットへ経路到達可能か（NavMesh 上で到達可能判定）")]
        public bool CanReachTarget;

        [Header("追従対象（フォロワー）との距離（m）")]
        public float DistanceFromFollower;

        [Header("NavMesh 経路計算の更新間隔（秒）")]
        public float CalculatePathSeconds = 1f; //Controls how often the NavMesh Calculate Path is updated.

        [Header("NavMesh 経路計算の内部タイマー")]
        float CalculatePathTimer = 0f; //Controls how often the NavMesh Calculate Path is updated.

        [Header("組み込み移動（Wander/CombatMovement）を一時停止するか")]
        public bool DefaultMovementPaused; //A bool used to pause any built-in movement functions (Wander and CombatMovement). This can be used during custom actions or behaviors if needed.

        // 徘徊方式の列挙
        public enum WanderTypes { Dynamic = 0, Waypoints = 1, Stationary = 2, Destination = 3, Custom = 4 };

        [Header("徘徊方式（Dynamic/Waypoints/Stationary/Destination/Custom）")]
        public WanderTypes WanderType = WanderTypes.Dynamic;

        // ウェイポイント巡回方式
        public enum WaypointTypes { Loop = 0, Reverse = 1, Random = 2 };

        [Header("ウェイポイント巡回方式（Loop/Reverse/Random）")]
        public WaypointTypes WaypointType = WaypointTypes.Random;

        // 移動の駆動方式
        public enum MovementTypes { RootMotion, NavMeshDriven };

        [Header("移動の駆動方式（RootMotion か NavMeshDriven）")]
        public MovementTypes MovementType = MovementTypes.RootMotion;

        // 移動状態
        public enum MovementStates { Walk = 0, Run };

        [Header("開始時の移動状態（Walk/Run）")]
        public MovementStates StartingMovementState = MovementStates.Run;

        [Header("現在の移動状態（Walk/Run）")]
        public MovementStates CurrentMovementState = MovementStates.Walk;

        [Header("開始時にランダム回転を与えるか")]
        public YesOrNo UseRandomRotationOnStart = YesOrNo.No;

        [Header("開始時に地面へ整列処理を行うか（AlignAIOnStart）")]
        public YesOrNo AlignAIOnStart = YesOrNo.No;

        [Header("常時地面へ整列（傾斜追従）を有効にするか")]
        public YesOrNo AlignAIWithGround = YesOrNo.No;

        // 整列（傾斜追従）の品質
        public enum AlignmentQualities { Low = 0, Medium = 1, High = 2 };

        [Header("地面整列の品質（更新頻度の目安）")]
        public AlignmentQualities AlignmentQuality = AlignmentQualities.Medium;

        [Header("後退（バック）歩行速度（Animator のスピード係数）")]
        public float WalkBackwardsSpeed = 1;

        [Header("歩行速度（NavMeshAgent.speed などの基準）")]
        public float WalkSpeed = 2;

        [Header("走行速度（NavMeshAgent.speed などの基準）")]
        public float RunSpeed = 5;

        [Header("目的地に近いときに強制歩行へ切り替える距離（m）")]
        public float ForceWalkDistance = 2.5f;

        [Header("停止距離（NavMeshAgent.stoppingDistance）")]
        public float StoppingDistance = 2;

        [Header("非戦闘時の回頭開始角度（度）")]
        public int NonCombatAngleToTurn = 20;

        [Header("戦闘時の回頭開始角度（度）")]
        public int CombatAngleToTurn = 30;

        [Header("非戦闘時のその場回転スピード")]
        public int StationaryTurningSpeedNonCombat = 10;

        [Header("戦闘時のその場回転スピード")]
        public int StationaryTurningSpeedCombat = 10;

        [Header("非戦闘時の移動回転スピード")]
        public int MovingTurnSpeedNonCombat = 200;

        [Header("戦闘時の移動回転スピード")]
        public int MovingTurnSpeedCombat = 200;

        [Header("バック歩行時の回転スピード")]
        public int BackupTurningSpeed = 150;

        [Header("地面法線の XZ 成分の最大傾斜（%表現の係数）")]
        public int MaxNormalAngle = 20;

        [Header("許容最大斜面角（度）")]
        public int MaxSlopeLimit = 30;

        [Header("非戦闘時の地面整列スピード")]
        public int NonCombatAlignmentSpeed = 15;

        [Header("戦闘時の地面整列スピード")]
        public int CombatAlignmentSpeed = 25;

        [Header("方向値（Direction）への反映感度（Animator 用）")]
        public float MovementTurningSensitivity = 2f;

        [Header("徘徊での最小待機秒")]
        public int MinimumWaitTime = 3;

        [Header("徘徊での最大待機秒")]
        public int MaximumWaitTime = 6;

        [Header("地面整列用のレイヤーマスク")]
        public LayerMask AlignmentLayerMask = 1;

        [Header("現在回頭すべき角度（内部更新値）")]
        public int AngleToTurn;

        [Header("減速のダンピング時間（Animator 用）")]
        public float DecelerationDampTime = 0.15f;

        [Header("徘徊におけるウェイポイント待機タイマー")]
        public float WaypointTimer;

        [Header("ウェイポイントのインデックス")]
        public int WaypointIndex = 0;

        [Header("ウェイポイントの座標リスト")]
        public List<Vector3> WaypointsList = new List<Vector3>();

        [Header("目的地方向と前方のなす角（度）")]
        public float DestinationAdjustedAngle;

        [Header("目的地への方向ベクトル（水平面で使用）")]
        public Vector3 DestinationDirection;

        [Header("ターゲットへ即時回頭中フラグ（外部回転の抑制）")]
        public bool RotateTowardsTarget = false;

        [Header("Stationary 待機アニメの最小秒")]
        public int StationaryIdleSecondsMin = 3;

        [Header("Stationary 待機アニメの最大秒")]
        public int StationaryIdleSecondsMax = 6;

        [Header("バック歩行を発生させる乱数関連（予約変数）")]
        public int GeneratedBackupOdds;

        [Header("開始時の徘徊方式（バックアップ用）")]
        public int StartingWanderingType;

        [Header("開始地点へ戻る途中かどうか")]
        public bool ReturnToStationaryPosition;

        [Header("単一目的地（Destination モードで使用）")]
        public Vector3 SingleDestination;

        [Header("開始地点（徘徊の基準）")]
        public Vector3 StartingDestination;

        [Header("ウェイポイントオブジェクト参照（任意）")]
        public EmeraldWaypointObject m_WaypointObject;

        [Header("Dynamic 徘徊の半径（m）")]
        public int WanderRadius = 25;

        [Header("Dynamic 徘徊時に地面判定を行うレイヤーマスク")]
        public LayerMask DynamicWanderLayerMask = ~0;

        [Header("バック歩行の際の背面レイキャスト用レイヤーマスク")]
        public LayerMask BackupLayerMask = 1;

        [Header("回転ロック（外部制御時に使用）")]
        public bool LockTurning;

        [Header("開始地点へ戻る処理の進行中フラグ")]
        public bool ReturningToStartInProgress = false;

        [Header("NavMeshAgent が有効で移動可能か（内部フラグ）")]
        public bool AIAgentActive = false;

        // ↓ delegate/event は「メンバ変数」ではないため [Header] を付けず、用途をコメントで説明します
        public delegate void ReachedDestinationHandler();     // 目的地到達コールバック
        public event ReachedDestinationHandler OnReachedDestination;

        public delegate void ReachedWaypointHandler();        // ウェイポイント到達コールバック
        public event ReachedWaypointHandler OnReachedWaypoint;

        public delegate void GeneratedWaypointHandler();      // ウェイポイント生成時コールバック
        public event GeneratedWaypointHandler OnGeneratedWaypoint;

        public delegate void OnBackupHandler();               // バック歩行開始コールバック
        public event OnBackupHandler OnBackup;
        #endregion

        #region Editor Variables
        [Header("インスペクタ：設定セクションを隠す")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ：Wander 設定の折りたたみ")]
        public bool WanderFoldout = false;

        [Header("インスペクタ：Waypoints 設定の折りたたみ")]
        public bool WaypointsFoldout = false;

        [Header("インスペクタ：WaypointsList の折りたたみ")]
        public bool WaypointsListFoldout = false;

        [Header("インスペクタ：Backup 設定の折りたたみ")]
        public bool BackupFoldout = false;

        [Header("インスペクタ：Movement 設定の折りたたみ")]
        public bool MovementFoldout = false;

        [Header("インスペクタ：Alignment（整列）設定の折りたたみ")]
        public bool AlignmentFoldout = false;

        [Header("インスペクタ：Turn（回頭）設定の折りたたみ")]
        public bool TurnFoldout = false;
        #endregion

        #region Private Variables
        [Header("主要コンポーネント EmeraldSystem 参照（内部）")]
        EmeraldSystem EmeraldComponent;

        [Header("アニメーション制御コンポーネント参照（内部）")]
        EmeraldAnimation AnimationComponent;

        [Header("NavMeshAgent の参照（内部）")]
        NavMeshAgent m_NavMeshAgent;

        [Header("現在計算済みの NavMesh 経路（内部）")]
        NavMeshPath AIPath;

        [Header("Animator 参照（内部）")]
        Animator AIAnimator;

        [Header("Direction パラメータのダンピング時間（内部）")]
        float DirectionDampTime = 0.25f;

        [Header("地面整列のレイキャスト更新タイマー（内部）")]
        float RayCastUpdateTimer;

        [Header("Stationary 待機アニメ用タイマー（内部）")]
        float StationaryIdleTimer = 0;

        [Header("地面法線へ回転するための基礎回転（内部）")]
        Quaternion NormalRotation;

        [Header("ウェイポイント逆走処理中のフラグ（内部）")]
        bool WaypointReverseActive;

        [Header("目的地へ到達したか（内部）")]
        bool ReachedDestination;

        [Header("直前のウェイポイントインデックス（内部）")]
        int m_LastWaypointIndex = 0;

        [Header("バック歩行の遅延中フラグ（内部）")]
        bool BackupDelayActive;

        [Header("背面障害物までの距離（バック用レイキャスト結果）")]
        float BackupDistance;

        [Header("回転処理用コルーチン参照（内部）")]
        Coroutine m_RotateTowards;

        [Header("バック歩行の経過タイマー（内部）")]
        float BackingUpTimer;

        [Header("Stationary 待機アニメ用秒数（内部）")]
        int StationaryIdleSeconds;

        [Header("現在の地面法線（内部）")]
        Vector3 SurfaceNormal;

        [Header("表面との距離（未使用/内部）")]
        float SurfaceDistance;

        [Header("地面整列のレイ更新間隔（品質で変化・内部）")]
        float RayCastUpdateSeconds = 0.1f;

        [Header("徘徊での待機秒（ランダムで再設定・内部）")]
        float WaitTime = 5;

        [Header("アイドルアニメが起動済みか（内部）")]
        bool IdleActivated;

        [Header("ウェイポイント到達フラグ（内部）")]
        bool ReachedWaypoint;

        [Header("移動初期化が完了したか（内部）")]
        bool MovementInitialized;

        [Header("目標回転（ターゲット方向）クォータニオン（内部）")]
        Quaternion qTarget;

        [Header("地面整列用の回転クォータニオン（内部）")]
        Quaternion qGround;

        [Header("地面傾斜に対する補助回転（内部）")]
        Quaternion Slope;

        [Header("最終回転の一時格納（内部）")]
        Quaternion Final;

        [Header("バック歩行の方向ベクトル（内部）")]
        Vector3 BackupDirection;

        [Header("バック歩行の内部タイマー（発動抑制など）")]
        float BackupTimer;

        [Header("バック歩行遅延のコルーチン参照（内部）")]
        Coroutine BackupCoroutine;

        [Header("アクション（回避等）の一時方向（内部）")]
        Vector3 ActionDirection;
        #endregion

        void Start()
        {
            InitializeMovement(); //Initialize the EmeraldMovement script.
        }

        /// <summary>
        /// （日本語）移動設定を初期化します。
        /// ・主要コンポーネント取得、イベント購読、各しきい値のランダム初期化
        /// ・NavMeshAgent のセットアップ、開始時のランダム回転や地面整列
        /// </summary>
        public void InitializeMovement()
        {
            AIPath = new NavMeshPath();
            AIAnimator = GetComponent<Animator>();
            EmeraldComponent = GetComponent<EmeraldSystem>();
            AnimationComponent = GetComponent<EmeraldAnimation>();
            EmeraldComponent.CombatComponent.OnExitCombat += DefaultMovement; //Subscribe to the OnExitCombat event to set an AI's DefaultMovement state.
            StartingMovementState = CurrentMovementState;
            WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
            StationaryIdleSeconds = Random.Range(StationaryIdleSecondsMin, StationaryIdleSecondsMax + 1);
            StartingWanderingType = (int)WanderType;
            StartingDestination = transform.position;
            SetupNavMeshAgent();

            if (AlignmentQuality == AlignmentQualities.Low)
            {
                RayCastUpdateSeconds = 0.3f;
            }
            else if (AlignmentQuality == AlignmentQualities.Medium)
            {
                RayCastUpdateSeconds = 0.2f;
            }
            else if (AlignmentQuality == AlignmentQualities.High)
            {
                RayCastUpdateSeconds = 0.1f;
            }

            if (UseRandomRotationOnStart == YesOrNo.Yes)
            {
                transform.rotation = Quaternion.AngleAxis(Random.Range(5, 360), Vector3.up);
            }

            if (AlignAIOnStart == YesOrNo.Yes && AlignAIWithGround == YesOrNo.Yes)
            {
                AlignOnStart();
            }
        }

        /// <summary>
        /// （日本語）NavMeshAgent の初期化と値の設定を行います。
        /// ・Rigidbody の挙動調整、Agent の追加/取得、停止距離や回転更新の無効化
        /// ・WanderType に応じた初期目的地の設定
        /// </summary>
        public void SetupNavMeshAgent()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();

            if (GetComponent<Rigidbody>())
            {
                Rigidbody RigidbodyComp = GetComponent<Rigidbody>();
                RigidbodyComp.isKinematic = true;
                RigidbodyComp.useGravity = false;
            }

            if (m_NavMeshAgent == null)
            {
                gameObject.AddComponent<NavMeshAgent>();
                m_NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            AIPath = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(transform.position, AIPath);
            m_NavMeshAgent.stoppingDistance = StoppingDistance;
            m_NavMeshAgent.updateRotation = false;
            m_NavMeshAgent.updateUpAxis = false;
            m_NavMeshAgent.speed = 0;
            if (MovementType == MovementTypes.NavMeshDriven) m_NavMeshAgent.acceleration = 75;

            if (m_NavMeshAgent.enabled)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    m_NavMeshAgent.autoBraking = false;
                    ReachedDestination = true;
                    StartCoroutine(SetDelayedDestination(SingleDestination));
                    CheckPath(SingleDestination);
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    if (WaypointType != WaypointTypes.Random)
                    {
                        if (WaypointsList.Count > 0)
                        {
                            m_NavMeshAgent.stoppingDistance = 0.1f;
                            m_NavMeshAgent.autoBraking = false;
                            StartCoroutine(SetDelayedDestination(WaypointsList[WaypointIndex]));
                        }
                    }
                    else if (WaypointType == WaypointTypes.Random)
                    {
                        if (WaypointsList.Count > 0)
                        {
                            WaypointIndex = Random.Range(0, WaypointsList.Count);
                            StartCoroutine(SetDelayedDestination(WaypointsList[WaypointIndex]));
                            m_NavMeshAgent.autoBraking = false;
                        }
                    }

                    if (WaypointsList.Count == 0)
                    {
                        WanderType = WanderTypes.Stationary;
                        ReachedDestination = true;
                        m_NavMeshAgent.stoppingDistance = StoppingDistance;
                        MovementInitialized = true;
                    }
                }
                else if (WanderType == WanderTypes.Stationary || WanderType == WanderTypes.Dynamic || WanderType == WanderTypes.Custom)
                {
                    ReachedDestination = true;
                    m_NavMeshAgent.autoBraking = false;
                    StartCoroutine(SetDelayedDestination(StartingDestination));
                }
            }
        }

        IEnumerator SetDelayedDestination(Vector3 Destination)
        {
            m_NavMeshAgent.destination = Destination;
            yield return new WaitForSeconds(1);
            LockTurning = false;
            AIAnimator.SetBool("Idle Active", false);
            MovementInitialized = true;
        }

        /// <summary>
        /// （日本語）AI の経路の到達可否を確認します。到達不能なら WanderType に応じ再設定します。
        /// </summary>
        void CheckPath(Vector3 Destination)
        {
            NavMeshPath path = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(Destination, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Path is valid
            }
            else if (path.status == NavMeshPathStatus.PathPartial)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
            }
            else if (path.status == NavMeshPathStatus.PathInvalid)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
            }
            else
            {
                Debug.Log("Path Invalid");
            }
        }

        /// <summary>
        /// （日本語）NavMesh 駆動時の移動処理。
        /// ・角度と速度パラメータを更新し、停止/歩行/走行を切り替えます。
        /// </summary>
        void MoveAINavMesh()
        {
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for NavMesh movement.
            if (AIAgentActive && m_NavMeshAgent.isOnNavMesh && MovementInitialized)
            {
                AIAnimator.SetFloat("Direction", angle * MovementTurningSensitivity, DirectionDampTime, Time.deltaTime);

                if (m_NavMeshAgent.isStopped || AIAnimator.GetBool("Hit") || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance || CanIdle())
                {
                    AIAnimator.SetFloat("Speed", 0, 0.3f, Time.deltaTime);
                    m_NavMeshAgent.speed = 0;
                }
                else if (!m_NavMeshAgent.isStopped && !AIAnimator.GetBool("Hit") && m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance && CanMove())
                {
                    //Force walk movement if getting close to an AI's destination. This helps prevent the AI from running into its target as Root Motion with NavMesh needs a bit of time to stop.
                    if (m_NavMeshAgent.remainingDistance < (m_NavMeshAgent.stoppingDistance + ForceWalkDistance))
                    {
                        m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, WalkSpeed, Time.deltaTime * 2);
                        AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                    }
                    else
                    {
                        if (CurrentMovementState == MovementStates.Run)
                        {
                            m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, RunSpeed, Time.deltaTime * 2);
                            AIAnimator.SetFloat("Speed", 1, 0.3f, Time.deltaTime);
                        }
                        else if (CurrentMovementState == MovementStates.Walk)
                        {
                            m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, WalkSpeed, Time.deltaTime * 2);
                            AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）Root Motion 駆動時の移動処理。
        /// ・角度とアニメ速度を更新し、移動/停止を切り替えます。
        /// </summary>
        void MoveAIRootMotion()
        {
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for Root Motion
            if (AIAgentActive && m_NavMeshAgent.isOnNavMesh && MovementInitialized)
            {
                AIAnimator.SetFloat("Direction", angle * MovementTurningSensitivity, DirectionDampTime, Time.deltaTime);

                //Stops the AI during various conditions. Adding an offset to the remaining distance is needed because Root Motion needs a little extra room to stop.
                //With out this, the AI will get stuck between two states (moving and idling).
                var MovingTurnLimit = AIAnimator.GetFloat("Speed") <= 0.5f ? 80 : 120;

                if (m_NavMeshAgent.isStopped || RotateTowardsTarget || AIAnimator.GetBool("Hit") || DestinationAdjustedAngle > MovingTurnLimit || m_NavMeshAgent.remainingDistance + 0.2f <= m_NavMeshAgent.stoppingDistance || CanIdle())
                {
                    AIAnimator.SetFloat("Speed", 0, DecelerationDampTime, Time.deltaTime);
                    m_NavMeshAgent.speed = 0;
                }
                //To Move
                else if (!m_NavMeshAgent.isStopped && !AIAnimator.GetBool("Hit") && m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance && CanMove())
                {
                    ReachedDestination = false;
                    m_NavMeshAgent.speed = 0.025f;

                    //Force walk movement if getting close to an AI's destination. This helps prevent the AI from running into its target as Root Motion with NavMesh needs a bit of time to stop.
                    if (m_NavMeshAgent.remainingDistance < (m_NavMeshAgent.stoppingDistance + ForceWalkDistance))
                    {
                        AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                    }
                    else
                    {
                        if (CurrentMovementState == MovementStates.Run)
                        {
                            AIAnimator.SetFloat("Speed", 1f, 0.35f, Time.deltaTime);
                        }
                        else if (CurrentMovementState == MovementStates.Walk)
                        {
                            AIAnimator.SetFloat("Speed", 0.5f, 0.15f, Time.deltaTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）各アニメ状態から「待機可能か」を判定します。
        /// </summary>
        bool CanIdle()
        {
            return AnimationComponent.IsBackingUp || AnimationComponent.IsAttacking || AnimationComponent.IsGettingHit || AnimationComponent.IsEquipping || AnimationComponent.IsEmoting || AnimationComponent.IsBlocking ||
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsStunned || AnimationComponent.IsSwitchingWeapons;
        }

        /// <summary>
        /// （日本語）各アニメ状態から「移動可能か」を判定します。
        /// </summary>
        bool CanMove()
        {
            return !AnimationComponent.IsGettingHit && !AnimationComponent.IsAttacking && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsBackingUp && !AnimationComponent.IsEmoting && !AnimationComponent.IsEquipping &&
                !AnimationComponent.IsBlocking && !AnimationComponent.IsTurning && !AnimationComponent.IsStrafing && !AnimationComponent.IsDodging && !AnimationComponent.IsRecoiling && !AnimationComponent.IsStunned;
        }

        /// <summary>
        /// （日本語）各アニメ状態から「静止回転が可能か」を判定します。
        /// </summary>
        bool CanRotateStationary()
        {
            return !DefaultMovementPaused && !AnimationComponent.IsMoving && !AnimationComponent.IsBackingUp && !AnimationComponent.IsWarning && !AnimationComponent.IsBlocking && !AnimationComponent.IsGettingHit &&
                !AnimationComponent.IsRecoiling && !AnimationComponent.IsStunned && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping;
        }

        /// <summary>
        /// （日本語）各アニメ状態から「移動中回転が可能か」を判定します。
        /// </summary>
        bool CanRotateMoving()
        {
            return !DefaultMovementPaused && AnimationComponent.IsMoving && !AnimationComponent.IsTurning && !AnimationComponent.IsBackingUp && !AnimationComponent.IsAttacking && !AnimationComponent.IsIdling && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsStrafing;
        }

        /// <summary>
        /// （日本語）状態に応じた回転と地面整列を処理します。
        /// </summary>
        void RotateAI()
        {
            if (AnimationComponent.IsDead || RotateTowardsTarget || m_NavMeshAgent.pathPending) return;

            //Rotate while stationary -  There's certain instances where steeringTarget, destination, or CurrentTarget need to be used.
            if (!AnimationComponent.IsMoving && !AnimationComponent.IsDead && !RotateTowardsTarget || DestinationAdjustedAngle > 110)
            {
                if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CombatTarget)
                {
                    if (CanRotateStationary())
                    {
                        if (CanReachTarget && !EmeraldComponent.AnimationComponent.IsStrafing && !BackupDelayActive || EmeraldComponent.BehaviorsComponent.CurrentBehaviorType == EmeraldBehaviors.BehaviorTypes.Coward)
                        {
                            if (m_NavMeshAgent.remainingDistance > 1 && !EmeraldComponent.DetectionComponent.TargetObstructed)
                            {
                                Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                            else if (m_NavMeshAgent.remainingDistance > 1 && EmeraldComponent.DetectionComponent.TargetObstructed)
                            {
                                Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                            else
                            {
                                Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                        }
                        else
                        {
                            Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                            UpdateRotations(Direction);
                        }
                    }
                }
                else if (!EmeraldComponent.CombatComponent.CombatState && !AnimationComponent.IsGettingHit)
                {
                    //Once our AI has returned to its stantionary positon, adjust its position so it rotates to its original rotation.
                    if (ReturnToStationaryPosition && AIAgentActive && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
                    {
                        ReturnToStationaryPosition = false;
                    }

                    Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                    UpdateRotations(Direction);
                }

                EmeraldComponent.AnimationComponent.CalculateTurnAnimations();
            }

            //Rotate while moving
            if (CanRotateMoving() && DestinationAdjustedAngle < 110)
            {
                Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                UpdateRotations(Direction);
            }
            //Rotate while backing up
            if (AnimationComponent.IsBackingUp)
            {
                Vector3 Direction = (new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized * -1;
                UpdateRotations(Direction);
            }
        }

        /// <summary>
        /// （日本語）AI を指定位置へ即時に向けます（外部で回転を上書きする際に使用）。
        /// </summary>
        public void InstantlyRotateTowards(Vector3 Target)
        {
            Vector3 DestinationDirection = new Vector3(Target.x, 0, Target.z) - new Vector3(transform.position.x, 0, transform.position.z);
            qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
            transform.rotation = qGround * qTarget;
        }

        /// <summary>
        /// （日本語）与えられた方向ベクトルに基づき、回転と地面整列を更新します。
        /// </summary>
        public void UpdateRotations(Vector3 DirectionSource)
        {
            if (EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.transform.localScale == Vector3.one * 0.003f || transform.localScale == Vector3.one * 0.003f) return;

            RayCastUpdateTimer += Time.deltaTime;

            if (RayCastUpdateTimer >= RayCastUpdateSeconds)
            {
                GetSurfaceNormal();
            }

            DestinationAdjustedAngle = Mathf.Abs(Vector3.Angle(transform.forward, DirectionSource)); //Get the angle between the current target and the AI.
            DestinationDirection = DirectionSource;
            Final *= transform.rotation;

            float CurrentMovingTurnSpeed = EmeraldComponent.CombatComponent.CombatState ? MovingTurnSpeedCombat : MovingTurnSpeedNonCombat;
            int AlignmentSpeed = EmeraldComponent.CombatComponent.CombatState ? CombatAlignmentSpeed : NonCombatAlignmentSpeed;
            float StationaryTurningSpeed = EmeraldComponent.CombatComponent.CombatState ? StationaryTurningSpeedCombat : StationaryTurningSpeedNonCombat;
            AngleToTurn = EmeraldComponent.CombatComponent.CombatState ? CombatAngleToTurn : NonCombatAngleToTurn;

            if (DestinationDirection != Vector3.zero && !AnimationComponent.IsStrafing) qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed);
            else if (DestinationDirection != Vector3.zero && AnimationComponent.IsStrafing) qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed * 0.25f);

            if (!AnimationComponent.IsIdling && !AnimationComponent.IsBlocking && !AnimationComponent.IsStunned && !AnimationComponent.IsMoving && !AnimationComponent.IsBackingUp && !AnimationComponent.IsStrafing && !AnimationComponent.IsAttacking && !AnimationComponent.IsDodging && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsGettingHit && DestinationDirection != Vector3.zero)
                qTarget = Quaternion.Slerp(qTarget * Quaternion.Inverse(Final), Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * StationaryTurningSpeed);
            else if (AnimationComponent.IsStrafing && AIAnimator.GetBool("Strafe Active") && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * 200);
            else if (AnimationComponent.IsMoving && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * CurrentMovingTurnSpeed);
            else if (AnimationComponent.IsBackingUp && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * BackupTurningSpeed);
            else if (AnimationComponent.IsDodging && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget * Quaternion.Inverse(Final), Quaternion.LookRotation(ActionDirection, Vector3.up), Time.deltaTime * StationaryTurningSpeed);

            NormalRotation = Quaternion.FromToRotation(transform.up, SurfaceNormal) * transform.rotation;
            float AlignmentAngle = Quaternion.Angle(transform.rotation, NormalRotation);


            if (!AnimationComponent.IsIdling && !AnimationComponent.IsAttacking && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsGettingHit)
            {
                if (EmeraldComponent.CombatComponent.CombatState || AnimationComponent.IsTurning || m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, qGround * qTarget, Time.deltaTime * 10);
                }
            }
            else
            {
                if (EmeraldComponent.CombatComponent.CombatState || AnimationComponent.IsTurning || m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance)
                {
                    Slope = Quaternion.Slerp(Slope, Quaternion.FromToRotation(transform.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Slope * transform.rotation, Time.deltaTime * 10);
                }
            }
        }

        public void SetActionDirection()
        {
            ActionDirection = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        }

        /// <summary>
        /// （日本語）AI の中心から下向きにレイを飛ばして地面法線を取得します。AlignAIWithGround = No の場合は無効。
        /// </summary>
        Vector3 GetSurfaceNormal()
        {
            if (AlignAIWithGround == YesOrNo.No)
                return Vector3.zero;

            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), -Vector3.up, out HitDown, 2f, AlignmentLayerMask))
            {
                if (HitDown.transform != this.transform)
                {
                    float m_MaxNormalAngle = MaxNormalAngle * 0.01f;
                    SurfaceNormal = HitDown.normal;
                    SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -m_MaxNormalAngle, m_MaxNormalAngle);
                    SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -m_MaxNormalAngle, m_MaxNormalAngle);
                    RayCastUpdateTimer = 0;
                }
            }

            return SurfaceNormal;
        }

        /// <summary>
        /// （日本語）Start 時に一度だけ地面へ整列します。
        /// </summary>
        void AlignOnStart()
        {
            GetSurfaceNormal();
            transform.rotation = Quaternion.FromToRotation(transform.up, SurfaceNormal) * transform.rotation;
        }

        /// <summary>
        /// （日本語）Waypoint 徘徊の進行処理。到達判定・次の目的地設定・逆走処理・検証など。
        /// </summary>
        void NextWaypoint()
        {
            if (WaypointsList.Count == 0)
                return;

            if (WaypointType != WaypointTypes.Random && WaypointsList.Count > 1 && !WaypointReverseActive && !m_NavMeshAgent.pathPending)
            {
                float WaypointStoppingDistance = WaypointIndex < WaypointsList.Count ? (m_NavMeshAgent.stoppingDistance + 1.25f) : StoppingDistance;

                if (m_NavMeshAgent.remainingDistance <= WaypointStoppingDistance)
                {
                    WaypointIndex++;

                    if (WaypointIndex == WaypointsList.Count)
                    {
                        WaypointIndex = 0;
                        OnReachedWaypoint?.Invoke();

                        if (WaypointType == WaypointTypes.Reverse)
                        {
                            m_NavMeshAgent.destination = WaypointsList[WaypointsList.Count - 1];
                            WaypointsList.Reverse();
                            m_NavMeshAgent.stoppingDistance = 10;
                            WaypointReverseActive = true;
                            LockTurning = false;
                            Invoke(nameof(ReverseDelay), 4);
                        }
                    }

                    if (m_NavMeshAgent.enabled && !WaypointReverseActive)
                    {
                        m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                    }
                }
            }
            else if (WaypointType == WaypointTypes.Random && WaypointsList.Count > 1)
            {
                m_LastWaypointIndex = WaypointIndex;

                do
                {
                    WaypointIndex = Random.Range(0, WaypointsList.Count);
                } while (m_LastWaypointIndex == WaypointIndex);

                if (m_NavMeshAgent.enabled)
                {
                    m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                }
            }

            //Check that our AI's path is valid.
            CheckPath(m_NavMeshAgent.destination);
            OnGeneratedWaypoint?.Invoke();
        }

        /// <summary>
        /// （日本語）外部から OnReachedWaypoint を起動するためのユーティリティ。
        /// </summary>
        public void TriggerOnReachedWaypoint()
        {
            OnReachedWaypoint?.Invoke();
        }

        /// <summary>
        /// （日本語）外部から OnGeneratedWaypoint を起動するためのユーティリティ。
        /// </summary>
        public void TriggerOnGeneratedWaypoint()
        {
            OnGeneratedWaypoint?.Invoke();
        }

        /// <summary>
        /// （日本語）組み込みの徘徊処理。Editor で選択した WanderType に基づき進行します。
        /// Custom の場合はユーザー側で目的地生成を行えます。
        /// </summary>
        public void Wander()
        {
            if (DefaultMovementPaused) return;

            if (WanderType == WanderTypes.Dynamic && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && !m_NavMeshAgent.pathPending && MovementInitialized && !AnimationComponent.IsSwitchingWeapons)
            {
                if (WaypointTimer == 0)
                {
                    if (Vector3.Distance(m_NavMeshAgent.destination, StartingDestination) > 0.25f)
                    {
                        ReachedDestination = true;
                        OnReachedWaypoint?.Invoke();
                    }
                }

                WaypointTimer += Time.deltaTime;

                if (WaypointTimer >= WaitTime)
                {
                    AIAnimator.SetBool("Idle Active", false);
                    GenerateDynamicWaypoint();
                    WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                    WaypointTimer = 0;
                }
            }
            else if (WanderType == WanderTypes.Destination && m_NavMeshAgent.destination != SingleDestination && !ReachedDestination && !AnimationComponent.IsSwitchingWeapons)
            {
                if (m_NavMeshAgent.remainingDistance <= StoppingDistance && !m_NavMeshAgent.pathPending)
                {
                    ReachedDestination = true;
                    LockTurning = false;
                    OnReachedDestination?.Invoke();
                }
            }
            else if (WanderType == WanderTypes.Waypoints && !WaypointReverseActive && m_NavMeshAgent.destination != WaypointsList[WaypointIndex] && !AnimationComponent.IsSwitchingWeapons)
            {
                if (WaypointType == WaypointTypes.Random)
                {
                    if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && MovementInitialized)
                    {
                        if (!ReachedWaypoint)
                        {
                            WaypointTimer = 0;
                            ReachedWaypoint = true;
                            OnReachedWaypoint?.Invoke();
                        }

                        WaypointTimer += Time.deltaTime;

                        if (WaypointTimer >= WaitTime)
                        {
                            ReachedWaypoint = false;
                            LockTurning = false;
                            AIAnimator.SetBool("Idle Active", false);
                            WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                            NextWaypoint();
                        }
                    }
                }
                else if (WaypointType != WaypointTypes.Random)
                {
                    if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 1.5f && MovementInitialized)
                    {
                        NextWaypoint();
                    }
                }
            }
            else if (WanderType == WanderTypes.Stationary && !AnimationComponent.IsMoving && !AnimationComponent.IsSwitchingWeapons)
            {
                StationaryIdleTimer += Time.deltaTime;
                if (StationaryIdleTimer >= StationaryIdleSeconds)
                {
                    EmeraldComponent.AnimationComponent.PlayIdleAnimation();
                    StationaryIdleSeconds = Random.Range(StationaryIdleSecondsMin, StationaryIdleSecondsMax);
                    StationaryIdleTimer = 0;
                }
            }
            else if (WanderType == WanderTypes.Custom && !ReachedDestination && m_NavMeshAgent.remainingDistance <= StoppingDistance && MovementInitialized && !AnimationComponent.IsSwitchingWeapons)
            {
                ReachedDestination = true;
                LockTurning = false;
                OnReachedDestination?.Invoke();
            }

            //Play an idle sound if the AI is not moving and the Idle Seconds have been met. 
            if (!AnimationComponent.IsMoving && EmeraldComponent.SoundComponent != null)
            {
                EmeraldComponent.SoundComponent.IdleSoundsUpdate();
            }

            //If the AI gets moved, for whatever reason, disable Idle Active so it can move back to its current destination.
            if (m_NavMeshAgent.remainingDistance > StoppingDistance + 0.1f && AIAnimator.GetBool("Idle Active"))
            {
                AIAnimator.SetBool("Idle Active", false);
            }

            ClearReturnToStart();
        }

        /// <summary>
        /// （日本語）開始地点への帰還処理フラグを解除します（到達時）。
        /// </summary>
        void ClearReturnToStart()
        {
            if (ReturningToStartInProgress && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                ReturningToStartInProgress = false;
            }
        }

        /// <summary>
        /// （日本語）Dynamic 徘徊用に新しい目的地を生成します（NavMesh 上・斜面制限を考慮）。
        /// </summary>
        void GenerateDynamicWaypoint()
        {
            LockTurning = false;
            float RandomDegree = Random.Range(0, 360);
            float posX = StartingDestination.x + WanderRadius * Mathf.Cos(RandomDegree);
            float posZ = StartingDestination.z + WanderRadius * Mathf.Sin(RandomDegree);
            Vector3 GeneratedDestination = new Vector3(posX, transform.position.y, posZ);

            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(GeneratedDestination.x, GeneratedDestination.y + 10, GeneratedDestination.z), -transform.up, out HitDown, 12, DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (HitDown.transform != this.transform)
                {
                    if (Vector3.Angle(Vector3.up, HitDown.normal) <= MaxSlopeLimit)
                    {
                        GeneratedDestination = new Vector3(GeneratedDestination.x, HitDown.point.y, GeneratedDestination.z);
                        NavMeshHit DestinationHit;

                        if (NavMesh.SamplePosition(GeneratedDestination, out DestinationHit, 4, m_NavMeshAgent.areaMask))
                        {
                            AIAnimator.SetBool("Idle Active", false);
                            m_NavMeshAgent.SetDestination(DestinationHit.position);
                            OnGeneratedWaypoint?.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）コンパニオン/ペット AI が追従対象を追う処理。停止距離を維持しつつ走行します。
        /// </summary>
        public void FollowCompanionTarget(float FollowingStoppingDistance)
        {
            if (DefaultMovementPaused) return;

            DistanceFromFollower = Vector3.Distance(EmeraldComponent.TargetToFollow.position, transform.position);
            if (DistanceFromFollower > FollowingStoppingDistance && !AnimationComponent.IsEmoting)
            {
                m_NavMeshAgent.destination = EmeraldComponent.TargetToFollow.position;
            }

            m_NavMeshAgent.stoppingDistance = FollowingStoppingDistance;
            CurrentMovementState = MovementStates.Run;
        }

        /// <summary>
        /// （日本語）EmeraldAISystem から呼ばれる移動のカスタム Update。
        /// ・到達可否の更新、RootMotion/NavMesh の移動、回転処理の呼び出し
        /// </summary>
        public void MovementUpdate()
        {
            if (m_NavMeshAgent.isOnOffMeshLink)
            {
                m_NavMeshAgent.speed = 1f;
                return;
            }

            CanReachTarget = CanReachTargetInternal();
            AIAgentActive = m_NavMeshAgent.enabled;

            if (EmeraldComponent.AnimationComponent.BusyBetweenStates) return;

            //Calculates an AI's movement speed when using Root Motion
            if (MovementType == MovementTypes.RootMotion && !AnimationComponent.IsDead) MoveAIRootMotion();

            //Calculates an AI's movement speed when using NavMesh
            else if (MovementType == MovementTypes.NavMeshDriven && !AnimationComponent.IsDead) MoveAINavMesh();

            RotateAI(); //Handles all of the rotations, and alignment of an AI, depending on its current state.
        }

        /// <summary>
        /// （日本語）組み込みの戦闘移動処理。
        /// ・近すぎる場合のバック歩行、到達可能なら追跡、到達不能時は停止
        /// </summary>
        public void CombatMovement()
        {
            if (AnimationComponent.IsBackingUp && !CanReachTarget) StopBackingUp(); //Stop backing up if the target cannot be reached.
            if (CanReachTarget) BackupState(); //Handles all backup related movement.

            if (!AnimationComponent.IsBackingUp && !AnimationComponent.IsEquipping && !AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.CombatComponent.DeathDelayActive && CanReachTarget && EmeraldComponent.m_NavMeshAgent.isOnNavMesh)
            {
                if (EmeraldComponent.CoverComponent == null)
                {
                    m_NavMeshAgent.destination = EmeraldComponent.CombatTarget.position;
                }
                else if (EmeraldComponent.CoverComponent.CoverState == EmeraldCover.CoverStates.Inactive && EmeraldComponent.CombatComponent.DistanceFromTarget > EmeraldComponent.CombatComponent.CurrentAttackData.AttackDistance) //1.2.1 Update NEW
                {
                    m_NavMeshAgent.destination = EmeraldComponent.CombatTarget.position;
                }
            }

            if (!CanReachTarget && m_NavMeshAgent.isOnNavMesh)
            {
                //m_NavMeshAgent.destination = new Vector3(EmeraldComponent.CombatTarget.position.x, transform.position.y, EmeraldComponent.CombatTarget.position.z);
                m_NavMeshAgent.destination = transform.position;
            }
        }

        /// <summary>
        /// （日本語）組み込みの逃走移動処理。
        /// ・ターゲット反対方向へランダム性を含めた目的地を生成
        /// ・行き止まり時は逆側にも生成して脱出
        /// </summary>
        public void FleeMovement()
        {
            Vector3 direction = (EmeraldComponent.CombatTarget.position - transform.position).normalized;
            Vector3 GeneratedDestination = transform.position + -direction * 30 + Random.insideUnitSphere * 5f;
            GeneratedDestination.y = transform.position.y;
            m_NavMeshAgent.destination = GeneratedDestination;

            //This finds the closest edge between the generated destination and the AI. If the AI gets too close to an edge, which can only happen when they are somewhere
            //where they can no longer generate new flee points, such as a corner, generate a new destination behind the current target to get out of the orcer.
            NavMeshHit hit;
            if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
            {
                //Stuck, generate a new position behind the current target
                if (Vector3.Distance(m_NavMeshAgent.destination, hit.position) < 3)
                {
                    GeneratedDestination = transform.position + direction * 30 + Random.insideUnitSphere * 5f;
                    GeneratedDestination.y = transform.position.y;
                    EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
                }
                //Move to the currently generated flee point
                else
                {
                    EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
                }
            }
        }

        /// <summary>
        /// （日本語）ターゲット座標へ回転アニメで向けます。停止して回頭します（移動不可）。
        /// </summary>
        /// <param name="TargetPosition">回頭先の座標（オブジェクト/プレイヤー/AI など）</param>
        public void RotateTowardsPosition(Vector3 TargetPosition)
        {
            EmeraldAPI.Movement.RotateTowardsPosition(EmeraldComponent, TargetPosition);
        }

        /// <summary>
        /// （日本語）バック歩行に関する一連の状態更新。条件成立時に後退目的地を生成し移動します。
        /// </summary>
        void BackupState()
        {
            CalculateBackupState(); //Check the distance between the target and the player. If the player gets too close, attempt a backup.

            if (EmeraldComponent.AIAnimator.GetBool("Walk Backwards") && !BackupDelayActive)
            {
                if (CancelBackup()) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(1)); return; }
            }

            //Don't allow the backup movement to run if the AI is currently attacking, dead, or if this AI is teleporting.
            if (!AnimationComponent.IsBackingUp || AnimationComponent.IsDead) return;

            //Reset the attack trigger so an attack doesn't play while backing up
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");

            if (BackingUpTimer > 3 && !BackupDelayActive) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(2)); EmeraldCombatManager.GenerateClosestAttack(EmeraldComponent); return; }
            ;

            //Have a 2 second delay before backing up according to the AI's TooCloseDistance as it can trigger multiple times (which causes a few hiccups).
            if (!BackupDelayActive && !m_NavMeshAgent.pathPending && EmeraldComponent.CombatComponent.DistanceFromTarget > (EmeraldComponent.CombatComponent.TooCloseDistance - m_NavMeshAgent.radius)) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(2)); return; }
            ;

            //If the AI loses its target, or priority action happens, stop the AI's backup process.
            if (EmeraldComponent.CombatTarget == null || EmeraldComponent.CombatComponent.DeathDelayActive) { StopBackingUp(); return; }
            ;

            //Track the time while backing up as the AI will only backup for according to its BackingUpSeconds.
            BackingUpTimer += Time.deltaTime;

            //Generates a backup destination that's in the opposite direction of the AI's current target.
            if (EmeraldComponent.m_NavMeshAgent.hasPath)
                EmeraldComponent.m_NavMeshAgent.destination = GetBackupDestination();
        }

        IEnumerator BackupDelay(float DelayTime)
        {
            BackupDelayActive = true;
            yield return new WaitForSeconds(DelayTime);
            BackupDelayActive = false;
        }

        /// <summary>
        /// （日本語）バック歩行を停止し、関連設定をリセットします。
        /// </summary>
        public void StopBackingUp()
        {
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            BackingUpTimer = 0;
            BackupTimer = 0;
        }

        /// <summary>
        /// （日本語）バック歩行の発動判定を行います。近すぎ・角度・状態等をチェック。
        /// </summary>
        void CalculateBackupState()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                if (!BackupDelayActive)
                    BackupTimer += Time.deltaTime;

                if (CanBackup() && BackupTimer > 1f)
                {
                    BackupTimer = 0;

                    if (EmeraldComponent.CombatComponent.DistanceFromTarget <= EmeraldComponent.CombatComponent.TooCloseDistance && !DefaultMovementPaused)
                    {
                        if (DestinationAdjustedAngle <= 90 && !AIAnimator.GetBool("Blocking"))
                        {
                            //Do a quick raycast to see if behind the AI is clear before calling the backup state.
                            RaycastHit m_BackupHit = BackupRaycast();
                            if (m_BackupHit.collider != null && m_BackupHit.distance > StoppingDistance || m_BackupHit.collider == null)
                            {
                                OnBackup?.Invoke(); //Invoke the backup callback
                                EmeraldComponent.AIAnimator.SetBool("Walk Backwards", true);
                                EmeraldComponent.m_NavMeshAgent.destination = GetBackupDestination(); //Generates a backup destination that's in the opposite direction of the AI's current target.

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）現在ターゲットの反対方向へ 3m 程度のバック目的地を生成します。
        /// </summary>
        Vector3 GetBackupDestination()
        {
            Vector3 direction = (EmeraldComponent.CombatTarget.position - transform.position).normalized;
            Vector3 GeneratedDestination = ((direction * -1f) * 3f) + transform.position;
            GeneratedDestination.y = transform.position.y;
            return GeneratedDestination;
        }

        /// <summary>
        /// （日本語）背面の障害物を確認するためのレイキャスト結果を返します（バック歩行の安全確認）。
        /// </summary>
        RaycastHit BackupRaycast()
        {
            RaycastHit HitBehind;
            if (Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, -transform.forward * 8 - transform.up * 2f, out HitBehind, 10, BackupLayerMask))
            {
                if (HitBehind.collider != null && HitBehind.collider.gameObject != this.gameObject && !HitBehind.transform.IsChildOf(this.transform))
                {
                    BackupDistance = HitBehind.distance;
                }
            }
            return HitBehind;
        }

        /// <summary>
        /// （日本語）バック歩行が可能かの最終判定。状態・フラグ・アニメ・遮蔽等を総合的にチェック。
        /// </summary>
        bool CanBackup()
        {
            if (EmeraldComponent.DetectionComponent.TargetObstructed) return false;
            else if (EmeraldComponent.CombatComponent.DeathDelayActive || !MovementInitialized || !AIAgentActive) return false;
            else if (BackupDelayActive || AnimationComponent.IsSwitchingWeapons || AnimationComponent.IsMoving || AnimationComponent.IsEquipping || AnimationComponent.IsRecoiling || AnimationComponent.IsBackingUp ||
                AnimationComponent.IsAttacking || AnimationComponent.IsTurning || AnimationComponent.IsDodging || AnimationComponent.IsStrafing || AnimationComponent.IsGettingHit || AnimationComponent.IsBlocking) return false; //State Conditions
            else if (EmeraldComponent.AIAnimator.GetBool("Blocking") || EmeraldComponent.AIAnimator.GetBool("Hit")) return false;
            else return true; //If all conditions have passed, backup
        }

        /// <summary>
        /// （日本語）優先度の高いアクションが発生したとき等に、バック歩行をキャンセルするかの判定。
        /// </summary>
        bool CancelBackup()
        {
            return DefaultMovementPaused || transform.localScale == Vector3.one * 0.003f || AnimationComponent.IsStunned || EmeraldComponent.AIAnimator.GetBool("Stunned Active") || AnimationComponent.IsTurning || AnimationComponent.IsStrafing ||
                AnimationComponent.IsBlocking || EmeraldComponent.AIAnimator.GetBool("Hit") || AnimationComponent.IsGettingHit || AnimationComponent.IsRecoiling || AnimationComponent.IsDodging || AnimationComponent.IsEquipping || AnimationComponent.IsSwitchingWeapons;
        }

        /// <summary>
        /// （日本語）歩行フットステップ音を再生可能か（速度・状態から判定）。
        /// </summary>
        public bool CanPlayWalkFootstepSound()
        {
            return MovementType == MovementTypes.RootMotion && AIAnimator.GetFloat("Speed") > 0.05f && AIAnimator.GetFloat("Speed") <= 0.5f ||
                MovementType == MovementTypes.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > 0.05f && EmeraldComponent.m_NavMeshAgent.velocity.magnitude <= WalkSpeed + 0.25f ||
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsBackingUp || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsGettingHit;
        }

        /// <summary>
        /// （日本語）走行フットステップ音を再生可能か（速度・状態から判定）。
        /// </summary>
        public bool CanPlayRunFootstepSound()
        {
            return MovementType == MovementTypes.RootMotion && AIAnimator.GetFloat("Speed") > 0.5f ||
                MovementType == MovementTypes.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > WalkSpeed + 0.25f ||
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsBackingUp || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsGettingHit;
        }

        /// <summary>
        /// （日本語）AI の現在目的地を設定します。
        /// </summary>
        /// <param name="Destination">設定したい目的地座標。</param>
        public void SetDestination(Vector3 Destination)
        {
            m_NavMeshAgent.SetDestination(Destination);
        }

        /// <summary>
        /// （日本語）AI の現在パス/目的地をリセットします。
        /// </summary>
        public void ResetPath()
        {
            m_NavMeshAgent.ResetPath();
        }

        void ReverseDelay()
        {
            //If there's a target while in the process of this function, return as the stoppingDistance does not need to be set to 0.1.
            if (EmeraldComponent.CombatTarget != null)
            {
                WaypointReverseActive = false;
                return;
            }

            m_NavMeshAgent.stoppingDistance = 0.1f;
            WaypointReverseActive = false;
        }

        /// <summary>
        /// （日本語）移動設定をデフォルトへ戻します（OnExitCombat 等で呼ばれる）。
        /// </summary>
        public void DefaultMovement()
        {
            StopBackingUp(); //Reset the backing up settings and state.

            if (!ReturningToStartInProgress)
                CurrentMovementState = StartingMovementState;

            BackingUpTimer = 0;

            //Resets the AI's stopping distances.
            if (WanderType != WanderTypes.Waypoints) m_NavMeshAgent.stoppingDistance = StoppingDistance;
            else m_NavMeshAgent.stoppingDistance = 0.1f;

            //Return the AI to its starting destination to continue wandering based on it WanderType.
            ReturnToStartingDestination();
        }

        /// <summary>
        /// （日本語）開始地点へ戻る処理（WanderType ごとに再開位置を設定）。
        /// </summary>
        void ReturnToStartingDestination()
        {
            if (EmeraldComponent.TargetToFollow) return; //Don't set the AI's wandering position if it has a follow target

            if (WanderType == WanderTypes.Dynamic)
            {
                GenerateDynamicWaypoint();
            }
            else if (WanderType == WanderTypes.Stationary && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.destination = StartingDestination;
                ReturnToStationaryPosition = true;
            }
            else if (WanderType == WanderTypes.Waypoints && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                if (WaypointType != WaypointTypes.Random)
                    EmeraldComponent.m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                else
                    WaypointTimer = 1;
            }
            else if (WanderType == WanderTypes.Destination && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.destination = SingleDestination;
                ReturnToStationaryPosition = true;
            }
        }

        /// <summary>
        /// （日本語）現在ターゲットが NavMesh 的に到達可能かを計算して返します。
        /// </summary>
        public bool CanReachTargetInternal()
        {
            if (EmeraldComponent.CombatTarget == null || !m_NavMeshAgent.enabled || !m_NavMeshAgent.isOnNavMesh)
                return false;

            CalculatePathTimer += Time.deltaTime;

            if (CalculatePathTimer >= 0.5f)
            {
                m_NavMeshAgent.CalculatePath(EmeraldComponent.CombatTarget.position, AIPath);
                CalculatePathTimer = 0;
            }

            return AIPath.status == NavMeshPathStatus.PathComplete;
        }

        /// <summary>
        /// （日本語）開始地点へ戻る処理を有効化し、走行状態に切り替えます。
        /// </summary>
        public void EnableReturnToStart()
        {
            ReturningToStartInProgress = true;
            CurrentMovementState = MovementStates.Run;
        }

        /// <summary>
        /// （日本語）WanderType を変更します。
        /// </summary>
        public void ChangeWanderType(WanderTypes NewWanderType)
        {
            WanderType = NewWanderType;
        }

        /// <summary>
        /// （日本語）新しいカスタム目的地を設定する前提で、徘徊状態をリセットします。
        /// </summary>
        public void ResetWanderSettings()
        {
            LockTurning = false;
            StationaryIdleTimer = 0;
            WaypointTimer = 0;
            ReachedWaypoint = false;
            ReachedDestination = false;
        }
    }
}
