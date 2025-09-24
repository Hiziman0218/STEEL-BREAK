using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StageEventTrigger : MonoBehaviour
{
    [Tooltip("Resources/StageEvents �t�H���_���̃e�L�X�g�t�@�C���� (��: event_001)")]
    public string eventDataPath;

    [Tooltip("��x�������΂��邩�H")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player")) // �v���C���[�ɂ�������
        {
            hasTriggered = true;

            // StageEventManager �ɒʒm
            StageEventManager manager = FindObjectOfType<StageEventManager>();
            if (manager != null)
            {
                manager.StartEvent(eventDataPath);
            }
            else
            {
                Debug.LogWarning("StageEventManager ���V�[���ɑ��݂��܂���BUI��\���ł��܂���B");
            }
        }
    }
}
