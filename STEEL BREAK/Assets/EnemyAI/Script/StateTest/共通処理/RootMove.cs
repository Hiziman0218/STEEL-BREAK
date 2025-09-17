using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMove : MonoBehaviour
{
    public Transform m_RootTaerget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_RootTaerget)
        {
            this.transform.position = Vector3.Slerp(m_RootTaerget.position, this.transform.position, 0.001f);
        }

    }
    private void OnDestroy()
    {
        if (m_RootTaerget != null && m_RootTaerget.parent != null)
        {
            Destroy(m_RootTaerget.parent.gameObject);
        }
    }
}
