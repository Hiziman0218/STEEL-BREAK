using System;
using UnityEngine;

public class CharaBase : MonoBehaviour
{
    //キャラクターが持つステータスのデータ
    [Header("ステータス設定")]
    [Tooltip("キャラのステータス(StatusDataを設定)")]
    [SerializeField] protected StatusData m_statusData; //インスペクタで設定

    public Action OnDamage;     //ダメージを受けた時のイベント
    public event Action OnDied; //死亡イベント

    protected Status m_status;  //インスペクタで設定されたものを代入

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        //各ステータス情報を設定
        m_status = new Status(m_statusData);
    }

    public void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 破壊(強制的にHPを0にするダメージ)
    /// </summary>
    public void Destruction()
    {
        GetDamage(GetStatus().GetMaxHP());
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    protected void Die()
    {
        //死亡エフェクトが取得出来たら、エフェクトを生成して非表示
        if (m_status.GetDestructionEffect())
        {
            //死亡エフェクトを生成(削除はエフェクトが担当)
            DestructionEffect Effect = Instantiate(m_status.GetDestructionEffect(), transform.position, transform.rotation);
            Effect.SetOwner(gameObject, OnDied);
            //自身を非表示に設定
            gameObject.SetActive(false);
        }
        //死亡エフェクトが取得できなければ、そのまま削除
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 被弾処理
    /// </summary>
    /// <param name="damage">受けるダメージ</param>
    public void GetDamage(float damage)
    {
        m_status.SetHP(m_status.GetHP() - damage);
        OnDamage?.Invoke();
        Debug.Log("ダメージ量 : " + damage);
    }

    /// <summary>
    /// ステータスを取得
    /// </summary>
    /// <returns></returns>
    public Status GetStatus()
    {
        return m_status;
    }

    /// <summary>
    /// 所属するチームを返却
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_status.GetTeam();
    }
}
