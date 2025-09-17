using UnityEngine; // UnityEngine 名前空間を使用するための宣言

namespace EmeraldAI // EmeraldAI 用の名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/debugger-component")] // ドキュメントへの参照URL（インスペクタから開ける）
    /// <summary>
    /// 【クラス概要】EmeraldDebugger：Emerald AI の挙動を可視化・検証するためのデバッグ支援コンポーネント。
    /// ラインオブサイト（視線）・NavMesh パス/目的地・未検知ターゲット線・LookAt点 などの描画や、検出/障害物のログ出力を行う。
    /// </summary>
    public class EmeraldDebugger : MonoBehaviour // MonoBehaviour を継承したデバッグ用クラス
    {
        #region Debugger Variables // デバッガ関連の公開設定（インスペクタで操作可能）

        [Header("デバッグツール全体の有効/無効（Yes=有効, No=無効）")] // インスペクタ見出し：全体スイッチ
        public YesOrNo EnableDebuggingTools = YesOrNo.Yes; // デバッグ機能の総合ON/OFF

        [Header("視線（Line of Sight）レイの描画（ターゲット可視判定の可視化）")] // 視線ライン可視化
        public YesOrNo DrawLineOfSightLines = YesOrNo.Yes; // ラインオブサイト描画のON/OFF

        [Header("NavMesh パスの可視化（経路のライン描画）")] // 経路ライン可視化
        public YesOrNo DrawNavMeshPath = YesOrNo.Yes; // NavMesh パス描画のON/OFF

        [Header("NavMesh パスラインの色（例：青）")] // 経路ライン色
        public Color NavMeshPathColor = Color.blue; // パスラインの表示色

        [Header("NavMesh 目的地の可視化（円＋ラインで示す）")] // 目的地可視化
        public YesOrNo DrawNavMeshDestination = YesOrNo.Yes; // 目的地描画のON/OFF

        [Header("NavMesh 目的地マーカーの色（例：赤）")] // 目的地色
        public Color NavMeshDestinationColor = Color.red; // 目的地マーカーの表示色

        [Header("IK使用時のLookAtポイント（注視点）をGizmosで描画するか")] // LookAt点可視化
        public YesOrNo DrawLookAtPoints = YesOrNo.Yes; // LookAtポイント描画のON/OFF

        [Header("警戒中（Alert）に未検知ターゲットへ向けたラインを描画するか")] // 未検知ターゲット線
        public YesOrNo DrawUndetectedTargetsLine = YesOrNo.Yes; // 未検知ターゲット線のON/OFF

        [Header("ターゲット検出時に情報をコンソールへ出力するか")] // ターゲットログ
        public YesOrNo DebugLogTargets = YesOrNo.Yes; // ターゲット検出ログON/OFF

        [Header("障害物（Obstruction）検出時に情報をコンソールへ出力するか")] // 障害物ログ
        public YesOrNo DebugLogObstructions = YesOrNo.Yes; // 障害物ログON/OFF

        [Header("フットステップ（足音/足位置）可視化（開発時の歩行確認用）")] // 足跡可視化（本クラスでは参照用）
        public YesOrNo DrawFootstepPositions = YesOrNo.Yes; // 足位置の可視化ON/OFF

        [Header("フットステップのログ出力（開発時のデバッグ用）")] // 足跡ログ（本クラスでは参照用）
        public YesOrNo DebugLogFootsteps = YesOrNo.Yes; // 足音ログON/OFF

        [Header("検出されたカバーノードの可視化（遮蔽物判定の可視化）")] // カバーノード可視化（本クラスでは参照用）
        public YesOrNo DrawDetectedCoverNodes = YesOrNo.Yes; // カバー可視化ON/OFF
        #endregion // デバッガ公開変数ここまで

        #region Private Variables // 内部で使用する参照・一時変数（実行時に利用）

        [Header("EmeraldSystem 参照（このAI本体の中核コンポーネント）")] // 内部参照：AI本体
        EmeraldSystem EmeraldComponent; // Emerald AI のメインシステム参照（実行時に取得）

        [Header("EmeraldInverseKinematics 参照（IK用。LookAt点描画に使用）")] // 内部参照：IK
        EmeraldInverseKinematics IKComponent; // IKコンポーネント参照（任意添付）

        [Header("デバッグ用ライン色（ターゲット可視/不可により緑↔赤を切替）")] // 動的に色変更
        Color DebugLineColor = Color.green; // 視線ラインの基本色（可視=緑, 遮蔽=赤）

        [Header("ターゲット方向ベクトル（一時計算用）")] // レイ向き計算
        Vector3 TargetDirection; // HeadTransform からターゲットへの方向

        [Header("目的地を示す任意オブジェクト（未使用/拡張用）")] // 拡張用フィールド
        Transform DestinationObject; // 目的地表示のための Transform（現状未使用）
        #endregion // 内部変数ここまで

        #region Editor Variables // エディタ表示制御用（インスペクタの折りたたみなど）

        [Header("インスペクタで設定セクションを隠す（折りたたみ状態）")] // UI状態
        public bool HideSettingsFoldout; // 設定セクションを隠すフラグ

        [Header("インスペクタで設定セクションを展開する（折りたたみトグル）")] // UI状態
        public bool SettingsFoldout; // 設定セクションを展開するフラグ
        #endregion // エディタ用フラグここまで

        void Start() // Unity ライフサイクル：初期化
        {
            InitializeDebugger(); // デバッガ初期化（参照取得とイベント購読）
        }

        /// <summary>
        /// デバッガコンポーネントの初期化：必要な参照取得と検出イベントの購読を行う。
        /// </summary>
        void InitializeDebugger() // 初期化メソッド
        {
            EmeraldComponent = GetComponent<EmeraldSystem>(); // 同一GameObject上の EmeraldSystem を取得
            IKComponent = GetComponent<EmeraldInverseKinematics>(); // 同一GameObject上の IK コンポーネントを取得（無い場合は null）

            // 敵ターゲット検出時のイベントに購読（検出情報をデバッグ出力）
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += DebugDetectedEnemyTarget; // DebugDetectedEnemyTarget にハンドラ登録

            // プレイヤー検出時のイベントに購読（検出情報をデバッグ出力）
            EmeraldComponent.DetectionComponent.OnPlayerDetected += DebugDetectedPlayerTarget; // DebugDetectedPlayerTarget にハンドラ登録
        }

        public void DebuggerUpdate() // EmeraldSystem 側から毎フレーム相当で呼ばれる前提の更新処理
        {
            if (!enabled) return; // このコンポーネントが無効なら何もしない

            DebugObstructions(); // 障害物デバッグログ出力
            DrawTargetRaycastLines(); // 視線レイの描画
            DrawUndetectedTargetsLineInternal(); // 未検知ターゲットへのライン描画
            DrawNavMeshPathInternal(); // 現在の NavMesh パス描画
            DrawNavMeshDestinationInternal(); // 現在の NavMesh 目的地描画
        }

        /// <summary>
        /// テスト用メッセージを Unity コンソールへ出力する。
        /// </summary>
        public void DebugLogMessage(string Message) // 任意のメッセージをログ出力
        {
            Debug.Log(Message); // コンソールに出力
        }

        /// <summary>
        /// 現在の障害物（視線遮蔽対象）を Unity コンソールに出力する。
        /// </summary>
        void DebugObstructions() // 障害物デバッグ
        {
            // デバッグ全体が無効、または障害物ログ無効、またはターゲット/注視対象がいない場合は終了
            if (EnableDebuggingTools == YesOrNo.No || DebugLogObstructions == YesOrNo.No || !EmeraldComponent.CombatTarget && !EmeraldComponent.LookAtTarget) return;

            Transform CurrentObstruction = EmeraldComponent.DetectionComponent.CurrentObstruction; // 現在の遮蔽物 Transform を取得

            // 死亡遅延中でなく、戦闘ターゲットが存在し、スケールが極小ではなく、遮蔽物がある場合に出力
            if (!EmeraldComponent.CombatComponent.DeathDelayActive && EmeraldComponent.CombatTarget && EmeraldComponent.CombatTarget.localScale != Vector3.one * 0.003f && CurrentObstruction)
            {
                Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Obstruction: " + "</color>" + "<color=red>" + CurrentObstruction.name + "</color>" + "</b>"); // 太字＋色付きで出力
            }
        }

        /// <summary>
        /// 敵ターゲット検出時の情報を Unity コンソールに出力する。
        /// </summary>
        void DebugDetectedEnemyTarget() // 敵検出イベントハンドラ
        {
            if (EnableDebuggingTools == YesOrNo.No || DebugLogTargets == YesOrNo.No) return; // 無効時は何もしない

            if (!EmeraldComponent.CombatComponent.DeathDelayActive) // 死亡遅延中でなければ
            {
                if (EmeraldComponent.CombatTarget != null) // 戦闘ターゲットが存在するなら
                {
                    Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Combat Target: " + "</color>" + "<color=red>" + EmeraldComponent.CombatTarget.gameObject.name + "</color>" + "</b>" + "  |" +
                        "<b>" + "<color=green>" + "  Relation Type: " + "</color>" + "<color=red>" + EmeraldComponent.DetectionComponent.GetTargetFactionRelation(EmeraldComponent.CombatTarget) + "</color>" + "</b>"); // ターゲット名と陣営関係を出力
                }
            }
        }

        /// <summary>
        /// プレイヤー検出時の情報を Unity コンソールに出力する。
        /// </summary>
        void DebugDetectedPlayerTarget() // プレイヤー検出イベントハンドラ
        {
            if (EnableDebuggingTools == YesOrNo.No || DebugLogTargets == YesOrNo.No) return; // 無効時は何もしない

            if (!EmeraldComponent.CombatComponent.DeathDelayActive) // 死亡遅延中でなければ
            {
                if (EmeraldComponent.LookAtTarget != null) // 注視対象が存在するなら
                {
                    Debug.Log("<b>" + "<color=green>" + gameObject.name + " - Current Look At Target: " + "</color>" + "<color=red>" + EmeraldComponent.LookAtTarget.gameObject.name + "</color>" + "</b>" + "  |" +
                        "<b>" + "<color=green>" + "  Relation Type: " + "</color>" + "<color=green>" + EmeraldComponent.DetectionComponent.GetTargetFactionRelation(EmeraldComponent.LookAtTarget) + "</color>" + "</b>"); // 注視対象名と陣営関係を出力
                }
            }
        }

        /// <summary>
        /// ターゲットおよび注視対象との間のレイ（視線）を描画する。
        /// </summary>
        void DrawTargetRaycastLines() // 視線ライン描画
        {
            // デバッグ全体が無効、視線ライン描画が無効、またはターゲット情報が無い場合は終了
            if (EnableDebuggingTools == YesOrNo.No || DrawLineOfSightLines == YesOrNo.No || EmeraldComponent.CurrentTargetInfo.TargetSource == null) return;

            Transform HeadTransform = EmeraldComponent.DetectionComponent.HeadTransform; // 視線発射元（頭位置）を取得

            if (EmeraldComponent.CombatTarget != null) // 戦闘ターゲットがある場合
            {
                TargetDirection = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position; // 頭から被ダメ位置への方向
                Debug.DrawRay(new Vector3(HeadTransform.position.x, HeadTransform.position.y, HeadTransform.position.z), TargetDirection, DebugLineColor); // レイを描画
            }
            else if (EmeraldComponent.LookAtTarget != null) // 注視対象のみある場合
            {
                Vector3 LookAtTargetDir = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position; // 頭から注視位置への方向
                Debug.DrawRay(new Vector3(EmeraldComponent.DetectionComponent.HeadTransform.position.x, HeadTransform.position.y, HeadTransform.position.z), LookAtTargetDir, DebugLineColor); // レイを描画
            }

            // 視線が遮蔽されている、または回転中なら赤、そうでなければ緑
            if (EmeraldComponent.DetectionComponent.TargetObstructed || EmeraldComponent.AnimationComponent.IsTurning)
            {
                DebugLineColor = Color.red; // 遮蔽/回転中：赤
            }
            else
            {
                DebugLineColor = Color.green; // クリア：緑
            }
        }

        /// <summary>
        /// Alert 状態中に、未検知のターゲットへ向けたラインを描画する。
        /// </summary>
        void DrawUndetectedTargetsLineInternal() // 未検知ターゲット線描画
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawUndetectedTargetsLine == YesOrNo.No) return; // 無効時は何もしない

            // 警戒中、戦闘ターゲット無し、かつ死亡していない場合に処理
            if (EmeraldComponent.DetectionComponent.CurrentDetectionState == EmeraldDetection.DetectionStates.Alert && EmeraldComponent.CombatTarget == null && !EmeraldComponent.AnimationComponent.IsDead)
            {
                foreach (Collider C in EmeraldComponent.DetectionComponent.LineOfSightTargets.ToArray()) // 視界内候補を列挙
                {
                    Bounds bounds = C.bounds; // 対象のバウンディングボックス
                    float heightOffset = bounds.size.y * 0.15f; // 少し下げた高さを狙う
                    float y = bounds.max.y - heightOffset; // 上部からオフセット
                    Vector3 targetTop = new Vector3(bounds.center.x, y, bounds.center.z); // 目標点（やや上方）

                    Vector3 direction = targetTop - EmeraldComponent.DetectionComponent.HeadTransform.position; // 頭から目標点への方向
                    Debug.DrawRay(EmeraldComponent.DetectionComponent.HeadTransform.position, direction, new Color(1, 0.549f, 0)); // オレンジ色で描画
                }
            }
        }

        /// <summary>
        /// 現在の NavMesh パスをラインで描画する。
        /// </summary>
        void DrawNavMeshPathInternal() // パス描画
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawNavMeshPath == YesOrNo.No) return; // 無効時は何もしない

            for (int i = 0; i < EmeraldComponent.m_NavMeshAgent.path.corners.Length; i++) // パスコーナーを順に結ぶ
            {
                if (i > 0) Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.path.corners[i - 1] + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.path.corners[i] + Vector3.up * 0.5f, NavMeshPathColor); // 直前点→現在点
                else Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.path.corners[0] + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.path.corners[i] + Vector3.up * 0.5f, NavMeshPathColor); // 始点→現在点
            }
        }

        /// <summary>
        /// 現在の NavMesh 目的地を可視化（円＋縦ライン）する。
        /// </summary>
        void DrawNavMeshDestinationInternal() // 目的地描画
        {
            if (EnableDebuggingTools == YesOrNo.No || DrawNavMeshDestination == YesOrNo.No) return; // 無効時は何もしない

            DrawCircle(EmeraldComponent.m_NavMeshAgent.destination, 0.25f, NavMeshDestinationColor); // 目的地周囲に小円を描く
            Debug.DrawLine(EmeraldComponent.m_NavMeshAgent.destination + Vector3.up * 0.5f, EmeraldComponent.m_NavMeshAgent.destination, NavMeshDestinationColor); // 上から下へ縦ライン
        }

        /// <summary>
        /// IK コンポーネント使用時の注視点（AimSource）を Gizmos で表示する。
        /// </summary>
        void DrawLookAtPointsInternal() // LookAt点 Gizmos 描画
        {
            if (DrawLookAtPoints == YesOrNo.No || !IKComponent || !IKComponent.m_AimSource || EmeraldComponent.AnimationComponent.IsDead) return; // 条件を満たさない場合は何もしない

            Gizmos.color = new Color(1, 0, 0, 0.35f); // 半透明の赤
            Gizmos.DrawSphere(IKComponent.m_AimSource.position, 0.12f); // 注視点位置に小さな球を描画
            Gizmos.color = Color.white; // 色を白へ戻す
        }

        void DrawCircle(Vector3 center, float radius, Color color) // Debug.DrawLine を用いた簡易円描画
        {
            Vector3 prevPos = center + new Vector3(radius, 0, 0); // 円周上の初期点
            for (int i = 0; i < 30; i++) // 30分割で多角形的に円を描く
            {
                float angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f; // 次の角度（ラジアン）
                Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius); // 次の点の座標
                Debug.DrawLine(prevPos, newPos, color); // 直線で結ぶ
                prevPos = newPos; // 直前点を更新
            }
        }

        private void OnDrawGizmos() // シーンビューでの Gizmos 描画フック
        {
            DrawLookAtPointsInternal(); // LookAtポイントを必要に応じて描画
        }
    }
}
