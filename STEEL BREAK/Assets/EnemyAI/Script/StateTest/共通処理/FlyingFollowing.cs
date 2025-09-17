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
    /// 飛行機能を持たせるだけ
    /// </summary>
    /// <param name="myAgent">自分のエージェント</param>
    /// <param name="m_My">自分の位置</param>
    /// <param name="m_Player">プレイヤーの位置</param>
    /// <param name="m_Rigidbody">リジットボディ</param>
    public static void FlyingFollowing(GameObject myAgent, Transform m_My, Transform m_Player, Rigidbody m_Rigidbody)
    {
        //プレイヤーがいなければリターン
        if (!m_Player)
            return;

        //FlyingAgentに追従
        m_My.position = myAgent.transform.position;

        //リジットボディとか重力を無効化して飛行できるようにする
        if (m_Rigidbody.freezeRotation || m_Rigidbody.useGravity)
        {
            m_Rigidbody.freezeRotation = false;
            m_Rigidbody.useGravity = false;
        }
    }
}
