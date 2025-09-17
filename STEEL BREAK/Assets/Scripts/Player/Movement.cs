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
    private bool dashHasDirection = true;   //ダッシュ開始時に方向入力があったか
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
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputManager>();

        // 初期ブーストを最大に設定
        boost = maxBoost;
    }

    void Update()
    {
        //カメラを取得
        if(cameraController == null) 
        {
            cameraController = Camera.main;
        }

        // ジャンプ開始
        if (input.IsJumpDown)
        {
            jumpPressed = true;
            jumpHoldTimer = 0f;
            hasStartedAscend = false;
            isFalling = false;
            rb.useGravity = false;
        }

        // ジャンプ長押し時間計測
        if (jumpPressed && input.IsJump)
        {
            jumpHoldTimer += Time.deltaTime;
            if (!hasStartedAscend && jumpHoldTimer >= shortAscendThreshold)
            {
                hasStartedAscend = true;
                rb.useGravity = false;
            }
        }

        // ジャンプボタン離し
        if (input.IsJumpUp && jumpPressed)
        {
            if (!hasStartedAscend)
            {
                // 短押し：瞬間上昇
                Vector3 v = rb.linearVelocity;
                v.y = initialAscendSpeed;
                rb.linearVelocity = v;

                // 水平移動力を少し維持
                Vector3 dir = GetRelativeInputDirection();
                if (dir.magnitude > 0.01f)
                    rb.AddForce(dir * moveForce, ForceMode.Force);

                // **短押し上昇のブースト消費を追加**
                boost = Mathf.Max(0f, boost - ascendConsumptionRate);
            }
            // 共通：ホバー状態へ移行
            hasStartedAscend = true;
            jumpPressed = false;
        }

        // 落下入力
        if (input.IsFall)
        {
            isFalling = true;
            jumpPressed = false;
            hasStartedAscend = false;
            rb.useGravity = true;
        }

        // ブーストダッシュ入力
        if (input.IsBoostDash && boost >= dashConsumptionRate)
        {
            //方向入力が無い場合は前方へ自動ダッシュ
            Vector3 dashDir = GetRelativeInputDirection().sqrMagnitude > 0.01f
                ? GetRelativeInputDirection()
                : transform.forward;

            // 初期加速のみ行う
            rb.linearVelocity = dashDir * dashSpeed;
            isDashing = true;
            dashTimer = dashDuration;
            // ダッシュ開始時の方向入力フラグ
            dashHasDirection = GetRelativeInputDirection().sqrMagnitude > 0.01f;
            isBoosting = true;
        }

        // ダッシュタイマー処理
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }

        // 維持用ブーストフラグ更新（通常移動やホバー時に使用）
        isBoosting = input.IsBoost && boost > 0f;
    }

    void FixedUpdate()
    {
        // ブースト非消費中なら常に回復
        if (!isBoosting && boost < maxBoost && !isDashing)
        {
            boost = Mathf.Min(maxBoost, boost + boostRegenRate * Time.fixedDeltaTime);
        }

        // 地上判定：地上かつ昇降中でない場合のみ
        if (IsGrounded() && !hasStartedAscend && !jumpPressed)
        {
            isFalling = false;
            hasStartedAscend = false;
            rb.useGravity = true;
        }

        Vector3 dir = GetRelativeInputDirection();
        Vector3 velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // ダッシュ中処理
        if (isDashing)
        {
            //ダッシュ維持中はブーストを消費し、以降の移動処理を行わない
            boost = Mathf.Max(0f, boost - dashConsumptionRate * Time.fixedDeltaTime);
            return;
        }

        // ホバー中(長押し上昇)
        if (hasStartedAscend && input.IsJump && jumpHoldTimer >= shortAscendThreshold && !isFalling)
        {
            Vector3 v = rb.linearVelocity;
            float vY = isBoosting ? ascendSpeed * boostMultiplier : ascendSpeed;
            v.y = vY;
            rb.linearVelocity = v;

            if (dir.magnitude > 0.01f)
                rb.AddForce(dir * moveForce * (isBoosting ? boostMultiplier : 1f), ForceMode.Force);

            // ブースト消費（ホバー中）
            if (isBoosting)
                boost = Mathf.Max(0f, boost - ascendConsumptionRate * Time.fixedDeltaTime);

            return;
        }

        // 空中通常 or 落下中
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

            // ブースト消費（空中通常）
            if (isBoosting)
                boost = Mathf.Max(0f, boost - boostConsumptionRate * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, currentVel.y, rb.linearVelocity.z);
            return;
        }

        // 地上移動 or ブースト中移動
        if (isBoosting)
        {
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
                rb.AddForce(-velH * brakePower, ForceMode.Force);
            }
        }

        // 地上での速度制限
        float limitH = isBoosting ? maxSpeed * boostMultiplier : maxSpeed;
        velH = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (velH.magnitude > limitH)
        {
            Vector3 clamped = velH.normalized * limitH;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }

    /// <summary>
    /// WASD入力（input.m_MovePoint）のX,Z成分を
    /// プレイヤーの正面／右方向にマッピングして返却
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
    /// Raycastによる接地判定
    /// </summary>
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
