using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerLookAt : MonoBehaviour
{
    /// <summary>
    /// 強制NPCモデル向き補正
    /// </summary>
    public static Vector3 LookAt(Transform m_Player, Transform m_EnemyModel)
    {
        // モデルをプレイヤーに向けさせる
        Vector3 Pos = m_Player.position;
        Pos.y = m_EnemyModel.position.y;

        m_EnemyModel.LookAt(Pos);
        return Pos;
    }

    /// <summary>
    /// 緩い追従
    /// </summary>
    /// <param name="m_My">自分の位置</param>
    /// <param name="m_Player">プレイヤー</param>
    /// <param name="m_turnsmooth">追従補正</param>
    public static void SoftLock(Transform m_My, Transform m_Player, float m_turnsmooth)
    {
        // 少しだけプレイヤーの方向へ補正回転
        Vector3 targetDir = (m_Player.position - m_My.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);

        // 追従補正（補正の割合は0〜1で制御）
        m_My.transform.rotation = Quaternion.Slerp(m_My.transform.rotation, targetRotation, m_turnsmooth);
    }

}
