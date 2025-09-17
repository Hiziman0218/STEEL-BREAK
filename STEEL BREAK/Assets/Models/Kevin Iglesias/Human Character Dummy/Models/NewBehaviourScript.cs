using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NewBehaviourScript : MonoBehaviour
{
    [Header("IK設定")]
    [Tooltip("Transform the hand should reach to")]
    public Transform ikTarget; //IKのターゲット
    [Range(0f, 1f)]
    public float ikPositionWeight = 1f; //IKの座標ウェイト
    [Range(0f, 1f)]
    public float ikRotationWeight = 1f; //IKの回転ウェイト
    private Animator animator; //アニメーター

    void Start()
    {
        //アニメーションを取得
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        //アニメーションがあれば
        if (animator)
        {
            //IKのターゲットが設定されていれば
            if (ikTarget != null)
            {
                //IKのウェイト、座標、回転を設定
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikPositionWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikRotationWeight);
                animator.SetIKPosition(AvatarIKGoal.RightHand, ikTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, ikTarget.rotation);
            }
            else
            {
                //IKのウェイトを0に設定(元の位置に戻す)
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }
}
