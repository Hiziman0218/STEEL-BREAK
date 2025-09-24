public class Status
{
    private float m_HP;      //���݂̑ϋv
    private float m_maxHP;   //�ő�ϋv
    private float m_power;   //�U����
    private float m_defence; //�h���
    private float m_speed;   //�ړ����x
    private string m_team;   //��������`�[��

    /// <summary>
    /// �R���X�g���N�^ �e��X�e�[�^�X��ݒ�
    /// </summary>
    /// <param name="data">�X�e�[�^�X�f�[�^</param>
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
    /// HP���擾
    /// </summary>
    /// <returns></returns>
    public float GetHP()
    {
        return m_HP;
    }

    /// <summary>
    /// HP��ݒ�
    /// </summary>
    /// <param name="HP"></param>
    public void SetHP(float HP)
    {
        m_HP = HP;
    }

    /// <summary>
    /// �ő�HP���擾
    /// </summary>
    /// <returns></returns>
    public float GetMaxHP()
    {
        return m_maxHP;
    }

    /// <summary>
    /// �ő�HP��ݒ�
    /// </summary>
    /// <param name="MaxHP"></param>
    public void SetMaxHP(float MaxHP)
    {
        m_maxHP = MaxHP;
    }

    /// <summary>
    /// �U���͂��擾
    /// </summary>
    /// <returns></returns>
    public float GetPower()
    {
        return m_power;
    }

    /// <summary>
    /// �U���͂�ݒ�
    /// </summary>
    /// <param name="Power"></param>
    public void SetPower(float Power)
    {
        m_power = Power;
    }

    /// <summary>
    /// �h��͂��擾
    /// </summary>
    /// <returns></returns>
    public float GetDefence()
    {
        return m_defence;
    }

    /// <summary>
    /// �h��͂�ݒ�
    /// </summary>
    /// <param name="Defence"></param>
    public void SetDefence(float Defence)
    {
        m_defence = Defence;
    }

    /// <summary>
    /// �ړ����x���擾
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        return m_speed;
    }

    /// <summary>
    /// �ړ����x��ݒ�
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        m_speed = speed;
    }

    /// <summary>
    /// ��������`�[�����擾
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_team;
    }
}
