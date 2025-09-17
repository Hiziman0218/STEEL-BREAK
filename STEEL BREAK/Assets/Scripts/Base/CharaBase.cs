using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaBase : MonoBehaviour
{
    //キャラクターが持つステータスのデータ
    [SerializeField]
    protected StatusData m_statusData; //インスペクタで設定
    protected Status m_status;         //インスペクタで設定されたものを代入

    //キャラクターが持つ詳細のデータ
    [SerializeField]
    protected CharaData m_charaData; //インスペクタで設定
    protected Parameter m_parameter; //インスペクタで設定されたものを代入

    protected string myTeam; //自身が所属するチーム

    protected virtual void Initialize()
    {
        m_status = new Status(m_statusData);
        m_parameter = new Parameter(m_charaData);
    }

    public void Start()
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
    }

    /// <summary>
    /// 所属するチームを返却
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_parameter.GetTeam();
    }

    /// <summary>
    /// 所属するチームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        myTeam = team;
    }
}
