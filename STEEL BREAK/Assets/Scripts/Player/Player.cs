using Ilumisoft.RadarSystem;
using UnityEngine;
using Game.Enum;
using System;

public class Player : PlayerBase
{
    [Header("デバッグ設定")]
    //デバッグ用 武装をインスペクタで設定
    [SerializeField] private bool m_isDebug = false; //デバッグか
    [SerializeField] private GameObject m_rightHandWeaponMono; //右手武装(デバッグ)
    [SerializeField] private GameObject m_leftHandWeaponMono;  //左手武装(デバッグ)
    [SerializeField] private GameObject m_rightBackWeaponMono; //右背面武装(デバッグ)
    [SerializeField] private GameObject m_leftBackWeaponMono;  //左背面武装(デバッグ)

    [Header("プレハブ設定")]
    [Tooltip("パーツ設定用オブジェクト")]
    [SerializeField] private MechAssemblyManager PartsSet;

    [Tooltip("破壊時エフェクト")]
    [SerializeField] private GameObject m_destroyEffect;

    [Header("基本設定")]
    [SerializeField] private float m_lerpSpeed; //ラープのスピード

    [SerializeField] private Transform cameraRig;
    [SerializeField] private float cameraRotateLerp = 8f;

    [Header("回転スムーズ設定")]
    [Tooltip("プレイヤー本体のYaw補間に使うスムースタイム（秒）")]
    [SerializeField] private float playerYawSmoothTime = 0.08f;
    [Tooltip("カメラリグのYaw補間に使うスムースタイム（秒）")]
    [SerializeField] private float cameraYawSmoothTime = 0.15f;
    [Tooltip("カメラ方向ベクトルのローパス速度（大きいほど追従が速い）")]
    [SerializeField] private float cameraDirSmoothSpeed = 8f;
    [Tooltip("角度変化がこれ以下なら更新を抑えて微振動を防ぐ（度）")]
    [SerializeField] private float yawDeadzoneDegrees = 0.25f;

    private float m_HPRate;           //現在の耐久割合
    private float m_boostRate;        //現在のブースト割合

    // 分離したSmoothDamp用の内部速度（プレイヤー/カメラそれぞれ）
    private float yawVelocityCamera = 0f;
    private float yawVelocityPlayer = 0f;

    // カメラ向き用の低域通過フィルタ（ターゲット方向をスムーズにする）
    private Vector3 smoothedCameraDir = Vector3.zero;

    private int m_laserCount;         //使用しているレーザーの数

    private bool m_isAutoHorizontal;  //水平に戻すか

    private InputManager inputManager; //入力受け取りクラス
    private Movement movement;         //コントローラーやキーによる移動
    private LockOn lockon;             //ロックオン機能

    private ProgressBar m_HPBar;      //HPバー
    private ProgressBar m_boostGauge; //ブーストゲージ
    private Radar m_radar;            //プレイヤーを中心とするレーダー

    /*デバッグ武装保持用*/
    IWeapon DebugWeaponRightH;
    IWeapon DebugWeaponLeftH;
    IWeapon DebugWeaponRightB;
    IWeapon DebugWeaponLeftB;

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Initialize()
    {
        //基底クラスの初期化呼び出し
        base.Initialize();

        //パーツ設定用オブジェクトに自身を設定
        PartsSet.SetPlayer(this);

        //各制御クラスを取得
        inputManager = GetComponent<InputManager>();
        movement = GetComponent<Movement>();
        lockon = GetComponent<LockOn>();
        IK = GetComponent<IK_Control>();

        // デフォルト初期化（カメラ方向フィルタ）
        if (cameraRig != null)
        {
            smoothedCameraDir = cameraRig.forward;
            smoothedCameraDir.y = 0f;
            if (smoothedCameraDir.sqrMagnitude < 0.001f) smoothedCameraDir = transform.forward;
            smoothedCameraDir.Normalize();
        }

        //デバッグ中なら、デバッグ用の処理を行う
        if (m_isDebug)
        {
            DebugWeaponAttach();
        }
    }

    void Update()
    {
        //フラグ管理
        m_isAutoHorizontal = true;

        //武装の使用
        //右手の攻撃入力を受け取っていたら
        if (inputManager.IsFireRightHand)
        {
            //武装が設定されているかを確認し、使用する
            m_rightHandWeapon?.Use();
        }
        //受け取っていなかったら
        else
        {
            //武装が設定されているかを確認し、使用しない
            m_rightHandWeapon?.NotUse();
        }

        //左手の攻撃入力を受け取っていたら
        if (inputManager.IsFireLeftHand)
        {
            //武装が設定されているかを確認し、使用
            m_leftHandWeapon?.Use();
        }
        //受け取っていなかったら
        else
        {
            //武装が設定されているかを確認し、使用しない
            m_leftHandWeapon?.NotUse();
        }

        //右背面の攻撃入力を受け取ったら
        if (inputManager.IsFireRightBack)
        {
            //IWeapon型からWeapon_Backを取得し、できたら発射をリクエスト
            if (m_rightBackWeapon is MonoBehaviour comp)
            {
                Weapon_Back BackWeapon = comp.GetComponent<Weapon_Back>();
                if (BackWeapon != null)
                {
                    BackWeapon.FireRequest();
                    if (BackWeapon.GetUseRotate())
                    {
                        m_isAutoHorizontal = false;
                    }
                }
            }
        }
        //受け取っていなかったら
        else
        {
            //武装が設定されているかを確認し、使用しない
            m_rightBackWeapon?.NotUse();
        }

        //左背面の攻撃入力を受け取ったら
        if (inputManager.IsFireLeftBack)
        {
            //IWeapon型からWeapon_Backを取得し、できたら発射をリクエスト
            if (m_leftBackWeapon is MonoBehaviour comp)
            {
                Weapon_Back BackWeapon = comp.GetComponent<Weapon_Back>();
                if (BackWeapon != null)
                {
                    BackWeapon.FireRequest();
                    if (BackWeapon.GetUseRotate())
                    {
                        m_isAutoHorizontal = false;
                    }
                }
            }
        }
        //受け取っていなかったら
        else
        {
            //武装が設定されているかを確認し、使用しない
            m_leftBackWeapon?.NotUse();
        }

        //手動リロード入力を受け取っていたら、対応するキーの武装を手動リロード
        if (inputManager.IsReloadRightHand) m_rightHandWeapon?.Reload();
        if (inputManager.IsReloadLeftHand) m_leftHandWeapon?.Reload();
        if (inputManager.IsReloadRightBack) m_rightBackWeapon?.Reload();
        if (inputManager.IsReloadLeftBack) m_leftBackWeapon?.Reload();

        //ターゲット変更の入力を受け取っていたら、次のターゲットへロックを変更
        if (inputManager.IsTargetChange) lockon.SwitchTarget();

        //割合計算/反映
        UpdateRate();

        //自動で水平に
        if (m_isAutoHorizontal && !IsFireLaser()) AutoHorizontal();

        //HPが0以下なら、破壊エフェクトを再生し自身を削除、その後ゲームオーバー画面へ遷移
        if (m_status.GetHP() <= 0f)
        {
            Instantiate(m_destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            GameData.ShowGameOver();
        }
    }

    void LateUpdate()
    {
        //レーザー使用中でなければ、ターゲットの方へ向く
        if(!IsFireLaser())
        LookAtTarget();
    }

    /// <summary>
    /// 各種割合を計算し、対応したUIに反映
    /// </summary>
    void UpdateRate()
    {
        //HPバーが設定されていたら
        if (m_HPBar != null)
        {
            //現在のHP割合を計算
            m_HPRate = m_status.GetHP() / m_status.GetMaxHP() * 100f;
            //HPバーに反映
            m_HPBar.BarValue = m_HPRate;
        }

        //ブーストゲージが設定されていたら
        if (m_boostGauge != null)
        {
            //現在のブースト割合を計算
            m_boostRate = movement.GetBoost / movement.GetMaxBoost * 100f;
            //ブーストゲージに反映
            m_boostGauge.BarValue = MathF.Floor(m_boostRate);
        }
    }

    /// <summary>
    /// ターゲットの方へ向く（プレイヤー本体） - Yawのみ滑らかに補間する実装に変更
    /// </summary>
    private void LookAtTarget()
    {
        Transform target = lockon.CurrentTarget;
        if (target == null) return;

        // ターゲット方向（水平のみ）
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f) return;

        // 目標Yaw角を計算
        float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

        // 現在のYaw
        float currentYaw = transform.eulerAngles.y;

        // デッドゾーン: 小さな角度変化は無視して微振動を防ぐ
        float delta = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));
        if (delta < yawDeadzoneDegrees)
        {
            // あまり動かさない（戻りやノイズを防ぐ）
            return;
        }

        // 滑らかにYaw補間（SmoothDampAngleを使用）
        float newYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref yawVelocityPlayer,
            playerYawSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );

        // Pitch/Roll を保持して Yaw のみ適用
        Vector3 euler = transform.eulerAngles;
        euler.y = newYaw;
        transform.eulerAngles = euler;
    }

    /// <summary>
    /// 自身を水平にラープ
    /// </summary>
    private void AutoHorizontal()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = Mathf.LerpAngle(euler.x, 0f, m_lerpSpeed * Time.deltaTime);
        euler.z = Mathf.LerpAngle(euler.z, 0f, m_lerpSpeed * Time.deltaTime);
        transform.eulerAngles = euler;
    }

    /// <summary>
    /// HPバーを設定
    /// </summary>
    /// <param name="bar">HPバー</param>
    public void SetHPBar(ProgressBar bar)
    {
        m_HPBar = bar;
    }

    /// <summary>
    /// ブーストゲージを設定
    /// </summary>
    /// <param name="bar">ブーストゲージ</param>
    public void SetBoostGauge(ProgressBar bar)
    {
        m_boostGauge = bar;
    }

    /// <summary>
    /// レーダーを設定
    /// </summary>
    /// <param name="radar">レーダー</param>
    public void SetRadar(Radar radar)
    {
        m_radar = radar;
        m_radar.player = this;
    }

    /// <summary>
    /// レーザーを使用
    /// </summary>
    public void FireLaser()
    {
        m_laserCount++;
    }

    /// <summary>
    /// レーザーの使用終了
    /// </summary>
    public void EndLaser()
    {
        m_laserCount = Mathf.Max(0, m_laserCount - 1);
    }

    /// <summary>
    /// レーザーを使用中か(使用中ならtrue)
    /// </summary>
    /// <returns></returns>
    public bool IsFireLaser()
    {
        return m_laserCount != 0;
    }

    /// <summary>
    /// インスペクタで設定した武器を装備する(デバッグ)
    /// </summary>
    public void DebugWeaponAttach()
    {
        // 各装備を複製してEquip
        if (m_rightHandWeaponMono != null)
        {
            var rightHandObj = Instantiate(m_rightHandWeaponMono.gameObject);
            DebugWeaponRightH = rightHandObj.GetComponent<IWeapon>();
            EquipWeapon(DebugWeaponRightH, WeaponSlot.RightHand);
        }

        if (m_leftHandWeaponMono != null)
        {
            var leftHandObj = Instantiate(m_leftHandWeaponMono.gameObject);
            DebugWeaponLeftH = leftHandObj.GetComponent<IWeapon>();
            EquipWeapon(DebugWeaponLeftH, WeaponSlot.LeftHand);
        }

        if (m_rightBackWeaponMono != null)
        {
            var rightBackObj = Instantiate(m_rightBackWeaponMono.gameObject);
            DebugWeaponRightB = rightBackObj.GetComponent<IWeapon>();
            EquipWeapon(DebugWeaponRightB, WeaponSlot.RightBack);
        }

        if (m_leftBackWeaponMono != null)
        {
            var leftBackObj = Instantiate(m_leftBackWeaponMono.gameObject);
            DebugWeaponLeftB = leftBackObj.GetComponent<IWeapon>();
            EquipWeapon(DebugWeaponLeftB, WeaponSlot.LeftBack);
        }
    }
}