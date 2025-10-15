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

    // カメラポイント群（中に CameraPoint 子オブジェクトあり）
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
    private Transform currentTargetPoint = null;   // カメラが移動する位置
    private Transform currentRotateCenter = null;  // 回転・ズームの中心（CameraPoint）
    private bool isDefaultView = true;             // デフォルトカメラ視点か？
    private bool hasReachedTarget = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // 初期位置設定
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

        // デフォルトビューでは常に操作可能
        // パーツビューでは到達後のみ操作可能
        bool canControlCamera = isDefaultView || hasReachedTarget;

        // 回転・ズーム処理
        if (canControlCamera && Input.GetKey(KeyCode.E))
        {
            if (isDefaultView)
                HandleModelRotation(); // モデル全体を回転
            else
                HandleCameraRotationY(); // カメラをY軸で回転

            HandleZoom();
        }

        // Qキーでデフォルトカメラに戻す
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetToDefaultView();
        }

        // 🟢 カメラ移動処理（ズーム中やEキー中は止める）
        if (currentTargetPoint != null)
        {
            if (isDefaultView)
            {
                // Eキーもマウスホイールも使っていないときだけ戻す
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

    // ===== カメラY軸回転 =====
    void HandleCameraRotationY()
    {
        if (currentRotateCenter == null) return;

        float mouseX = Input.GetAxis("Mouse X");

        // CameraPointのY軸を中心に回転
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
    }

    // ===== カメラ位置補間 =====
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

    // ===== カメラを初期状態に戻す =====
    void ResetToDefaultView()
    {
        if (defaultPoint == null) return;

        currentTargetPoint = defaultPoint;
        currentRotateCenter = null;
        isDefaultView = true;
        hasReachedTarget = false;
    }

    // ===== UIボタン用 =====
    public void FocusHead() => SetCameraTarget(headPoint);
    public void FocusBody() => SetCameraTarget(bodyPoint);
    public void FocusLArm() => SetCameraTarget(lArmPoint);
    public void FocusRArm() => SetCameraTarget(rArmPoint);
    public void FocusLeg() => SetCameraTarget(legPoint);
    public void FocusBooster() => SetCameraTarget(boosterPoint);
    public void FocusWLArm() => SetCameraTarget(WlArmPoint);
    public void FocusWRArm() => SetCameraTarget(WrArmPoint);

    // 🟢 新規追加：バックパック内のカメラポイントに移動
    public void FocusBackpackLeft() => SetCameraToBackpackPoint("BWLArmPoint");
    public void FocusBackpackRight() => SetCameraToBackpackPoint("BWRArmPoint");

    // ===== B-chest 探索 → バックパック → カメラポイント移動 =====
    void SetCameraToBackpackPoint(string pointName)
    {
        Transform bChest = FindDeepChild(transform, "B-chest");
        if (bChest == null)
        {
            Debug.LogWarning("❌ B-chest が見つかりません。階層を確認してください。");
            return;
        }

        Transform foundPoint = null;

        // B-chest配下のすべての子を探索（バックパック1,2など）
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

    // ===== 階層の深い場所からオブジェクトを探す =====
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

    // ===== カメラターゲット設定 =====
    void SetCameraTarget(Transform point)
    {
        if (point == null) return;

        currentTargetPoint = point;
        isDefaultView = false;
        hasReachedTarget = false;

        // 子オブジェクトに「CameraPoint」があればそれを中心点にする
        Transform childCenter = point.Find("CameraPoint");
        currentRotateCenter = childCenter != null ? childCenter : point;
    }
}
