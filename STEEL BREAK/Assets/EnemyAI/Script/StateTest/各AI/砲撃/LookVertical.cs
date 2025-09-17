using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LookVertical : MonoBehaviour
{
    //砲身の縦回転
    public static void Look_Vertical(Transform m_My, Transform m_Player, float minPitchAngle, float maxPitchAngle, float m_rotationSpeed)
    {
        Vector3 dir = m_Player.position - m_My.position;

        // 親ローカル空間での方向ベクトル
        Vector3 localDir = m_My.parent.InverseTransformDirection(dir);

        // ターゲットとの角度からピッチ（上下）を取得
        float pitch = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

        //角度制限
        float clampedPitch = -Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);

        // 目標の回転
        Quaternion targetRotation = Quaternion.Euler(clampedPitch, 0f, 0f);

        // 現在の回転から目標回転へ補間（ラグを加える）
        m_My.localRotation = Quaternion.Lerp(m_My.localRotation, targetRotation, Time.deltaTime * m_rotationSpeed);
    }
}
