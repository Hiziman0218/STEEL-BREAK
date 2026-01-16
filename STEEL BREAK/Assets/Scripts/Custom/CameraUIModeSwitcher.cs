using UnityEngine;

/// <summary>
/// TabキーでUI表示とカメラ操作モードを切り替える制御クラス
/// カメラモード時は無操作で専用UIをフェードアウト
/// </summary>
public class CameraUIModeSwitcher : MonoBehaviour
{
    [Header("通常UI（カメラモード時は非表示）")]
    [SerializeField] private GameObject[] ui_objects_;

    [Header("カメラモード時のみ表示するUI")]
    [SerializeField] private GameObject[] camera_mode_only_objects_;

    [Header("フェード設定")]
    [SerializeField] private float fade_out_delay_ = 3f;
    [SerializeField] private float fade_speed_ = 3f;

    [Header("カメラ回転制御")]
    [SerializeField] private CustomPlayerRotation custom_player_rotation_;

    private bool is_ui_visible_ = true;
    private bool is_camera_mode_ = false;

    private bool[] ui_prev_active_states_;
    private CanvasGroup camera_ui_canvas_group_;
    private float no_input_timer_ = 0f;
    private bool is_faded_out_ = false;

    void Start()
    {
        // ===== 通常UIの初期状態保存 =====
        ui_prev_active_states_ = new bool[ui_objects_.Length];

        for (int i = 0; i < ui_objects_.Length; i++)
        {
            if (ui_objects_[i] != null)
            {
                ui_prev_active_states_[i] = ui_objects_[i].activeSelf;
            }
        }

        // ===== カメラモードUI初期化 =====
        foreach (var obj in camera_mode_only_objects_)
        {
            if (obj == null) continue;

            obj.SetActive(false);

            // CanvasGroup自動付与
            if (camera_ui_canvas_group_ == null)
            {
                camera_ui_canvas_group_ = obj.GetComponent<CanvasGroup>();
                if (camera_ui_canvas_group_ == null)
                {
                    camera_ui_canvas_group_ = obj.AddComponent<CanvasGroup>();
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMode();
        }

        if (is_camera_mode_)
        {
            HandleCameraModeUIFade();
        }
    }

    /// <summary>
    /// UI表示とカメラ操作モード切り替え
    /// </summary>
    private void ToggleMode()
    {
        is_ui_visible_ = !is_ui_visible_;

        if (!is_ui_visible_)
        {
            // ===== カメラモードへ =====
            is_camera_mode_ = true;
            no_input_timer_ = 0f;
            FadeInCameraUI();

            for (int i = 0; i < ui_objects_.Length; i++)
            {
                if (ui_objects_[i] != null)
                {
                    ui_prev_active_states_[i] = ui_objects_[i].activeSelf;
                    ui_objects_[i].SetActive(false);
                }
            }

            foreach (var obj in camera_mode_only_objects_)
            {
                if (obj != null) obj.SetActive(true);
            }

            custom_player_rotation_.SetFreeControl(true);
        }
        else
        {
            // ===== 通常モードへ =====
            is_camera_mode_ = false;

            for (int i = 0; i < ui_objects_.Length; i++)
            {
                if (ui_objects_[i] != null)
                {
                    ui_objects_[i].SetActive(ui_prev_active_states_[i]);
                }
            }

            foreach (var obj in camera_mode_only_objects_)
            {
                if (obj != null) obj.SetActive(false);
            }

            custom_player_rotation_.ReturnToDefaultFromExternal();
            custom_player_rotation_.SetFreeControl(false);
        }
    }

    /// <summary>
    /// カメラモード時のUIフェード制御
    /// </summary>
    private void HandleCameraModeUIFade()
    {
        bool has_input =
            Input.anyKey ||
            Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f;

        if (has_input)
        {
            no_input_timer_ = 0f;
            FadeInCameraUI();
        }
        else
        {
            no_input_timer_ += Time.deltaTime;

            if (no_input_timer_ >= fade_out_delay_)
            {
                FadeOutCameraUI();
            }
        }

        float target_alpha = is_faded_out_ ? 0f : 1f;
        camera_ui_canvas_group_.alpha =
            Mathf.Lerp(camera_ui_canvas_group_.alpha, target_alpha, Time.deltaTime * fade_speed_);
    }

    private void FadeInCameraUI()
    {
        is_faded_out_ = false;
        camera_ui_canvas_group_.blocksRaycasts = true;
        camera_ui_canvas_group_.interactable = true;
    }

    private void FadeOutCameraUI()
    {
        is_faded_out_ = true;
        camera_ui_canvas_group_.blocksRaycasts = false;
        camera_ui_canvas_group_.interactable = false;
    }
}
