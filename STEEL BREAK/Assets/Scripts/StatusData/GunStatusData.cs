using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStatusData", menuName = "Game/GunStatusData")]
public class GunStatusData : ScriptableObject
{
    public string Name;      //武器名
    public float Rate;       //発射間隔
    public float ReloadTime; //リロードに必要な時間
    public int MaxAmmo;      //最大弾数
}
