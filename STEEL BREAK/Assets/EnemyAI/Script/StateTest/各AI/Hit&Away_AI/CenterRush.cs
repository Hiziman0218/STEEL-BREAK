using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Center_Rush : MonoBehaviour
{
    /// <summary>
    /// �ːi�s��
    /// </summary>
    public static Vector3 CenterRush(GameObject CenterMarker, Transform m_My, Transform m_Player, float m_AttackDistance)
    {
        ///�܂��́A�v���C���[(�^�[�Q�b�g)�̈ʒu���擾
        Vector3 TargetPosition = m_Player.position;
        ///�Z���^�[�|�C���g�̍��W��(Y�␳�t��)�^�[�Q�b�g�ɍ��킹��
        CenterMarker.transform.position = TargetPosition;
        ///�Z���^�[�|�C���g�̌�����NPC�֌���������
        CenterMarker.transform.LookAt(m_My.position);
        ///�Z���^�[�|�C���g���^�[�Q�b�g����w�蕪��������(���΋����ʒu�w��)
        CenterMarker.transform.position = TargetPosition - CenterMarker.transform.forward * (m_AttackDistance + 5f);
        ///���̒n�_��NPC�̖ڕW�n�_�Ƃ���
        return CenterMarker.transform.position;
    }
}
