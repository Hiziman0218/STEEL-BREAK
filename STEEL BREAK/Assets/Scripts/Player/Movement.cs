using UnityEngine;

/// <summary>
/// Movement: プレイヤーの移動（地上 / 空中 / ホバー / ダッシュ / 長押しで徐々に落下）を扱うコンポーネント
/// 要点：
/// - Rigidbody を用いて物理ベースに近い移動を行う（rb.linearVelocity の直接操作と AddForce の併用）
/// - 空中は Rigidbody.drag = 0 の前提で、コード側で擬似的な抵抗と補間を入れて操作性を確保している
/// - Inspector のシリアライズフィールドで操作感を調整できるようにしている（airControl 等）
/// - 今回追加した機能：「長押しで徐々に落下（ホバー下降）」
///   - Fall（下降）入力を一定時間長押しすると「ホバー下降（垂直速度を固定してゆっくり落ちる）」に入る
///   - ホバー下降中は重力を切り、垂直速度を descendSpeed に固定（必要に応じて boost による倍率がかかる）
/// 注意：
/// - Rigidbody の設定（mass, drag, interpolation 等）や Ground の PhysicMaterial によって挙動が大きく変わるので、
///   チューニングは必ず複数状況（小ジャンプ / 高滞空 / 着地直後 / ダッシュ）で確認してください。
/// </summary>
public class Movement : MonoBehaviour
{
    [Header("移動/ブレーキ設定")]
    [SerializeField, Tooltip("入力に対して加える基礎力(地上・空中ともに使う補助的な力)")] private float moveForce = 10f;
    [SerializeField, Tooltip("地上での水平最高速度")] private float maxSpeed = 5f;
    [SerializeField, Tooltip("地上で入力がないときに慣性を減らすためのブレーキ係数")] private float brakePower = 5f;

    [Header("ダッシュ/ブースト設定")]
    [SerializeField, Tooltip("ダッシュ時に水平速度を一気に上書きする速度")] private float dashSpeed = 20f;
    [SerializeField, Tooltip("ダッシュ継続時間(秒)")] private float dashDuration = 0.1f;
    [SerializeField, Tooltip("ダッシュ時に消費するブースト量(/s 用ではなく消費率に使用)")] private float dashConsumptionRate = 30f;
    [SerializeField, Tooltip("ブースト状態での速度倍率(地上・空中で適用場所あり)")] private float boostMultiplier = 2.0f;
    [SerializeField, Tooltip("ブーストを維持する際の消費速度(/s)")] private float boostConsumptionRate = 20f;
    [SerializeField, Tooltip("ブーストの最大値")] private float maxBoost = 100f;
    [SerializeField, Tooltip("ブースト回復速度(/s)")] private float boostRegenRate = 10f;

    [Header("上昇/滞空/落下設定")]
    [SerializeField, Tooltip("長押し(ホバー)時の上昇速度(y方向)")] private float ascendSpeed = 5f;
    [SerializeField, Tooltip("短押しで与える瞬間上昇(ジャンプ的な即時速度上書き)")] private float initialAscendSpeed = 20f;
    [SerializeField, Tooltip("上昇中の垂直減速(ホバーに入る際のブレーキ)")] private float ascendBrake = 10f;
    [SerializeField, Tooltip("短押し/長押し判定の閾値(秒)")] private float shortAscendThreshold = 0.15f;
    [SerializeField, Tooltip("上昇中のブースト消費(短押し時の即時消費も兼ねる)")] private float ascendConsumptionRate = 15f;

    [Header("下降/落下設定")]
    [SerializeField, Tooltip("落下時の重力倍率(1 = デフォルト、>1 なら速く落ちる)")]
    private float fallMultiplier = 2.5f;    //落下を早める倍率(経験的に 1.5〜3 が調整範囲)
    [SerializeField, Tooltip("最大落下速度(m/s) 速くしすぎると衝突時の挙動が荒れるので注意")]
    private float maxFallSpeed = 20f;
    [SerializeField, Tooltip("長押しホバーでの下降速度(絶対値)")]
    private float descendSpeed = 5f;

    // --- 空中操作チューニング ---
    [Header("空中操作チューニング")]
    [SerializeField, Tooltip("空中での水平速度補間係数(大きいほど目標速度に速く近づく)")]
    private float airControl = 12f;         //推奨: 5〜20 MoveTowards にして使う場合は「m/s per second」相当で考える
    [SerializeField, Tooltip("空中で目指す水平速度(通常は地上と同程度)")]
    private float airMaxSpeed = 5f;         //地上 maxSpeed と揃える 空中で速くしたければ大きくする
    [SerializeField, Tooltip("空中擬似抵抗(速度に比例して減速する 小さめに設定)")]
    private float airResistance = 0.6f;    //drag=0 の代替 0 にすると擬似抵抗なし(慣性強め)
    [SerializeField, Tooltip("入力補助の乗数(加速補助用 小さい値が自然)")]
    private float airAssistMultiplier = 0.12f; //補助力の割合(0.05〜0.3 程度を試す)

    [SerializeField, Tooltip("Inspectorでセット nullならCamera.mainを使う")] private Camera cameraController;

    // ------------- 参照 -------------
    private Rigidbody rb;       // Rigidbody コンポーネント参照（物理挙動を操作）
    private InputManager input; // 入力管理コンポーネント参照（外部）
    private Player player;      // プレイヤー固有の管理クラス参照（レーザー使用判定等）

    // ------------- ダッシュ／ブースト状態 -------------
    private bool isDashing = false;        // ダッシュ中フラグ
    private float dashTimer = 0f;          // ダッシュ残時間
    private bool dashHasDirection = true;  // ダッシュ開始時に入力があったか
    private bool isBoosting = false;       // ブースト（維持）フラグ
    //private bool isBoostRelease = true;    // ブースト入力をやめたか
    private bool boostExhaustedLocked = false; // ブースト枯渇時に再利用を禁止するロック
    private float boost;                   // 現在のブースト量
    private float moveMultiplier = 1f;     // BackPackから渡される移動倍率(1 = 等倍)

    // 公開用の読み取りプロパティ
    public float GetMaxBoost => maxBoost;
    public float GetBoost => boost;
    public bool IsBoosting => isBoosting;

    // ------------- ジャンプ／滞空／下降（長押し） -------------
    private bool jumpPressed = false;        // ジャンプ入力を押している最中か
    private float jumpHoldTimer = 0f;        // ジャンプ押下継続時間（秒）
    private bool hasStartedAscend = false;   // ホバー（滞空）状態に移行したか
    private bool isFalling = false;          // 強制落下状態かどうか（入力やブースト切れで強制的に落下するフラグ）

    // ↓ ここから「長押しで徐々に落下（ホバー下降）」に関わる変数群
    private bool fallPressed = false;        // 下降キーを押している最中か
    private float fallHoldTimer = 0f;        // 下降長押し時間（秒）
    private bool hasStartedDescend = false;  // ホバー下降に入ったか
    // ↑ これらのフラグで「短押し = 通常落下」「長押し = ホバー下降」を区別する

    private float jumpGraceTimer = 0f;       // ジャンプ直後の接地判定無視時間
    private const float jumpGraceDuration = 0.2f; // 推奨値: 0.05〜0.15

    // 地面判定用 Raycast の長さ（必要に応じて Collider の中心や高さに合わせて調整）
    private float groundCheckDistance = 0.1f;

    void Awake()
    {
        InitializeReferences();
    }

    /// <summary>
    /// コンポーネント参照の初期化とブーストの初期値設定
    /// - Awake で一度だけ呼ぶ
    /// </summary>
    private void InitializeReferences()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();
        player = GetComponent<Player>();
        boost = maxBoost;

        // airMaxSpeed が 0 以下なら地上の maxSpeed をデフォルトにする（Inspectorで上書き推奨）
        if (airMaxSpeed <= 0f) airMaxSpeed = maxSpeed;
    }

    void Update()
    {
        // 非物理部分（入力判定やフラグ更新）は Update で行う（毎フレーム）
        EnsureCameraAssigned();

        // レーザーなどで移動を中断する場合はそれを優先
        if (ShouldSkipMovementByLaser())
            return;

        // ジャンプ入力関連（押した瞬間／長押し計測／離した瞬間）を順に処理
        HandleJumpStart();
        UpdateJumpHoldTimer();
        HandleJumpRelease();

        // 下降（Fall）入力関連の処理（長押しでのホバー下降をここで扱う）
        HandleFallStart();
        UpdateFallHoldTimer();
        HandleFallRelease();

        // ダッシュ開始（瞬間判定）
        TryStartDash();

        if (input.IsBoostDashUp)
        {
            boostExhaustedLocked = false; // ボタンを放したら枯渇ロックを解除
        }

        // ダッシュのタイマー減算（Updateで良い）
        UpdateDashTimer();

        // ブースト維持フラグを最終決定（入力ベース）
        UpdateBoostingFlag();

        // ブーストが無くなれば強制落下（状態更新）
        ForcedFall();

        if (jumpGraceTimer > 0f)
            jumpGraceTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // 物理挙動は FixedUpdate で処理する（AddForce / velocity 変更など）
        if (StopMovementIfLaserActive())
            return;

        RegenerateBoostIfNeeded();
        GroundCheckAndResetGravity();

        // カメラ基準の入力方向（ワールド座標）を取得
        Vector3 dir = GetRelativeInputDirection();

        // 現在の水平速度（XZ 平面のみ）
        Vector3 velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // ダッシュ処理（継続中は他の移動処理をスキップ）
        if (ProcessDash())
            return;

        // ホバー処理（長押しによる上昇維持）を優先
        if (HandleHover(dir))
            return;

        // ホバー下降（長押しでゆっくり落ちる）を優先（上の上昇ホバーと並列）
        if (HandleHoverDescend(dir))
            return;

        // 空中（上昇/落下）処理。条件を満たすなら処理して FixedUpdate を終了
        if (HandleAirMovement(dir))
            return;

        // 地上移動処理
        HandleGroundMovement(dir, velH);

        // 最後に水平速度制限（地上/空中で別扱いしてもよい）
        ApplyHorizontalSpeedLimit();
    }

    // ----------------------------
    // Update 内の細かい処理（各種入力・フラグ管理）
    // ----------------------------
    private void ForcedFall() //削除
    {
        // 滞空（hasStartedAscend）中にブーストが枯渇している、かつまだ落下していないなら強制落下へ
        if (hasStartedAscend && !isFalling && boost <= 0f && !IsGrounded())
        {
            isFalling = true;
            hasStartedAscend = false;
            jumpPressed = false;
            rb.useGravity = true; // 重力復帰
        }
    }

    private void EnsureCameraAssigned()
    {
        // Inspector未設定なら MainCamera を参照する
        if (cameraController == null)
            cameraController = Camera.main;
    }

    private bool ShouldSkipMovementByLaser()
    {
        // player がレーザーを発射中なら移動・ジャンプ関連を停止し、フラグをリセットして true を返す
        if (player == null) return false;

        if (player.IsFireLaser())
        {
            // 移動処理を打ち切る
            return true;
        }

        return false;
    }

    /// <summary>
    /// ジャンプの押下開始（押した瞬間の初期化）
    /// - InputManager の IsJumpDown（GetKeyDown 相当）で検出
    /// - 押された瞬間は重力を切り（rb.useGravity = false）、滞空準備をする
    /// </summary>
    private void HandleJumpStart()
    {
        if (input.IsJumpDown)
        {
            jumpPressed = true;
            jumpHoldTimer = 0f;
            hasStartedAscend = false;
            fallPressed = false;
            hasStartedDescend = false;
            fallHoldTimer = 0f;
            jumpGraceTimer = jumpGraceDuration;
            isFalling = false;
            rb.useGravity = false; // 重力を無効にして滞空の制御を行う
        }
    }

    /// <summary>
    /// ジャンプの長押し時間を計測し、しきい値を超えればホバー状態へ移行（hasStartedAscend = true）
    /// - Update 内で Time.deltaTime を使って計測
    /// </summary>
    private void UpdateJumpHoldTimer()
    {
        if (jumpPressed && input.IsJump)
        {
            jumpHoldTimer += Time.deltaTime;
            if (!hasStartedAscend && jumpHoldTimer >= shortAscendThreshold)
            {
                hasStartedAscend = true;
                rb.useGravity = false; // ホバー中は重力を無効化（垂直速度は明示制御）
            }
        }
    }

    /// <summary>
    /// ジャンプ離し時（短押し判定を含む）
    /// - 短押し（hasStartedAscend が false）なら初速を上書きして瞬間上昇を与える
    /// - 共通でホバー状態へ遷移（hasStartedAscend = true）
    /// </summary>
    private void HandleJumpRelease()
    {
        if (input.IsJumpUp && jumpPressed)
        {
            if (!hasStartedAscend)
            {
                // 短押し：垂直速度を上書きしてジャンプ的な挙動にする
                Vector3 v = rb.linearVelocity;
                v.y = initialAscendSpeed;
                rb.linearVelocity = v;

                // 水平入力があれば少し補助的に力を入れる（短押しでの勢いづけ）
                Vector3 dir = GetRelativeInputDirection();
                if (dir.magnitude > 0.01f)
                    rb.AddForce(dir * moveForce, ForceMode.Force);

                // 短押しでは即時でブーストを消費する仕様（deltaTime ではない）
                boost = Mathf.Max(0f, boost - ascendConsumptionRate);
            }

            hasStartedAscend = true;
            jumpPressed = false; // 押下状態の終了
        }
    }

    /// <summary>
    /// 下降（Fall）押下開始処理
    /// - IsFallDown（GetKeyDown 相当）で検出し、長押し計測を開始する
    /// - 長押し判定までは重力をオフにしておき、短押しなら後で重力を有効にして通常落下に戻す
    /// </summary>
    private void HandleFallStart()
    {
        if (input.IsFallDown)   // GetKeyDown
        {
            fallPressed = true;
            fallHoldTimer = 0f;
            hasStartedDescend = false;

            // 下降判別に入った時点では重力を切る（上昇と対称）
            rb.useGravity = false;
            isFalling = false;
        }
    }

    /// <summary>
    /// 下降（Fall）離し時の処理
    /// - 長押し（hasStartedDescend == true）ならホバー下降を終了（重力は切ったまま）
    /// - 短押しなら通常落下（重力有効、isFalling=true）
    /// </summary>
    private void HandleFallRelease()
    {
        if (input.IsFallUp && fallPressed)
        {
            // 長押し中 → ホバー下降を終了（重力ONにはしない）
            if (hasStartedDescend)
            {
                fallPressed = false;

                // 高度維持用に垂直速度をゼロクリア（ホバー下降をやめて高度を保つ）
                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                rb.linearVelocity = vel;

                rb.useGravity = false;
                return;
            }

            // 単押し → 通常落下へ
            if (!hasStartedDescend)
            {
                fallPressed = false;
                hasStartedDescend = false;

                rb.useGravity = true; // 通常落下に戻す
                isFalling = true;
            }
        }
    }

    /// <summary>
    /// 下降（Fall）長押し時間の更新
    /// - fallPressed=True の間、Time.deltaTime を溜めて shortAscendThreshold を超えたらホバー下降を開始する
    /// - ホバー下降開始時は hasStartedDescend=true、重力はオフのままにする
    /// </summary>
    private void UpdateFallHoldTimer()
    {
        if (fallPressed && input.IsFall)   // 押し続けている間
        {
            fallHoldTimer += Time.deltaTime;

            // 長押し判定 → ホバー下降開始
            if (!hasStartedDescend && fallHoldTimer >= shortAscendThreshold)
            {
                hasStartedDescend = true;
                rb.useGravity = false;    // ホバー下降は重力OFFのまま
            }
        }
    }

    /// <summary>
    /// ブーストダッシュの開始判定
    /// - 入力がありブーストが十分なら水平速度を即座に上書きしてダッシュ状態へ移行
    /// - 入力が無ければ正面方向へダッシュ
    /// </summary>
    private void TryStartDash()
    {
        if (input.IsBoostDash && boost >= dashConsumptionRate && !boostExhaustedLocked)
        {
            Vector3 inputDir = GetRelativeInputDirection();
            Vector3 dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir : transform.forward;

            // 水平速度をダッシュ速度で上書き（瞬間加速）
            rb.linearVelocity = dashDir * dashSpeed;
            isDashing = true;
            dashTimer = dashDuration;
            dashHasDirection = inputDir.sqrMagnitude > 0.01f;
            isBoosting = true;
        }
    }

    /// <summary>
    /// ダッシュタイマーの更新（Update側）
    /// - dashTimer を減らして 0 以下になればダッシュ終了
    /// </summary>
    private void UpdateDashTimer()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }
    }

    /// <summary>
    /// Update の最後でブースト維持フラグを決める
    /// - 入力がありかつ boost が残っていれば isBoosting = true
    /// </summary>
    private void UpdateBoostingFlag()
    {
        isBoosting = input.IsBoost && boost > 0f && !boostExhaustedLocked;
    }

    // ----------------------------
    // FixedUpdate 内の細かい処理（物理関係）
    // ----------------------------
    private bool StopMovementIfLaserActive()
    {
        if (player == null) return false;

        if (player.IsFireLaser())
        {
            // レーザー中は全速度をゼロにして以降の処理をスキップ
            rb.linearVelocity = Vector3.zero;
            return true;
        }

        return false;
    }

    /// <summary>
    /// ブーストの自動回復処理（FixedUpdate タイミング）
    /// - isBoosting 中は回復しない
    /// - ダッシュ中も回復しない
    /// </summary>
    private void RegenerateBoostIfNeeded()
    {
        if (boost <= 0f)
        {
            boost = 0f;
            isBoosting = false;
            // ブーストが枯渇したら再使用を封印（ボタン放しで解除させる）
            boostExhaustedLocked = true;
        }

        if (!isBoosting && boost < maxBoost && !isDashing)
            boost = Mathf.Min(maxBoost, boost + boostRegenRate * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 地面接地時の重力復帰とフラグリセット
    /// - IsGrounded() が true かつ上昇中でなければ重力を戻し、滞空関連フラグをクリア
    /// - jumpGraceTimer による接地無視時間がある場合は判定を遅延する
    /// </summary>
    private void GroundCheckAndResetGravity()
    {
        if (jumpGraceTimer > 0f)
            return;

        if (IsGrounded())
        {
            isFalling = false;

            // 上昇・下降ホバーの両方をリセット
            hasStartedAscend = false;
            hasStartedDescend = false;
            fallPressed = false;

            rb.useGravity = true;
        }
    }

    /// <summary>
    /// ダッシュ処理（継続中は他処理をスキップさせるため true を返す）
    /// - 固有のブースト消費処理のみ行う
    /// </summary>
    private bool ProcessDash()
    {
        if (isDashing)
        {
            // ダッシュ中はブーストを時間依存で消費
            boost = Mathf.Max(0f, boost - dashConsumptionRate * Time.fixedDeltaTime);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ホバー処理（長押しによる上昇維持）
    /// - hasStartedAscend が true かつジャンプ入力が継続している場合に維持
    /// - 垂直速度を ascendSpeed に固定して水平は空中の制御を行う
    /// - ブースト維持中は ascendSpeed に boostMultiplier をかけて上昇を強化し、ブースト消費する
    /// </summary>
    private bool HandleHover(Vector3 dir)
    {
        if (hasStartedAscend && input.IsJump && jumpHoldTimer >= shortAscendThreshold && !isFalling)
        {
            // 垂直は一定速度で上昇（ブースト時は倍率）
            Vector3 currentVel = rb.linearVelocity;
            float vY = isBoosting ? ascendSpeed * boostMultiplier : ascendSpeed;
            currentVel.y = vY;

            // 空中専用の水平制御（補間・抵抗・補助加速）を使う
            ApplyAirControl(dir, currentVel.y);

            // ホバー中はブーストを消費（時間依存）
            if (isBoosting)
                boost = Mathf.Max(0f, boost - ascendConsumptionRate * Time.fixedDeltaTime);

            return true;
        }
        return false;
    }

    /// <summary>
    /// ホバー下降（長押しで徐々に落下）処理
    /// - hasStartedDescend が true かつ下降入力が継続している場合に実行
    /// - 垂直速度を -descendSpeed に固定（isBoosting 時は倍率をかける）
    /// - 重力はオフのまま維持し、水平は空中制御で扱う
    /// - これにより「長押しでゆっくり落ちる」操作が可能になる
    /// </summary>
    private bool HandleHoverDescend(Vector3 dir)
    {
        if (hasStartedDescend && input.IsFall && fallHoldTimer >= shortAscendThreshold)
        {
            // 垂直速度を下降速度に固定（マイナス方向）
            Vector3 currentVel = rb.linearVelocity;
            float vY = -descendSpeed * (isBoosting ? boostMultiplier : 1f);
            currentVel.y = vY;

            // 水平空中制御は上昇ホバーと同様に扱う
            ApplyAirControl(dir, currentVel.y);

            return true;
        }

        return false;
    }

    /// <summary>
    /// 空中（上昇中 or 落下中）での通常移動処理
    /// 実行条件：
    /// - 接地していない（IsGrounded()==false）
    /// - ダッシュ中でない
    /// - かつ (縦速度がある || 強制落下フラグ || ホバー開始済み)
    /// 
    /// 処理ポイント：
    /// - 上昇中は ascendBrake で垂直速度を徐々に減衰
    /// - 落下中（jumpPressed していない場合）は fallMultiplier を加えて重力を強める
    /// - 水平は擬似抵抗（airResistance）と入力に対する補間（ApplyAirControl）で制御
    /// </summary>
    private bool HandleAirMovement(Vector3 dir)
    {
        // 実行条件：接地していない AND (縦速度差がある OR isFalling OR hasStartedAscend)
        bool verticalMoving = Mathf.Abs(rb.linearVelocity.y) > 0.01f;
        if (!IsGrounded() && !isDashing && (verticalMoving || isFalling || hasStartedAscend))
        {
            Vector3 currentVel = rb.linearVelocity;

            // 垂直処理：上昇中は ascendBrake で徐々に落とす
            if (currentVel.y > 0f)
            {
                currentVel.y = Mathf.MoveTowards(currentVel.y, 0f, ascendBrake * Time.fixedDeltaTime);
            }
            else
            {
                // 落下処理は「ジャンプ保持中かどうか」を確認して適用を分ける
                // jumpPressed が true の間はホバーへの遷移待ちとみなして落下強化を控える
                if (!jumpPressed && !hasStartedAscend && !hasStartedDescend)
                {
                    if (fallMultiplier > 1f)
                    {
                        // fallMultiplier による重力強化（ForceMode.Acceleration を使い質量に依存させない）
                        rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
                    }
                    currentVel.y = Mathf.Max(currentVel.y, -Mathf.Abs(maxFallSpeed));
                }
                else
                {
                    // jumpPressed == true の場合は重力抑制（ホバー遷移の可能性あり）
                    // 垂直はそのまま（HandleHover が最終的にホバー速度をつくる）
                }
            }

            // 現在の水平速度（XZ）
            Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);

            // 空中擬似抵抗（速度に比例して減速させる。drag=0 の代替）
            if (airResistance > 0f)
                rb.AddForce(-horizontalVel * airResistance, ForceMode.Acceleration);

            // 入力がある場合は補間で目標水平速度へ滑らかに遷移（ApplyAirControl）
            if (dir.magnitude > 0.01f)
            {
                ApplyAirControl(dir, currentVel.y);

                // 小さめの補助的な加速を入れる（質量に依存させない Acceleration）
                rb.AddForce(dir * moveForce * (isBoosting ? boostMultiplier : 1f) * airAssistMultiplier, ForceMode.Acceleration);

                if (isBoosting)
                    boost = Mathf.Max(0f, boost - boostConsumptionRate * Time.fixedDeltaTime);
            }
            else
            {
                // 入力がなければ垂直のみ上書き（水平は擬似抵抗に任せて減衰させる）
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, currentVel.y, rb.linearVelocity.z);
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// 地上での通常移動（AddForce ベース）
    /// - isBoosting によって速度上限や加速力を変える
    /// - 入力がないときは慣性ブレーキをかける
    /// </summary>
    private void HandleGroundMovement(Vector3 dir, Vector3 velH)
    {
        //バックパックからの倍率反映
        float finalMoveForce = moveForce * moveMultiplier;
        float finalMaxSpeed = maxSpeed * moveMultiplier;

        if (isBoosting)
        {
            boost = Mathf.Max(0f, boost - boostConsumptionRate * Time.fixedDeltaTime);
            float speedLimit = finalMaxSpeed * boostMultiplier;

            if (dir.magnitude > 0.01f && velH.magnitude < speedLimit)
                rb.AddForce(dir * finalMoveForce * boostMultiplier, ForceMode.Force);
        }
        else
        {
            if (dir.magnitude > 0.01f)
            {
                if (velH.magnitude < finalMaxSpeed)
                    rb.AddForce(dir * finalMoveForce, ForceMode.Force);
            }
            else
            {
                rb.AddForce(-velH * brakePower, ForceMode.Force);
            }
        }
    }

    /// <summary>
    /// 水平速度の上限を適用（地上用の maxSpeed を基準）
    /// - 空中と分けたい場合は条件追加して airMaxSpeed を使うようにしてもよい
    /// </summary>
    private void ApplyHorizontalSpeedLimit()
    {
        //バックパックからの倍率反映
        float limitH =
        (isBoosting ? maxSpeed * boostMultiplier : maxSpeed)
        * moveMultiplier;

        Vector3 velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (velH.magnitude > limitH)
        {
            Vector3 clamped = velH.normalized * limitH;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }

    /// <summary>
    /// 空中（ホバー含む）での水平速度補間＋補助力投入の共通処理
    /// - horizontalVel を目標速度へ Lerp で近づける方式
    /// - Lerp の挙動は airControl と Time.fixedDeltaTime の積で影響されるため、
    ///   小さい値だと遅く、大きい値だと素早く目標に近づく
    /// - 最終的に rb.linearVelocity を直接上書きする（垂直は targetY を保持）
    /// - さらに小さめの補助加速を AddForce で投入して操作感を向上させる
    /// </summary>
    private void ApplyAirControl(Vector3 dir, float targetY)
    {
        //バックパックからの倍率反映
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float targetSpeed =
            (isBoosting ? airMaxSpeed * boostMultiplier : airMaxSpeed)
            * moveMultiplier;

        Vector3 desiredH = dir * targetSpeed;

        Vector3 newH = Vector3.Lerp(horizontalVel, desiredH,
            Mathf.Clamp01(airControl * Time.fixedDeltaTime));

        rb.linearVelocity = new Vector3(newH.x, targetY, newH.z);

        // 補助加速も倍率を適用
        float assist = moveForce * moveMultiplier * (isBoosting ? boostMultiplier : 1f) * airAssistMultiplier;
        rb.AddForce(dir * assist, ForceMode.Acceleration);
    }

    /// <summary>
    /// 入力（WASD/左右）をカメラの向きに合わせてワールド方向に変換して返す
    /// - InputManager.m_MovePoint を使っているため、そちらが Update で正しく更新されていることが前提
    /// </summary>
    private Vector3 GetRelativeInputDirection()
    {
        Vector3 raw = input.m_MovePoint;
        Vector3 inputDir = new Vector3(raw.x, 0f, raw.z);
        if (inputDir.sqrMagnitude < 0.0001f) return Vector3.zero;

        Quaternion yawRot = Quaternion.Euler(0f, cameraController.transform.eulerAngles.y, 0f);
        Vector3 camF = yawRot * Vector3.forward;
        Vector3 camR = yawRot * Vector3.right;

        Vector3 worldDir = camF * inputDir.z + camR * inputDir.x;
        return worldDir.normalized;
    }

    /// <summary>
    /// Raycast による接地判定
    /// - groundCheckDistance は Collider と Raycast 起点（transform.position）の差を考慮して調整する
    /// - 判定が不安定なら distance を少し増やす（0.1 → 0.2 等）またはキャラクタコライダの中心を確認
    /// </summary>
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    /// <summary>
    /// BackPack から移動倍率を設定する
    /// </summary>
    public void SetMoveMultiplier(float value)
    {
        moveMultiplier = Mathf.Max(0f, value); // 万が一0以下でも安全に
    }
}