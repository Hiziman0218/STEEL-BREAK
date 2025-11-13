using System;
using UnityEngine;

public class Enemy : CharaBase
{
    public bool IsAlive { get; private set; } = true; //生存中か
    public event Action<Enemy> OnDeath;  //死亡イベント
    public event Action<Enemy> OnDiedField; //死亡時のフィールド用イベント
    public GameObject DestructionEffect; //破壊エフェクト
    public EnemyGun weaponR; //右武器
    public EnemyGun weaponL; //左武器

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
        //デバッグ用
        //UseR();
        //UseL();

        //HPが0以下なら、死亡
        if (m_status.GetHP() <= 0)
        {
            //Die();
            Destroy(gameObject);
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
    /// 死亡処理
    /// </summary>
    private void Die()
    {
        //死亡イベント
        OnDeath?.Invoke(this);
        OnDiedField?.Invoke(this);

        //フラグをfalseにし、エフェクトを再生した後削除
        IsAlive = false;
        if (DestructionEffect)
        {
            Instantiate(DestructionEffect, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Die();
    }
}