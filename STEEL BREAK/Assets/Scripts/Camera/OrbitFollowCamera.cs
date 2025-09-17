using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�ʒu��Ǐ]���A�u��ʒ����v�����Ƀ}�E�X�Ŏ��R�ɉ�]�ł���I�[�r�b�g�J����
/// ��InertiaCamera �Ƃ͕ʃt�@�C���^�ʃN���X�Ƃ��ĉ^�p���Ă��������B
/// </summary>
public class OrbitFollowCamera : MonoBehaviour
{
    [Header("�\�\�\�\�\ �Ǐ]�ݒ� �\�\�\�\�\")]
    [Tooltip("�Ǐ]�ΏۂƂȂ�v���C���[�i�܂��͔C�ӂ̃^�[�Q�b�g�j")]
    public Transform target;

    [Tooltip("�^�[�Q�b�g����̏����I�t�Z�b�g�i���[�J�� Z��-�O�AY����AX���E�j")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Tooltip("�J�����ʒu�̒Ǐ]�u�����h���x�i�傫���قǑf�����Ǐ]�j")]
    public float followSpeed = 10f;

    [Header("�\�\�\�\�\ �}�E�X����ݒ� �\�\�\�\�\")]
    [Tooltip("�}�E�X���E�i���[�j���x")]
    public float yawSensitivity = 3f;
    [Tooltip("�}�E�X�㉺�i�s�b�`�j���x")]
    public float pitchSensitivity = 3f;
    
    [Header("�\�\�\�\�\ ��]���� �\�\�\�\�\")]
    [Tooltip("�s�b�`�i�㉺��]�j�̍ŏ��p�x�i���������~�b�g�j")]
    public float pitchMin = -30f;
    [Tooltip("�s�b�`�i�㉺��]�j�̍ő�p�x�i��������~�b�g�j")]
    public float pitchMax = 60f;

    // �����ێ��p�F���݂̃��[�^�s�b�`�p�x
    public float yaw;
    public float pitch;

    /// <summary>
    /// �������F���݂̃J�����p�x��ǂݍ��݁A�J�[�\�����b�N
    /// </summary>
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitFollowCamera: Target ���ݒ肳��Ă��܂���B");
            enabled = false;
            return;
        }

        // ���݂̉�]�������l�Ƃ��Ď擾
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // �J�[�\���𒆉��Ƀ��b�N����\��
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// LateUpdate�F���ׂĂ̓����X�V��Ɂu��]���ʒu�Ǐ]�������v���s���A�K�^����h�~
    /// </summary>
    void LateUpdate()
    {
        /*
        //�ڕW�����Ȃ��ꍇ�A�ȍ~�̏������s��Ȃ�
        if (target == null) return;

        // 1) �}�E�X���͂ɂ��p�x�X�V
        float mouseX = Input.GetAxis("Mouse X") * yawSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * pitchSensitivity;

        yaw += mouseX;      // ���E
        pitch -= mouseY;    // �㉺�i�㉺�Ŕ��]�K�p�j

        // 2) �C���X�y�N�^�ݒ�Ŋp�x�𐧌�
        yaw = yaw % 360f;
        //pitch = pitch % 360f;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 3) ��]���쐬�i�I�[�r�b�g��]�p�N�H�[�^�j�I���j
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 4) �^�[�Q�b�g�ʒu�{��]�~�I�t�Z�b�g �ŗ��z�ʒu���Z�o
        Vector3 desiredPosition = target.position + orbitRotation * offset;

        // 5) �����ɒǏ]
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // 6) �J�������^�[�Q�b�g�Ɍ�����i��ʒ����ɏ�Ƀ^�[�Q�b�g���f���j
        transform.LookAt(target.position);
        */

        
    }

    /// <summary>
    /// �O������J�[�\�����b�N������
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// �O������ēx�J�[�\�����b�N��L���ɂ�
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

