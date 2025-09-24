using UnityEngine;

public class shotgun : MonoBehaviour
{
    public int m_count = 10; //������
    public float m_maxRange; //�ő�p�x
    public float m_minRange; //�ŏ��p�x
    public GameObject m_bulletPrefab; //�e�v���n�u
    [SerializeField] private GameObject m_hitEffect; //�q�b�g���̃G�t�F�N�g

    private void Start()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        for(int i = 0; i <= m_count; i++)
        {
            //�g�U�p�v���n�u�𐶐�
            GameObject Dummy = Instantiate(m_bulletPrefab, transform.position, transform.rotation);
            Dummy.transform.Rotate(new Vector3(Random.Range(m_minRange, m_maxRange), Random.Range(m_minRange, m_maxRange), 0));
            Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 10000.0f);
            Destroy(Dummy, 10f);
        }
    }

    /// <summary>
    /// �����蔻��
    /// </summary>
    /// <param name="other">���������I�u�W�F�N�g</param>
    private void OnTriggerEnter(Collider other)
    {
        var chara = other.GetComponentInParent<CharaBase>();
        //�L�����N�^�[�Ƀ_���[�W��^���A�q�b�g�G�t�F�N�g�𐶐�������A���g���폜
        if (chara != null)
        {
            chara.GetDamage(1.0f);
            Instantiate(m_hitEffect, chara.transform.position, Quaternion.Inverse(transform.rotation));
            Destroy(gameObject);
        }
    }
}
