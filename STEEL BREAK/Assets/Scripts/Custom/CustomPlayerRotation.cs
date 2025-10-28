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
    public float moveSpeed = 5f;

    [Header("上下移動制限（デフォルトビュー用）")]
    public float verticalMoveLimit = 2f;   // 🆕 上下の最大距離
    public float verticalMoveSpeed = 0.5f; // 🆕 上下移動感度

    // カメラポイント群
    public Transform defaultPoint;
    public Transform headPoint;
    public Transform bodyPoint;
    public Transform lArmPoint;
    public Transform rArmPoint;
    public Transform legPoint;
    public Transform boosterPoint;
    public Transform WlArmPoint;
    public Transform WrArmPoint;

    private float currentZoomDistance;
    private Transform currentTargetPoint = null;
    private Transform currentRotateCenter = null;
    private bool isDefaultView = true;
    private bool hasReachedTarget = false;

    // 🆕 現在の上下オフセット値（デフォルトカメラ用）
    private float currentVerticalOffset = 0f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (defaultPoint != null)
        {
            playerCamera.transform.position = defaultPoint.position;
            playerCamera.transform.rotation = defaultPoint.rotation;
            currentTargetPoint = defaultPoint;
        }

        currentZoomDistance = Vector3.Distance(transform.position, playerCamera.transform.position);
    }

    void Update()
    {
        bool isRotatingOrZooming = Input.GetKey(KeyCode.E) || Input.GetAxis("Mouse ScrollWheel") != 0f;
        bool canControlCamera = isDefaultView || hasReachedTarget;

        // カメラ操作
        if (canControlCamera && Input.GetKey(KeyCode.E))
        {
            if (isDefaultView)
            {
                HandleModelRotation();
                HandleCameraVerticalMove(); // 🆕 デフォルト時のみ上下移動を追加
            }
            else
            {
                HandleCameraRotationY(); // パーツビュー時は回転のみ
            }

            HandleZoom();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetToDefaultView();
        }

        // カメラ位置補間
        if (currentTargetPoint != null)
        {
            if (isDefaultView)
            {
                if (!isRotatingOrZooming)
                    MoveCameraToTarget();
            }
            else if (!hasReachedTarget)
            {
                MoveCameraToTarget();

                if (Vector3.Distance(playerCamera.transform.position, currentTargetPoint.position) < 0.05f)
                    hasReachedTarget = true;
            }
        }
    }

    // ===== モデル回転（デフォルト時） =====
    void HandleModelRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * -mouseX * rotationSpeed);
    }

    // ===== 🆕 カメラ上下移動（デフォルトビュー限定） =====
    void HandleCameraVerticalMove()
    {
        float mouseY = Input.GetAxis("Mouse Y");

        // マウスを下に動かすとカメラも下に移動するように符号を反転
        playerCamera.transform.position += Vector3.up * mouseY * moveSpeed * Time.deltaTime;

        // 上下移動の制限
        Vector3 pos = playerCamera.transform.position;
        pos.y = Mathf.Clamp(pos.y, defaultPoint.position.y - 2f, defaultPoint.position.y + 2f);
        playerCamera.transform.position = pos;
    }

    // ===== カメラY軸回転（パーツビュー時） =====
    void HandleCameraRotationY()
    {
        if (currentRotateCenter == null) return;

        float mouseX = Input.GetAxis("Mouse X");

        playerCamera.transform.RotateAround(
            currentRotateCenter.position,
            Vector3.up,
            mouseX * rotationSpeed
        );
    }

    // ===== ズーム処理 =====
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        currentZoomDistance -= scroll * zoomSpeed;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

        Vector3 center = isDefaultView ? transform.position :
                          (currentRotateCenter != null ? currentRotateCenter.position : transform.position);

        Vector3 direction = (playerCamera.transform.position - center).normalized;
        playerCamera.transform.position = center + direction * currentZoomDistance;

        // 🆕 デフォルトビュー中は上下オフセットも反映
        if (isDefaultView)
            playerCamera.transform.position += Vector3.up * currentVerticalOffset;
    }

    void MoveCameraToTarget()
    {
        playerCamera.transform.position = Vector3.Lerp(
            playerCamera.transform.position,
            currentTargetPoint.position,
            Time.deltaTime * moveSpeed
        );

        playerCamera.transform.rotation = Quaternion.Lerp(
            playerCamera.transform.rotation,
            currentTargetPoint.rotation,
            Time.deltaTime * moveSpeed
        );
    }

    void ResetToDefaultView()
    {
        if (defaultPoint == null) return;

        currentTargetPoint = defaultPoint;
        currentRotateCenter = null;
        isDefaultView = true;
        hasReachedTarget = false;
        currentVerticalOffset = 0f; // 🆕 リセット
    }

    // ===== 各フォーカス =====
    public void FocusHead() => SetCameraTarget(headPoint);
    public void FocusBody() => SetCameraTarget(bodyPoint);
    public void FocusLArm() => SetCameraTarget(lArmPoint);
    public void FocusRArm() => SetCameraTarget(rArmPoint);
    public void FocusLeg() => SetCameraTarget(legPoint);
    public void FocusBooster() => SetCameraTarget(boosterPoint);
    public void FocusWLArm() => SetCameraTarget(WlArmPoint);
    public void FocusWRArm() => SetCameraTarget(WrArmPoint);

    public void FocusBackpackLeft() => SetCameraToBackpackPoint("BWLArmPoint");
    public void FocusBackpackRight() => SetCameraToBackpackPoint("BWRArmPoint");

    void SetCameraToBackpackPoint(string pointName)
    {
        Transform bChest = FindDeepChild(transform, "B-chest");
        if (bChest == null)
        {
            Debug.LogWarning("❌ B-chest が見つかりません。");
            return;
        }

        Transform foundPoint = null;
        foreach (Transform child in bChest)
        {
            if (child.name.StartsWith("バックパック"))
            {
                Transform targetPoint = child.Find(pointName);
                if (targetPoint != null)
                {
                    foundPoint = targetPoint;
                    break;
                }
            }
        }

        if (foundPoint != null)
        {
            SetCameraTarget(foundPoint);
            Debug.Log($"✅ {pointName} にカメラ移動しました。");
        }
        else
        {
            Debug.LogWarning($"❌ {pointName} がバックパック内に見つかりませんでした。");
        }
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void SetCameraTarget(Transform point)
    {
        if (point == null) return;

        currentTargetPoint = point;
        isDefaultView = false;
        hasReachedTarget = false;
        currentVerticalOffset = 0f; // 🆕 上下オフセットリセット

        Transform childCenter = point.Find("CameraPoint");
        currentRotateCenter = childCenter != null ? childCenter : point;
    }
}
