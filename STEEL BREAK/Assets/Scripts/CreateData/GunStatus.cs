using UnityEngine;

public class GunStatus
{
    private string Name;      //���햼
    private float Rate;       //���˃��[�g
    private float ReloadTime; //�����[�h����
    private int MaxAmmo;      //�ő�e��
    private int Ammo;         //���݂̒e��
    private NewBullet BulletPrefab;       //�e�ۃv���n�u
    private GameObject MuzzleFlashEffect; //�}�Y���t���b�V���̃G�t�F�N�g

    /// <summary>
    /// �R���X�g���N�^ �e��X�e�[�^�X��ݒ�
    /// </summary>
    /// <param name="data"></param>
    public GunStatus(GunStatusData data)
    {
        Name = data.Name;
        Rate = data.Rate;
        ReloadTime = data.ReloadTime;
        MaxAmmo = data.MaxAmmo;
        Ammo = data.MaxAmmo;
        BulletPrefab = data.BulletPrefab;
        MuzzleFlashEffect = data.MuzzleFlashEffect;
    }

    /// <summary>
    /// ���������擾
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return Name;
    }

    /// <summary>
    /// ���˃��[�g���擾
    /// </summary>
    /// <returns></returns>
    public float GetRate()
    {
        return Rate;
    }

    /// <summary>
    /// �����[�h���Ԃ��擾
    /// </summary>
    /// <returns></returns>
    public float GetReloadTime()
    {
        return ReloadTime;
    }

    /// <summary>
    /// �ő�e�����擾
    /// </summary>
    /// <returns></returns>
    public int GetMaxAmmo()
    {
        return MaxAmmo;
    }

    /// <summary>
    /// ���݂̒e�����擾
    /// </summary>
    /// <returns></returns>
    public int GetAmmo()
    {
        return Ammo;
    }

    /// <summary>
    /// ���݂̒e����ݒ�
    /// </summary>
    /// <param name="ammo"></param>
    public void SetAmmo(int ammo)
    {
        Ammo = ammo;
    }

    /// <summary>
    /// �e�ۃv���n�u���擾
    /// </summary>
    /// <returns></returns>
    public NewBullet GetBulletPrefab()
    {
        return BulletPrefab;
    }

    /// <summary>
    /// �}�Y���t���b�V���G�t�F�N�g���擾
    /// </summary>
    /// <returns></returns>
    public GameObject GetMuzzleFlashEffect()
    {
        return MuzzleFlashEffect;
    }
}
