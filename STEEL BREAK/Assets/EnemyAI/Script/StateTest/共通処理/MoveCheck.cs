using UnityEngine;

public class MoveCheck : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float stopThreshold = 0.1f; // 停止判定の閾値

    void Reset()
    {
        // Inspectorで自動アタッチ
        rb = GetComponent<Rigidbody>();
    }

    private float stopTimer = 0f;

    void FixedUpdate()
    {
        // 停止判定の閾値になれば
        if (rb.linearVelocity.sqrMagnitude < stopThreshold * stopThreshold)
        {
            //リジットボディのVelocityを０にするかどうかのカウントダウン
            stopTimer += Time.fixedDeltaTime;

            if (stopTimer > 0.2f) // 0.2秒以上停止していたら
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // デバッグログを出す
            Debug.Log($"[MoveCheck] 停止処理実行: velocity={rb.linearVelocity}, angularVelocity={rb.angularVelocity}");

        }
        else
        {
            stopTimer = 0f;

            // 動いているときの速度も確認したいなら
        Debug.Log($"[MoveCheck] 移動中: velocity={rb.linearVelocity}");

        }
    }
}
