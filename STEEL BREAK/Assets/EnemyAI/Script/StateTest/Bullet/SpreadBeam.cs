using UnityEngine;

public class SpreadBeam : MonoBehaviour
{
    public int beamCount = 5;          // ビームの本数
    public float spreadAngle = 45f;    // 拡散角度
    public float beamLength = 10f;     // ビームの長さ
    public LineRenderer linePrefab;    // LineRendererのプレハブ

    void Fire()
    {
        float startAngle = -spreadAngle / 2f;
        for (int i = 0; i < beamCount; i++)
        {
            float angle = startAngle + (spreadAngle / (beamCount - 1)) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            LineRenderer lr = Instantiate(linePrefab, transform.position, Quaternion.identity);
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + dir * beamLength);
        }
    }
}
