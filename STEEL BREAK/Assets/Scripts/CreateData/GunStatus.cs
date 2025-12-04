using UnityEngine;
using Game.Enum;

public class GunStatus
{
    private string Name;        //武装名
    private float Rate;         //発射レート
    private float ReloadTime;   //リロード時間
    private float Speed;        //発射力
    private float Damege;       //ダメージ量
    private float StaggerPower; //与えるよろけ値
    private int MaxAmmo;        //最大弾数
    private int Ammo;           //現在の弾数
    private bool UseMuzzleControl; //銃口の制御を使用するか
    private bool UseMirror;   //反転機能を使うか
    private AttachSide MirrorWhenHeld;    //どちらで装備した時に反転させるか
    private BulletBase BulletPrefab;          //弾丸プレハブ
    private GameObject MuzzleFlashEffect; //マズルフラッシュのエフェクト
    private AudioClip FireSE;             //発射音
    private AudioClip EmptyFireSE;        //空撃ち音
    private AudioClip ReloadSE;           //リロード音

    /// <summary>
    /// コンストラクタ 各種ステータスを設定
    /// </summary>
    /// <param name="data"></param>
    public GunStatus(GunStatusData data)
    {
        Name = data.Name;
        Rate = data.Rate;
        ReloadTime = data.ReloadTime;
        Speed = data.Speed;
        Damege = data.Damage;
        StaggerPower = data.StaggerPower;
        MaxAmmo = data.MaxAmmo;
        Ammo = data.MaxAmmo;
        UseMuzzleControl = data.UseMuzzleControl;
        UseMirror = data.UseMirror;
        MirrorWhenHeld = data.MirrorHand;
        BulletPrefab = data.BulletPrefab;
        MuzzleFlashEffect = data.MuzzleFlashEffect;
        FireSE = data.FireSE;
        EmptyFireSE = data.EmptyFireSE;
        ReloadSE = data.ReloadSE;
    }

    /// <summary>
    /// 武装名を取得
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return Name;
    }

    /// <summary>
    /// 発射レートを取得
    /// </summary>
    /// <returns></returns>
    public float GetRate()
    {
        return Rate;
    }

    /// <summary>
    /// リロード時間を取得
    /// </summary>
    /// <returns></returns>
    public float GetReloadTime()
    {
        return ReloadTime;
    }

    /// <summary>
    /// 弾速を取得
    /// </summary>
    /// <returns></returns>
    public float GetSpeed()
    {
        return Speed;
    }

    /// <summary>
    /// ダメージ量を取得
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        return Damege;
    }

    /// <summary>
    /// 与えるよろけ値を取得
    /// </summary>
    /// <returns></returns>
    public float GetStaggerPower()
    {
        return StaggerPower;
    }

    /// <summary>
    /// 最大弾数を取得
    /// </summary>
    /// <returns></returns>
    public int GetMaxAmmo()
    {
        return MaxAmmo;
    }

    /// <summary>
    /// 現在の弾数を取得
    /// </summary>
    /// <returns></returns>
    public int GetAmmo()
    {
        return Ammo;
    }

    /// <summary>
    /// 現在の弾数を設定
    /// </summary>
    /// <param name="ammo"></param>
    public void SetAmmo(int ammo)
    {
        Ammo = ammo;
    }

    /// <summary>
    /// 銃口の制御を行うか取得
    /// </summary>
    /// <returns></returns>
    public bool GetUseMuzzleControl()
    {
        return UseMuzzleControl;
    }

    /// <summary>
    /// 反転機能を使うかを取得
    /// </summary>
    /// <returns></returns>
    public bool GetUseMirror()
    {
        return UseMirror;
    }

    /// <summary>
    /// どちらで装備した時に反転させるかを取得
    /// </summary>
    /// <returns></returns>
    public AttachSide GetMirrorWhenHeld()
    {
        return MirrorWhenHeld;
    }

    /// <summary>
    /// 弾丸プレハブを取得
    /// </summary>
    /// <returns></returns>
    public BulletBase GetBulletPrefab()
    {
        return BulletPrefab;
    }

    /// <summary>
    /// マズルフラッシュエフェクトを取得
    /// </summary>
    /// <returns></returns>
    public GameObject GetMuzzleFlashEffect()
    {
        return MuzzleFlashEffect;
    }

    /// <summary>
    /// 発射音を取得
    /// </summary>
    /// <returns></returns>
    public AudioClip GetFireSE()
    {
        return FireSE;
    }

    /// <summary>
    /// 空撃ち音を取得
    /// </summary>
    /// <returns></returns>
    public AudioClip GetEmptyFireSE()
    {
        return EmptyFireSE;
    }

    /// <summary>
    /// リロード音を取得
    /// </summary>
    /// <returns></returns>
    public AudioClip GetReloadSE()
    {
        return ReloadSE;
    }
}
