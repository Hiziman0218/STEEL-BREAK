using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Centering : MonoBehaviour
{
    /// <summary>
    /// ����x���pAI�s��
    /// </summary>
    public static Vector3 CenterPoint(GameObject m_CenterMarker, Transform m_My,Transform m_Player,float m_Muki, float m_AttackDistance)
    {
        ///�܂��́A�v���C���[(�^�[�Q�b�g)�̈ʒu���擾
        Vector3 TargetPosition = m_Player.position;
        ///�^�[�Q�b�g��Y���𑵂���
        TargetPosition.y = m_My.position.y;
        ///�Z���^�[�|�C���g�̍��W��(Y�␳�t��)�^�[�Q�b�g�ɍ��킹��
        m_CenterMarker.transform.position = TargetPosition;
        ///�Z���^�[�|�C���g�̌�����NPC�֌���������
        m_CenterMarker.transform.LookAt(m_My.position);
        ///�P�񕪂̐���p�x����]
        m_CenterMarker.transform.Rotate(new Vector3(0, 10f * m_Muki, 0));
        ///�Z���^�[�|�C���g���^�[�Q�b�g����w�蕪��������(���΋����ʒu�w��)
        m_CenterMarker.transform.Translate(new Vector3(0, 0, m_AttackDistance - 4f));
        ///���̒n�_��NPC�̖ڕW�n�_�Ƃ���
        return m_CenterMarker.transform.position;
    }

    /// <summary>
    /// ���̓I�Ȑ��� ������g���Ă��Ȃ�
    /// </summary>
    /// <param name="m_GuardPointer">���ʒu</param>
    /// <param name="time">���ԁi��]�̊p�x�Ɏg���j</param>
    /// <param name="m_radius">���a�i�����j</param>
    /// <param name="m_RotSpeed">��]���x</param>
    /// <param name="m_Vertical">�㉺�h��̕�</param>
    /// <param name="m_Twist_x">X���̂˂���̕�</param>
    /// <returns></returns>
    public static Vector3 RotAroundGuardPoint3DFixed(Vector3 m_GuardPointer, float time, float m_radius, float m_RotSpeed, float m_Vertical, float m_Twist_x)
    {
        float angle = time * m_RotSpeed;

        // ��]�p��������x�N�g���𐶐�
        Vector3 horizontal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * m_radius;

        // ���������ɂ�炬��������
        float y = Mathf.Sin(angle * 3.9f) * m_Vertical;

        // ����ɂ˂���iX�������j��������
        float xTwist = Mathf.Sin(angle * 3.3f) * m_Twist_x;

        Vector3 offset = new Vector3(horizontal.x + xTwist, y, horizontal.z);
        return m_GuardPointer + offset;
    }

}
