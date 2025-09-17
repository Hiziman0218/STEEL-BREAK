using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InertiaCamera : MonoBehaviour
{
    [Header("基本設定")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);

    [Header("慣性設定")]
    public float inertiaX = 0.5f;
    public float inertiaZ = 0.2f;
    public float followSpeed = 5f;

    [Header("最大慣性オフセット距離")]
    public float maxInertiaDistance = 5f;

    [Header("マウス操作設定")]
    public float mouseSensitivity = 3f;

    private float yaw = 0f;    // 水平方向回転
    private float pitch = 20f; // 垂直方向回転（初期角度）

    private Vector3 previousTargetPosition;
    private Vector3 velocity;

    void Start()
    {
        if (target == null)
        {
            enabled = false;
            return;
        }

        previousTargetPosition = target.position;

        // 初期回転値を現在のTransformから取得
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // カーソルロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // マウス入力で角度更新
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        // 回転制限を完全撤廃
        yaw = yaw % 360f;
        pitch = pitch % 360f;

        // 回転適用
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void FixedUpdate()
    {
        // プレイヤーの速度算出
        velocity = (target.position - previousTargetPosition) / Time.deltaTime;
        previousTargetPosition = target.position;

        Vector3 dynamicOffset = new Vector3(
            -velocity.x * inertiaX,
            0,
            -velocity.z * inertiaZ
        );
        if (dynamicOffset.magnitude > maxInertiaDistance)
            dynamicOffset = dynamicOffset.normalized * maxInertiaDistance;

        // 3) 現在の yaw/pitch から回転と位置を決定
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 rotatedOffset = rotation * offset;
        Vector3 targetCameraPos = target.position + rotatedOffset + dynamicOffset;

        transform.position = Vector3.Lerp(transform.position, targetCameraPos, followSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }
}
