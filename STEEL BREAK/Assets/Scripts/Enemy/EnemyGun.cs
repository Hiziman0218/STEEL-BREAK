using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [SerializeField] private NewBullet bulletPrefab;      // �C���X�y�N�^�Œe�̃v���n�u���Z�b�g
    [SerializeField] private Transform muzzleTransform;    // �C���X�y�N�^�ŏe���ʒu���Z�b�g
    [SerializeField] private GameObject muzzleFlashEffect; //�}�Y���t���b�V���̃G�t�F�N�g
    [SerializeField] private float bulletSpeed = 20f;      // �e��

    private string m_myTeam;
    private Transform targetPoint;  // �uBP�v�I�u�W�F�N�g��Transform

    public void Fire()
    {
        // �܂��^�[�Q�b�g���������Ă��Ȃ���ΒT��
        if (targetPoint == null)
        {
            FindPlayerBP();
            if (targetPoint == null) return;  // ������Ȃ���Ή������Ȃ�
        }

        // ���˕������v�Z
        Vector3 dir = (targetPoint.position - muzzleTransform.position).normalized;

        // �e�𐶐����Č��������킹�ARigidbody�ő��x��^����
        NewBullet bullet = Instantiate(bulletPrefab, muzzleTransform.position, Quaternion.LookRotation(dir));
        bullet.SetTeam(m_myTeam);
        Destroy(bullet.gameObject, 10f);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
        }
    }

    private void FindPlayerBP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform bp = player.transform.Find("BP");
        if (bp != null)
        {
            targetPoint = bp;
        }
    }

    public void SetTeam(string team)
    {
        m_myTeam = team;
    }
}
