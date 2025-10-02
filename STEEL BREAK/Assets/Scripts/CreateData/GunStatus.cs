using UnityEngine;

public class GunStatus
{
    private string Name;      //武器名
    private float Rate;       //発射レート
    private float ReloadTime; //リロード時間
    private float Speed;      //発射力
    private float Damege;     //与えるダメージ量
    private int MaxAmmo;      //最大弾数
    private int Ammo;         //現在の弾数
    private Bullet BulletPrefab;       //弾丸プレハブ
    private GameObject MuzzleFlashEffect; //マズルフラッシュのエフェクト

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
        BulletPrefab = data.BulletPrefab;
        MuzzleFlashEffect = data.MuzzleFlashEffect;
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
    /// 与えるダメージ量を取得
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
}
