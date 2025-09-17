using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunStatus : MonoBehaviour
{
    private string Name;      //���햼
    private float Rate;       //���ˊԊu
    private float ReloadTime; //�����[�h�ɕK�v�Ȏ���
    private int MaxAmmo;      //�ő�e��
    private int Ammo;         //���݂̒e��

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
