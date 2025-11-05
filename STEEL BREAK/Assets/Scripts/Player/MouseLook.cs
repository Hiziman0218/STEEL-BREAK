using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("マウス横回転感度")]
    [Tooltip("値が大きいほど速く回転します")]
    public float mouseSensitivity = 100f;

    [Tooltip("レーザー照射中の回転速度倍率（0〜1）")]
    public float laserRotateMultiplier = 0.1f; //回転を遅くする倍率

    private Player m_player;

    void Start()
    {
        // ルートから PlayerController を取得
        m_player = transform.GetComponent<Player>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");

        // 感度を反映させる
        float rotationSpeed = mouseSensitivity * Time.deltaTime;

        // レーザー照射中なら回転を遅くする
        if (m_player != null && m_player.IsFireLaser())
        {
            rotationSpeed *= laserRotateMultiplier;
        }

        // 実際の回転
        transform.Rotate(0f, mouseX * rotationSpeed, 0f);
    }
}
