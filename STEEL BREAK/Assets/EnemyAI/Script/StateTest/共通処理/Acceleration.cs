using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Acceleration : MonoBehaviour
{

    /// <summary>
    /// 徐々に早くなっていく加速処理(滑らかな加速)
    /// </summary>
    /// <param name="m_currentspeed">現在スピード</param>
    /// <param name="m_maxspeed">最大スピード</param>
    /// <param name="m_acceleration">加速度</param>
    /// <returns></returns>
    public static float Smooth(float m_currentspeed, float m_maxspeed, float m_acceleration)
    {
        // 現在速度を保持
        float nowspeed = m_currentspeed;

        // 徐々に加速
        m_currentspeed = Mathf.MoveTowards(nowspeed, m_maxspeed, m_acceleration * Time.deltaTime);

        return m_currentspeed;
    }
}
