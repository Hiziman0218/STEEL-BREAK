using Ilumisoft.RadarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerBase
{
    //デバッグ用 武装をインスペクタで設定
    [SerializeField] private MonoBehaviour m_righthandWeaponMono; //右手武装(デバッグ)
    [SerializeField] private MonoBehaviour m_lefthandWeaponMono;  //左手武装(デバッグ)

    //パーツ設定用オブジェクト
    public MechAssemblyManager PartsSet;

    [SerializeField] private GameObject m_destroyEffect;

    private InputManager inputManager; //入力受け取りクラス
    private Movement movement;         //コントローラーやキーによる移動

    private ProgressBar m_HPBar;      //HPバー
    private float m_HPRate;           //現在の耐久割合
    private ProgressBar m_boostGauge; //ブーストゲージ
    private float m_boostRate;        //現在のブースト割合
    private Radar m_radar;            //プレイヤーを中心とするレーダー

    /// <summary>
    /// 初期化
    /// </summary>
    protected override void Initialize()
    {
        //基底クラスの初期化呼び出し
        base.Initialize();

        PartsSet.SetPlayer(this);

        //各制御クラスを取得
        inputManager = GetComponent<InputManager>();
        movement = GetComponent<Movement>();
        IK = GetComponent<IK_Control>();

        // インスペクタ上で設定したモノビヘイビア型の武装を武装クラスに変換し設定
        //EquipWeapon(m_righthandWeaponMono as IWeapon, WeaponSlot.RightHand);
        //EquipWeapon(m_lefthandWeaponMono as IWeapon, WeaponSlot.LeftHand);
    }

    void Update()
    {
        //右手の攻撃入力を受け取っていたら
        if (inputManager.IsFireinRightHand)
        {
            //武装が設定されているかを確認し、使用
            m_righthandWeapon?.Use();
        }
        //左手の攻撃入力を受け取っていたら
        if (inputManager.IsFireinLeftHand)
        {
            //武装が設定されているかを確認し、使用
            m_lefthandWeapon?.Use();
        }
        //手動リロード入力を受け取っていたら
        if (inputManager.IsReload)
        {
            //各武装が設定されているかを確認し、リロード
            m_righthandWeapon?.Reload();
            m_lefthandWeapon?.Reload();
        }

        //割合計算
        UpdateRate();

        //HPが0以下なら、破壊エフェクトを再生し自身を削除、その後ゲームオーバー画面へ遷移
        if(m_status.GetHP() <= 0f)
        {
            Instantiate(m_destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            GameData.ShowGameOver();
        }
    }

    /// <summary>
    /// 各種割合を計算
    /// </summary>
    void UpdateRate()
    {
        if (m_HPBar != null)
        {
            //現在のHP割合を計算
            m_HPRate = m_status.GetHP() / m_status.GetMaxHP() * 100f;
            //HPバーに反映
            m_HPBar.BarValue = m_HPRate;
        }


        if (m_boostGauge != null)
        {
            //現在のブースト割合を計算
            m_boostRate = movement.GetBoost / movement.GetMaxBoost * 100f;
            //ブーストゲージに反映
            m_boostGauge.BarValue = m_boostRate;
        }
    }
    public void SetHPBar(ProgressBar bar)
    {
        m_HPBar = bar;
    }

    public void SetBoostGauge(ProgressBar bar)
    {
        m_boostGauge = bar;
    }

    public void SetRadar(Radar radar)
    {
        m_radar = radar;
        m_radar.player = this;
    }
}