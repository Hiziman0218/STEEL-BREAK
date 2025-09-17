using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Control : MonoBehaviour
{
    [System.Serializable]
    public class HandIK
    {
        [Header("IK�ݒ�")]
        public AvatarIKGoal hand;
        [Tooltip("IK�̃^�[�Q�b�g(����Ȃ�)")]
        public Transform ikTarget;          //IK�̃^�[�Q�b�g
        [Range(0f, 1f)]
        public float ikPositionWeight = 1f; //IK�̍��W�E�F�C�g
        [Range(0f, 1f)]
        public float ikRotationWeight = 1f; //IK�̉�]�E�F�C�g
        public float Counter = 0f;          //IK�̃E�F�C�g�����p�J�E���^�[
        public bool isIKFinished = false;   //IK�̔��f������������
        public IWeapon Weapon;              //����IK�ƑΉ�������Ɏ�����
        [Tooltip("����]�̃I�t�Z�b�g�iInspector�Œ����j")]
        public Vector3 rotationOffsetEuler = Vector3.zero; //��]�I�t�Z�b�g
    }

    private Animator animator;         // �A�j���[�^�[
    private InputManager inputManager; // ���͊Ǘ��N���X
    public HandIK[] hands;             // ��̃��X�g(�E��ƍ���)

    [Header("IK �L�����^�������ݒ�")]
    [Tooltip("IK �������܂ł̗P�\���ԁi�b�j")]
    public float disableDelay = 1.5f;
    [Tooltip("�E�F�C�g��Ԃ̑���")]
    public float lerpSpeed = 5f;
    [Header("IK �L���p�x����")]
    [Tooltip("IK ��L���ɂ��鍶�E�̍ő�p�x�i���j")]
    public float maxIKAngle = 60f;

    // �育�Ƃ̏�ԊǗ�
    private float lastFireTimeRight = -Mathf.Infinity;
    private float lastFireTimeLeft = -Mathf.Infinity;
    private float targetWeightRight = 0f;
    private float targetWeightLeft = 0f;

    void Start()
    {
        // �A�j���[�V�������擾
        animator = GetComponent<Animator>();
        // ���͎󂯎��N���X���擾
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // �E�蕐���g�p
        if (inputManager.IsFireinRightHand)
        {
            lastFireTimeRight = Time.time;
            targetWeightRight = 1f;
            hands[0].Weapon.SetIKFinished(hands[0].isIKFinished);
        }
        else if (Time.time - lastFireTimeRight > disableDelay)
        {
            targetWeightRight = 0f;
        }

        // ���蕐���g�p
        if (inputManager.IsFireinLeftHand)
        {
            lastFireTimeLeft = Time.time;
            targetWeightLeft = 1f;
            hands[1].Weapon.SetIKFinished(hands[1].isIKFinished);
        }
        else if (Time.time - lastFireTimeLeft > disableDelay)
        {
            targetWeightLeft = 0f;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        // �A�j���[�^�[���ݒ肳��Ă��Ȃ��ꍇ�A�ȍ~�̏������s��Ȃ�
        if (animator == null) return;

        // hands�̗v�f�̐������J��Ԃ�
        foreach (var handIK in hands)
        {
            // �ǂ���̎肩���肵�ĖڕW�E�F�C�g��I��
            float goal = handIK.hand == AvatarIKGoal.RightHand
                         ? targetWeightRight
                         : targetWeightLeft;

            //�p�x���v�Z
            Vector3 toTarget = handIK.ikTarget != null
                           ? (handIK.ikTarget.position - transform.position)
                           : Vector3.zero;
            toTarget.y = 0f;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

            //�p�x���ő�p�x�𒴂��Ă�����Agoal��0�ɐݒ�
            if (Mathf.Abs(angle) > maxIKAngle)
            {
                goal = 0f;
            }

            // �^�[�Q�b�g�Ȃ����͕K�� 0
            if (handIK.ikTarget == null) goal = 0f;

            //�S�[����0���ǂ����ŉ��Z�ƌ��Z��؂�ւ�
            if(goal == 0)
            {
                handIK.Counter -= Mathf.Lerp(0f, 1, Time.deltaTime * lerpSpeed);
            }
            else
            {
                handIK.Counter += Mathf.Lerp(0f, 1, Time.deltaTime * lerpSpeed);
            }
            //�J�E���^�[��0��1(�E�F�C�g�̍ő�l�ȏ�/�ŏ��l�ȉ�)�Ȃ�A���̒l�ɐݒ�   
            if (handIK.Counter <= 0)
            {
                handIK.Counter = 0;
            }   
            if (handIK.Counter >= 1)
            {
                handIK.Counter = 1;
                //IK�����t���O��true�ɐݒ�
                handIK.isIKFinished = true;
            }
            else
            {
                //IK�����t���O��false�ɐݒ�
                handIK.isIKFinished = false;
            }

            //�v�Z��̃J�E���^�[�����W/��]�̃E�F�C�g�ɔ��f
            handIK.ikPositionWeight = handIK.Counter;
            handIK.ikRotationWeight = handIK.Counter;

            // IK��K�p�i�E�F�C�g�ݒ�͏�ɍs���j
            animator.SetIKPositionWeight(handIK.hand, handIK.ikPositionWeight);
            animator.SetIKRotationWeight(handIK.hand, handIK.ikRotationWeight);

            if (handIK.ikTarget != null)
            {
                // ��ɖ��t���[���n�����ƂŁA�E�F�C�g�ɉ������u�����h������
                animator.SetIKPosition(handIK.hand, handIK.ikTarget.position);

                //��]�ɃI�t�Z�b�g�������Đݒ�
                Quaternion targetRotation = handIK.ikTarget.rotation
                                            * Quaternion.Euler(handIK.rotationOffsetEuler);
                animator.SetIKRotation(handIK.hand, targetRotation);
            }
        }
    }
}
