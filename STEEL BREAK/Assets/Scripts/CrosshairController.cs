using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRect = null;
    public LockOn lockOn;
    public Camera mainCamera;
    public Vector2 screenCenter;
    public Vector2 screenOffset;

    void Awake()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Start()
    {
        // Start でも一度だけ探す（TutorialField.Start が終わっている前提）
        TryFindDependencies();
    }

    void Update()
    {
        // もしまだどちらかが null なら毎フレーム再検索
        if (lockOn == null || mainCamera == null)
            TryFindDependencies();

        if (lockOn == null || mainCamera == null || crosshairRect == null)
            return;

        // 敵に照準マーカーを設定
        if (lockOn.CurrentTarget != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(lockOn.CurrentTarget.position);
            crosshairRect.position = screenPos;
        }
        else
        {
            crosshairRect.position = screenCenter + screenOffset;
        }
    }

    private void TryFindDependencies()
    {
        if (lockOn == null)
        {
            lockOn = FindObjectOfType<LockOn>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (crosshairRect == null)
        {
            Debug.LogError("[CrosshairController] crosshairRect が Inspector で未設定です！");
        }
    }
}
