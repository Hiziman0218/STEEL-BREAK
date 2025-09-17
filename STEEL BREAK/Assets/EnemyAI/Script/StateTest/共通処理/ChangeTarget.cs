using Plugins.RaycastPro.Demo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTarget : MonoBehaviour
{
    public static void Change(Transform m_target, GameObject myAgent)
    {
        // myAgentのエージェント制御スクリプトを取得
        var controller = myAgent.GetComponent<SteeringController>();
        // エージェントが追従するターゲット変更
        controller.detector.destination = m_target;
    }

}
