using Plugins.RaycastPro.Demo.Scripts;
using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Flying_Following : MonoBehaviour
{
    /// <summary>
    /// ��s�@�\���������邾��
    /// </summary>
    /// <param name="myAgent">�����̃G�[�W�F���g</param>
    /// <param name="m_My">�����̈ʒu</param>
    /// <param name="m_Player">�v���C���[�̈ʒu</param>
    /// <param name="m_Rigidbody">���W�b�g�{�f�B</param>
    public static void FlyingFollowing(GameObject myAgent, Transform m_My, Transform m_Player, Rigidbody m_Rigidbody)
    {
        //�v���C���[�����Ȃ���΃��^�[��
        if (!m_Player)
            return;

        //FlyingAgent�ɒǏ]
        m_My.position = myAgent.transform.position;

        //���W�b�g�{�f�B�Ƃ��d�͂𖳌������Ĕ�s�ł���悤�ɂ���
        if (m_Rigidbody.freezeRotation || m_Rigidbody.useGravity)
        {
            m_Rigidbody.freezeRotation = false;
            m_Rigidbody.useGravity = false;
        }
    }
}
