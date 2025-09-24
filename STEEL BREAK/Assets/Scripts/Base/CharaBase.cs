using UnityEngine;

public class CharaBase : MonoBehaviour
{
    //�L�����N�^�[�����X�e�[�^�X�̃f�[�^
    [Header("�X�e�[�^�X�ݒ�")]
    [Tooltip("�L�����̃X�e�[�^�X(StatusData��ݒ�)")]
    [SerializeField] protected StatusData m_statusData; //�C���X�y�N�^�Őݒ�

    protected Status m_status; //�C���X�y�N�^�Őݒ肳�ꂽ���̂���

    /// <summary>
    /// ������
    /// </summary>
    protected virtual void Initialize()
    {
        //�e�X�e�[�^�X����ݒ�
        m_status = new Status(m_statusData);
    }

    public void Start()
    {
        Initialize();
    }

    /// <summary>
    /// ��e����
    /// </summary>
    /// <param name="damage">�󂯂�_���[�W</param>
    public void GetDamage(float damage)
    {
        m_status.SetHP(m_status.GetHP() - damage);
    }

    /// <summary>
    /// ��������`�[����ԋp
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_status.GetTeam();
    }
}
