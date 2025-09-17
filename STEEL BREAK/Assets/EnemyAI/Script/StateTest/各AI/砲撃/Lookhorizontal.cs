using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lookhorizontal : MonoBehaviour
{
    //�C��̉���]
    public static void Look_horizontal(Transform m_My, Transform m_Player, float m_rotationSpeed)
    {
        // �v���C���[�̈ʒu
        Vector3 targetPosition = m_Player.transform.position;

        // �����̌��݂̈ʒu
        Vector3 myPosition = m_My.transform.position;

        // Y���������l�����邽�߂ɁA�������Œ�
        targetPosition.y = myPosition.y;

        // �v���C���[�̕������������߂̖ڕW��]
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);

        // ���݂̉�]����ڕW��]�֕�ԁi���O��������j
        m_My.rotation = Quaternion.Lerp(m_My.rotation, targetRotation, Time.deltaTime * m_rotationSpeed);

    }
}
