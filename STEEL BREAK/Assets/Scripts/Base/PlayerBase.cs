using UnityEngine;

public class PlayerBase : CharaBase
{
    [Header("�@�̐ݒ�")]
    [Tooltip("�E��̕�������I�u�W�F�N�g")]
    [SerializeField] protected Transform m_rightHandTransform; //�E��̃{�[��
    [Tooltip("����̕�������I�u�W�F�N�g")]
    [SerializeField] protected Transform m_leftHandTransform;  //����̃{�[��

    [Tooltip("Root")]
    public Transform mechRoot; //���J�S�̂̃��[�g�m�[�h

    [Tooltip("neck")]
    public Transform headSlot;
    [Tooltip("spine")]
    public Transform bodySlot;
    [Tooltip("OutPointR")]
    public Transform weaponSlot;
    [Tooltip("OutPointL")]
    public Transform weaponLSlot;
    [Tooltip("chest")]
    public Transform boosterSlot;
    // �����������\�ȕ���(��F�r��r)
    [Tooltip("Left [upperArm�~2, forearm, hand]")]
    public Transform[] lArmSlots;
    [Tooltip("Right [upperArm�~2, forearm, hand]")]
    public Transform[] rArmSlots;
    [Tooltip("hips, Left [things, shin, footm], Right [thing, shin, foot]")]
    public Transform[] legSlots;

    protected IWeapon m_righthandWeapon; //�E�蕐��
    protected IWeapon m_lefthandWeapon;  //���蕐��

    protected IK_Control IK; //IK

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
        //���킪������΁A�ȍ~�̏������s��Ȃ�
        if (weapon == null) return;
        //���g�̕���Ƃ��đ�����A�������̎q���ɐݒ�A����̃`�[�������g�Ɠ����ɐݒ肵�AIK�ɂ�����̏���ݒ�
        switch (slot)
        {
            case WeaponSlot.RightHand:
                m_righthandWeapon = weapon;
                weapon.AttachToHand(m_rightHandTransform, false);
                m_righthandWeapon.SetTeam(m_status.GetTeam());
                if(IK != null)
                {
                    IK.hands[0].Weapon = m_righthandWeapon;
                }
                break;
            case WeaponSlot.LeftHand:
                m_lefthandWeapon = weapon;
                weapon.AttachToHand(m_leftHandTransform, true);
                m_lefthandWeapon.SetTeam(m_status.GetTeam());
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
