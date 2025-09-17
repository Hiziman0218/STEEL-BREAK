using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHitDead : MonoBehaviour
{
    public GameObject m_Hit;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject Dummy = Instantiate(m_Hit, this.transform.position, this.transform.rotation);
            Destroy(Dummy,3.0f);
            Destroy(this.gameObject);
        }
    }
}
