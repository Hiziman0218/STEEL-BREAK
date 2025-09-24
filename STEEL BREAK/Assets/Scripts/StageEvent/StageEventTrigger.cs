using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageEventTrigger : MonoBehaviour
{
    [Tooltip("Resources/StageEvents フォルダ内のテキストファイル名 (例: event_001)")]
    public string eventDataPath;

    [Tooltip("一度だけ発火するか？")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player")) // プレイヤーにだけ反応
        {
            hasTriggered = true;

            // StageEventManager に通知
            StageEventManager manager = FindObjectOfType<StageEventManager>();
            if (manager != null)
            {
                manager.StartEvent(eventDataPath);
            }
            else
            {
                Debug.LogWarning("StageEventManager がシーンに存在しません。UIを表示できません。");
            }
        }
    }
}
