using UnityEngine;

/// <summary>
/// TabキーでUI表示とカメラ操作モードを切り替える制御クラス
/// </summary>
public class CameraUIModeSwitcher : MonoBehaviour
{
    [Header("通常UI（カメラモード時は非表示）")]
    [SerializeField] private GameObject[] ui_objects_;

    [Header("カメラモード時のみ表示するUI")]
    [SerializeField] private GameObject[] camera_mode_only_objects_;

    [Header("カメラ回転制御")]
    [SerializeField] private CustomPlayerRotation custom_player_rotation_;

    private bool is_ui_visible_ = true;

    // 通常UIの元の表示状態
    private bool[] ui_prev_active_states_;

    void Start()
    {
        // ===== 通常UIの初期状態を保存 =====
        ui_prev_active_states_ = new bool[ui_objects_.Length];

        for (int i = 0; i < ui_objects_.Length; i++)
        {
            if (ui_objects_[i] != null)
            {
                ui_prev_active_states_[i] = ui_objects_[i].activeSelf;
            }
        }

        // カメラモード専用UIは初期OFFを保証
        for (int i = 0; i < camera_mode_only_objects_.Length; i++)
        {
            if (camera_mode_only_objects_[i] != null)
            {
                camera_mode_only_objects_[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMode();
        }
    }

    /// <summary>
    /// UI表示とカメラ操作モードを切り替える
    /// </summary>
    private void ToggleMode()
    {
        is_ui_visible_ = !is_ui_visible_;

        if (!is_ui_visible_)
        {
            // ===== カメラモードへ =====

            // 通常UIを非表示
            for (int i = 0; i < ui_objects_.Length; i++)
            {
                if (ui_objects_[i] != null)
                {
                    ui_prev_active_states_[i] = ui_objects_[i].activeSelf;
                    ui_objects_[i].SetActive(false);
                }
            }

            // カメラモード専用UIを表示
            for (int i = 0; i < camera_mode_only_objects_.Length; i++)
            {
                if (camera_mode_only_objects_[i] != null)
                {
                    camera_mode_only_objects_[i].SetActive(true);
                }
            }

            custom_player_rotation_.SetFreeControl(true);
        }
        else
        {
            // ===== 通常モードへ =====

            // 通常UIを元に戻す
            for (int i = 0; i < ui_objects_.Length; i++)
            {
                if (ui_objects_[i] != null)
                {
                    ui_objects_[i].SetActive(ui_prev_active_states_[i]);
                }
            }

            // カメラモード専用UIを非表示
            for (int i = 0; i < camera_mode_only_objects_.Length; i++)
            {
                if (camera_mode_only_objects_[i] != null)
                {
                    camera_mode_only_objects_[i].SetActive(false);
                }
            }

            custom_player_rotation_.ReturnToDefaultFromExternal();
            custom_player_rotation_.SetFreeControl(false);
        }
    }
}
