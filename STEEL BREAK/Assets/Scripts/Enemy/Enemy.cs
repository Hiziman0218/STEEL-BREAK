using System;
using UnityEngine;

public class Enemy : CharaBase
{
    public bool IsAlive { get; private set; } = true; //生存中か
    public event Action<Enemy> OnDeath;  //死亡イベント
    public GameObject DestructionEffect; //破壊エフェクト
    public EnemyGun weaponR; //右武器
    public EnemyGun weaponL; //左武器

    protected override void Initialize()
    {
        //基底クラスの初期化処理呼び出し
        base.Initialize();

        //自身の武器のチームを設定
        weaponR.SetTeam(m_status.GetTeam());
        weaponL.SetTeam(m_status.GetTeam());
    }

    private void Update()
    {
        //UseR();
        //UseL();

        //HPが0以下なら、死亡
        if (m_status.GetHP() <= 0)
        {
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
    /// 死亡処理
    /// </summary>
    private void Die()
    {
        //死亡イベントを通知
        OnDeath?.Invoke(this);

        //フラグをfalseにし、エフェクトを再生した後削除
        IsAlive = false;
        Instantiate(DestructionEffect, transform.position, transform.rotation);
        Destroy(gameObject);
        StageCount();
    }

    /// <summary>
    /// ステージに死亡を通知
    /// </summary>
    private void StageCount()
    {
        GameObject stageObj = GameObject.FindGameObjectWithTag("Stage");
        if (stageObj != null)
        {
            Stage stage = stageObj.GetComponent<Stage>();
            if (stage != null)
            {
                stage.OnEnemyDestroyed();
            }
        }
    }
}