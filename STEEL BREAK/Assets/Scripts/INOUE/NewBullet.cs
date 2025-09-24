using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBullet : MonoBehaviour
{
    [SerializeField] private Collider m_attackCollider; //�����蔻��̃R���C�_�[
    [SerializeField] private GameObject m_hitEffect;    //�q�b�g���̃G�t�F�N�g
    [SerializeField] private float m_speed;             //�e��(�폜�H)
    [SerializeField] private bool m_disappearOnHit = true; //�q�b�g���ɏ����邩
    private List<CharaBase> m_hitList = new List<CharaBase>(); //��x�̍U�����œ��������G�̃��X�g(���i�q�b�g�΍�)�������e�ۂ��G�ɓ������Ă���������Ȃ炢��Ȃ�
    private string m_myTeam; //���g�̃`�[��

    /// <summary>
    /// ���g�̏�������`�[����ݒ�
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    /// <summary>
    /// �����蔻��
    /// </summary>
    /// <param name="other">���������I�u�W�F�N�g</param>
    private void OnTriggerEnter(Collider other)
    {
        //�L�����Ƃ��Ď擾�A�L�����ł͂Ȃ��Ȃ�A�ȍ~�̏������s��Ȃ�
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;
        //�q�b�g�����L���������g�Ɠ����`�[���Ȃ�A�ȍ~�̏������s��Ȃ�
        if (chara.GetTeam() == m_myTeam) return;
        //���������I�u�W�F�N�g���L�����N�^�[���A�q�b�g���X�g�ɖ�����΁A
        //�L�����N�^�[�Ƀ_���[�W��^���A�q�b�g���X�g�ɒǉ�
        if (chara != null && !m_hitList.Contains(chara))
        {
            chara.GetDamage(1.0f);
            m_hitList.Add(chara);
            //�q�b�g�G�t�F�N�g��L����
            Instantiate(m_hitEffect, chara.transform.position, Quaternion.Inverse(transform.rotation));
            //�q�b�g���ɏ�����e�Ȃ�A���g���폜
            if (m_disappearOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
