using Plugins.RaycastPro.Demo.Scripts;
using UnityEngine;

public class Flying_Following : MonoBehaviour
{
    /// <summary>
    /// エージェントに追従させる
    /// </summary>
    /// <param name="myAgent">自分のエージェント</param>
    /// <param name="m_Controller">エージェントのコントローラー</param>
    /// <param name="m_Player">プレイヤーの位置</param>
    /// <param name="m_Rigidbody">リジットボディ</param>
    public static void FlyingFollowing(GameObject myAgent, SteeringController m_Controller, Transform m_Player, Rigidbody m_Rigidbody)
    {
        //プレイヤーがいなければリターン
        if (!m_Player)
            return;

        // ターゲット位置
        Vector3 nextPos = myAgent.transform.position;
        Vector3 dir = (nextPos - m_Rigidbody.position).normalized;

        // 目標速度
        Vector3 targetVelocity = dir * m_Controller.speed;

        // 現在の速度から滑らかに補間
        m_Rigidbody.linearVelocity = Vector3.Lerp(
            m_Rigidbody.linearVelocity,
            targetVelocity,
            1 - Mathf.Exp(-5f * Time.deltaTime)
        );

        //リジットボディとか重力を無効化して飛行できるようにする
        if (m_Rigidbody.useGravity)
        {
            m_Rigidbody.useGravity = false;
        }
    }
}
