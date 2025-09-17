using System.Collections;                         // コルーチン等の基礎コレクションAPI
using System.Collections.Generic;                 // List<T> などのジェネリックコレクション
using UnityEngine;                                // Unity の基本API
using UnityEngine.Animations.Rigging;             // Animation Rigging（Rig, MultiAimConstraint など）
using System.Linq;                                // Linq 機能

using EmeraldAI;                                  // EmeraldAI 名前空間の型へ直接アクセス

namespace EmeraldAI                                 // EmeraldAI に属する名前空間
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/inverse-kinematics-component")] // 公式WikiへのヘルプURL

    // 【クラス概要】EmeraldInverseKinematics：
    //  上半身リグ（Rig）と MultiAimConstraint を用いて、AI がターゲット方向へ自然に上半身・視線を向けるためのIK制御を行うコンポーネント。
    //  目標（Aim Goal）を逐次更新し、Aim Source を補間移動させることで追従させる。戦闘/徘徊時で注視角度や速度・距離などの閾値を切り替える。
    public class EmeraldInverseKinematics : MonoBehaviour // MonoBehaviour を継承
    {
        #region IK Variables // —— IK 設定（インスペクタで調整する公開パラメータ） ——

        [Header("徘徊時：注視許容角度（度）。この角度以内ならターゲットを向く")]
        public int WanderingLookAtLimit = 75;              // 徘徊（非戦闘）時の注視角度制限

        [Header("徘徊時：注視追従速度（補間速度）")]
        public float WanderingLookSpeed = 4f;              // 徘徊時の注視追従スピード

        [Header("徘徊時：注視対象までの最大距離（m）")]
        public int WanderingLookDistance = 12;             // 徘徊時の注視距離上限

        [Header("徘徊時：注視点の高さオフセット（Y軸加算）")]
        public float WanderingLookHeightOffset = 0;        // 徘徊時の注視Yオフセット

        [Header("戦闘時：注視許容角度（度）。この角度以内ならターゲットを向く")]
        public int CombatLookAtLimit = 75;                 // 戦闘時の注視角度制限

        [Header("戦闘時：注視追従速度（補間速度）")]
        public float CombatLookSpeed = 6f;                 // 戦闘時の注視追従スピード

        [Header("戦闘時：注視対象までの最大距離（m）")]
        public int CombatLookDistance = 12;                // 戦闘時の注視距離上限

        [Header("戦闘時：注視点の高さオフセット（Y軸加算）")]
        public float CombatLookHeightOffset = 0;           // 戦闘時の注視Yオフセット

        [Header("上半身に適用する Rig の一覧（MultiAimConstraint を内包）")]
        public List<Rig> UpperBodyRigsList = new List<Rig>(); // 対象となる上半身Rig群

        [Header("Aim Source（IKの参照ターゲットTransform）※起動時に自動生成/設定")]
        public Transform m_AimSource;                      // 参照用Aim Transform（親は m_AimSourceParent）
        #endregion

        #region Private Variables // —— 内部状態／参照（実行時に使用） ——

        [Header("上半身Rig以下から収集した MultiAimConstraint の一覧")]
        List<MultiAimConstraint> m_MultiAimConstraints = new List<MultiAimConstraint>(); // 収集した制約のリスト

        [Header("Rig フェード用の進行コルーチン参照")]
        Coroutine FadeRigCoroutine;                        // フェードの重複起動防止に使用

        [Header("Aim Source の親Transform（AI頭部付近に生成）")]
        Transform m_AimSourceParent;                       // Aim Source の親

        [Header("EmeraldSystem 参照（AI本体）")]
        EmeraldSystem EmeraldComponent;                    // 各サブコンポーネントへアクセス

        [Header("Aimの目標ワールド座標（補間先）")]
        Vector3 m_AimGoal;                                 // 目標とするAim位置

        [Header("現在のターゲットのワールド座標（被ダメ位置など）")]
        Vector3 CurrentTargetPosition;                     // 現ターゲット位置

        [Header("RigBuilder 参照（Animation Rigging のビルド/有効化制御）")]
        RigBuilder m_RigBuilder;                           // RigBuilder 参照

        [Header("一時的に使用される MultiAimConstraint（子要素探索用）")]
        MultiAimConstraint ChildMultiAimConstraints;       // 一時参照（ループ内で使用）

        [Header("自動フェードイン用のタイマー（待機状態での復帰に使用）")]
        float AutoFadeInTimer;                             // 自動フェードインの経過時間

        [Header("フェードイン進行中フラグ")]
        bool FadeInProgress;                               // フェードイン中の状態

        [Header("現在のターゲット角度（AIから見た角度）")]
        float CurrentTargetAngle;                          // ターゲットまでの角度
        #endregion

        #region Editor Variables // —— エディタ表示制御 ——

        [Header("【Editor表示】設定セクションを隠す（折りたたみ制御）")]
        public bool HideSettingsFoldout;                   // インスペクタの表示制御

        [Header("【Editor表示】一般IK設定セクションの折りたたみ")]
        public bool GeneralIKSettingsFoldout;              // 一般IK設定の開閉

        [Header("【Editor表示】Rig設定セクションの折りたたみ")]
        public bool RigSettingsFoldout;                    // Rig設定の開閉
        #endregion

        void Start()                                       // Unityライフサイクル：開始時
        {
            InitializeIK();                                // IK初期化
        }

        /// <summary>
        /// IKシステムを初期化する。RigBuilder/イベント購読/Aim Source生成などを行う。
        /// </summary>
        private void InitializeIK()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();    // AI本体参照を取得
            m_RigBuilder = GetComponent<RigBuilder>();           // RigBuilder を取得（必須）

            if (m_RigBuilder == null)                           // RigBuilder が無い場合は無効化
            {
                this.enabled = false;                           // 本コンポーネントを無効に
                return;                                         // 初期化中断
            }

            EmeraldComponent.HealthComponent.OnDeath += DisableInverseKinematics; // 死亡時にIKを無効化する購読

            // Aim Source 親の生成と初期配置（頭部付近）
            m_AimSourceParent = new GameObject("Aim Source Parent").transform;   // 親Transform作成
            m_AimSourceParent.position = EmeraldComponent.DetectionComponent.HeadTransform.position; // 頭位置へ
            m_AimSourceParent.SetParent(transform);                               // 自身を親に設定

            // Aim Source 本体の生成・初期化（前方へ少しオフセット）
            m_AimSource = new GameObject("Aim Source").transform;                 // 本体Transform作成
            m_AimSource.position = EmeraldComponent.DetectionComponent.HeadTransform.position; // 頭位置へ
            m_AimSource.localPosition = m_AimSource.localPosition + transform.forward * 2;     // 前方へオフセット
            m_AimSource.SetParent(m_AimSourceParent);                             // 親子関係を設定
            m_AimSource.localEulerAngles = Vector3.zero;                          // 局所回転を初期化
            m_AimGoal = m_AimSourceParent.position + (transform.forward * 3);     // 初期Aim目標

            // 各Rig内の MultiAimConstraint に Aim Source を割り当て
            for (int i = 0; i < UpperBodyRigsList.Count; i++)                     // 対象Rigを走査
            {
                var ChildMultiAimConstraints = UpperBodyRigsList[i].GetComponentsInChildren<MultiAimConstraint>(); // 子から収集

                if (ChildMultiAimConstraints.Length > 0)                          // 見つかった場合
                {
                    for (int j = 0; j < ChildMultiAimConstraints.Length; j++)     // すべて追加
                    {
                        m_MultiAimConstraints.Add(ChildMultiAimConstraints[j]);   // リストに格納
                    }
                }

                for (int j = 0; j < m_MultiAimConstraints.Count; j++)             // 各制約に対して
                {
                    var m_SourceObjects = m_MultiAimConstraints[j].data.sourceObjects; // 参照元リスト
                    m_MultiAimConstraints[j].data.maintainOffset = false;              // 初期オフセットを保持しない
                    m_SourceObjects.SetTransform(0, m_AimSource);                       // 0番に Aim Source を設定

                    // sourceObjects が空の場合は新規追加
                    if (m_MultiAimConstraints[j].data.sourceObjects.Count == 0)
                        m_SourceObjects.Add(new WeightedTransform(m_AimSource, 1f));    // 重み1で追加

                    m_MultiAimConstraints[j].data.sourceObjects = m_SourceObjects;      // 反映
                }
            }

            m_RigBuilder.Build();                                                  // Rig を再ビルドして反映
        }

        void Update()                                                               // 毎フレーム更新
        {
            UpdateIK();                                                             // IK更新処理
        }

        /// <summary>
        /// 現在の Aim 目標に合わせて Aim Source を時間的に補間移動させ、IK を更新する。
        /// </summary>
        void UpdateIK()
        {
            if (transform.localScale == Vector3.one * 0.003f) return;               // スケール極小時は処理しない

            // フェードイン進行中かつ待機中なら自動でウェイトを戻す
            if (FadeInProgress && EmeraldComponent.AnimationComponent.IsIdling)
            {
                AutoFadeInTimer += Time.deltaTime;                                   // 経過時間加算

                if (AutoFadeInTimer > 0.5f)                                          // 0.5秒後にフェード開始
                {
                    for (int i = 0; i < UpperBodyRigsList.Count; i++)
                    {
                        UpperBodyRigsList[i].weight = Mathf.Lerp(UpperBodyRigsList[i].weight, 1, Time.deltaTime * 5); // 重みを1へ補間

                        if (UpperBodyRigsList[UpperBodyRigsList.Count - 1].weight >= 1f) // 全部1付近になったら
                        {
                            if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine); // コルーチン停止
                            AutoFadeInTimer = 0;                                         // タイマーリセット
                            FadeInProgress = false;                                      // フラグ解除
                        }
                    }
                }
            }

            CurrentTargetAngle = EmeraldComponent.CombatComponent.TargetAngle;       // 現在のターゲット角度を取得
            int LookAtLimit = EmeraldComponent.CombatComponent.CombatState ? CombatLookAtLimit : WanderingLookAtLimit; // 状態に応じた閾値
            float LookSpeed = EmeraldComponent.CombatComponent.CombatState ? CombatLookSpeed : WanderingLookSpeed;     // 追従速度
            int LookDistance = EmeraldComponent.CombatComponent.CombatState ? CombatLookDistance : WanderingLookDistance; // 距離
            float LookHeightOffset = EmeraldComponent.CombatComponent.CombatState ? CombatLookHeightOffset : WanderingLookHeightOffset; // 高さオフセット

            if (EmeraldComponent.CurrentTargetInfo.TargetSource != null)            // ターゲットが存在する場合
            {
                CurrentTargetPosition = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition(); // ターゲットの被弾基準位置
                float Distance = Vector3.Distance(m_AimSourceParent.position, CurrentTargetPosition);       // Aim親からの距離

                // 注視角度と距離が許容範囲内、かつ状態的に注視可能、またはカバー中の極小角度の場合
                if (CurrentTargetAngle <= LookAtLimit && Distance < LookDistance && !EmeraldComponent.AnimationComponent.IsStunned && !EmeraldComponent.DetectionComponent.TargetObstructed && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AnimationComponent.IsDodging ||
                    CurrentTargetAngle <= 1 && Distance < LookDistance && EmeraldComponent.CoverComponent && EmeraldComponent.CoverComponent.HasCover)
                {
                    if (Distance > 0.75f)                                           // ある程度距離がある場合は高さも追随
                    {
                        float CurrentLookDistance = Vector3.Distance(new Vector3(m_AimSource.position.x, m_AimSource.position.y, CurrentTargetPosition.z), CurrentTargetPosition); // Z除外距離
                        EmeraldComponent.BehaviorsComponent.IsAiming = (CurrentLookDistance > 0.5f); // エイミング状態を更新
                        m_AimGoal = new Vector3(CurrentTargetPosition.x, Mathf.Lerp(m_AimGoal.y, CurrentTargetPosition.y, Time.deltaTime * 3f), CurrentTargetPosition.z); // 目標をターゲットへ補間
                    }
                    else
                    {
                        m_AimGoal = new Vector3(CurrentTargetPosition.x, m_AimGoal.y, CurrentTargetPosition.z); // 近距離はY据え置きで注視
                    }
                }
                // 許容範囲外（距離/角度など）の場合の目線処理
                else
                {
                    if (EmeraldComponent.CombatComponent.CombatState)               // 戦闘中
                    {
                        EmeraldComponent.BehaviorsComponent.IsAiming = false;       // エイミング解除
                        if (!EmeraldComponent.AnimationComponent.IsTurningLeft && !EmeraldComponent.AnimationComponent.IsTurningRight)
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) + transform.up * LookHeightOffset; // 正面
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                        else if (EmeraldComponent.AnimationComponent.IsTurningLeft) // 左旋回中は左寄りを見る
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) + m_AimSourceParent.right * 5 + transform.up * LookHeightOffset;
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                        else if (EmeraldComponent.AnimationComponent.IsTurningRight) // 右旋回中は右寄りを見る
                        {
                            Vector3 LookPos = m_AimSourceParent.position + (transform.forward * EmeraldComponent.CombatComponent.DistanceFromTarget) - m_AimSourceParent.right * 5 + transform.up * LookHeightOffset;
                            m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                        }
                    }
                    else                                                             // 非戦闘（徘徊）時
                    {
                        Vector3 LookPos = m_AimSourceParent.position + (transform.forward * 3) + transform.up * LookHeightOffset; // 前方固定
                        m_AimGoal = new Vector3(LookPos.x, Mathf.Lerp(m_AimGoal.y, LookPos.y, Time.deltaTime), LookPos.z);
                    }
                }
            }
            else
            {
                // ターゲットがいない、死亡遅延ではない、または遮蔽されている等の条件下では正面を見る
                if (!EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.CombatTarget == null && !EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.DetectionComponent.TargetObstructed)
                {
                    // デフォルトの注視位置を前方に設定
                    m_AimGoal = m_AimSourceParent.position + (transform.forward * 3) + transform.up * LookHeightOffset;
                    EmeraldComponent.BehaviorsComponent.IsAiming = false;           // エイミング解除
                }
            }

            // 旋回・回避中は遅く、それ以外は設定速度で補間
            if (!EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AnimationComponent.IsDodging)
                m_AimSource.position = Vector3.Slerp(m_AimSource.position, m_AimGoal, Time.deltaTime * LookSpeed);
            else
                m_AimSource.position = Vector3.Slerp(m_AimSource.position, m_AimGoal, Time.deltaTime * 1);
        }


        void Debugging()                                                              // デバッグ用の可視化（未使用想定）
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)                         // 各Rigを走査
            {
                var ChildMultiAimConstraints = UpperBodyRigsList[i].GetComponentsInChildren<MultiAimConstraint>(); // 制約群取得

                if (ChildMultiAimConstraints.Length > 0)
                {
                    for (int j = 0; j < ChildMultiAimConstraints.Length; j++)
                    {
                        Vector3 AimDir = (m_AimSource.position - ChildMultiAimConstraints[j].data.constrainedObject.position); // 方向ベクトル
                        //Debug.DrawLine(ChildMultiAimConstraints[j].data.constrainedObject.position, AimDir); // 旧可視化
                        Debug.DrawRay(ChildMultiAimConstraints[j].data.constrainedObject.position, AimDir);                    // レイ可視化
                    }
                }
            }
        }

        /// <summary>
        /// 指定した Rig 名のリグをフェードアウトする。
        /// （AnimationEvent.stringParameter = Rig名[,カンマ区切り]、AnimationEvent.floatParameter = フェード速度）
        /// </summary>
        public void FadeOutIK(AnimationEvent animationEvent)
        {
            if (!this.enabled) return;                                               // 無効時は何もしない
            string[] RigNames = animationEvent.stringParameter.Split(',');           // カンマ区切りRig名
            if (RigNames.Length == 0)
            {
                // 単一名として Rig を検索
                Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == animationEvent.stringParameter);
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);       // 既存フェード停止
                if (m_Rig != null) FadeRigCoroutine = StartCoroutine(FadeOutRigInternal(new Rig[] { m_Rig }, animationEvent.floatParameter)); // 開始
                else Debug.LogError("The Rig " + animationEvent.stringParameter + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named."); // 見つからない場合
            }
            else
            {
                List<Rig> m_Rigs = new List<Rig>();                                  // 対象Rig群

                for (int i = 0; i < RigNames.Length; i++)
                {
                    // 先頭がスペースなら取り除く
                    if (RigNames[i][0] == ' ')
                        RigNames[i] = RigNames[i].Substring(1);

                    // 名前一致の Rig を収集
                    Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == RigNames[i]);
                    if (m_Rig != null) m_Rigs.Add(m_Rig);
                    else Debug.LogError("The Rig " + RigNames[i] + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named."); // 未検出警告
                }

                // すべての対象Rigをフェードアウト
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);       // 既存フェード停止
                FadeRigCoroutine = StartCoroutine(FadeOutRigInternal(m_Rigs.ToArray(), animationEvent.floatParameter)); // 開始
            }
        }

        IEnumerator FadeOutRigInternal(Rig[] rigs, float Speed)                      // 内部：Rigを徐々に0へ
        {
            if (EmeraldComponent.CombatComponent.CombatState) FadeInProgress = true; // 戦闘中なら後で自動復帰

            for (int i = 0; i < rigs.Length; i++)                                    // 各Rigに対して
            {
                float t = 0;                                                         // 補間用t
                float StartingWeight = rigs[i].weight;                               // 開始重み

                while (t < 1)                                                        // 0→1まで補間
                {
                    t += Time.deltaTime * Speed;                                     // 経過
                    EmeraldComponent.BehaviorsComponent.IsAiming = true;             // エイミング継続
                    rigs[i].weight = Mathf.Lerp(StartingWeight, 0, t);               // 0へ補間
                    yield return null;                                               // 次フレーム
                }
            }
        }

        /// <summary>
        /// 指定した Rig 名のリグをフェードインする。
        /// （AnimationEvent.stringParameter = Rig名[,カンマ区切り]、AnimationEvent.floatParameter = フェード速度）
        /// </summary>
        public void FadeInIK(AnimationEvent animationEvent)
        {
            if (!this.enabled) return;                                              // 無効時は何もしない
            string[] RigNames = animationEvent.stringParameter.Split(',');           // カンマ区切りRig名
            if (RigNames.Length == 0)
            {
                // 単一名として Rig を検索
                Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == animationEvent.stringParameter);
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);       // 既存フェード停止
                if (m_Rig != null) FadeRigCoroutine = StartCoroutine(FadeInRigInternal(new Rig[] { m_Rig }, animationEvent.floatParameter)); // 開始
                else Debug.LogError("The Rig " + animationEvent.stringParameter + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named."); // 見つからない場合
            }
            else
            {
                List<Rig> m_Rigs = new List<Rig>();                                  // 対象Rig群

                for (int i = 0; i < RigNames.Length; i++)
                {
                    // 先頭スペースを除去
                    if (RigNames[i][0] == ' ')
                        RigNames[i] = RigNames[i].Substring(1);

                    // 名前一致の Rig を収集
                    Rig m_Rig = UpperBodyRigsList.Find(x => x.gameObject.name == RigNames[i]);
                    if (m_Rig != null) m_Rigs.Add(m_Rig);
                    else Debug.LogError("The Rig " + RigNames[i] + " could not be found on the " + gameObject.name + " AI. Please check to ensure everything was properly named."); // 未検出警告
                }

                // すべての対象Rigをフェードイン
                if (FadeRigCoroutine != null) StopCoroutine(FadeRigCoroutine);       // 既存フェード停止
                FadeRigCoroutine = StartCoroutine(FadeInRigInternal(m_Rigs.ToArray(), animationEvent.floatParameter)); // 開始
            }

            FadeInProgress = false;                                                  // フラグリセット
        }

        IEnumerator FadeInRigInternal(Rig[] rigs, float Speed)                       // 内部：Rigを徐々に1へ
        {
            for (int i = 0; i < rigs.Length; i++)
            {
                float t = 0;                                                         // 補間用t
                float StartingWeight = rigs[i].weight;                               // 開始重み

                while (t < 1)                                                        // 0→1まで補間
                {
                    t += Time.deltaTime * Speed;                                     // 経過
                    EmeraldComponent.BehaviorsComponent.IsAiming = true;             // エイミング継続
                    rigs[i].weight = Mathf.Lerp(StartingWeight, 1, t);               // 1へ補間
                    yield return null;                                               // 次フレーム
                }
            }

            FadeRigCoroutine = null;                                                 // コルーチン参照をクリア
        }

        /// <summary>
        /// Inverse Kinematics と RigBuilder を有効化する。
        /// </summary>
        public void EnableInverseKinematics()
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)                        // 各Rigの重みを1に
            {
                UpperBodyRigsList[i].weight = 1;
            }

            m_RigBuilder.enabled = true;                                             // RigBuilder を有効化
            this.enabled = true;                                                     // 本コンポーネントも有効化
        }

        /// <summary>
        /// Inverse Kinematics と RigBuilder を無効化する。
        /// </summary>
        public void DisableInverseKinematics()
        {
            for (int i = 0; i < UpperBodyRigsList.Count; i++)                        // 各Rigの重みを0に
            {
                UpperBodyRigsList[i].weight = 0;
            }

            m_RigBuilder.enabled = false;                                            // RigBuilder を無効化
            this.enabled = false;                                                    // 本コンポーネントも無効化
        }
    }
}