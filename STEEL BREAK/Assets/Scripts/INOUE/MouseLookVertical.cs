using UnityEngine;

public class MouseLookVertical : MonoBehaviour
{
    [Header("マウス感度（上下移動）")]
    public float mouseSensitivity = 1.5f;

    [Header("注視点の高さ制限")]
    public float minOffsetY = -1.0f;
    public float maxOffsetY = 1.0f;

    [Header("基準位置（プレイヤーの頭や胸など）")]
    public Transform m_Base; // 注視点の基準位置（プレイヤーのTransform）

    [Header("カメラ本体のTransform")]
    public Transform m_CameraTransform; // カメラのTransform

    [Header("カメラの基準位置")]
    public Vector3 cameraBaseLocalOffset = new Vector3(0f, 1.5f, -4f); // プレイヤーから見たカメラの初期オフセット

    private float currentYOffset = 0f;

    void Update()
    {
        if (m_Base == null || m_CameraTransform == null) return;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 注視点Yオフセットをマウス入力に応じて更新
        currentYOffset -= mouseY;
        currentYOffset = Mathf.Clamp(currentYOffset, minOffsetY, maxOffsetY);

        // 注視点の座標を更新（プレイヤーの前方位置にオフセット付きで）
        Vector3 gazePos = m_Base.position + m_Base.forward * 1.0f;
        gazePos.y += currentYOffset;
        transform.position = gazePos;

        // カメラ位置をシーソー関係で更新（注視点が上がればカメラが下がる）
        Vector3 camLocalOffset = cameraBaseLocalOffset;
        camLocalOffset.y -= currentYOffset; // シーソー効果：逆方向に変動
        m_CameraTransform.position = m_Base.TransformPoint(camLocalOffset);
    }
}
