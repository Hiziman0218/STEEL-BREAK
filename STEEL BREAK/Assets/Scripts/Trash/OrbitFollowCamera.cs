using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー位置を追従しつつ、「画面中央」を軸にマウスで自由に回転できるオービットカメラ
/// ※InertiaCamera とは別ファイル／別クラスとして運用してください。
/// </summary>
public class OrbitFollowCamera : MonoBehaviour
{
    [Header("――――― 追従設定 ―――――")]
    [Tooltip("追従対象となるプレイヤー（または任意のターゲット）")]
    public Transform target;

    [Tooltip("ターゲットからの初期オフセット（ローカル Z＝-前、Y＝上、X＝右）")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Tooltip("カメラ位置の追従ブレンド速度（大きいほど素早く追従）")]
    public float followSpeed = 10f;

    [Header("――――― マウス操作設定 ―――――")]
    [Tooltip("マウス左右（ヨー）感度")]
    public float yawSensitivity = 3f;
    [Tooltip("マウス上下（ピッチ）感度")]
    public float pitchSensitivity = 3f;
    
    [Header("――――― 回転制限 ―――――")]
    [Tooltip("ピッチ（上下回転）の最小角度（下向きリミット）")]
    public float pitchMin = -30f;
    [Tooltip("ピッチ（上下回転）の最大角度（上向きリミット）")]
    public float pitchMax = 60f;

    // 内部保持用：現在のヨー／ピッチ角度
    public float yaw;
    public float pitch;

    /// <summary>
    /// 初期化：現在のカメラ角度を読み込み、カーソルロック
    /// </summary>
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitFollowCamera: Target が設定されていません。");
            enabled = false;
            return;
        }

        // 現在の回転を初期値として取得
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // カーソルを中央にロック＆非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// LateUpdate：すべての動き更新後に「回転→位置追従→注視」を行い、ガタつきを防止
    /// </summary>
    void LateUpdate()
    {
        /*
        //目標がいない場合、以降の処理を行わない
        if (target == null) return;

        // 1) マウス入力による角度更新
        float mouseX = Input.GetAxis("Mouse X") * yawSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * pitchSensitivity;

        yaw += mouseX;      // 左右
        pitch -= mouseY;    // 上下（上下で反転適用）

        // 2) インスペクタ設定で角度を制限
        yaw = yaw % 360f;
        //pitch = pitch % 360f;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 3) 回転を作成（オービット回転用クォータニオン）
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 4) ターゲット位置＋回転×オフセット で理想位置を算出
        Vector3 desiredPosition = target.position + orbitRotation * offset;

        // 5) 平滑に追従
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // 6) カメラをターゲットに向ける（画面中央に常にターゲットを映す）
        transform.LookAt(target.position);
        */

        
    }

    /// <summary>
    /// 外部からカーソルロックを解除
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 外部から再度カーソルロックを有効にし
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

