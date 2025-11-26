using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("マウス横回転感度")]
    public float mouseSensitivity = 100f;

    [Tooltip("レーザー照射中の回転速度倍率（0〜1）")]
    public float laserRotateMultiplier = 0.1f;

    private Player m_player;
    private LockOn m_lockOn;

    void Start()
    {
        m_player = GetComponent<Player>();
        m_lockOn = GetComponent<LockOn>();
    }

    void Update()
    {
        //プレイヤーがレーザー使用中以外で、ターゲットがいる場合は以降の処理を行わない
        if(m_player != null && !m_player.IsFireLaser())
        {
            if (m_lockOn != null && m_lockOn.CurrentTarget != null) return;
        }

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
