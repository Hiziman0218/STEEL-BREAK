using System;
using UnityEngine;

public class Enemy : CharaBase
{

    [SerializeField] private float m_staggerThreshold; //よろけるのに必要なよろけ値

    public event Action<Enemy> OnStagger; //よろけイベント
    public event Action<Enemy> OnDeath;  //死亡イベント
    public event Action<Enemy> OnDiedField; //死亡時のフィールド用イベント

    public GameObject DestructionEffect; //破壊エフェクト(ステータスに移す)

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
        //よろけ値が最大になればよろけイベント
        if(m_currentStagger >= m_staggerThreshold)
        {
            OnStagger?.Invoke(this);
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

    /*
    /// <summary>
    /// 死亡処理
    /// </summary>
    private void Die()
    {
        //死亡イベント
        OnDeath?.Invoke(this);
        OnDiedField?.Invoke(this);

        //フラグをfalseにし、死亡エフェクトを再生(削除はDestructionEffectが担当)
        IsAlive = false;
        /*
        if (DestructionEffect)
        {
            Instantiate(DestructionEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject);
        base.Die();
    }*/
}