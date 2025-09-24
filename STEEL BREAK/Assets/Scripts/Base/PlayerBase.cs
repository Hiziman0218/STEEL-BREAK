using UnityEngine;

public class PlayerBase : CharaBase
{
    [Header("機体設定")]
    [Tooltip("右手の武器を持つオブジェクト")]
    [SerializeField] protected Transform m_rightHandTransform; //右手のボーン
    [Tooltip("左手の武器を持つオブジェクト")]
    [SerializeField] protected Transform m_leftHandTransform;  //左手のボーン

    [Tooltip("Root")]
    public Transform mechRoot; //メカ全体のルートノード

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
    // 複数装着が可能な部位(例：腕や脚)
    [Tooltip("Left [upperArm×2, forearm, hand]")]
    public Transform[] lArmSlots;
    [Tooltip("Right [upperArm×2, forearm, hand]")]
    public Transform[] rArmSlots;
    [Tooltip("hips, Left [things, shin, footm], Right [thing, shin, foot]")]
    public Transform[] legSlots;

    protected IWeapon m_righthandWeapon; //右手武装
    protected IWeapon m_lefthandWeapon;  //左手武装

    protected IK_Control IK; //IK

    //装備できる箇所
    public enum WeaponSlot
    {
        RightHand,
        LeftHand,
        //右背面
        //左背面
    }

    protected override void Initialize()
    {
        base.Initialize();

        IK = GetComponent<IK_Control>();
    }

    /// <summary>
    /// 武装を設定
    /// </summary>
    /// <param name="weapon">設定する武装</param>
    /// <param name="slot">設定したい部位</param>
    public void EquipWeapon(IWeapon weapon, WeaponSlot slot)
    {
        //武器が無ければ、以降の処理を行わない
        if (weapon == null) return;
        //自身の武器として代入し、武器を手の子供に設定、武器のチームを自身と同じに設定し、IKにも武器の情報を設定
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
