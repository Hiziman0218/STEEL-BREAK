using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunStatus : MonoBehaviour
{
    private string Name;      //•Ší–¼
    private float Rate;       //”­ËŠÔŠu
    private float ReloadTime; //ƒŠƒ[ƒh‚É•K—v‚ÈŠÔ
    private int MaxAmmo;      //Å‘å’e”
    private int Ammo;         //Œ»İ‚Ì’e”

    public GunStatus(GunStatusData data)
    {
        Name = data.Name;
        Rate = data.Rate;
        ReloadTime = data.ReloadTime;
        MaxAmmo = data.MaxAmmo;
        Ammo = data.MaxAmmo;
    }

    public string GetName()
    {
        return Name;
    }

    public float GetRate()
    {
        return Rate;
    }

    public float GetReloadTime()
    {
        return ReloadTime;
    }

    public int GetMaxAmmo()
    {
        return MaxAmmo;
    }

    public int GetAmmo()
    {
        return Ammo;
    }

    public void SetAmmo(int ammo)
    {
        Ammo = ammo;
    }
}
