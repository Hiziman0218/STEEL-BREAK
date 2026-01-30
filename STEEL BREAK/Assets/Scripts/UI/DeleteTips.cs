using UnityEngine;

public class DeleteTips : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.V))
        {
            Destroy(gameObject);
        }
    }
}
