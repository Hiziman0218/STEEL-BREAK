using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("依存オブジェクト")]
    [Tooltip("ロックオン管理クラス（Inspector で設定しても、自動検索もされます）")]
    public LockOn lockOn;
    private Transform enemy;
    [Tooltip("マーカーを配置する親 Canvas の RectTransform")]
    public RectTransform canvasRect;
    [Tooltip("このゲームオブジェクトにアタッチされた Image")]
    public Image markerImage;
    private Camera cam;
    public Vector2 screenOffset = Vector2.zero;

    void Awake()
    {
        // Canvas 自動取得（直接 Canvas の RectTransform が渡されていなければ）
        if (canvasRect == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.transform as RectTransform;
            else
                Debug.LogError("[LockOnMarker] Canvas が見つかりません");
        }

        // Image 自動取得
        if (markerImage == null)
        {
            markerImage = GetComponent<Image>();
            if (markerImage == null)
                Debug.LogError("[LockOnMarker] Image コンポーネントが見つかりません");
        }

        // カメラ参照は MainCamera
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[LockOnMarker] Camera.main が見つかりません");
    }

    void Start()
    {
        //メインカメラを取得
        cam = Camera.main;
        if (canvasRect == null) Debug.LogError("Canvas Rect が未設定です");
        if (markerImage == null) Debug.LogError("Image コンポーネントが未設定です");

        // Inspector でセットされていなければ、自動検索
        if (lockOn == null)
        {
            lockOn = FindObjectOfType<LockOn>();
            if (lockOn == null)
                Debug.LogError("[LockOnMarker] LockOn スクリプトがシーン内に見つかりません");
        }
    }

    void Update()
    {
        //ロックオンを取得
        if (lockOn == null)
            lockOn = FindObjectOfType<LockOn>();

        //必要な要素が一つでもnullなら、以降の処理を行わない
        if (lockOn == null || canvasRect == null || markerImage == null || cam == null)
            return;

        //ロックオンする
        var enemy = lockOn.CurrentTarget;
        if (enemy != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(enemy.position);
            if (screenPos.z < 0)
            {
                markerImage.enabled = false; // カメラの裏にいるなら非表示
                return;
            }

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null, // Overlayなのでカメラはnull
                out localPos
            );
            markerImage.rectTransform.anchoredPosition = localPos + screenOffset;
            markerImage.enabled = true;
        }
    }
}
