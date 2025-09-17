using Ilumisoft.RadarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerBase
{
    //�f�o�b�O�p �������C���X�y�N�^�Őݒ�
    [SerializeField] private MonoBehaviour m_righthandWeaponMono; //�E�蕐��(�f�o�b�O)
    [SerializeField] private MonoBehaviour m_lefthandWeaponMono;  //���蕐��(�f�o�b�O)

    //�p�[�c�ݒ�p�I�u�W�F�N�g
    public MechAssemblyManager PartsSet;

    [SerializeField] private GameObject m_destroyEffect;

    private InputManager inputManager; //���͎󂯎��N���X
    private Movement movement;         //�R���g���[���[��L�[�ɂ��ړ�

    private ProgressBar m_HPBar;      //HP�o�[
    private float m_HPRate;           //���݂̑ϋv����
    private ProgressBar m_boostGauge; //�u�[�X�g�Q�[�W
    private float m_boostRate;        //���݂̃u�[�X�g����
    private Radar m_radar;            //�v���C���[�𒆐S�Ƃ��郌�[�_�[

    /// <summary>
    /// ������
    /// </summary>
    protected override void Initialize()
    {
        //���N���X�̏������Ăяo��
        base.Initialize();

        PartsSet.SetPlayer(this);

        //�e����N���X���擾
        inputManager = GetComponent<InputManager>();
        movement = GetComponent<Movement>();
        IK = GetComponent<IK_Control>();

        // �C���X�y�N�^��Őݒ肵�����m�r�w�C�r�A�^�̕����𕐑��N���X�ɕϊ����ݒ�
        //EquipWeapon(m_righthandWeaponMono as IWeapon, WeaponSlot.RightHand);
        //EquipWeapon(m_lefthandWeaponMono as IWeapon, WeaponSlot.LeftHand);
    }

    void Update()
    {
        //�E��̍U�����͂��󂯎���Ă�����
        if (inputManager.IsFireinRightHand)
        {
            //�������ݒ肳��Ă��邩���m�F���A�g�p
            m_righthandWeapon?.Use();
        }
        //����̍U�����͂��󂯎���Ă�����
        if (inputManager.IsFireinLeftHand)
        {
            //�������ݒ肳��Ă��邩���m�F���A�g�p
            m_lefthandWeapon?.Use();
        }
        //�蓮�����[�h���͂��󂯎���Ă�����
        if (inputManager.IsReload)
        {
            //�e�������ݒ肳��Ă��邩���m�F���A�����[�h
            m_righthandWeapon?.Reload();
            m_lefthandWeapon?.Reload();
        }

        //�����v�Z
        UpdateRate();

        //HP��0�ȉ��Ȃ�A�j��G�t�F�N�g���Đ������g���폜�A���̌�Q�[���I�[�o�[��ʂ֑J��
        if(m_status.GetHP() <= 0f)
        {
            Instantiate(m_destroyEffect, transform.position, transform.rotation);
            Destroy(gameObject);
            GameData.ShowGameOver();
        }
    }

    /// <summary>
    /// �e�튄�����v�Z
    /// </summary>
    void UpdateRate()
    {
        if (m_HPBar != null)
        {
            //���݂�HP�������v�Z
            m_HPRate = m_status.GetHP() / m_status.GetMaxHP() * 100f;
            //HP�o�[�ɔ��f
            m_HPBar.BarValue = m_HPRate;
        }


        if (m_boostGauge != null)
        {
            //���݂̃u�[�X�g�������v�Z
            m_boostRate = movement.GetBoost / movement.GetMaxBoost * 100f;
            //�u�[�X�g�Q�[�W�ɔ��f
            m_boostGauge.BarValue = m_boostRate;
        }
    }
    public void SetHPBar(ProgressBar bar)
    {
        m_HPBar = bar;
    }

    public void SetBoostGauge(ProgressBar bar)
    {
        m_boostGauge = bar;
    }

    public void SetRadar(Radar radar)
    {
        m_radar = radar;
        m_radar.player = this;
    }
}