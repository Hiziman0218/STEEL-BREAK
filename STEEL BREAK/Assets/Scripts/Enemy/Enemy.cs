using System;
using UnityEngine;

public class Enemy : CharaBase
{
    [SerializeField] private float m_staggerThreshold = 1; //よろけるのに必要なよろけ値

    public event Action<Enemy> OnStagger; //よろけイベント
    public event Action<Enemy> OnDeath;   //死亡イベント
    public event Action<Enemy> OnDiedField; //死亡時のフィールド用イベント

    public EnemyGun weaponR; //右武器
    public EnemyGun weaponL; //左武器

    private float m_currentStagger; //現在のよろけ値

    public bool IsAlive { get; private set; } = true; //生存中か

    protected override void Initialize()
    {
        //基底クラスの初期化処理呼び出し
        base.Initialize();

        //自身の武器のチームを設定
        if (weaponR) weaponR.SetTeam(m_status.GetTeam());
        if (weaponL) weaponL.SetTeam(m_status.GetTeam());
    }

    private void Update()
    {
        Debug.Log("現在のよろけ値 : " + m_currentStagger);
        //よろけ値が最大になればよろけイベント
        if(m_currentStagger >= m_staggerThreshold)
        {
            OnStagger?.Invoke(this);
            m_currentStagger = 0;
            Debug.Log("よろけイベント呼び出し");
        }

        //HPが0以下なら、死亡
        if (m_status.GetHP() <= 0)
        {
            //死亡イベント
            OnDeath?.Invoke(this);
            OnDiedField?.Invoke(this);

            //フラグをfalseにし、死亡処理
            IsAlive = false;
            Die();
        }
    }

    /// <summary>
    /// 右に装備された武装を使用
    /// </summary>
    public void UseR()
    {
        weaponR ?.Fire();
    }

    /// <summary>
    /// 左に装備された武装を使用
    /// </summary>
    public void UseL()
    {
        weaponL ?.Fire();
    }

    /// <summary>
    /// よろけ値を加算
    /// </summary>
    /// <param name="StaggerValue">与えるよろけ値</param>
    public void GetStaggerValue(float StaggerValue)
    {
        m_currentStagger += StaggerValue;
    }
}