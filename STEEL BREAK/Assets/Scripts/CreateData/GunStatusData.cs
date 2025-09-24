using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStatusData", menuName = "Game/GunStatusData")]
public class GunStatusData : ScriptableObject
{
    public string Name;      //武器名
    public float Rate;       //発射レート
    public float ReloadTime; //リロードに必要な時間
    public int MaxAmmo;      //最大弾数
    public NewBullet BulletPrefab;       //弾丸プレハブ
    public GameObject MuzzleFlashEffect; //マズルフラッシュエフェクト
}
