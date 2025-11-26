using UnityEngine;

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
    /// 砲台とかに使っている
    /// </summary>
    /// <param name="m_My">自分の位置</param>
    /// <param name="m_Player">プレイヤー</param>
    /// <param name="m_turnsmooth">追従補正</param>
    public static void SoftLock(Transform m_My, Transform m_Player, float m_turnsmooth)
    {
        if (m_Player == null) return;

        // 少しだけプレイヤーの方向へ補正回転
        Vector3 targetDir = (m_Player.position - m_My.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);

        // 追従補正（補正の割合は0〜1で制御）
        m_My.transform.rotation = Quaternion.Slerp(m_My.transform.rotation, targetRotation, m_turnsmooth);
    }

    /// <summary>
    /// 即座に水平方向だけ向く
    ///PlayerLookAt.LookAtFlat(プレイヤー, 敵（自AIのモデル）);
    /// 滑らかに水平方向だけ向く（0.05でゆっくり）
    ///PlayerLookAt.LookAtFlat(プレイヤー, 敵（自AIのモデル）, 0.05f);
    /// </summary>
    /// <param name="m_Player"></param>
    /// <param name="m_my"></param>
    /// <param name="turnSmooth"></param>
    public static void LookAtFlat(Transform m_Player, Transform m_my, float turnSmooth)
    {
        if (m_Player == null) return;

        // プレイヤーのXZ座標を使ってターゲット位置を作る
        Vector3 targetPos = new Vector3(m_Player.position.x, m_my.position.y, m_Player.position.z);
        Vector3 dir = targetPos - m_my.position;

        if (dir.sqrMagnitude < 0.001f) return; // ゼロ割り防止

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        if (turnSmooth >= 1f)
        {
            // 即座に回転
            m_my.rotation = targetRot;
        }
        else
        {
            // スムーズに回転（Time.deltaTime を掛けると安定）
            m_my.rotation = Quaternion.Slerp(m_my.rotation, targetRot, turnSmooth * Time.deltaTime);
        }
    }
}
