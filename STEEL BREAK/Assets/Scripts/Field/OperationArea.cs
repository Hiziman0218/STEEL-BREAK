using UnityEngine;

public class OperationArea : MonoBehaviour
{
    [SerializeField] private OperationUI ui;
    [SerializeField] private float timeLimit = 5f;

    private float timer;
    private bool isOutside = false;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = true;
            timer = timeLimit;
            ui.ShowWarning(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = false;
            ui.ShowWarning(false);
        }
    }

    private void Update()
    {
        if (!isOutside) return;

        timer -= Time.deltaTime;
        ui.UpdateTimer(timer);

        if (timer <= 0f)
        {
            Player player = FindAnyObjectByType<Player>();
            player.GetDamage(player.GetStatus().GetMaxHP());
            isOutside = false;
            ui.ShowWarning(false);
        }
    }
}
