using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    //�v�[���{��
    private ObjectPool<Bullet> bulletPool;
    //���̃v�[�����g���g�p��
    private GameObject shooter;

    ///<summary>
    ///�v�[���������� Weapon_Shooting ����Ăяo��
    ///</summary>
    ///<param name="shooter">�g�p��(Player��Enemy)</param>
    ///<param name="bulletPrefab">�e�ۃv���n�u</param>
    ///<param name="initialSize">�v�[���̏����T�C�Y</param>
    public void Initialize(GameObject shooter, Bullet bulletPrefab, int initialSize = 20)
    {
        this.shooter = shooter;
        bulletPool = new ObjectPool<Bullet>(bulletPrefab, initialSize);
    }

    ///<summary>
    ///�e�𔭎�
    ///</summary>
    public void Fire(Vector3 position, Quaternion rotation)
    {
        //�v�[��������o���A�ʒu/��]���Z�b�g
        var b = bulletPool.Get();
        b.transform.position = position;
        b.transform.rotation = rotation;

        //�e�ɁA�N�����������Ƃǂ̃v�[����������������
        b.Initialize(shooter, bulletPool);

        //�����蔻��J�n
        b.AttackStart();
    }
}