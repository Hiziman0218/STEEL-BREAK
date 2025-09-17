using Plugins.RaycastPro.Demo.Scripts;
using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Flying_Away : MonoBehaviour
{
    //�v���C���[���痣���
    //�i�����̈ʒu�A���̎��̐U�ꕝ�A��]���x�A�g�̐i�s�ʒu�j
    public static void FlyingAway(Transform m_My, float m_radius, float m_speed, float m_time)
    {
        // 8�̎��O���i���T�[�W���Ȑ��j
        // �������œ����̃p�^�[������h�~
        float irrational = Mathf.Sqrt(2f);
        float x = Mathf.Sin(m_time) * m_radius;
        float z = Mathf.Sin(m_time * irrational) * m_radius / 2f;

        Vector3 offset = new Vector3(x, 0, z);
        // �����𒆐S�ɐ���
        m_My.position = m_My.position + offset;
    }
}
