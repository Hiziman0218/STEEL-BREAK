using UnityEngine;
using Game.Enum;

[CreateAssetMenu(fileName = "NewGunStatusData", menuName = "Game/GunStatusData")]
public class GunStatusData : ScriptableObject
{
    [Tooltip("武装の名前")]
    public string Name;      //武装名
    [Tooltip("発射レート")]
    public float Rate;       //発射レート
    [Tooltip("リロードに必要な時間")]
    public float ReloadTime; //リロード時間
    [Tooltip("弾速")]
    public float Speed;      //弾速
    [Tooltip("与えるダメージ量")]
    public float Damage;     //ダメージ量
    [Tooltip("与えるよろけ値")]
    public float StaggerPower;
    [Tooltip("最大弾数")]
    public int MaxAmmo;      //最大弾数
    [Tooltip("銃口の制御をするか")]
    public bool UseMuzzleControl; //銃口の補正をするか
    [Tooltip("反転機能を使うか(使うならtrue)")]
    public bool UseMirror;        //反転機能を使うか
    [Tooltip("どちらで装備した時に反転させるか")]
    public AttachSide MirrorHand; //どちらで装備した時に反転させるか
    [Tooltip("弾丸のプレハブ")]
    public BulletBase BulletPrefab; //弾丸プレハブ
    [Tooltip("マズルフラッシュのエフェクト")]
    public GameObject MuzzleFlashEffect; //マズルフラッシュエフェクト
    [Tooltip("発射時の効果音")]
    public AudioClip FireSE;      //発射音
    [Tooltip("空撃ちの効果音")]
    public AudioClip EmptyFireSE; //空撃ち音
    [Tooltip("リロード時の効果音")]
    public AudioClip ReloadSE;    //リロード音
    [Tooltip("武器の詳細")]
    public string Detail;         //武器の詳細
}
