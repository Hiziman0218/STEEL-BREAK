using Ilumisoft.RadarSystem;
using UnityEngine;
using Game.Enum;

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

    private float m_HPRate;           //現在の耐久割合
    private float m_boostRate;        //現在のブースト割合

    private bool m_isAutoHorizontal;  //水平に戻すか

    private InputManager inputManager; //入力受け取りクラス
    private Movement movement;         //コントローラーやキーによる移動

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
        IK = GetComponent<IK_Control>();

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

        //右手の攻撃入力を受け取っていたら
        if (inputManager.IsFireinRightHand)
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
        if (inputManager.IsFireinLeftHand)
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
        if (inputManager.IsFireinRightBack)
        {
            //IWeapon型からWeapon_Backを取得し、できたら発射をリクエスト
            if (m_rightBackWeapon is MonoBehaviour comp)
            {
                Weapon_Back BackWeapon = comp.GetComponent<Weapon_Back>();
                if(BackWeapon != null)
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
        if (inputManager.IsFireinLeftBack)
        {
            //IWeapon型からWeapon_Backを取得し、できたら発射をリクエスト
            if (m_leftBackWeapon is MonoBehaviour comp)
            {
                comp.GetComponent<Weapon_Back>()?.FireRequest();
            }
        }
        //受け取っていなかったら
        else
        {
            //武装が設定されているかを確認し、使用しない
            m_leftBackWeapon?.NotUse();
        }

        //手動リロード入力を受け取っていたら
        if (inputManager.IsReload)
        {
            //各武装が設定されているかを確認し、リロード
            m_rightHandWeapon?.Reload();
            m_leftHandWeapon?.Reload();
        }

        //割合計算/反映
        UpdateRate();

        //自動で水平に
        if (m_isAutoHorizontal) AutoHorizontal();

        //HPが0以下なら、破壊エフェクトを再生し自身を削除、その後ゲームオーバー画面へ遷移
        if(m_status.GetHP() <= 0f)
        {
            Instantiate(m_destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            GameData.ShowGameOver();
        }
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
            m_boostGauge.BarValue = m_boostRate;
        }
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