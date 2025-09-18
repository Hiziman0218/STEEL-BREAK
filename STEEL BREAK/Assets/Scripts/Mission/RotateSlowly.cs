using UnityEngine;

public class RotateSlowly : MonoBehaviour
{
    // 毎秒の回転速度（度）
    public Vector3 speed = new Vector3(0f, 10f, 0f);
    // ローカル回転かワールド回転か
    public Space space = Space.Self;

    void Update()
    {
        // フレームに依存しないように deltaTime を掛ける
        transform.Rotate(speed * Time.deltaTime, space);
    }
}
