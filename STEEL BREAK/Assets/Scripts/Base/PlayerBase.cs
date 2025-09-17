using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : CharaBase
{
    protected IWeapon m_righthandWeapon; //�E�蕐��
    protected IWeapon m_lefthandWeapon;  //���蕐��

    [SerializeField] protected Transform m_rightHandTransform; //�E��̃{�[��
    [SerializeField] protected Transform m_leftHandTransform;  //����̃{�[��

    protected IK_Control IK; //IK

    [Header("�X���b�g�̐e�i��FMechRoot�j")]
    public Transform mechRoot;  // ���J�S�̂̃��[�g�m�[�h

    [Header("�e�X���b�g��Transform�i���ʂ��Ƃɐݒ�j")]
    public Transform headSlot;
    public Transform bodySlot;
    public Transform weaponSlot;
    public Transform weaponLSlot;
    public Transform boosterSlot;

    // �����������\�ȕ��ʁi��F�r��r�j
    public Transform[] lArmSlots;
    public Transform[] rArmSlots;
    public Transform[] legSlots;

    //�����ł���ӏ�
    public enum WeaponSlot
    {
        RightHand,
        LeftHand,
        //�E�w��
        //���w��
    }

    protected override void Initialize()
    {
        base.Initialize();

        IK = GetComponent<IK_Control>();
    }

    /// <summary>
    /// ������ݒ�
    /// </summary>
    /// <param name="weapon">�ݒ肷�镐��</param>
    /// <param name="slot">�ݒ肵��������</param>
    public void EquipWeapon(IWeapon weapon, WeaponSlot slot)
    {
        if (weapon == null) return;
        switch (slot)
        {
            case WeaponSlot.RightHand:
                m_righthandWeapon = weapon;
                weapon.AttachToHand(m_rightHandTransform, false);
                m_righthandWeapon.SetTeam(m_parameter.GetTeam());
                if(IK != null)
                {
                    IK.hands[0].Weapon = m_righthandWeapon;
                }
                break;
            case WeaponSlot.LeftHand:
                m_lefthandWeapon = weapon;
                weapon.AttachToHand(m_leftHandTransform, true);
                m_lefthandWeapon.SetTeam(m_parameter.GetTeam());
                if(IK != null)
                {
                    IK.hands[1].Weapon = m_lefthandWeapon;
                }
                
                break;
            default:
                break;
        }
    }
}
