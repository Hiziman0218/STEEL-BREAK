using System.Collections.Generic;
using UnityEngine;

public class Wepon_Fighting : MonoBehaviour , IWeapon
{
    [Header("��{�ݒ�")]
    [SerializeField] private string m_name; //�����̖��O
    private bool m_isIKFinished;            //IK���������Ă��邩�t���O
    [SerializeField] private Collider m_attackCollider; //�����蔻��̃R���C�_�[
    private List<CharaBase> m_hitList = new List<CharaBase>(); //��x�̍U�����œ��������G�̃��X�g(���i�q�b�g�΍�)

    [SerializeField] private Vector3 m_attachOffsetPos;

    private string m_myTeam;

    /// <summary>
    /// �����蔻��
    /// </summary>
    /// <param name="collision">���������I�u�W�F�N�g</param>
    public void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        CharaBase chara = other.GetComponent<CharaBase>();
        //���������I�u�W�F�N�g���L�����Ȃ�
        if (chara != null)
        {
            //���������L���������X�g���ɑ��݂��Ȃ����
            if (!m_hitList.Contains(chara))
            {
                //�_���[�W��^���A�q�b�g���X�g�ɒǉ�(�����������܂ł̓q�b�g���Ȃ�)
                chara.GetDamage(1.0f);
                m_hitList.Add(chara);
            }
        }
    }

    /// <summary>
    /// �U���J�n�̏���
    /// </summary>
    public void AttackStart()
    {
        //���X�g��������
        m_hitList.Clear();
        //�R���C�_�[��L����
        m_attackCollider.enabled = true;
    }

    /// <summary>
    /// �U���I���̏���
    /// </summary>
    public void AttackEnd()
    {
        //�R���C�_�[�𖳌���
        m_attackCollider.enabled = false;
    }

    /// <summary>
    /// �������Ɏ����A����������
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="left"></param>
    public void AttachToHand(Transform hand, bool left)
    {
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        transform.SetParent(hand, false);
        //transform.localPosition = -grip.localPosition;
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= left ? -1f : 1f;
        transform.localPosition = offsetPos;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
    }

    public void Use()
    {
        AttackStart();
    }

    public void Reload()
    {

    }

    public void SetIKFinished(bool IKFinished)
    {
        m_isIKFinished = IKFinished;
    }

    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    public string GetName() => m_name;
}
