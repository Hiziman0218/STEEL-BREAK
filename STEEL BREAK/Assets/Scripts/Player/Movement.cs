using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("移動/ブレーキ設定")]
    [SerializeField] private float moveForce = 10f; //地上・空中ともに左右前後入力を受け付ける力
    [SerializeField] private float maxSpeed = 5f;   //地上での最高水平速度
    [SerializeField] private float brakePower = 5f; //地上の慣性ブレーキ

    [Header("ダッシュ/ブースト設定")]
    [SerializeField] private float dashSpeed = 20f;            //ダッシュ時に水平速度をいきなり上書き
    [SerializeField] private float dashDuration = 0.1f;        //ダッシュ継続時間
    [SerializeField] private float dashConsumptionRate = 30f;  //ダッシュ時のブースト消費
    [SerializeField] private float boostMultiplier = 2.0f;     //ブースト倍率
    [SerializeField] private float boostConsumptionRate = 20f; //ブースト維持時に減らす量
    [SerializeField] private float maxBoost = 100f;            //最大ブースト量
    [SerializeField] private float boostRegenRate = 10f;       //ブースト回復速度

    [Header("上昇/滞空/落下設定")]
    [SerializeField] private float ascendSpeed = 5f;               //長押し時の上昇速度
    [SerializeField] private float initialAscendSpeed = 20f;       //単押しで瞬間的に与える初速
    [SerializeField] private float ascendBrake = 10f;              //ホバー中の垂直慣性ブレーキ
    [SerializeField] private float shortAscendThreshold = 0.15f;   //単押しと長押しのしきい値
    [SerializeField] private float ascendConsumptionRate = 15f;    //上昇中のブースト消費速度（短押し含む）

    [SerializeField] private Camera cameraController;    //Inspector でセット

    //参照
    private Rigidbody rb;       //リジッドボディ
    private InputManager input; //入力受け取りクラス

    //ダッシュ／ブースト関連
    private bool isDashing = false;        //ブーストダッシュしているか
    private float dashTimer = 0f;          //ブーストダッシュの残り時間
    private bool dashHasDirection = true;  //ダッシュ開始時に方向入力があったか
    private bool isBoosting = false;       //ブーストしているか（維持用）
    private float boost;                   //残りブースト

    public float GetMaxBoost => maxBoost; //最大ブースト(読み取り専用)
    public float GetBoost => boost;       //残りブースト(読み取り専用)

    //上昇・滞空・落下関連
    private bool jumpPressed = false;      //ジャンプボタンを押しているか
    private float jumpHoldTimer = 0f;      //ジャンプボタンを押し始めてからの経過時間
    private bool hasStartedAscend = false; //上昇を開始したか
    private bool isFalling = false;        //自由落下中かどうか

    //地面判定用のレイキャスト設定
    private float groundCheckDistance = 0.1f;

    void Awake()
    {
        InitializeReferences();
    }

    /// <summary>
    /// Awake中の参照初期化＆初期ブースト設定
    /// </summary>
    private void InitializeReferences()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();
        boost = maxBoost;
    }

    void Update()
    {
        // カメラ参照の確保（Inspector未設定時はMainを使う）
        EnsureCameraAssigned();

        // ジャンプ開始入力処理（押した瞬間）
        HandleJumpStart();

        // ジャンプ長押しの計測（押し続けている間）
        UpdateJumpHoldTimer();

        // ジャンプ離し（短押し判定やホバーへの遷移）
        HandleJumpRelease();

        // 落下入力（強制落下など）
        HandleFallInput();

        // ブーストダッシュの開始（瞬間）
        TryStartDash();

        // ダッシュタイマーの減算と終了判定
        UpdateDashTimer();

        // 最終的な「維持用ブーストフラグ」を更新（Updateで上書きしている元の挙動を保持）
        UpdateBoostingFlag();
    }

    /// <summary>
    /// FixedUpdateでは物理処理を順序どおりに行う。
    /// - ブースト回復（非消費中）
    /// - 接地時の重力リセット
    /// - ダッシュ処理（処理後リターン）
    /// - ホバー処理（処理後リターン）
    /// - 空中通常処理（処理後リターン）
    /// - 地上移動処理
    /// - 最後に水平速度制限を適用
    /// </summary>
    void FixedUpdate()
    {
        // ブースト回復
        RegenerateBoostIfNeeded();

        // 地上判定が取れれば（上昇中でない）重力やフラグをリセット
        GroundCheckAndResetGravity();

        Vector3 dir = GetRelativeInputDirection();
        Vector3 velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // ダッシュ中はブースト消費だけして終了（元コードの early return）
        if (ProcessDash())
            return;

        // ホバー（長押し上昇）処理（条件満たせば処理して終了）
        if (HandleHover(dir))
            return;

        // 空中通常 / 落下中の処理（条件満たせば処理して終了）
        if (HandleAirMovement(dir))
            return;

        // 地上移動 or ブースト中移動
        HandleGroundMovement(dir, velH);

        // 地上での水平速度制限
        ApplyHorizontalSpeedLimit();
    }

    // ----------------------------
    // Update 内の細かい処理（関数化）
    // ----------------------------
    private void EnsureCameraAssigned()
    {
        if (cameraController == null)
        {
            cameraController = Camera.main;
        }
    }

    /// <summary>
    /// ジャンプの「押した瞬間」を扱う（短押し/長押し開始に先立つ初期化）
    /// </summary>
    private void HandleJumpStart()
    {
        if (input.IsJumpDown)
        {
            jumpPressed = true;
            jumpHoldTimer = 0f;
            hasStartedAscend = false;
            isFalling = false;
            rb.useGravity = false;
        }
    }

    /// <summary>
    /// ジャンプの長押し時間を計測し、短押し→長押ししきい値を超えたら滞空（ホバー）開始フラグを立てる
    /// </summary>
    private void UpdateJumpHoldTimer()
    {
        if (jumpPressed && input.IsJump)
        {
            jumpHoldTimer += Time.deltaTime;
            if (!hasStartedAscend && jumpHoldTimer >= shortAscendThreshold)
            {
                hasStartedAscend = true;
                rb.useGravity = false;
            }
        }
    }

    /// <summary>
    /// ジャンプボタンを離した瞬間の処理
    /// - 短押しなら瞬間上昇を与える（水平移動力を少し加える）
    /// - 共通でホバー状態へ移行（hasStartedAscend = true）
    /// - 元のコードどおり、短押しの際は ascendConsumptionRate を即時消費する（deltaTimeではない）
    /// </summary>
    private void HandleJumpRelease()
    {
        if (input.IsJumpUp && jumpPressed)
        {
            if (!hasStartedAscend)
            {
                // 短押し：瞬間上昇（垂直速度を上書き）
                Vector3 v = rb.linearVelocity;
                v.y = initialAscendSpeed;
                rb.linearVelocity = v;

                // 水平入力があれば少しだけ力を追加
                Vector3 dir = GetRelativeInputDirection();
                if (dir.magnitude > 0.01f)
                    rb.AddForce(dir * moveForce, ForceMode.Force);

                // 元コード同様、短押しで即座にブーストを消費（時間ではない）
                boost = Mathf.Max(0f, boost - ascendConsumptionRate);
            }

            // 共通：ホバー状態へ移行（論理的には短押し・長押しどちらでも）
            hasStartedAscend = true;
            jumpPressed = false;
        }
    }

    /// <summary>
    /// 落下入力（強制落下）を処理
    /// </summary>
    private void HandleFallInput()
    {
        if (input.IsFall)
        {
            isFalling = true;
            jumpPressed = false;
            hasStartedAscend = false;
            rb.useGravity = true;
        }
    }

    /// <summary>
    /// ブーストダッシュ入力の開始処理（瞬間動作）
    /// - 方向入力が無ければ前方向へダッシュ（元コード同様）
    /// - ダッシュ開始時に水平速度を上書きする
    /// </summary>
    private void TryStartDash()
    {
        if (input.IsBoostDash && boost >= dashConsumptionRate)
        {
            Vector3 inputDir = GetRelativeInputDirection();
            Vector3 dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir : transform.forward;

            // 初期加速を上書き
            rb.linearVelocity = dashDir * dashSpeed;
            isDashing = true;
            dashTimer = dashDuration;

            // 開始時の方向入力有無フラグ
            dashHasDirection = inputDir.sqrMagnitude > 0.01f;

            // 元コードはここで isBoosting = true としているが、後段で UpdateBoostingFlag() により上書きされる可能性がある点は保持
            isBoosting = true;
        }
    }

    /// <summary>
    /// ダッシュの持続時間タイマーを更新（Update側）
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
    /// Update の最後で入力に基づいて維持用の isBoosting を決める（元コードと同じ最終代入）
    /// </summary>
    private void UpdateBoostingFlag()
    {
        isBoosting = input.IsBoost && boost > 0f;
    }

    // ----------------------------
    // FixedUpdate 内の細かい処理（関数化）
    // ----------------------------

    /// <summary>
    /// ブーストが消費されていない場合の回復処理
    /// </summary>
    private void RegenerateBoostIfNeeded()
    {
        if (!isBoosting && boost < maxBoost && !isDashing)
        {
            boost = Mathf.Min(maxBoost, boost + boostRegenRate * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 地面判定が取れていて、かつ上昇中でなければ重力を再有効化しフラグをリセット
    /// </summary>
    private void GroundCheckAndResetGravity()
    {
        if (IsGrounded() && !hasStartedAscend && !jumpPressed)
        {
            isFalling = false;
            hasStartedAscend = false;
            rb.useGravity = true;
        }
    }

    /// <summary>
    /// ダッシュ中はブーストを消費し、以降の移動処理を行わず帰る（元コードの early return を保持）
    /// 戻り値: ダッシュ処理が行われたか
    /// </summary>
    private bool ProcessDash()
    {
        if (isDashing)
        {
            boost = Mathf.Max(0f, boost - dashConsumptionRate * Time.fixedDeltaTime);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 長押しによるホバー（上昇維持）処理
    /// 戻り値: ホバー処理が行われたら true（その後 FixedUpdate は終了）
    /// </summary>
    private bool HandleHover(Vector3 dir)
    {
        if (hasStartedAscend && input.IsJump && jumpHoldTimer >= shortAscendThreshold && !isFalling)
        {
            Vector3 v = rb.linearVelocity;
            float vY = isBoosting ? ascendSpeed * boostMultiplier : ascendSpeed;
            v.y = vY;
            rb.linearVelocity = v;

            if (dir.magnitude > 0.01f)
                rb.AddForce(dir * moveForce * (isBoosting ? boostMultiplier : 1f), ForceMode.Force);

            // ホバー中のブースト消費（時間依存）
            if (isBoosting)
                boost = Mathf.Max(0f, boost - ascendConsumptionRate * Time.fixedDeltaTime);

            return true;
        }
        return false;
    }

    /// <summary>
    /// 空中（上昇中または落下中）の通常移動処理
    /// 戻り値: 空中処理が行われたら true（その後 FixedUpdate は終了）
    /// </summary>
    private bool HandleAirMovement(Vector3 dir)
    {
        if ((rb.linearVelocity.y > 0f || isFalling) && !IsGrounded())
        {
            Vector3 currentVel = rb.linearVelocity;
            if (currentVel.y > 0f)
                currentVel.y = Mathf.MoveTowards(currentVel.y, 0f, ascendBrake * Time.fixedDeltaTime);

            Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);
            Vector3 brakeForceAir = -horizontalVel * (brakePower * 0.5f);
            rb.AddForce(brakeForceAir, ForceMode.Force);

            if (dir.magnitude > 0.01f)
                rb.AddForce(dir * moveForce * (isBoosting ? boostMultiplier : 1f), ForceMode.Force);

            // 空中での維持ブースト消費（時間依存）
            if (isBoosting)
                boost = Mathf.Max(0f, boost - boostConsumptionRate * Time.fixedDeltaTime);

            // 垂直成分だけ上書き（水平は既存のまま）
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, currentVel.y, rb.linearVelocity.z);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 地上での通常移動やブースト時の移動処理
    /// </summary>
    private void HandleGroundMovement(Vector3 dir, Vector3 velH)
    {
        if (isBoosting)
        {
            // ブースト使用時は速度上限が増える。消費は時間依存。
            boost = Mathf.Max(0f, boost - boostConsumptionRate * Time.fixedDeltaTime);
            float speedLimit = maxSpeed * boostMultiplier;
            if (dir.magnitude > 0.01f && velH.magnitude < speedLimit)
                rb.AddForce(dir * moveForce * boostMultiplier, ForceMode.Force);
        }
        else
        {
            if (dir.magnitude > 0.01f)
            {
                if (velH.magnitude < maxSpeed)
                    rb.AddForce(dir * moveForce, ForceMode.Force);
            }
            else
            {
                // 入力なし時の慣性ブレーキ
                rb.AddForce(-velH * brakePower, ForceMode.Force);
            }
        }
    }

    /// <summary>
    /// 地上での水平速度を最大値にクランプ（垂直成分は保持）
    /// </summary>
    private void ApplyHorizontalSpeedLimit()
    {
        float limitH = isBoosting ? maxSpeed * boostMultiplier : maxSpeed;
        Vector3 velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (velH.magnitude > limitH)
        {
            Vector3 clamped = velH.normalized * limitH;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }

    /// <summary>
    /// WASD入力（input.m_MovePoint）のX,Z成分を
    /// プレイヤーの正面／右方向にマッピングして返却
    /// （元コードをそのまま関数化）
    /// </summary>
    private Vector3 GetRelativeInputDirection()
    {
        Vector3 raw = input.m_MovePoint;
        Vector3 inputDir = new Vector3(raw.x, 0f, raw.z);
        if (inputDir.sqrMagnitude < 0.0001f) return Vector3.zero;

        // カメラの yaw から擬似的に forward/right を作る
        Quaternion yawRot = Quaternion.Euler(0f, cameraController.transform.eulerAngles.y, 0f);
        Vector3 camF = yawRot * Vector3.forward;
        Vector3 camR = yawRot * Vector3.right;

        Vector3 worldDir = camF * inputDir.z + camR * inputDir.x;
        return worldDir.normalized;
    }

    /// <summary>
    /// Raycastによる接地判定（元の groundCheckDistance を使用）
    /// </summary>
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
