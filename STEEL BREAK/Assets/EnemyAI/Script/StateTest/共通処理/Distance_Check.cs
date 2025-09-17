using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Distance_Check : MonoBehaviour
{
    // �v���C���[�Ƃ̋����Ɗp�x���v�Z���鏈��
    /// <summary>
    /// �y�g�����z
    /// Distance_Check.Check(������Transform, �v���C���[��Transform);
    /// 
    /// �Ԃ�l:
    /// - float distance: �v���C���[�Ƃ̋���
    /// - float directionP: �v���C���[���猩������
    /// - float directionE: �G���猩������
    ///
    /// �y��z
    /// (float distance, float direction) = Distance_Check.Check(owner.transform, owner.m_Player);
    /// �����distance��direction���擾���邱�Ƃ��ł���
    /// 
    /// side�̎擾���K�v�Ȃ��ꍇ��
    /// (float distance, _)
    /// �̂悤�ɂ���Ɛ��l�𖳎��ł���炵��
    /// </summary>


    public static (float distance, float directionP, float directionE) Check(Transform m_My, Transform m_Player)
    {
        ///�v���C���[�̐��ʃx�N�g�����擾
        Vector3 playerForward = m_Player.forward;

        // �G�� forward �x�N�g�����擾
        Vector3 enemyForward = m_My.forward;

        // �G����v���C���[�ւ̕����x�N�g��
        Vector3 directionToPlayer = (m_Player.position - m_My.position).normalized;

        // �v���C���[���猩���G�̈ʒu�� Dot �l
        float directionP = Vector3.Dot(playerForward, directionToPlayer);

        // �G���猩���v���C���[�̈ʒu�� Dot �l
        float directionE = Vector3.Dot(enemyForward, directionToPlayer);

        ///�v���C���[�Ƃ̑��΋����`�F�b�N
        float distance = Vector3.Distance(m_My.position, m_Player.position);

        return (distance, directionP, directionE);

    }
}
