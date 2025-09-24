using UnityEngine;

[CreateAssetMenu(fileName = "NewGunStatusData", menuName = "Game/GunStatusData")]
public class GunStatusData : ScriptableObject
{
    public string Name;      //���햼
    public float Rate;       //���˃��[�g
    public float ReloadTime; //�����[�h�ɕK�v�Ȏ���
    public int MaxAmmo;      //�ő�e��
    public NewBullet BulletPrefab;       //�e�ۃv���n�u
    public GameObject MuzzleFlashEffect; //�}�Y���t���b�V���G�t�F�N�g
}
