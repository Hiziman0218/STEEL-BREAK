using Plugins.RaycastPro.Demo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTarget : MonoBehaviour
{
    public static void Change(Transform m_target, GameObject myAgent)
    {
        // myAgent�̃G�[�W�F���g����X�N���v�g���擾
        var controller = myAgent.GetComponent<SteeringController>();
        // �G�[�W�F���g���Ǐ]����^�[�Q�b�g�ύX
        controller.detector.destination = m_target;
    }

}
