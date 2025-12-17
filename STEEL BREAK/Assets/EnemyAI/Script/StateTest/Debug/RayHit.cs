using UnityEngine;

public class RayDebugVisualizer : MonoBehaviour
{
    public Transform origin;       // レイの開始位置
    public Vector3 direction;      // レイの方向
    public float distance = 10f;   // レイの長さ
    public Color rayColor = Color.red; // レイの色

    private void OnDrawGizmos()
    {
        if (origin == null) return;

        Gizmos.color = rayColor;
        // レイを描画
        Gizmos.DrawRay(origin.position, direction.normalized * distance);

        // ヒットした場合はヒット点まで別色で描画
        if (Physics.Raycast(origin.position, direction, out RaycastHit hit, distance))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.2f); // ヒット点を球で表示
        }
    }
}
