using UnityEngine;
using System.Collections;

public class OutOfBoundsArea : MonoBehaviour
{
    [SerializeField] Transform respawnPoint; //復帰地点
    [SerializeField] float fadeDuration = 1f;

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StartCoroutine(ReturnRoutine(other.transform));
    }

    private IEnumerator ReturnRoutine(Transform player)
    {
        //暗転
        yield return FadeManager.Instance.FadeOut(fadeDuration);

        //位置を戻す
        player.position = respawnPoint.position;

        //Rigidbodyがあるなら速度リセット
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        //明転
        yield return FadeManager.Instance.FadeIn(fadeDuration);
    }
}
