using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("マウス横回転感度")]
    public float mouseSensitivity = 100f;

    [Tooltip("レーザー照射中の回転速度倍率（0〜1）")]
    public float laserRotateMultiplier = 0.1f;

    private Player m_player;

    void Start()
    {
        m_player = GetComponent<Player>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float rotationSpeed = mouseSensitivity * Time.deltaTime;

        if (m_player != null && m_player.IsFireLaser())
        {
            rotationSpeed *= laserRotateMultiplier;
        }

        // ローカルではなく、ワールドY軸で回転
        transform.Rotate(Vector3.up, mouseX * rotationSpeed, Space.World);
    }
}
