using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaBase : MonoBehaviour
{
    //�L�����N�^�[�����X�e�[�^�X�̃f�[�^
    [SerializeField]
    protected StatusData m_statusData; //�C���X�y�N�^�Őݒ�
    protected Status m_status;         //�C���X�y�N�^�Őݒ肳�ꂽ���̂���

    //�L�����N�^�[�����ڍׂ̃f�[�^
    [SerializeField]
    protected CharaData m_charaData; //�C���X�y�N�^�Őݒ�
    protected Parameter m_parameter; //�C���X�y�N�^�Őݒ肳�ꂽ���̂���

    protected string myTeam; //���g����������`�[��

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
        return m_parameter.GetTeam();
    }

    /// <summary>
    /// ��������`�[����ݒ�
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        myTeam = team;
    }
}
