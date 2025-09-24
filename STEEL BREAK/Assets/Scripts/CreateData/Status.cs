public class Status
{
    private float m_HP;      //現在の耐久
    private float m_maxHP;   //最大耐久
    private float m_power;   //攻撃力
    private float m_defence; //防御力
    private float m_speed;   //移動速度
    private string m_team;   //所属するチーム

    /// <summary>
    /// コンストラクタ 各種ステータスを設定
    /// </summary>
    /// <param name="data">ステータスデータ</param>
    public Status(StatusData data)
    {
        m_HP = data.HP;
        m_maxHP = data.HP;
        m_power = data.Power;
        m_defence = data.Defence;
        m_speed = data.Speed;
        m_team = data.Team;
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
    /// 攻撃力を取得
    /// </summary>
    /// <returns></returns>
    public float GetPower()
    {
        return m_power;
    }

    /// <summary>
    /// 攻撃力を設定
    /// </summary>
    /// <param name="Power"></param>
    public void SetPower(float Power)
    {
        m_power = Power;
    }

    /// <summary>
    /// 防御力を取得
    /// </summary>
    /// <returns></returns>
    public float GetDefence()
    {
        return m_defence;
    }

    /// <summary>
    /// 防御力を設定
    /// </summary>
    /// <param name="Defence"></param>
    public void SetDefence(float Defence)
    {
        m_defence = Defence;
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
}
