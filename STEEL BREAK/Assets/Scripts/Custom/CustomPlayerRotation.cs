using UnityEngine;

public class CustomPlayerRotation : MonoBehaviour
{
    [Header("回転設定")]
    public float rotationSpeed = 5f;

    [Header("ズーム設定")]
    public Camera playerCamera;
    public float zoomSpeed = 2f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 10f;

    [Header("カメラ移動設定")]
    public float moveSpeed = 5f; // カメラがターゲットに寄る速さ

    // 各部位のカメラポイント（Emptyオブジェクトをプレイヤー付近に配置して指定）
    public Transform headPoint;
    public Transform bodyPoint;
    public Transform lArmPoint;
    public Transform rArmPoint;
    public Transform legPoint;
    public Transform boosterPoint;

    private float currentZoomDistance;
    private Transform currentTargetPoint = null; // 現在注視中のポイント

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        currentZoomDistance = Vector3.Distance(transform.position, playerCamera.transform.position);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            HandleRotation();
            HandleZoom();
        }

        // カメラポイントへ移動
        if (currentTargetPoint != null)
        {
            MoveCameraToTarget();
        }
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * -mouseX * rotationSpeed);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoomDistance -= scroll * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

            Vector3 direction = (playerCamera.transform.position - transform.position).normalized;
            playerCamera.transform.position = transform.position + direction * currentZoomDistance;
        }
    }

    void MoveCameraToTarget()
    {
        // カメラ位置を補間（スムーズに移動）
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position,
            currentTargetPoint.position,
            Time.deltaTime * moveSpeed
        );

        // カメラがターゲットを向く
        playerCamera.transform.rotation = Quaternion.Lerp(
            playerCamera.transform.rotation,
            currentTargetPoint.rotation,
            Time.deltaTime * moveSpeed
        );
    }

    // ==== 以下はUIボタンから呼び出すメソッド ====
    public void FocusHead() => SetCameraTarget(headPoint);
    public void FocusBody() => SetCameraTarget(bodyPoint);
    public void FocusLArm() => SetCameraTarget(lArmPoint);
    public void FocusRArm() => SetCameraTarget(rArmPoint);
    public void FocusLeg() => SetCameraTarget(legPoint);
    public void FocusBooster() => SetCameraTarget(boosterPoint);

    void SetCameraTarget(Transform target)
    {
        if (target != null)
            currentTargetPoint = target;
    }
}
