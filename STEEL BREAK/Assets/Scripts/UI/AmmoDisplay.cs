using TMPro;
using UnityEngine;
using Game.Enum;

public class AmmoDisplay : MonoBehaviour
{
    [Header("右手武器残弾数")]
    [SerializeField] private TextMeshProUGUI rightHandAmmo;
    [Header("右手武器最大弾数")]
    [SerializeField] private TextMeshProUGUI rightHandMaxAmmo;
    [Header("左手武器残弾数")]
    [SerializeField] private TextMeshProUGUI leftHandAmmo;
    [Header("左手武器最大弾数")]
    [SerializeField] private TextMeshProUGUI leftHandMaxAmmo;
    [Header("右肩武器残弾数")]
    [SerializeField] private TextMeshProUGUI rightBackAmmo;
    [Header("右肩武器最大弾数")]
    [SerializeField] private TextMeshProUGUI rightBackMaxAmmo;
    [Header("左肩武器残弾数")]
    [SerializeField] private TextMeshProUGUI leftBackAmmo;
    [Header("左肩武器最大弾数")]
    [SerializeField] private TextMeshProUGUI leftBackMaxAmmo;

    private IWeapon RH;
    private IWeapon LH;
    private IWeapon RB;
    private IWeapon LB;

    private Player m_player;

    private void Update()
    {
        UpdateAllAmmoUI();
    }

    /// <summary>
    /// 全てのUIを更新
    /// </summary>
    public void UpdateAllAmmoUI()
    {
        //各種武器をプレイヤーから取得し、取得次第反映
        if(RH == null) RH = m_player.GetWeapon(WeaponSlot.RightHand);
        else
        {
            rightHandMaxAmmo.text = $"{RH.GetMaxAmmo()}";
            rightHandAmmo.text = $"{RH.GetAmmo()}";
        }
        if(LH == null) LH = m_player.GetWeapon(WeaponSlot.LeftHand);
        else
        {
            leftHandMaxAmmo.text = $"{LH.GetMaxAmmo()}";
            leftHandAmmo.text = $"{LH.GetAmmo()}";
        }
        if(RB == null) RB = m_player.GetWeapon(WeaponSlot.RightBack);
        else
        {
            rightBackMaxAmmo.text = $"{RB.GetMaxAmmo()}";
            rightBackAmmo.text = $"{RB.GetAmmo()}";
        }
        if(LB == null) LB = m_player.GetWeapon(WeaponSlot.LeftBack);
        else
        {
            leftBackMaxAmmo.text = $"{LB.GetMaxAmmo()}";
            leftBackAmmo.text = $"{LB.GetAmmo()}";
        }
    }

    /// <summary>
    /// プレイヤーを設定
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayer(Player player)
    {
        m_player = player;
    }
}
