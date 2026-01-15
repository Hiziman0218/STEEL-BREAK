using UnityEngine;

/// <summary>
/// 機体中心を基準としたカメラ制御クラス
/// UI非表示時のみフリー操作を許可する
/// </summary>
public class CustomPlayerRotation : MonoBehaviour
{
    //==============================
    // 設定
    //==============================

    [Header("回転設定")]
    [SerializeField] private float rotation_speed_ = 90f;

    [Header("ズーム設定")]
    [SerializeField] private Camera player_camera_;
    [SerializeField] private float zoom_speed_ = 5f;
    [SerializeField] private float min_zoom_distance_ = 2f;
    [SerializeField] private float max_zoom_distance_ = 10f;

    [Header("上下移動設定")]
    [SerializeField] private float vertical_move_speed_ = 2f;
    [SerializeField] private float vertical_move_limit_ = 2f;

    //==============================
    // カメラポイント
    //==============================

    public Transform defaultPoint;
    public Transform headPoint;
    public Transform bodyPoint;
    public Transform lArmPoint;
    public Transform rArmPoint;
    public Transform legPoint;
    public Transform boosterPoint;
    public Transform wlArmPoint;
    public Transform wrArmPoint;

    //==============================
    // 内部状態
    //==============================

    private Transform current_target_point_;
    private float vertical_offset_;
    private float current_zoom_distance_;

    private bool is_default_view_ = true;
    private bool is_free_control_ = false;
    private bool has_reached_target_ = false;

    private Quaternion default_body_rotation_;

    //==============================
    // Unity
    //==============================

    void Start()
    {
        if (player_camera_ == null)
        {
            player_camera_ = Camera.main;
        }

        default_body_rotation_ = transform.rotation;

        // 初期ズーム距離を保存
        current_zoom_distance_ =
            Vector3.Distance(defaultPoint.position, player_camera_.transform.position);

        ReturnToDefault();
    }

    void Update()
    {
        // Tabキーで必ずデフォルトへ戻す
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!IsAtDefaultPoint())
            {
                ReturnToDefault();
            }
        }

        // フリー操作はデフォルトビューのみ
        if (is_free_control_ && is_default_view_)
        {
            HandleFreeCameraControl();
        }

        // パーツビューは自動補間のみ
        if (!is_default_view_ && !has_reached_target_)
        {
            MoveCameraToTarget();
        }
    }

    //==============================
    // 外部（UI制御用）
    //==============================

    public void SetFreeControl(bool enable)
    {
        is_free_control_ = enable;

        if (!enable)
        {
            ReturnToDefault();
        }
    }

    public void ReturnToDefaultFromExternal()
    {
        ReturnToDefault();
    }

    //==============================
    // フリー操作
    //==============================

    private void HandleFreeCameraControl()
    {
        // A / D 機体回転
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;

        transform.Rotate(Vector3.up * horizontal * rotation_speed_ * Time.deltaTime);

        // W / S 上下移動
        float vertical = 0f;
        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;

        vertical_offset_ += vertical * vertical_move_speed_ * Time.deltaTime;
        vertical_offset_ = Mathf.Clamp(
            vertical_offset_,
            -vertical_move_limit_,
            vertical_move_limit_
        );

        // Q / E ズーム（カメラ向き基準）
        if (Input.GetKey(KeyCode.Q)) Zoom(-1f);
        if (Input.GetKey(KeyCode.E)) Zoom(1f);

        UpdateCameraPosition();
    }

    private void Zoom(float direction)
    {
        current_zoom_distance_ +=
            direction * zoom_speed_ * Time.deltaTime;

        current_zoom_distance_ = Mathf.Clamp(
            current_zoom_distance_,
            min_zoom_distance_,
            max_zoom_distance_
        );
    }

    private void UpdateCameraPosition()
    {
        Vector3 base_pos = defaultPoint.position;
        base_pos.y += vertical_offset_;

        player_camera_.transform.position =
            base_pos - defaultPoint.forward * current_zoom_distance_;

        player_camera_.transform.rotation = defaultPoint.rotation;
    }


    //==============================
    // パーツビュー
    //==============================

    private void MoveCameraToTarget()
    {
        player_camera_.transform.position = Vector3.Lerp(
            player_camera_.transform.position,
            current_target_point_.position,
            Time.deltaTime * 5f
        );

        player_camera_.transform.rotation = Quaternion.Lerp(
            player_camera_.transform.rotation,
            current_target_point_.rotation,
            Time.deltaTime * 5f
        );

        if (Vector3.Distance(
            player_camera_.transform.position,
            current_target_point_.position) < 0.05f)
        {
            has_reached_target_ = true;
        }
    }

    //==============================
    // デフォルト復帰
    //==============================

    private void ReturnToDefault()
    {
        if (defaultPoint == null) return;

        current_target_point_ = defaultPoint;
        is_default_view_ = true;
        has_reached_target_ = false;
        vertical_offset_ = 0f;

        transform.rotation = default_body_rotation_;

        player_camera_.transform.position = defaultPoint.position;
        player_camera_.transform.rotation = defaultPoint.rotation;
    }

    private bool IsAtDefaultPoint()
    {
        return Vector3.Distance(
            player_camera_.transform.position,
            defaultPoint.position) < 0.01f;
    }

    //==============================
    // パーツフォーカス（通常）
    //==============================

    public void FocusHead() => SetCameraTarget(headPoint);
    public void FocusBody() => SetCameraTarget(bodyPoint);
    public void FocusLArm() => SetCameraTarget(lArmPoint);
    public void FocusRArm() => SetCameraTarget(rArmPoint);
    public void FocusLeg() => SetCameraTarget(legPoint);
    public void FocusBooster() => SetCameraTarget(boosterPoint);
    public void FocusWLArmPoint() => SetCameraTarget(wlArmPoint);
    public void FocusWRArmPoint() => SetCameraTarget(wrArmPoint);

    //==============================
    // バックパックフォーカス（追加）
    //==============================

    public void FocusBackpackLeft() => SetCameraToBackpackPoint("BWLArmPoint");
    public void FocusBackpackRight() => SetCameraToBackpackPoint("BWRArmPoint");

    private void SetCameraToBackpackPoint(string point_name_)
    {
        Transform b_chest_ = FindDeepChild(transform, "B-chest");
        if (b_chest_ == null)
        {
            Debug.LogWarning("❌ B-chest が見つかりません。");
            return;
        }

        Transform found_point_ = null;

        foreach (Transform child in b_chest_)
        {
            if (child.name.StartsWith("バックパック"))
            {
                Transform target_point_ = child.Find(point_name_);
                if (target_point_ != null)
                {
                    found_point_ = target_point_;
                    break;
                }
            }
        }

        if (found_point_ != null)
        {
            SetCameraTarget(found_point_);
            Debug.Log($"✅ {point_name_} にカメラ移動しました。");
        }
        else
        {
            Debug.LogWarning($"❌ {point_name_} がバックパック内に見つかりませんでした。");
        }
    }

    private Transform FindDeepChild(Transform parent_, string name_)
    {
        foreach (Transform child in parent_)
        {
            if (child.name == name_)
            {
                return child;
            }

            Transform result_ = FindDeepChild(child, name_);
            if (result_ != null)
            {
                return result_;
            }
        }
        return null;
    }

    //==============================
    // 内部共通
    //==============================

    private void SetCameraTarget(Transform point_)
    {
        if (point_ == null) return;

        current_target_point_ = point_;
        is_default_view_ = false;
        has_reached_target_ = false;
        is_free_control_ = false;
    }
}
