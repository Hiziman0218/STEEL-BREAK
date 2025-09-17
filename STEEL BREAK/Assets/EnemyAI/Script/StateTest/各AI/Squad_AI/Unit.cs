using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public BankerSet m_BankerSet;
    public Transform m_Target;
    public NavMeshAgent m_NavMeshAgent;
    void Start()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(m_NavMeshAgent && m_Target)
            m_NavMeshAgent.destination = m_Target.position;
    }
}
