using UnityEngine;

public class FlySystemSetter : MonoBehaviour
{
    public GameObject m_FlySystemPrefab;
    public GameObject m_FlySystem;
    void Update()
    {
        if (!m_FlySystem)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (m_FlySystemPrefab)
                {
                    m_FlySystem = Instantiate(m_FlySystemPrefab, transform.position, transform.rotation);
                    m_FlySystem.GetComponent<FlyNaviSystem>().SetUp(this.gameObject);
                    gameObject.GetComponent<Rigidbody>().useGravity = false;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                m_FlySystem.GetComponent<FlyNaviSystem>().Reset();
            }
        }
    }
}
