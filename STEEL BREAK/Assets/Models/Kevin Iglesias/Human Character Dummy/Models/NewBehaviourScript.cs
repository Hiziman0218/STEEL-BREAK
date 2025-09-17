using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NewBehaviourScript : MonoBehaviour
{
    [Header("IK�ݒ�")]
    [Tooltip("Transform the hand should reach to")]
    public Transform ikTarget; //IK�̃^�[�Q�b�g
    [Range(0f, 1f)]
    public float ikPositionWeight = 1f; //IK�̍��W�E�F�C�g
    [Range(0f, 1f)]
    public float ikRotationWeight = 1f; //IK�̉�]�E�F�C�g
    private Animator animator; //�A�j���[�^�[

    void Start()
    {
        //�A�j���[�V�������擾
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        //�A�j���[�V�����������
        if (animator)
        {
            //IK�̃^�[�Q�b�g���ݒ肳��Ă����
            if (ikTarget != null)
            {
                //IK�̃E�F�C�g�A���W�A��]��ݒ�
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikPositionWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikRotationWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, ikTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, ikTarget.rotation);
            }
            else
            {
                //IK�̃E�F�C�g��0�ɐݒ�(���̈ʒu�ɖ߂�)
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }
}
