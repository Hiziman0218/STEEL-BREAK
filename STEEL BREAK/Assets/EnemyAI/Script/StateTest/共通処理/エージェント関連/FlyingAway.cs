using Plugins.RaycastPro.Demo.Scripts;
using RaycastPro.Detectors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Flying_Away : MonoBehaviour
{
    //プレイヤーから離れる
    //（自分の位置、八の字の振れ幅、回転速度、波の進行位置）
    public static void FlyingAway(Transform m_My, float m_radius, float m_speed, float m_time)
    {
        // 8の字軌道（リサージュ曲線）
        // 無理数で動きのパターン化を防止
        float irrational = Mathf.Sqrt(2f);
        float x = Mathf.Sin(m_time) * m_radius;
        float z = Mathf.Sin(m_time * irrational) * m_radius / 2f;

        Vector3 offset = new Vector3(x, 0, z);
        // 自分を中心に旋回
        m_My.position = m_My.position + offset;
    }
}
