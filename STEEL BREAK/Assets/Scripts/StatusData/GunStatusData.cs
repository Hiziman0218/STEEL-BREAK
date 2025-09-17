using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStatusData", menuName = "Game/GunStatusData")]
public class GunStatusData : ScriptableObject
{
    public string Name;      //���햼
    public float Rate;       //���ˊԊu
    public float ReloadTime; //�����[�h�ɕK�v�Ȏ���
    public int MaxAmmo;      //�ő�e��
}
