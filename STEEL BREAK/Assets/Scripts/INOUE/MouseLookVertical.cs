using UnityEngine;

public class MouseLookVertical : MonoBehaviour
{
    [Header("�}�E�X���x�i�㉺�ړ��j")]
    public float mouseSensitivity = 1.5f;

    [Header("�����_�̍�������")]
    public float minOffsetY = -1.0f;
    public float maxOffsetY = 1.0f;

    [Header("��ʒu�i�v���C���[�̓��⋹�Ȃǁj")]
    public Transform m_Base; // �����_�̊�ʒu�i�v���C���[��Transform�j

    [Header("�J�����{�̂�Transform")]
    public Transform m_CameraTransform; // �J������Transform

    [Header("�J�����̊�ʒu")]
    public Vector3 cameraBaseLocalOffset = new Vector3(0f, 1.5f, -4f); // �v���C���[���猩���J�����̏����I�t�Z�b�g

    private float currentYOffset = 0f;

    void Update()
    {
        if (m_Base == null || m_CameraTransform == null) return;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �����_Y�I�t�Z�b�g���}�E�X���͂ɉ����čX�V
        currentYOffset -= mouseY;
        currentYOffset = Mathf.Clamp(currentYOffset, minOffsetY, maxOffsetY);

        // �����_�̍��W���X�V�i�v���C���[�̑O���ʒu�ɃI�t�Z�b�g�t���Łj
        Vector3 gazePos = m_Base.position + m_Base.forward * 1.0f;
        gazePos.y += currentYOffset;
        transform.position = gazePos;

        // �J�����ʒu���V�[�\�[�֌W�ōX�V�i�����_���オ��΃J������������j
        Vector3 camLocalOffset = cameraBaseLocalOffset;
        camLocalOffset.y -= currentYOffset; // �V�[�\�[���ʁF�t�����ɕϓ�
        m_CameraTransform.position = m_Base.TransformPoint(camLocalOffset);
    }
}
