using UnityEngine;

//レーザーのテスト
public class Test : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Destroy(other.gameObject);
    }
}
