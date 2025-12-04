using UnityEngine;

public class OperationArea : MonoBehaviour
{
    [SerializeField] private float limitTime = 5f;

    private float timer;
    private bool isOutside = false;

    private Player player;
    private OperationUI operationUI;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        operationUI = FindObjectOfType<OperationUI>();
        timer = limitTime;
    }

    private void Update()
    {
        if (!isOutside) return;

        timer -= Time.deltaTime;
        operationUI.UpdateTimer(timer);

        if (timer <= 0f)
        {
            player.GetDamage(player.GetStatus().GetMaxHP());
            operationUI.HideWarning();
            isOutside = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOutside = true;
            timer = limitTime;

            operationUI.ShowWarning();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isOutside)
        {
            isOutside = false;

            operationUI.HideWarning();
        }
    }
}
