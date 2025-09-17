using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMS : MonoBehaviour
{
    [Header("��{�ݒ�")]
    [SerializeField] private NewBullet m_bulletPrefab;   //�e�ۃv���n�u
    [SerializeField] private Transform m_muzzleTransform; //���ˌ�
    [SerializeField] private GunStatusData m_statusData;  //�e�̐��\(�C���X�y�N�^�Őݒ�)

    private GunStatus m_status;      //�e�̐��\(�C���X�y�N�^�Őݒ肵�����̂���)
    private float m_ElapsedTime;     //�o�ߎ���
    private bool m_FireFlag = false; //���ˉ\��
    private bool m_Reload = false;   //�����[�h����

    void Start()
    {
        //�e�̃X�e�[�^�X��ݒ�
        m_status = new GunStatus(m_statusData);
        //�ŏ��͂����Ɍ��Ă�悤�ɐݒ�
        m_ElapsedTime = m_status.GetRate();
    }

    void Update()
    {
        //���ˉ\�t���O��false�ɐݒ�
        m_FireFlag = false;
        //�o�ߎ��Ԃ��v��
        m_ElapsedTime += Time.deltaTime;

        /*�����[�h����*/
        //�����[�h���Ȃ�
        if (m_Reload)
        {
            //�o�ߎ��Ԃ������[�h���Ԉȏ�Ȃ�
            if (m_ElapsedTime > m_status.GetReloadTime())
            {
                //�����[�h������
                ReloadComplete();
            }
            return; //�ȍ~�̏������s��Ȃ�
        }

        /*�N�[���^�C������*/
        //�o�ߎ��Ԃ����ˊԊu�ȏ�Ȃ�
        if (m_ElapsedTime >= m_status.GetRate())
        {
            //���ˉ\�t���O��true�ɐݒ�
            m_FireFlag = true;
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public void OnFire()
    {
        //���ˉ\�Ȃ�
        if (m_FireFlag)
        {
            //�e�𐶐�
            NewBullet Dummy = Instantiate(m_bulletPrefab, m_muzzleTransform.position, m_muzzleTransform.rotation);
            //�e�ɗ͂������Ĉړ�������(AddForse)
            Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 1000.0f);
            //10�b��ɍ폜
            Destroy(Dummy, 10.0f);
            //�e��������
            m_status.SetAmmo(m_status.GetAmmo() - 1);
            //�e�����c���Ă��邩�m�F�A�c���Ă��Ȃ��Ȃ烊���[�h�J�n
            if(m_status.GetAmmo() <= 0)
            {
                ReloadStart();
            }
            //�o�ߎ��Ԃ����Z�b�g
            m_ElapsedTime = 0f;
        }
    }

    /// <summary>
    /// �����[�h�J�n
    /// </summary>
    public void ReloadStart()
    {
        //�����[�h�t���O��true�ɐݒ�
        m_Reload = true;
    }

    /// <summary>
    /// �����[�h����
    /// </summary>
    private void ReloadComplete()
    {
        //�����[�h�t���O��false�ɐݒ�
        m_Reload = false;
        //�e�����ő�ɐݒ�
        m_status.SetAmmo(m_status.GetMaxAmmo());
    }

    /// <summary>
    /// ���g���q���ɐݒ�
    /// </summary>
    /// <param name="hand">�e�ɂȂ�g�����X�t�H�[��</param>
    public void AttachToHand(Transform hand)
    {
        
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        transform.SetParent(hand, false);
        transform.localPosition = -grip.localPosition;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
        
        //transform.SetParent(hand);
    }
}
