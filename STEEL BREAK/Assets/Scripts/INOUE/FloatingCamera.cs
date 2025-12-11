using UnityEngine;

public class FloatingCamera : MonoBehaviour
{
    [Header("注視点:カメラが何処を見ているか")]
    public Transform m_GazingPoint;
    [Header("カメラ台:カメラが何処に移動しているか")]
    public Transform m_Target;

    [Header("プレイヤーとの理想距離")]
    public float desiredDistance = 3.0f;

    [Header("壁との衝突判定用レイヤー")]
    public LayerMask obstacleLayers;

    [Header("カメラ当たり判定の太さ")]
    public float cameraRadius = 0.2f;

    [Header("カメラがプレイヤーを表示しなくなる距離")]
    public float hideDistance = 2f;

    private void Start()
    {
        //プレイヤーとの親子関係を解除
        transform.SetParent(null);
    }

    private void FixedUpdate()
    {
        if (!m_GazingPoint || !m_Target) return;

        //----------------------------------------------------------
        // ① GazingPoint → Target を SphereCast して壁をチェック
        //----------------------------------------------------------
        Vector3 from = m_GazingPoint.position;
        Vector3 to = m_Target.position;
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);

        float correctedDistance = distance;

        if (Physics.SphereCast(
                from,
                cameraRadius,
                direction,
                out RaycastHit hit,
                distance,
                obstacleLayers))
        {
            // 壁に当たったなら、カメラが少し手前に来るよう距離を補正
            correctedDistance = hit.distance - cameraRadius;
        }

        // 実際に移動させるべき位置を決定
        Vector3 correctedTargetPos = from + direction * correctedDistance;

        //----------------------------------------------------------
        // ② カメラの向き：注視点を向く（遅れつつ追従）
        //----------------------------------------------------------
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(m_GazingPoint.position - transform.position),
            0.1f);

        //----------------------------------------------------------
        // ③ カメラ位置：補正済みのカメラ台に向けて Lerp 移動
        //----------------------------------------------------------
        transform.position = Vector3.Lerp(
            transform.position,
            correctedTargetPos,
            0.1f);

        //----------------------------------------------------------
        // ④ プレイヤー透明化：距離に応じてレイヤー切り替え
        //----------------------------------------------------------
        float currentDistanceToPlayer = Vector3.Distance(transform.position, m_GazingPoint.position);

        // カメラが描画しないレイヤー
        int hiddenLayer = LayerMask.NameToLayer("PlayerHidden");

        // プレイヤーの通常レイヤー
        int defaultLayer = LayerMask.NameToLayer("Player");

        if (currentDistanceToPlayer < hideDistance)
        {
            SetLayerRecursively(m_GazingPoint.root.gameObject, hiddenLayer);
        }
        else
        {
            SetLayerRecursively(m_GazingPoint.root.gameObject, defaultLayer);
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
