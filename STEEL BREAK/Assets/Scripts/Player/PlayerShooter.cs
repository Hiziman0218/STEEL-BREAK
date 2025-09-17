using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public LockOn lockOn;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public void Shoot()
    {
        Vector3 direction;

        if (lockOn.CurrentTarget != null)
        {
            direction = (lockOn.CurrentTarget.position - firePoint.position).normalized;
        }
        else
        {
            direction = transform.forward; // ���b�N�I�����Ă��Ȃ���ΑO���Ɍ���
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * bulletSpeed;
    }
}
