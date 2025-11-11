using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Enum;
using System.Collections.Generic;

public class AmmoDisplay : MonoBehaviour
{
    [Header("右手武器残弾数")]
    [Tooltip("右手武器残弾数")]
    [SerializeField] private TextMeshProUGUI rightHandAmmo;
    [Tooltip("右手武器最大弾数")]
    [SerializeField] private TextMeshProUGUI rightHandMaxAmmo;
    [Tooltip("右手武器リロードオブジェクト")]
    [SerializeField] private GameObject rightHandReloadUI;
    [Tooltip("右手武器リロード画像")]
    [SerializeField] private Image rightHandReloadImage;

    [Header("左手武器残弾数")]
    [Tooltip("左手武器残弾数")]
    [SerializeField] private TextMeshProUGUI leftHandAmmo;
    [Tooltip("左手武器最大弾数")]
    [SerializeField] private TextMeshProUGUI leftHandMaxAmmo;
    [Tooltip("左手武器リロードオブジェクト")]
    [SerializeField] private GameObject leftHandReloadUI;
    [Tooltip("左手武器リロード画像")]
    [SerializeField] private Image leftHandReloadImage;

    [Header("右肩武器残弾数")]
    [Tooltip("右肩武器残弾数")]
    [SerializeField] private TextMeshProUGUI rightBackAmmo;
    [Tooltip("右肩武器最大弾数")]
    [SerializeField] private TextMeshProUGUI rightBackMaxAmmo;
    [Tooltip("右肩武器リロードオブジェクト")]
    [SerializeField] private GameObject rightBackReloadUI;
    [Tooltip("右肩武器リロード画像")]
    [SerializeField] private Image rightBackReloadImage;

    [Header("左肩武器残弾数")]
    [Tooltip("左肩武器残弾数")]
    [SerializeField] private TextMeshProUGUI leftBackAmmo;
    [Tooltip("左肩武器最大弾数")]
    [SerializeField] private TextMeshProUGUI leftBackMaxAmmo;
    [Tooltip("左肩武器リロードオブジェクト")]
    [SerializeField] private GameObject leftBackReloadUI;
    [Tooltip("左肩武器リロード画像")]
    [SerializeField] private Image leftBackReloadImage;

    [Header("回転設定")]
    [Tooltip("回転速度")]
    [SerializeField] private float rotateSpeed;

    // 各リロード画像の角度
    private Dictionary<Image, float> angleDict = new Dictionary<Image, float>();
    // 各リロードUIの表示状態（毎フレームSetActive防止用）
    private Dictionary<GameObject, bool> reloadUIStates = new Dictionary<GameObject, bool>();

    private IWeapon RH, LH, RB, LB; //各種武器のインターフェース
    private Player m_player;        //プレイヤー
    private float angle = 0f;

    private void Update()
    {
        if (m_player == null) return;
        UpdateAllAmmoUI();
    }

    /// <summary>
    /// 全てのUIを更新
    /// </summary>
    public void UpdateAllAmmoUI()
    {
        //各武器の情報を取得し反映
        if (RH == null) RH = m_player.GetWeapon(WeaponSlot.RightHand);
        else UpdateWeaponUI(RH, rightHandAmmo, rightHandMaxAmmo, rightHandReloadUI, rightHandReloadImage);

        if (LH == null) LH = m_player.GetWeapon(WeaponSlot.LeftHand);
        else UpdateWeaponUI(LH, leftHandAmmo, leftHandMaxAmmo, leftHandReloadUI, leftHandReloadImage);

        if (RB == null) RB = m_player.GetWeapon(WeaponSlot.RightBack);
        else UpdateWeaponUI(RB, rightBackAmmo, rightBackMaxAmmo, rightBackReloadUI, rightBackReloadImage);

        if (LB == null) LB = m_player.GetWeapon(WeaponSlot.LeftBack);
        else UpdateWeaponUI(LB, leftBackAmmo, leftBackMaxAmmo, leftBackReloadUI, leftBackReloadImage);
    }

    /// <summary>
    /// UIの更新
    /// </summary>
    /// <param name="weapon">更新したいUIと対応する武器</param>
    /// <param name="ammo">残弾数</param>
    /// <param name="maxAmmo">最大弾数</param>
    /// <param name="reloadImage">リロード画像</param>
    private void UpdateWeaponUI(IWeapon weapon, TextMeshProUGUI ammo, TextMeshProUGUI maxAmmo, GameObject reloadObj, Image reloadImg)
    {
        if (weapon == null) return;

        //弾数を文字に設定
        maxAmmo.text = $"{weapon.GetMaxAmmo()}";
        ammo.text = $"{weapon.GetAmmo()}";

        // 弾が0でリロード中なら赤く
        ammo.color = (weapon.GetAmmo() <= 0) ? Color.red : Color.white;

        //リロード中かどうかを確認
        bool isReloading = weapon.IsReloading();

        //弾数が無いなら、文字を赤くしてリロードUIを表示
        if (isReloading)
        {
            ShowReloadUI(reloadObj, reloadImg);
        }
        //弾数が残っているなら、文字を白くしてリロードUIを非表示
        else
        {
            HideReloadUI(reloadObj, reloadImg);
        }
    }

    /// <summary>
    /// リロード中の回転処理
    /// </summary>
    private void ShowReloadUI(GameObject reloadObj, Image reloadImg)
    {
        if (reloadObj == null || reloadImg == null) return;

        // 初回登録
        if (!angleDict.ContainsKey(reloadImg))
            angleDict[reloadImg] = 0f;

        if (!reloadUIStates.ContainsKey(reloadObj) || !reloadUIStates[reloadObj])
        {
            reloadObj.SetActive(true);
            reloadUIStates[reloadObj] = true;
        }

        // 回転更新
        angleDict[reloadImg] -= rotateSpeed * Time.deltaTime;
        reloadImg.rectTransform.localEulerAngles = new Vector3(0, 0, angleDict[reloadImg]);
    }

    /// <summary>
    /// リロード完了時の停止処理
    /// </summary>
    private void HideReloadUI(GameObject reloadObj, Image reloadImg)
    {
        if (reloadObj == null || reloadImg == null) return;

        // 状態が変わった時だけ非表示
        if (!reloadUIStates.ContainsKey(reloadObj) || reloadUIStates[reloadObj])
        {
            reloadObj.SetActive(false);
            reloadUIStates[reloadObj] = false;
        }

        // 角度リセット
        angleDict[reloadImg] = 0f;
        reloadImg.rectTransform.localEulerAngles = Vector3.zero;
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
