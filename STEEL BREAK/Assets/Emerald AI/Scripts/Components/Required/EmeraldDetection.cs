using System.Collections;                                   // コルーチン
using System.Collections.Generic;                           // ジェネリックコレクション
using UnityEngine;                                          // Unity API
using System.Linq;                                          // LINQ
using static UnityEngine.GraphicsBuffer;                    // GraphicsBuffer の static 参照（元コード保持）

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component")]
    // 【クラス概要】EmeraldDetection：
    //  Emerald AI の「検知（視界・レイキャスト・視野角・距離）」と「派閥（Faction）」に関する処理を管理。
    //  ・OverlapSphere によるターゲット検出、視線判定、遮蔽（Obstruction）チェック
    //  ・敵/味方/プレイヤーの振り分け、検知状態（Alert/Unaware）
    //  ・ターゲット選択（Closest/Random/FirstDetected）およびフォロワー/味方リストの管理
    public class EmeraldDetection : MonoBehaviour, IFaction
    {
        #region Detection Variables
        [Header("無視するコライダーのリスト（自己や特定オブジェクトを除外）")]
        public List<Collider> IgnoredColliders = new List<Collider>();

        [Header("LBD（部位ダメージ）用のレイヤーマスク（静的共有）")]
        public static LayerMask LBDLayers;

        [Header("現在の遮蔽物（レイがヒットしたトランスフォーム）")]
        public Transform CurrentObstruction;

        [Header("視線の起点となる頭部の Transform")]
        public Transform HeadTransform;

        [Header("検知の更新頻度（秒）")]
        public float DetectionFrequency = 1;

        [Header("検知対象のレイヤーマスク")]
        public LayerMask DetectionLayerMask = 3;

        [Header("遮蔽検知に使用するレイヤーマスク")]
        public LayerMask ObstructionDetectionLayerMask = 4;

        [Header("内部用：遮蔽検知レイヤーマスク（LBDレイヤーを加算する）")]
        LayerMask InternalObstructionLayerMask = 4;

        [Header("プレイヤー判定に用いるタグ名")]
        public string PlayerTag = "Player";

        [Header("遮蔽レイキャストの更新頻度（秒）")]
        public float ObstructionDetectionFrequency = 0.1f;

        [Header("遮蔽レイキャストの更新タイマー（内部）")]
        public float ObstructionDetectionUpdateTimer;

        [Header("遮蔽が継続したとみなすまでの秒数（アクション発火閾値）")]
        public float ObstructionSeconds = 1.5f;

        [Header("初期の検知半径（バックアップ用）")]
        public int StartingDetectionRadius;

        [Header("現在の検知半径（m）")]
        public int DetectionRadius = 18;

        [Header("初期の追跡距離（未使用の保持値）")]
        public int StartingChaseDistance;

        [Header("視野角（度）。非戦闘時はこの半分を使用、戦闘時は360°")]
        public int FieldOfViewAngle = 270;

        [Header("初期の視野角（バックアップ用）")]
        public int StartingFieldOfViewAngle;

        // 検知状態の列挙（Alert=警戒、Unaware=未検知）
        public enum DetectionStates { Alert = 0, Unaware = 1 };

        [Header("現在の検知状態（Alert/Unaware）")]
        public DetectionStates CurrentDetectionState = DetectionStates.Unaware;

        [Header("ターゲット選択方式（Closest/Random/FirstDetected）")]
        public PickTargetTypes PickTargetType = PickTargetTypes.Closest;

        [Header("ターゲットが遮蔽されているか")]
        public bool TargetObstructed = false;

        // 遮蔽の種類（AI による遮り / その他の遮り / なし）
        public enum ObstructedTypes { AI, Other, None };

        [Header("現在の遮蔽タイプ（AI/Other/None）")]
        public ObstructedTypes ObstructionType = ObstructedTypes.None;

        [Header("視線上にある候補ターゲットのコライダー一覧")]
        public List<Collider> LineOfSightTargets = new List<Collider>();

        [Header("このAIを現在フォローしている Transform 一覧")]
        public List<Transform> CurrentFollowers = new List<Transform>();

        [Header("近くの味方 AI の一覧")]
        public List<EmeraldSystem> NearbyAllies = new List<EmeraldSystem>();

        [Header("低体力の味方 AI の一覧（将来拡張用）")]
        public List<EmeraldSystem> LowHealthAllies = new List<EmeraldSystem>();

        // ↓ delegate/event はインスペクタに表示されないため Header は付けず、用途をコメントで説明
        public delegate void OnDetectTargetHandler();        // 検知更新時（DetectionUpdate の周期で発火）
        public event OnDetectTargetHandler OnDetectionUpdate;

        public delegate void OnEnemyTargetDetectedHandler(); // 敵ターゲット確定時に発火
        public event OnEnemyTargetDetectedHandler OnEnemyTargetDetected;

        public delegate void OnNullTargetHandler();          // 現在ターゲットが null になったときに発火
        public event OnNullTargetHandler OnNullTarget;

        public delegate void OnPlayerDetectedHandler();      // プレイヤー検知時に発火
        public event OnPlayerDetectedHandler OnPlayerDetected;

        public delegate void OnTargetObstructedHandler();    // 遮蔽が一定時間続いたときに発火
        public event OnTargetObstructedHandler OnTargetObstructed;

        [Header("検知から除外するターゲットの共有リスト（静的）")]
        public static List<Transform> IgnoredTargetsList = new List<Transform>();
        #endregion

        #region Faction Variables
        [Header("このAIの所属派閥ID")]
        [SerializeField]
        public int CurrentFaction;

        [Header("派閥データ（Resources から読み込み・静的共有）")]
        public static EmeraldFactionData FactionData;

        [Header("派閥名の一覧（静的・エディタ用）")]
        [SerializeField]
        public static List<string> StringFactionList = new List<string>();

        [Header("派閥関係（0=敵/1=中立/2=友好）の数値リスト")]
        public List<int> FactionRelations = new List<int>();

        [Header("派閥関係の詳細（FactionClass のリスト）")]
        [SerializeField]
        public List<FactionClass> FactionRelationsList = new List<FactionClass>();

        [Header("AI 側の派閥IDリスト（自AIが認識する派閥ID）")]
        [SerializeField]
        public List<int> AIFactionsList = new List<int>();
        #endregion

        #region Editor Specific Variables
        [Header("インスペクタ：設定を隠すフラグ")]
        public bool HideSettingsFoldout;

        [Header("インスペクタ：検知設定の折りたたみ")]
        public bool DetectionFoldout;

        [Header("インスペクタ：タグ設定の折りたたみ")]
        public bool TagFoldout;

        [Header("インスペクタ：派閥設定の折りたたみ")]
        public bool FactionFoldout;
        #endregion

        #region Private Variables
        [Header("検知更新タイマー（内部）")]
        float DetectionTimer;

        [Header("ターゲット方向ベクトル（内部計算用）")]
        Vector3 TargetDirection;

        [Header("遮蔽継続時間の計測タイマー（内部）")]
        float ObstructionTimer;

        [Header("遮蔽を一時的にバイパスして候補化したターゲット")]
        List<Transform> BypassedTargets = new List<Transform>();

        [Header("主要コンポーネント EmeraldSystem 参照")]
        EmeraldSystem EmeraldComponent;
        #endregion

        void Start()
        {
            InitializeDetection();                          // 検知機能の初期化
            Invoke(nameof(InitializeLayers), 0.1f);         // レイヤーの内部マスクを組み立て（LBD レイヤーを加算）
        }

        /// <summary>
        /// （日本語）ObstructionDetectionLayerMask の内容をコピーし、AI の内部コライダー（LBD）レイヤーも含めます。
        /// これにより自分自身のコライダーで誤検知しないようにします。
        /// </summary>
        void InitializeLayers()
        {
            InternalObstructionLayerMask = ObstructionDetectionLayerMask; // 基本はユーザー指定

            for (int i = 0; i < 32; i++)                                  // 0〜31 のレイヤーを走査
            {
                if (LBDLayers == (LBDLayers | (1 << i)))                  // LBDLayers に含まれるビットなら
                {
                    InternalObstructionLayerMask |= (1 << i);             // 内部マスクに加算
                }
            }
        }

        /// <summary>
        /// （日本語）検知に関する初期設定を行います。
        /// </summary>
        void InitializeDetection()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // 主要参照の取得

            // 戦闘終了時・死亡時・ターゲット消失時などのイベント購読
            EmeraldComponent.CombatComponent.OnExitCombat += ReturnToDefaultState; // OnExitCombat 時に既定状態へ戻す
            EmeraldComponent.HealthComponent.OnDeath += ClearTargetToFollow;       // 死亡時にフォローターゲットを解除
            EmeraldComponent.HealthComponent.OnDeath += NearbyAllyDeathHandler;    // 死亡時に味方リストから自分を消す
            OnNullTarget += NullNonCombatTarget;                                   // 非戦闘時のターゲット消失処理

            if (FactionData == null) FactionData = Resources.Load("Faction Data") as EmeraldFactionData; // 派閥データを読み込み
            if (EmeraldComponent.LBDComponent == null) Utility.EmeraldCombatManager.DisableRagdoll(EmeraldComponent); // LBD が無ければラグドール無効化

            StartingDetectionRadius = DetectionRadius; // 初期値バックアップ
            TargetObstructed = true;                   // 初期は遮蔽ありとする（安全側）
            StartingFieldOfViewAngle = FieldOfViewAngle;
            StartingDetectionRadius = DetectionRadius;

            // 頭部Transform未設定時は一時的に作成（動作を継続させるため）
            if (HeadTransform == null)
            {
                Transform TempHeadTransform = new GameObject("AI Head Transform").transform; // ※名前は変更せず
                TempHeadTransform.SetParent(transform);
                TempHeadTransform.localPosition = new Vector3(0, 1, 0);
                HeadTransform = TempHeadTransform;
            }

            SetupFactions();                                  // 派閥データのセットアップ
            Invoke(nameof(CheckFactionRelations), 0.1f);      // 自派閥=敵 になっていないか確認
        }

        /// <summary>
        /// （日本語）初期化時に、自分の派閥が「敵対」と設定されていないかを確認し、問題があれば通知します。
        /// </summary>
        void CheckFactionRelations()
        {
            if (AIFactionsList.Contains(CurrentFaction) && FactionRelations[AIFactionsList.IndexOf(CurrentFaction)] == 0)
            {
                Debug.LogError("The AI '" + gameObject.name + "' contains an Enemy Faction Relation of its own Faction '" + GetTargetFactionName(transform) +
                    "'. Please remove the faction from the AI Faction Relation List (within the AI's Detection Component) or change it to Friendly to avoid incorrect target detection.");
            }
        }

        /// <summary>
        /// （日本語）ランタイム用に派閥リストを整備します（FactionRelationsList → 数値/ID リストへ反映）。
        /// </summary>
        public void SetupFactions()
        {
            FactionRelations.Clear();
            for (int i = 0; i < FactionRelationsList.Count; i++)
            {
                AIFactionsList.Add(FactionRelationsList[i].FactionIndex);
                FactionRelations.Add((int)FactionRelationsList[i].RelationType);
            }
        }

        void FixedUpdate()
        {
            if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType == EmeraldBehaviors.BehaviorTypes.Passive) return; // Passive は視線判定しない

            if (!EmeraldComponent.CombatComponent.CombatState || EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                LineOfSightDetection(); // 非戦闘時/死亡遅延中は視線判定でターゲットを探索
            }
        }

        /// <summary>
        /// （日本語）EmeraldAISystem から呼ばれる Detection の疑似 Update。
        /// ・戦闘/非戦闘それぞれで遮蔽チェック
        /// ・DetectionFrequency ごとに OverlapSphere で検知更新
        /// ・ターゲットの null/死亡監視、遮蔽時のアクション制御
        /// </summary>
        public void DetectionUpdate()
        {
            if (EmeraldComponent.CombatComponent.CombatState) CheckForObstructions(EmeraldComponent.CombatTarget);      // 戦闘時：CombatTarget への遮蔽チェック
            else if (!EmeraldComponent.CombatComponent.CombatState) CheckForObstructions(EmeraldComponent.LookAtTarget); // 非戦闘時：LookAtTarget への遮蔽チェック

            // DetectionFrequency に基づいて OverlapSphere を更新
            if (EmeraldComponent.CombatComponent.TargetDetectionActive && !EmeraldComponent.MovementComponent.ReturningToStartInProgress)
            {
                DetectionTimer += Time.deltaTime;

                if (DetectionTimer >= DetectionFrequency)
                {
                    UpdateAIDetection();           // OverlapSphere による検出（DetectionLayerMask 対象のみ）
                    LookAtTargetDistanceCheck();   // LookAtTarget が検知半径内か確認
                    OnDetectionUpdate?.Invoke();   // 検知更新イベント
                    DetectionTimer = 0;            // タイマーリセット
                }
            }

            CheckForNullTarget();  // TargetSource が null になったら OnNullTarget を発火
            CheckLookAtTarget();   // LookAtTarget が死亡したらクリア
            ObstructionAction();   // 遮蔽中の振る舞い（AI/その他で挙動分岐）
        }

        /// <summary>
        /// （日本語）TargetSource（現在の対象）が null になったら OnNullTarget を発火します。
        /// </summary>
        void CheckForNullTarget()
        {
            if (EmeraldComponent.CurrentTargetInfo.TargetSource == null && EmeraldComponent.CurrentTargetInfo.CurrentICombat != null)
            {
                OnNullTarget?.Invoke();
            }
        }

        /// <summary>
        /// （日本語）OnNullTarget 経由で呼ばれる。非戦闘時に LookAt/Follow などの非戦闘ターゲット情報をクリアします。
        /// </summary>
        void NullNonCombatTarget()
        {
            if (!EmeraldComponent.CombatComponent.CombatState)
            {
                EmeraldComponent.TargetToFollow = null;
                EmeraldComponent.LookAtTarget = null;
                EmeraldComponent.CurrentTargetInfo.TargetSource = null;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;
            }
        }

        /// <summary>
        /// （日本語）非戦闘時に LookAtTarget の体力を監視し、0 になったら LookAt 情報をクリアします。
        /// </summary>
        void CheckLookAtTarget()
        {
            if (EmeraldComponent.LookAtTarget && !EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0)
            {
                EmeraldComponent.LookAtTarget = null;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;
            }
        }

        /// <summary>
        /// （日本語）戦闘中の遮蔽状態に応じた振る舞い。
        /// ・AI に遮られたら別ターゲット探索
        /// ・その他に遮られたら停止距離変更 等
        /// 遮蔽が ObstructionSeconds 続いたときに実行。
        /// </summary>
        void ObstructionAction()
        {
            if (TargetObstructed && EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.CanReachTarget)
            {
                ObstructionTimer += Time.deltaTime;
                if (ObstructionTimer >= ObstructionSeconds)
                {
                    if (ObstructionType == ObstructedTypes.AI)
                    {
                        SearchForTarget(PickTargetTypes.Random); // AI に遮られているならランダムで切替
                    }
                    else if (ObstructionType == ObstructedTypes.Other && EmeraldComponent.CoverComponent == null)
                    {
                        EmeraldComponent.m_NavMeshAgent.stoppingDistance = 3; // その他の障害物：少し近づく
                        //EmeraldAPI.Internal.GenerateRandomPositionWithinRadius(EmeraldComponent);
                    }

                    OnTargetObstructed?.Invoke(); // 遮蔽イベント発火
                    ObstructionTimer = 0;
                }
            }
            else if (!TargetObstructed && EmeraldComponent.CombatComponent.CombatState && !EmeraldComponent.AnimationComponent.IsBackingUp)
            {
                if (EmeraldComponent.CoverComponent == null)
                {
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                }
                else if (EmeraldComponent.CoverComponent.CoverState != EmeraldCover.CoverStates.MovingToCover)
                {
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                }
            }
        }

        /// <summary>
        /// （日本語）Physics.OverlapSphere を用いて、DetectionLayerMask 上の対象のみを検出します。
        /// 併せて、視野内ターゲット/味方リストの距離チェックも実施します。
        /// </summary>
        public void UpdateAIDetection()
        {
            if (LineOfSightTargets.Count > 0) LineOfSightTargetsDistanceCheck(); // 視線候補の距離チェック
            if (NearbyAllies.Count > 0) NearbyAlliesDistanceCheck();             // 味方の距離チェック

            Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, DetectionRadius, DetectionLayerMask); // 検知

            foreach (Collider C in CurrentlyDetectedTargets)
            {
                if (C.gameObject != this.gameObject && IsValidTarget(C.transform)) // 自分自身以外で、Faction を持つもののみ
                {
                    DetectTarget(C.transform); // 敵/味方/プレイヤーの判定と格納
                }
            }
        }

        /// <summary>
        /// （日本語）与えられた Transform を元に、敵/味方/プレイヤーを判定して各リストや LookAt/CombatTarget を更新します。
        /// </summary>
        void DetectTarget(Transform Target)
        {
            if (IgnoredTargetsList.Contains(Target))
                return;

            if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            {
                if (IsEnemyTarget(Target))
                {
                    CurrentDetectionState = DetectionStates.Alert;
                    if (!LineOfSightTargets.Contains(Target.GetComponent<Collider>()))
                        LineOfSightTargets.Add(Target.GetComponent<Collider>());
                }
                // 味方の収集（味方は AI のみ）
                else if (IsFriendlyTarget(Target))
                {
                    EmeraldSystem AllyEmeraldComponent = Target.GetComponent<EmeraldSystem>();
                    if (AllyEmeraldComponent && !NearbyAllies.Contains(AllyEmeraldComponent)) NearbyAllies.Add(AllyEmeraldComponent);
                }
            }

            // まだ LookAt/Combat どちらのターゲットもない場合は、プレイヤー等を LookAt として設定
            if (EmeraldComponent.LookAtTarget == null && EmeraldComponent.CombatTarget == null)
            {
                if (IsLookAtTarget(Target))
                {
                    EmeraldComponent.LookAtTarget = Target;
                    GetTargetInfo(EmeraldComponent.LookAtTarget);
                    OnPlayerDetected?.Invoke();
                }
            }
        }

        /// <summary>
        /// （日本語）視線ロジック：視野角内にある LineOfSightTargets へレイを飛ばし、遮蔽されていなければ SearchForTarget を呼びます。
        /// 非戦闘時/死亡でない時に実行。
        /// </summary>
        void LineOfSightDetection()
        {
            if (CurrentDetectionState == DetectionStates.Alert && EmeraldComponent.CombatTarget == null && !EmeraldComponent.AnimationComponent.IsDead)
            {
                for (int i = LineOfSightTargets.Count - 1; i >= 0; --i)
                {
                    if (LineOfSightTargets[i] == null)
                    {
                        LineOfSightTargets.RemoveAt(i);
                    }
                    else
                    {
                        Bounds bounds = LineOfSightTargets[i].bounds;
                        float heightOffset = bounds.size.y * 0.15f;
                        float y = bounds.max.y - heightOffset;
                        Vector3 targetTop = new Vector3(bounds.center.x, y, bounds.center.z);

                        Vector3 direction = targetTop - HeadTransform.position;
                        float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

                        // 視野角内のみチェック。戦闘時は360°、非戦闘時は FieldOfViewAngle*0.5
                        float fieldOfView = EmeraldComponent.CombatComponent.CombatState ? 360f : FieldOfViewAngle * 0.5f;
                        if (angle < fieldOfView)
                        {
                            if (!EmeraldComponent.CombatComponent.CombatState)
                            {
                                RaycastHit hit;
                                // 内部コライダー（LBD）も遮蔽として扱わないように InternalObstructionLayerMask を使用
                                if (Physics.Raycast(HeadTransform.position, direction, out hit, DetectionRadius, ~InternalObstructionLayerMask))
                                {
                                    if (hit.collider != null && LineOfSightTargets.Contains(hit.collider))
                                    {
                                        SearchForTarget(PickTargetType);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）現在の検知半径内で「実際に見えている」ターゲット（遮蔽なし）を全て返します。
        /// </summary>
        public List<Transform> GetVisibleTargets()
        {
            List<Transform> VisibleTargets = new List<Transform>();

            foreach (Collider C in LineOfSightTargets.ToArray())
            {
                RaycastHit hit;

                Bounds bounds = C.bounds;
                float heightOffset = bounds.size.y * 0.15f;
                float y = bounds.max.y - heightOffset;
                Vector3 targetTop = new Vector3(bounds.center.x, y, bounds.center.z);

                Vector3 direction = targetTop - HeadTransform.position;

                if (Physics.Raycast(HeadTransform.position, direction, out hit, DetectionRadius, ~InternalObstructionLayerMask))
                {
                    if (hit.collider != null && LineOfSightTargets.Contains(hit.collider))
                    {
                        if (!VisibleTargets.Contains(hit.collider.transform) && EmeraldComponent.CombatTarget != hit.collider.transform || hit.collider.CompareTag(PlayerTag))
                        {
                            VisibleTargets.Add(hit.collider.transform);
                        }
                    }
                }
            }

            return VisibleTargets;
        }

        /// <summary>
        /// （日本語）PickTargetType（Closest/Random/FirstDetected）に基づき、可視ターゲットから戦闘ターゲットを選択します。
        /// 可視がゼロでも、戦闘中かつ候補がいれば一時的に「遮蔽をバイパス」して選ぶ場合があります。
        /// </summary>
        public void SearchForTarget(PickTargetTypes pickTargetType)
        {
            List<Transform> VisibleTargets = GetVisibleTargets();
            if (EmeraldComponent.CombatTarget != null && pickTargetType != PickTargetTypes.Closest) VisibleTargets.Remove(EmeraldComponent.CombatTarget); // 既存ターゲットを除外

            if (VisibleTargets.Count > 0)
            {
                if (pickTargetType == PickTargetTypes.Closest)
                {
                    LineOfSightTargets = LineOfSightTargets.OrderBy(Target => (Target.transform.position - transform.position).sqrMagnitude).ToList();
                    SetDetectedTarget(LineOfSightTargets[0].transform);
                }
                else if (pickTargetType == PickTargetTypes.Random)
                {
                    SetDetectedTarget(VisibleTargets[Random.Range(0, VisibleTargets.Count)]);
                }
                else if (pickTargetType == PickTargetTypes.FirstDetected)
                {
                    SetDetectedTarget(VisibleTargets[0]);
                }
            }
            // 可視対象がいないが、戦闘中で候補はある場合：遮蔽を一時バイパスして選択
            else if (EmeraldComponent.CombatComponent.CombatState && LineOfSightTargets.Count > 0)
            {
                BypassedTargets.Clear();

                // CombatState が有効な AI のみ抽出
                for (int i = 0; i < LineOfSightTargets.Count; i++)
                {
                    if (IsCombatActiveAITarget(LineOfSightTargets[i].transform))
                    {
                        BypassedTargets.Add(LineOfSightTargets[i].transform);
                    }
                }

                if (BypassedTargets.Count > 0)
                {
                    if (pickTargetType == PickTargetTypes.Closest)
                    {
                        BypassedTargets = BypassedTargets.OrderBy(Target => (Target.transform.position - transform.position).sqrMagnitude).ToList();
                        SetDetectedTarget(BypassedTargets[0].transform);
                    }
                    else if (pickTargetType == PickTargetTypes.Random)
                    {
                        SetDetectedTarget(BypassedTargets[Random.Range(0, BypassedTargets.Count)]);
                    }
                    else if (pickTargetType == PickTargetTypes.FirstDetected)
                    {
                        SetDetectedTarget(BypassedTargets[0].transform);
                    }
                }
            }
        }

        /// <summary>
        /// （日本語）頭部からターゲットの DamagePosition へレイを飛ばし、遮蔽の有無をチェックします。
        /// 一定間隔（ObstructionDetectionFrequency）で更新します。
        /// </summary>
        void CheckForObstructions(Transform TargetSource)
        {
            ObstructionDetectionUpdateTimer += Time.deltaTime;

            if (ObstructionDetectionUpdateTimer >= ObstructionDetectionFrequency && TargetSource != null && EmeraldComponent.CurrentTargetInfo.CurrentICombat != null)
            {
                TargetDirection = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position;

                RaycastHit hit;

                // 遮蔽チェック：停止距離を段階的に下げ、5 以下でも解決しない場合はターゲット再探索
                if (Physics.Raycast(HeadTransform.position, (TargetDirection), out hit, EmeraldComponent.CombatComponent.DistanceFromTarget, ~InternalObstructionLayerMask))
                {
                    if (!hit.collider.transform.IsChildOf(TargetSource) && !hit.collider.transform.IsChildOf(this.transform) && hit.collider.transform != TargetSource && !IgnoredColliders.Contains(hit.collider))
                    {
                        // ObstructionType を設定（AI かそれ以外か）
                        if ((LBDLayers & (1 << hit.collider.gameObject.layer)) != 0 || (DetectionLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
                        {
                            ObstructionType = ObstructedTypes.AI;
                        }
                        else
                        {
                            ObstructionType = ObstructedTypes.Other;
                        }

                        EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                        TargetObstructed = true;
                        CurrentObstruction = hit.collider.transform;
                    }
                    else
                    {
                        TargetObstructed = false;
                        CurrentObstruction = null;
                        ObstructionType = ObstructedTypes.None;
                    }
                }
                else
                {
                    TargetObstructed = false;
                    CurrentObstruction = null;
                    ObstructionType = ObstructedTypes.None;
                }

                ObstructionDetectionUpdateTimer = 0;
            }
        }

        /// <summary>
        /// （日本語）与えられた Transform を現在のターゲットとして確定し、必要なリセットとイベント発火を行います。
        /// </summary>
        public void SetDetectedTarget(Transform DetectedTarget)
        {
            // 同一ターゲットなら何もしない
            if (EmeraldComponent.CombatTarget == DetectedTarget) return;

            EmeraldAI.Utility.EmeraldCombatManager.ActivateCombatState(EmeraldComponent); // 戦闘状態へ
            ResetDetectionValues();                                                       // いくつかの設定を初期化
            GetTargetInfo(DetectedTarget);                                                // ターゲット情報を格納
            EmeraldComponent.CombatTarget = DetectedTarget;                               // CombatTarget 設定
            OnEnemyTargetDetected?.Invoke();                                              // 敵ターゲット検知イベント
        }

        /// <summary>
        /// （日本語）ターゲット確定後に、検知半径/視野角/停止距離/回転フラグ/撃破遅延 などを既定値へ戻します。
        /// </summary>
        void ResetDetectionValues()
        {
            DetectionRadius = StartingDetectionRadius;
            FieldOfViewAngle = StartingFieldOfViewAngle;
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            EmeraldComponent.AnimationComponent.IsTurning = false;
            EmeraldComponent.CombatComponent.DeathDelayActive = false;
            EmeraldComponent.CombatComponent.DeathDelayTimer = 0;
        }

        /// <summary>
        /// （日本語）ターゲットの各種インターフェイス参照（IDamageable/ICombat など）を取得して格納します。
        /// </summary>
        public void GetTargetInfo(Transform Target, bool? OverrideFactionRequirement = false)
        {
            if (Target != null)
            {
                EmeraldComponent.CurrentTargetInfo.TargetSource = Target;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = Target.GetComponent<IDamageable>();
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = Target.GetComponent<ICombat>();
            }
        }

        /// <summary>
        /// （日本語）LineOfSightTargets のうち、検知半径外/死亡したものをリストから除去します。
        /// </summary>
        void LineOfSightTargetsDistanceCheck()
        {
            for (int i = 0; i < LineOfSightTargets.Count; i++)
            {
                // null / 死亡は除外
                if (LineOfSightTargets[i] == null || LineOfSightTargets[i].GetComponent<IDamageable>().Health <= 0)
                {
                    LineOfSightTargets.RemoveAt(i);
                }
                else
                {
                    float distance = Vector3.Distance(LineOfSightTargets[i].transform.position, transform.position);

                    // 検知半径外なら除外
                    if (distance > DetectionRadius)
                        LineOfSightTargets.Remove(LineOfSightTargets[i]);
                }
            }
        }

        /// <summary>
        /// （日本語）NearbyAllies のうち、検知半径外のものをリストから除去します。
        /// </summary>
        void NearbyAlliesDistanceCheck()
        {
            for (int i = 0; i < NearbyAllies.Count; i++)
            {
                // null は除外
                if (NearbyAllies[i] == null)
                {
                    NearbyAllies.RemoveAt(i);
                }
                else
                {
                    float distance = Vector3.Distance(NearbyAllies[i].transform.position, transform.position);

                    // 検知半径外なら除外
                    if (distance > DetectionRadius)
                        NearbyAllies.Remove(NearbyAllies[i]);
                }
            }
        }

        /// <summary>
        /// （日本語）LookAtTarget が検知半径外に出たら解除します。
        /// </summary>
        void LookAtTargetDistanceCheck()
        {
            if (EmeraldComponent.LookAtTarget != null)
            {
                float distance = Vector3.Distance(EmeraldComponent.LookAtTarget.transform.position, transform.position);

                // 検知半径外なら LookAt を解除
                if (distance > DetectionRadius)
                    NullNonCombatTarget();
            }
        }

        /// <summary>
        /// （日本語）戦闘終了時（OnExitCombat）に既定状態へ戻し、LookAtTarget があれば情報を再設定します。
        /// </summary>
        void ReturnToDefaultState()
        {
            CurrentDetectionState = DetectionStates.Unaware;
            if (EmeraldComponent.LookAtTarget != null)
                GetTargetInfo(EmeraldComponent.LookAtTarget);
        }

        /// <summary>
        /// （日本語）自分が死亡した際、味方の NearbyAllies から自分の参照を除去します。
        /// </summary>
        void NearbyAllyDeathHandler()
        {
            if (NearbyAllies.Count > 0)
            {
                for (int i = 0; i < NearbyAllies.Count; i++)
                {
                    if (NearbyAllies[i].DetectionComponent.NearbyAllies.Contains(EmeraldComponent)) NearbyAllies[i].DetectionComponent.NearbyAllies.Remove(EmeraldComponent);
                }
            }
        }

        /// <summary>
        /// （日本語）与えられた Transform の派閥関係名（Enemy/Neutral/Friendly/Invalid Target）を返します。
        /// </summary>
        public string GetTargetFactionRelation(Transform Target)
        {
            return EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, Target);
        }

        /// <summary>
        /// （日本語）与えられた AI ターゲットの派閥名を返します。
        /// </summary>
        public string GetTargetFactionName(Transform Target)
        {
            return EmeraldAPI.Faction.GetTargetFactionName(Target);
        }

        /// <summary>
        /// （日本語）フォロー対象（コンパニオン）を設定します。必要に応じて派閥データをコピーします。
        /// </summary>
        public void SetTargetToFollow(Transform Target, bool CopyFactionData = true)
        {
            EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>(); // 対象の EmeraldSystem を取得
            if (TargetEmeraldComponent != null)
            {
                if (TargetEmeraldComponent.CombatTarget == transform) TargetEmeraldComponent.CombatComponent.ClearTarget(); // 対象が AI なら、こちらをターゲットしていた状態を解除
                TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Add(transform); // この AI をフォロワーとして登録
                if (TargetEmeraldComponent.CombatComponent.CombatState) TargetEmeraldComponent.CombatComponent.DeathDelayActive = true;

                // フォロー対象の派閥データをコピー（同様の反応をさせる）
                if (CopyFactionData)
                {
                    CurrentFaction = TargetEmeraldComponent.DetectionComponent.CurrentFaction;
                    AIFactionsList = TargetEmeraldComponent.DetectionComponent.AIFactionsList;
                    FactionRelations = TargetEmeraldComponent.DetectionComponent.FactionRelations;
                    FactionRelationsList = TargetEmeraldComponent.DetectionComponent.FactionRelationsList;
                }
            }

            if (Target == EmeraldComponent.CombatTarget) EmeraldComponent.CombatComponent.ClearTarget();
            if (EmeraldComponent.CombatComponent.CombatState) EmeraldComponent.CombatComponent.DeathDelayActive = true;

            EmeraldComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.ResetState();
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
            EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
        }

        /// <summary>
        /// （日本語）召喚されたターゲット（Summon）を初期化します。必要に応じて派閥データをコピーします。
        /// </summary>
        public void InitializeSummonTarget(Transform Target, bool CopyFactionData = true)
        {
            EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>(); // 対象の EmeraldSystem を取得
            if (TargetEmeraldComponent != null)
            {
                if (TargetEmeraldComponent.CombatTarget == transform) TargetEmeraldComponent.CombatComponent.ClearTarget();
                TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Add(transform);

                if (CopyFactionData)
                {
                    CurrentFaction = TargetEmeraldComponent.DetectionComponent.CurrentFaction;
                    AIFactionsList = TargetEmeraldComponent.DetectionComponent.AIFactionsList;
                    FactionRelations = TargetEmeraldComponent.DetectionComponent.FactionRelations;
                    FactionRelationsList = TargetEmeraldComponent.DetectionComponent.FactionRelationsList;
                }
            }

            if (Target == EmeraldComponent.CombatTarget) EmeraldComponent.CombatComponent.ClearTarget();

            EmeraldComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.ResetState();
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
            EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
            Invoke(nameof(UpdateAIDetection), 0.1f); // Detection の更新タイミング前に何か起きた場合へ備えて即時更新
        }

        /// <summary>
        /// （日本語）フォロー対象を解除します（コンパニオンを終了）。死亡時にも呼ばれます。
        /// </summary>
        public void ClearTargetToFollow()
        {
            // 死亡時にも呼ばれ、対象のフォロワーリストから自分を外す
            if (EmeraldComponent.TargetToFollow)
            {
                EmeraldSystem TargetEmeraldComponent = EmeraldComponent.TargetToFollow.GetComponent<EmeraldSystem>();

                if (TargetEmeraldComponent)
                {
                    TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Remove(transform);
                }
            }

            EmeraldComponent.BehaviorsComponent.TargetToFollow = null;
            EmeraldComponent.TargetToFollow = null;

            if (!EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.BehaviorsComponent.ResetState();
            }
        }

        /// <summary>
        /// （日本語）与えられた Transform が敵ターゲットであれば true。
        /// </summary>
        bool IsEnemyTarget(Transform Target)
        {
            int ReceivedFaction = Target.GetComponent<IFaction>().GetFaction();
            return AIFactionsList.Contains(ReceivedFaction) && FactionRelations[AIFactionsList.IndexOf(ReceivedFaction)] == 0;
        }

        /// <summary>
        /// （日本語）与えられた Transform が味方（または同派閥）であれば true。
        /// </summary>
        bool IsFriendlyTarget(Transform Target)
        {
            int ReceivedFaction = Target.GetComponent<IFaction>().GetFaction();
            return AIFactionsList.Contains(ReceivedFaction) && FactionRelations[AIFactionsList.IndexOf(ReceivedFaction)] == 2 || ReceivedFaction == CurrentFaction;
        }

        /// <summary>
        /// （日本語）与えられた Transform が LookAtTarget として妥当（PlayerTag かつ敵対ではない）なら true。
        /// </summary>
        bool IsLookAtTarget(Transform Target)
        {
            return Target.gameObject.CompareTag(PlayerTag) && GetTargetFactionRelation(Target) != "Enemy";
        }

        // TESTING：CombatState が有効な AI かどうか
        bool IsCombatActiveAITarget(Transform Target)
        {
            EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>();
            return TargetEmeraldComponent != null && TargetEmeraldComponent.CombatComponent.CombatState;
        }

        /// <summary>
        /// （日本語）与えられた Transform がプレイヤー/AI/非AIいずれであっても、IFaction を実装しているか確認します。
        /// 実装していなければ警告を出し、無効とします。
        /// </summary>
        bool IsValidTarget(Transform Target)
        {
            if (Target.GetComponent<IFaction>() != null)
            {
                return true;
            }
            else
            {
                Debug.Log("The " + Target.name + " object is set as a valid target (both Tag and Layer), but does not have a Faction Extension component on it. Please add one in order for this target to be properly detected.");
                return false;
            }
        }

        /// <summary>
        /// （日本語）IFaction の実装：この AI の派閥 ID を返します。
        /// </summary>
        public int GetFaction()
        {
            return CurrentFaction;
        }
    }
}
