using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    private float m_timer = 0f;
    private bool m_isStopping = false;

    private Vector3 m_savedPosition;
    private Quaternion m_savedRotation;

    /// <summary>
    /// ヒットストップを開始
    /// </summary>
    /// <param name="duration">ヒットストップの持続時間</param>
    public void StartHitStop(float duration)
    {
        if (m_isStopping) return;

        m_savedPosition = transform.position;
        m_savedRotation = transform.rotation;
        m_timer = duration;
        m_isStopping = true;
    }

    void LateUpdate()
    {
        if (!m_isStopping) return;

        //フレームごとに位置を強制固定(AIのTransform書き換えを無効化)
        transform.position = m_savedPosition;
        transform.rotation = m_savedRotation;

        m_timer -= Time.deltaTime;
        if (m_timer <= 0)
        {
            m_isStopping = false;
            Destroy(this);
        }
    }
}
