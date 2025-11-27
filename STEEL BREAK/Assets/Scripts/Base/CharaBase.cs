using Game.Enum;
using System;
using UnityEngine;

public class CharaBase : MonoBehaviour
{
    //キャラクターが持つステータスのデータ
    [Header("ステータス設定")]
    [Tooltip("キャラのステータス(StatusDataを設定)")]
    [SerializeField] protected StatusData m_statusData; //インスペクタで設定

    public System.Action OnDamage; //ダメージを受けた時のイベント

    protected Status m_status; //インスペクタで設定されたものを代入

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        //各ステータス情報を設定
        m_status = new Status(m_statusData);
    }

    public void Awake()
    {
        Initialize();
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
    /// 所属するチームを返却
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_status.GetTeam();
    }
}
