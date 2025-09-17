using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("�ˑ��I�u�W�F�N�g")]
    [Tooltip("���b�N�I���Ǘ��N���X�iInspector �Őݒ肵�Ă��A��������������܂��j")]
    public LockOn lockOn;
    private Transform enemy;
    [Tooltip("�}�[�J�[��z�u����e Canvas �� RectTransform")]
    public RectTransform canvasRect;
    [Tooltip("���̃Q�[���I�u�W�F�N�g�ɃA�^�b�`���ꂽ Image")]
    public Image markerImage;
    private Camera cam;
    public Vector2 screenOffset = Vector2.zero;

    void Awake()
    {
        // Canvas �����擾�i���� Canvas �� RectTransform ���n����Ă��Ȃ���΁j
        if (canvasRect == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.transform as RectTransform;
            else
                Debug.LogError("[LockOnMarker] Canvas ��������܂���");
        }

        // Image �����擾
        if (markerImage == null)
        {
            markerImage = GetComponent<Image>();
            if (markerImage == null)
                Debug.LogError("[LockOnMarker] Image �R���|�[�l���g��������܂���");
        }

        // �J�����Q�Ƃ� MainCamera
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[LockOnMarker] Camera.main ��������܂���");
    }

    void Start()
    {
        //���C���J�������擾
        cam = Camera.main;
        if (canvasRect == null) Debug.LogError("Canvas Rect �����ݒ�ł�");
        if (markerImage == null) Debug.LogError("Image �R���|�[�l���g�����ݒ�ł�");

        // Inspector �ŃZ�b�g����Ă��Ȃ���΁A��������
        if (lockOn == null)
        {
            lockOn = FindObjectOfType<LockOn>();
            if (lockOn == null)
                Debug.LogError("[LockOnMarker] LockOn �X�N���v�g���V�[�����Ɍ�����܂���");
        }
    }

    void Update()
    {
        //���b�N�I�����擾
        if (lockOn == null)
            lockOn = FindObjectOfType<LockOn>();

        //�K�v�ȗv�f����ł�null�Ȃ�A�ȍ~�̏������s��Ȃ�
        if (lockOn == null || canvasRect == null || markerImage == null || cam == null)
            return;

        //���b�N�I������
        var enemy = lockOn.CurrentTarget;
        if (enemy != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(enemy.position);
            if (screenPos.z < 0)
            {
                markerImage.enabled = false; // �J�����̗��ɂ���Ȃ��\��
                return;
            }

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                null, // Overlay�Ȃ̂ŃJ������null
                out localPos
            );
            markerImage.rectTransform.anchoredPosition = localPos + screenOffset;
            markerImage.enabled = true;
        }
    }
}
