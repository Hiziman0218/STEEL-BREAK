using UnityEngine;
using Game.Enum;

public class GunStatus
{
    private string Name;      //武装名
    private float Rate;       //発射レート
    private float ReloadTime; //リロード時間
    private float Speed;      //発射力
    private float Damege;     //ダメージ量
    private int MaxAmmo;      //最大弾数
    private int Ammo;         //現在の弾数
    private bool UseMirror;   //反転機能を使うか
    private HandSide MirrorWhenHeld;      //どの手で持った時に反転させるか
    private Bullet BulletPrefab;          //弾丸プレハブ
    private GameObject MuzzleFlashEffect; //マズルフラッシュのエフェクト
    private AudioClip FireSE;             //発射音
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
        MaxAmmo = data.MaxAmmo;
        Ammo = data.MaxAmmo;
        UseMirror = data.UseMirror;
        MirrorWhenHeld = data.MirrorHand;
        BulletPrefab = data.BulletPrefab;
        MuzzleFlashEffect = data.MuzzleFlashEffect;
        FireSE = data.FireSE;
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
    /// 反転機能を使うかを取得
    /// </summary>
    /// <returns></returns>
    public bool GetUseMirror()
    {
        return UseMirror;
    }

    /// <summary>
    /// どの手で持った時に反転させるかを取得
    /// </summary>
    /// <returns></returns>
    public HandSide GetMirrorWhenHeld()
    {
        return MirrorWhenHeld;
    }

    /// <summary>
    /// 弾丸プレハブを取得
    /// </summary>
    /// <returns></returns>
    public Bullet GetBulletPrefab()
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
    /// リロード音を取得
    /// </summary>
    /// <returns></returns>
    public AudioClip GetReloadSE()
    {
        return ReloadSE;
    }
}
