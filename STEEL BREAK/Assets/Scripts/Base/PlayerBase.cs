using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : CharaBase
{
    protected IWeapon m_righthandWeapon; //右手武装
    protected IWeapon m_lefthandWeapon;  //左手武装

    [SerializeField] protected Transform m_rightHandTransform; //右手のボーン
    [SerializeField] protected Transform m_leftHandTransform;  //左手のボーン

    protected IK_Control IK; //IK

    [Header("スロットの親（例：MechRoot）")]
    public Transform mechRoot;  // メカ全体のルートノード

    [Header("各スロットのTransform（部位ごとに設定）")]
    public Transform headSlot;
    public Transform bodySlot;
    public Transform weaponSlot;
    public Transform weaponLSlot;
    public Transform boosterSlot;

    // 複数装着が可能な部位（例：腕や脚）
    public Transform[] lArmSlots;
    public Transform[] rArmSlots;
    public Transform[] legSlots;

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
