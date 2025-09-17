using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_Test : MonoBehaviour
{
    public Transform m_Base;
    public Transform m_Target;
    public Rigidbody m_Rigidbody;
    public BoxCollider m_BoxCollider;
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_BoxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_Target)
            return;

        //プレイヤーとの距離が近くなったら地面に降りる
        if (Vector3.Distance(transform.position, m_Target.position) < 10.0f)
        {
            transform.LookAt(m_Target);
            m_Rigidbody.freezeRotation = true;
            m_Rigidbody.useGravity = true;
        }
        else
        {
            transform.position = m_Base.position;
            transform.rotation = m_Base.rotation;
            m_Rigidbody.freezeRotation = false;
            m_Rigidbody.useGravity = false;
        }
    }
}
