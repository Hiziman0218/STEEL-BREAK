using UnityEngine;
using Game.Enum;

public class PlayerBase : CharaBase
{
    [Header("機体設定")]
    [Tooltip("右手の武器を持つオブジェクト")]
    [SerializeField] protected Transform m_rightHandTransform; //右手の武装装備ポイント
    [Tooltip("左手の武器を持つオブジェクト")]
    [SerializeField] protected Transform m_leftHandTransform;  //左手の武装装備ポイント
    [Tooltip("右背面の武器を装備するオブジェクト(インスペクタで設定不可)")]
    [SerializeField] protected Transform m_rightBackTransform; //右背面の武装装備ポイント
    [Tooltip("左背面の武器を装備するオブジェクト(インスペクタで設定不可)")]
    [SerializeField] protected Transform m_leftBackTransform;  //左背面の武装装備ポイント

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

    protected IWeapon m_rightHandWeapon; //右手武装
    protected IWeapon m_leftHandWeapon;  //左手武装
    protected IWeapon m_rightBackWeapon; //右背面武装
    protected IWeapon m_leftBackWeapon;  //左背面武装

    protected IK_Control IK; //IK

    protected override void Initialize()
    {
        base.Initialize();

        IK = GetComponent<IK_Control>();
    }

    /// <summary>
    /// 武装を装備
    /// </summary>
    /// <param name="weapon">設定する武装</param>
    /// <param name="slot">設定したい部位</param>
    public void EquipWeapon(IWeapon weapon, WeaponSlot slot)
    {
        //武器が無ければ、以降の処理を行わない
        if (weapon == null) return;
        //自身の武装として代入し、武装を手の子供に設定、武装のチームを自身と同じに設定
        //手持ち武装ならIKにも武装の情報を設定
        switch (slot)
        {
            case WeaponSlot.RightHand:
                m_rightHandWeapon = weapon;
                if(m_rightHandTransform != null)
                {
                    m_rightHandWeapon.AttachToPoint(m_rightHandTransform, AttachSide.Right);
                }
                if(m_status != null) { 
                    m_rightHandWeapon.SetTeam(m_status.GetTeam());
                }
                if(IK != null)
                {
                    IK.hands[0].Weapon = m_rightHandWeapon;
                }
                break;
            case WeaponSlot.LeftHand:
                m_leftHandWeapon = weapon;
                if (m_leftHandTransform != null)
                {
                    m_leftHandWeapon.AttachToPoint(m_leftHandTransform, AttachSide.Left);
                }
                if (m_status != null) { 
                    m_leftHandWeapon.SetTeam(m_status.GetTeam());
                }
                if(IK != null)
                {
                    IK.hands[1].Weapon = m_leftHandWeapon;
                }
                break;
            case WeaponSlot.RightBack:
                m_rightBackWeapon = weapon;
                if(m_rightBackTransform != null)
                {
                    m_rightBackWeapon.AttachToPoint(m_rightBackTransform, AttachSide.Right);
                }
                if (m_status != null)
                {
                    m_rightBackWeapon.SetTeam(m_status.GetTeam());
                }
                break;
            case WeaponSlot.LeftBack:
                m_leftBackWeapon = weapon;
                if (m_leftBackTransform != null)
                {
                    m_leftBackWeapon.AttachToPoint(m_leftBackTransform, AttachSide.Left);
                }
                if (m_status != null)
                {
                    m_leftBackWeapon.SetTeam(m_status.GetTeam());
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 背面武装の装備ポイントを設定
    /// </summary>
    /// <param name="rightPoint"></param>
    /// <param name="leftPoint"></param>
    public void SetBackAttachPoints(Transform rightPoint, Transform leftPoint)
    {
        m_rightBackTransform = rightPoint;
        m_leftBackTransform = leftPoint;
    }
}
