using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Control : MonoBehaviour
{
    [System.Serializable]
    public class HandIK
    {
        [Header("IK設定")]
        public AvatarIKGoal hand;
        [Tooltip("IKのターゲット(武器など)")]
        public Transform ikTarget;          //IKのターゲット
        [Range(0f, 1f)]
        public float ikPositionWeight = 1f; //IKの座標ウェイト
        [Range(0f, 1f)]
        public float ikRotationWeight = 1f; //IKの回転ウェイト
        public float Counter = 0f;          //IKのウェイト調整用カウンター
        public bool isIKFinished = false;   //IKの反映が完了したか
        public IWeapon Weapon;              //このIKと対応した手に持つ武器
        [Tooltip("手首回転のオフセット（Inspectorで調整可）")]
        public Vector3 rotationOffsetEuler = Vector3.zero; //回転オフセット
    }

    private Animator animator;         // アニメーター
    private InputManager inputManager; // 入力管理クラス
    public HandIK[] hands;             // 手のリスト(右手と左手)

    [Header("IK 有効化／無効化設定")]
    [Tooltip("IK 無効化までの猶予時間（秒）")]
    public float disableDelay = 1.5f;
    [Tooltip("ウェイト補間の速さ")]
    public float lerpSpeed = 5f;
    [Header("IK 有効角度制限")]
    [Tooltip("IK を有効にする左右の最大角度（°）")]
    public float maxIKAngle = 60f;

    // 手ごとの状態管理
    private float lastFireTimeRight = -Mathf.Infinity;
    private float lastFireTimeLeft = -Mathf.Infinity;
    private float targetWeightRight = 0f;
    private float targetWeightLeft = 0f;

    void Start()
    {
        // アニメーションを取得
        animator = GetComponent<Animator>();
        // 入力受け取りクラスを取得
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // 右手武装使用
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

        // 左手武装使用
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
        // アニメーターが設定されていない場合、以降の処理を行わない
        if (animator == null) return;

        // handsの要素の数だけ繰り返し
        foreach (var handIK in hands)
        {
            // どちらの手か判定して目標ウェイトを選ぶ
            float goal = handIK.hand == AvatarIKGoal.RightHand
                         ? targetWeightRight
                         : targetWeightLeft;

            //角度を計算
            Vector3 toTarget = handIK.ikTarget != null
                           ? (handIK.ikTarget.position - transform.position)
                           : Vector3.zero;
            toTarget.y = 0f;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);

            //角度が最大角度を超えていたら、goalを0に設定
            if (Mathf.Abs(angle) > maxIKAngle)
            {
                goal = 0f;
            }

            // ターゲットなし時は必ず 0
            if (handIK.ikTarget == null) goal = 0f;

            //ゴールが0かどうかで加算と減算を切り替え
            if(goal == 0)
            {
                handIK.Counter -= Mathf.Lerp(0f, 1, Time.deltaTime * lerpSpeed);
            }
            else
            {
                handIK.Counter += Mathf.Lerp(0f, 1, Time.deltaTime * lerpSpeed);
            }
            //カウンターが0か1(ウェイトの最大値以上/最小値以下)なら、その値に設定   
            if (handIK.Counter <= 0)
            {
                handIK.Counter = 0;
            }   
            if (handIK.Counter >= 1)
            {
                handIK.Counter = 1;
                //IK完了フラグをtrueに設定
                handIK.isIKFinished = true;
            }
            else
            {
                //IK完了フラグをfalseに設定
                handIK.isIKFinished = false;
            }

            //計算後のカウンターを座標/回転のウェイトに反映
            handIK.ikPositionWeight = handIK.Counter;
            handIK.ikRotationWeight = handIK.Counter;

            // IKを適用（ウェイト設定は常に行う）
            animator.SetIKPositionWeight(handIK.hand, handIK.ikPositionWeight);
            animator.SetIKRotationWeight(handIK.hand, handIK.ikRotationWeight);

            if (handIK.ikTarget != null)
            {
                // 常に毎フレーム渡すことで、ウェイトに応じたブレンドが成立
                animator.SetIKPosition(handIK.hand, handIK.ikTarget.position);

                //回転にオフセットをかけて設定
                Quaternion targetRotation = handIK.ikTarget.rotation
                                            * Quaternion.Euler(handIK.rotationOffsetEuler);
                animator.SetIKRotation(handIK.hand, targetRotation);
            }
        }
    }
}
