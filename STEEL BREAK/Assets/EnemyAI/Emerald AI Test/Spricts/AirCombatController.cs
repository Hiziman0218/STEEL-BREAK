using EmeraldAI;
using UnityEngine;

public class AirCombatController : MonoBehaviour
{
    public EmeraldMovement emeraldMovement;
    public Transform target;
    public float airSpeed = 5f;
    public float turnSpeed = 10f;

    private void Start()
    {
        //プレイヤータグでターゲット検索
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void OnEnable()
    {
        if (emeraldMovement != null)
        {
            emeraldMovement.enabled = false; // 地上移動を停止
        }
    }

    void OnDisable()
    {
        if (emeraldMovement != null)
        {
            emeraldMovement.enabled = true; // 地上移動を再開
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * airSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }
}
