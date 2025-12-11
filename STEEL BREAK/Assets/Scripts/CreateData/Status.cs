public class Status
{
    private float m_HP;      //現在の耐久
    private float m_maxHP;   //最大耐久
    private float m_speed;   //移動速度
    private string m_team;   //所属チーム
    private DestructionEffect m_destructionEffect; //死亡エフェクト

    /// <summary>
    /// コンストラクタ 各種ステータスを設定
    /// </summary>
    /// <param name="data">ステータスデータ</param>
    public Status(StatusData data)
    {
        m_HP = data.HP;
        m_maxHP = data.HP;
        m_speed = data.Speed;
        m_team = data.Team;
        m_destructionEffect = data.DestructionEffect;
    }

    /// <summary>
    /// HPを取得
    /// </summary>
    /// <returns></returns>
    public float GetHP()
    {
        return m_HP;
    }

    /// <summary>
    /// HPを設定
    /// </summary>
    /// <param name="HP"></param>
    public void SetHP(float HP)
    {
        m_HP = HP;
    }

    /// <summary>
    /// 最大HPを取得
    /// </summary>
    /// <returns></returns>
    public float GetMaxHP()
    {
        return m_maxHP;
    }

    /// <summary>
    /// 最大HPを設定
    /// </summary>
    /// <param name="MaxHP"></param>
    public void SetMaxHP(float MaxHP)
    {
        m_maxHP = MaxHP;
    }

    /// <summary>
    /// 移動速度を取得
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        return m_speed;
    }

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }

    /// <summary>
    /// 所属するチームを取得
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_team;
    }

    /// <summary>
    /// 死亡エフェクトを取得
    /// </summary>
    /// <returns></returns>
    public DestructionEffect GetDestructionEffect()
    {
        return m_destructionEffect;
    }
}
