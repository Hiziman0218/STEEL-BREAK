using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lookhorizontal : MonoBehaviour
{
    //砲台の横回転
    public static void Look_horizontal(Transform m_My, Transform m_Player, float m_rotationSpeed)
    {
        // プレイヤーの位置
        Vector3 targetPosition = m_Player.transform.position;

        // 自分の現在の位置
        Vector3 myPosition = m_My.transform.position;

        // Y軸だけを考慮するために、高さを固定
        targetPosition.y = myPosition.y;

        // プレイヤーの方向を向くための目標回転
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);

        // 現在の回転から目標回転へ補間（ラグを加える）
        m_My.rotation = Quaternion.Lerp(m_My.rotation, targetRotation, Time.deltaTime * m_rotationSpeed);

    }
}
