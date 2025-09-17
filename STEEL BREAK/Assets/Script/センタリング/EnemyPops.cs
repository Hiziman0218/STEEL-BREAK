using UnityEngine;

public class EnemyPops : MonoBehaviour
{
    public GameObject m_Enemy;
    public Transform m_Player;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameObject Dummy = Instantiate(m_Enemy, this.transform.position, this.transform.rotation);
            Dummy.GetComponent<CenterNPC>().m_Player = m_Player;
        }
    }
}
