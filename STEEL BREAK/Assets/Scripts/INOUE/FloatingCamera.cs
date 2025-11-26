using UnityEngine;

public class FloatingCamera : MonoBehaviour
{
    [Header("注視点:カメラが何処を見ているか")]
    public Transform m_GazingPoint;
    [Header("カメラ台:カメラが何処に移動しているか")]
    public Transform m_Target;

    private void Start()
    {
        //プレイヤーとの親子関係を解除
        transform.SetParent(null);   
    }

    /// <summary>
    /// フレームではなく時間で起動するアップデート
    /// 重い処理とかに使用するが、場合によって処理をすっ飛ばす可能性がある
    /// </summary>
    private void FixedUpdate()
    {
        /*
        if (m_GazingPoint && m_Target)
        {
            ///フローティングカメラの向きをプレイヤーの位置情報から向き情報を獲得し、分割してゆっくり回転
            ///Slerpは、AからBまでの補完を行い、分割して値を提出する
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(m_GazingPoint.position - transform.position),
                0.1f);
            ///フローティングカメラの位置をフローティングカメラ台にゆっくり移動させる
            transform.position = Vector3.Lerp(
                transform.position,
                m_Target.position,
                0.1f);
        }*/
        if (!m_GazingPoint || !m_Target) return;

        // --- ① 位置補正前の「理想的なカメラ位置」
        Vector3 idealPosition = m_Target.position;

        // --- ② プレイヤー → 理想カメラ位置の間に障害物があるか判定
        Vector3 direction = idealPosition - m_GazingPoint.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(m_GazingPoint.position, direction.normalized, out RaycastHit hit, distance))
        {
            // 壁に当たった → 衝突点の少し手前に配置
            idealPosition = hit.point - direction.normalized * 0.3f;
        }

        // --- ③ 位置補間
        transform.position = Vector3.Lerp(transform.position, idealPosition, 0.1f);

        // --- ④ 回転補間
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(m_GazingPoint.position - transform.position),
            0.1f
        );
    }
}
