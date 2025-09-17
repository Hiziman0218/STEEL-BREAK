using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventRandomRot : MonoBehaviour
{
    public float m_Speed;
    public Vector3 m_Rot;
    public float m_RotTimeCounter;
    public float m_RotTimeMaxCounter = 2.0f;
    void Start()
    {
        
    }

    void Update()
    {
        if (m_RotTimeCounter <= 0.0f)
        {
            m_RotTimeCounter = m_RotTimeMaxCounter + Random.RandomRange(0.0f, m_RotTimeMaxCounter / 2);
            m_Rot.x = Random.Range(-1.0f, 1.0f) * 10.0f;
            m_Rot.y = 0.0f;
            m_Rot.z = Random.Range(-1.0f, 1.0f) * 2.0f;
        }
        else
        {
            m_RotTimeCounter -= m_Speed * Time.deltaTime;
        }
        this.transform.Rotate(m_Rot);
    }
}
