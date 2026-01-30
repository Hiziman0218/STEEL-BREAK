using Ilumisoft.RadarSystem;
using UnityEngine;
using System;

public class Player : PlayerBase
{
    [Header("プレハブ設定")]
    [Tooltip("パーツ設定用オブジェクト")]
    [SerializeField] private MechAssemblyManager m_partsSet;

    [Header("重量設定")]
    [SerializeField] float m_minWeight = 85f;
    [SerializeField] float m_maxWeight = 235f;
    [SerializeField] float m_minMultiplier = 0.8f;
    [SerializeField] float m_maxMultiplier = 1.4f;

    [Header("回転設定")]
    [Tooltip("プレイヤーの回転速度")]
    [SerializeField] private float m_lerpSpeed;
    [Tooltip("プレイヤー本体のYaw補間に使うスムースタイム(秒)")]
    [SerializeField] private float m_playerYawSmoothTime = 0.08f;
    [Tooltip("Yawの最大回転速度（度/秒）")]
    [SerializeField] private float m_maxYawSpeed = 180f;
    [Tooltip("角度変化がこれ以下なら更新を抑えて微振動を防ぐ(度)")]
    [SerializeField] private float m_yawDeadzoneDegrees = 0.25f;

    private float m_HPRate;           //現在の耐久割合
    private float m_boostRate;        //現在のブースト割合

    private float m_weight;           //機体の重さ

    private float m_yawVelocityPlayer = 0f; //分離したSmoothDamp用の内部速度

    private int m_laserCount;         //使用しているレーザーの数

    private bool m_isAutoHorizontal;  //水平に戻すか

    private InputManager m_inputManager; //入力受け取りクラス
    private Movement m_movement;         //コントローラーやキーによる移動
    private LockOn m_lockon;             //ロックオン機能

    private ProgressBar m_HPBar;      //HPバー
    private ProgressBar m_boostGauge; //ブーストゲージ
    private Radar m_radar;            //プレイヤーを中心とするレーダー

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Initialize()
    {
        //基底クラスの初期化呼び出し
        base.Initialize();

        //セーブデータ呼び出し/ステータスの一部にカスタムによる変更を反映
        m_partsSet.SetPlayer(this);
        m_status.SetMaxHP(GameData.mechSaveData.GetTotalAP());
        m_status.SetHP(m_status.GetMaxHP());
        m_weight = GameData.mechSaveData.GetTotalWeight();

        //各制御クラスを取得
        m_inputManager = GetComponent<InputManager>();
        m_movement = GetComponent<Movement>();
        m_lockon = GetComponent<LockOn>();
        IK = GetComponent<IKControl>();

        //ブースト消費量倍率をMovementクラスに設定
        m_movement.SetBoostMultiplierinWeight(GetBoostMultiplier());

        //死亡イベント設定
        OnDied += GameOver;
    }

    void Update()
    {
        //フラグ管理
        m_isAutoHorizontal = true;

        //武装の使用
        //右手の攻撃入力を受け取っていたら
        if (m_inputManager.IsFireRightHand)
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
        if (m_inputManager.IsFireLeftHand)
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
        if (m_inputManager.IsFireRightBack)
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
        if (m_inputManager.IsFireLeftBack)
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
        if (m_inputManager.IsReloadRightHand) m_rightHandWeapon?.Reload();
        if (m_inputManager.IsReloadLeftHand) m_leftHandWeapon?.Reload();
        if (m_inputManager.IsReloadRightBack) m_rightBackWeapon?.Reload();
        if (m_inputManager.IsReloadLeftBack) m_leftBackWeapon?.Reload();

        //ターゲット変更の入力を受け取っていたら、次のターゲットへロックを変更
        if (m_inputManager.IsTargetChange) m_lockon.SwitchTarget();

        //自動で水平に
        if (m_isAutoHorizontal && !IsFireLaser()) AutoHorizontal();

        //HPが0以下なら、死亡処理
        if (m_status.GetHP() <= 0f)
        {
            Die();
        }
    }

    void LateUpdate()
    {
        //レーザー使用中でなければ、ターゲットの方へ向く
        if(!IsFireLaser())
        LookAtTarget();

        //割合計算/反映
        UpdateRate();
    }

    /// <summary>
    /// 各種割合を計算し、対応したUIに反映
    /// </summary>
    void UpdateRate()
    {
        //HPバーが設定されていたら
        if (m_HPBar != null)
        {
            /*
            //現在のHP割合を計算
            m_HPRate = m_status.GetHP() / m_status.GetMaxHP() * 100f;
            //HPバーに反映(体力が完全に0になるまでは最低1%として表示)
            m_HPBar.BarValue = (m_status.GetHP() > 0f) ? Math.Max(1f, MathF.Floor(m_HPRate)) : 0f;*/

            float hp = Mathf.Max(0f, m_status.GetHP());
            float maxHp = m_status.GetMaxHP();

            m_HPRate = hp / maxHp * 100f;
            m_HPBar.BarValue = (hp <= 0f) ? 0f : Math.Max(1f, MathF.Floor(m_HPRate));
        }

        //ブーストゲージが設定されていたら
        if (m_boostGauge != null)
        {
            //現在のブースト割合を計算
            m_boostRate = m_movement.GetBoost / m_movement.GetMaxBoost * 100f;
            //ブーストゲージに反映
            m_boostGauge.BarValue = MathF.Floor(m_boostRate);
        }
    }

    /// <summary>
    /// ターゲットの方へ向く - Yawのみ滑らかに補間する実装に変更
    /// </summary>
    private void LookAtTarget()
    {
        Transform target = m_lockon.CurrentTarget;
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
        if (delta < m_yawDeadzoneDegrees)
        {
            // あまり動かさない（戻りやノイズを防ぐ）
            return;
        }

        // 滑らかにYaw補間（SmoothDampAngleを使用）
        float newYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref m_yawVelocityPlayer,
            m_playerYawSmoothTime,
            m_maxYawSpeed,
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
    /// 重さによるブースト消費量倍率を計算し取得
    /// </summary>
    /// <returns></returns>
    private float GetBoostMultiplier()
    {
        // 0〜1 に正規化
        float normalized =
            Mathf.InverseLerp(m_minWeight, m_maxWeight, m_weight);

        // 安全のためクランプ
        normalized = Mathf.Clamp01(normalized);

        // 倍率に変換
        return Mathf.Lerp(m_minMultiplier, m_maxMultiplier, normalized);
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    private void GameOver()
    {
        //ゲームマネージャーへ死亡を通知
        GameManager.Instance.PlayerDie();
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
}