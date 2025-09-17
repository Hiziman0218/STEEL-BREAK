using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IKPointControl : MonoBehaviour
{
    [Header("�Q�Ɛݒ�")]
    [Tooltip("LockOn �X�N���v�g�i�蓮�A�T�C���j")]
    public LockOn lockOn;

    [Tooltip("�v���C���[�{�̂� Transform")]
    public Transform player;

    [Header("�G�����b�N���̃I�t�Z�b�g")]
    [Tooltip("�G�����Ȃ��Ƃ��ɐL�΂�����")]
    public float forwardDistance = 2.0f;

    [Tooltip("�G�����Ȃ��Ƃ��ɐL�΂�����")]
    public float heightOffset = 1.2f;

    void Start()
    {
        // �K�v�Ȃ瓮�I�ɒT��
        if (lockOn == null)
            lockOn = FindObjectOfType<LockOn>();
        if (player == null)
            player = transform.root; // IKPoint ���q�ɂ��Ă����ꍇ�̗�
    }

    void LateUpdate()
    {
        //IKpoint�̍��W���X�V
        UpdateIKPoint();
    }

    /// <summary>
    /// �^�[�Q�b�g�̗L���ɂ���č��W���X�V
    /// </summary>
    private void UpdateIKPoint()
    {
        Transform target = lockOn != null ? lockOn.CurrentTarget : null;

        if (target != null)
        {
            // �G�ƃv���C���[�̒��_���v�Z���A�ʒu�Ɖ�]�𔽉f
            Vector3 midPoint = (player.position + target.position) * 0.5f;
            transform.position = midPoint;

            Vector3 dir = target.position - transform.position;
            if (dir.sqrMagnitude > 0.0001f) // �[�����Z�h�~
            {
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        else
        {
            // �v���C���[���ʕ����֑O���I�t�Z�b�g
            Vector3 forwardPoint = player.position + player.forward * forwardDistance;
            Quaternion forwardRotate = player.rotation;
            //���������Z
            forwardPoint.y += heightOffset;
            transform.position = forwardPoint;
            transform.rotation = forwardRotate;
        }
    }
}
