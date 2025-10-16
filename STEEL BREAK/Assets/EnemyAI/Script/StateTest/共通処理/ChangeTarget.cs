using Plugins.RaycastPro.Demo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTarget : MonoBehaviour
{
    /// <summary>
    /// エージェントが追いかける対象を変更する処理
    /// </summary>
    /// <param name="m_target">変更対象</param>
    /// <param name="myAgent">自分が使っているエージェント</param>
    public static void Change(Transform m_target, GameObject myAgent)
    {
        // myAgentのエージェント制御スクリプトを取得
        var controller = myAgent.GetComponent<SteeringController>();
        // エージェントが追従するターゲット変更
        controller.detector.destination = m_target;
    }

}
