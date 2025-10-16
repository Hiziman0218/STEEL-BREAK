using UnityEngine;

public class Weapon_Back : MonoBehaviour
{
    [Tooltip("発射前に回転機能を使うか")]
    [SerializeField] private bool m_useRotate = true;
    [Tooltip("発射前に減速機能を使うか")]
    [SerializeField] private bool m_useDeceleration = true;

    private bool m_isTrigger;         //発射入力を受けたか
    private bool m_isRotated;         //敵方向への回転が完了したか
    private bool m_isDecelerated;     //減速が完了したか

    private Vector3 m_targetPos;      //発射時に取得した敵の位置
    private Quaternion m_targetRot;   //向きたい方向
    private float m_rotateSpeed = 5f; //向きを変える速さ

    private float m_decelerateRate = 5f; //減速の速さ(大きいほど急減速)
    private float m_minMoveThreshold = 0.05f; //減速完了とみなす速度閾値

    private Weapon_Shooting m_shooting;
    private InputManager m_inputManager;
    private LockOn m_lockOn;

    private void Awake()
    {
        //各参照取得
        m_shooting = GetComponent<Weapon_Shooting>();

        if (m_shooting != null)
        {
            //武装セットの探索オブジェクト名を変更
            m_shooting.SetCheckPoint("AttachPoint");
        }
    }

    private void Start()
    {
        //各参照取得
        //m_shooting = GetComponent<Weapon_Shooting>();
        m_inputManager = transform.root.GetComponent<InputManager>();
        m_lockOn = transform.root.GetComponent<LockOn>();

        //各機能を使うかでフラグを管理(使わない機能のフラグは常にtrue)
        if (!m_useRotate) m_isRotated = true;
        if (!m_useDeceleration) m_isDecelerated = true;

        /*
        if (m_shooting != null)
        {
            //武装セットの探索オブジェクト名を変更
            m_shooting.SetCheckPoint("AttachPoint");
        }*/
    }

    private void Update()
    {
        //銃クラスが無い場合は、以降の処理を行わない
        if (m_shooting == null) return;

        //発射の入力を受け取っていたら
        if (m_isTrigger)
        {
            //敵方向へ向かせる
            if (m_useRotate) RotateTowardTarget();
            //減速させる
            if (m_useDeceleration) DecelerateMovement();

            //両方完了したら射撃しフラグ管理
            if (m_isRotated && m_isDecelerated)
            {
                m_shooting.SetWeaponBackReady(true);
                m_shooting.Use();
                //発射が完了していたら、フラグ管理
                if (m_shooting.GetIsFireComplete())
                {
                    m_shooting.SetWeaponBackReady(false);
                    m_isTrigger = false;
                }
            }
        }
    }

    /// <summary>
    /// 敵の方向へラープで向かせる
    /// </summary>
    private void RotateTowardTarget()
    {
        Transform root = transform.root;
        root.rotation = Quaternion.RotateTowards(root.rotation, m_targetRot, m_rotateSpeed * Time.deltaTime);

        float angleDiff = Quaternion.Angle(root.rotation, m_targetRot);
        if (angleDiff < 1f) // 1度未満なら完了とみなす
        {
            root.rotation = m_targetRot;
            m_isRotated = true;
        }
    }

    /// <summary>
    /// 徐々に減速
    /// </summary>
    private void DecelerateMovement()
    {
        if (m_inputManager == null) return;

        Vector3 currentMove = m_inputManager.m_MovePoint;
        currentMove = Vector3.Lerp(currentMove, Vector3.zero, Time.deltaTime * m_decelerateRate);
        m_inputManager.m_MovePoint = currentMove;

        if (currentMove.magnitude < m_minMoveThreshold)
        {
            m_inputManager.m_MovePoint = Vector3.zero;
            m_isDecelerated = true;
        }
    }

    /// <summary>
    /// 背面武器の発射リクエスト
    /// </summary>
    public void FireRequest()
    {
        Debug.Log("肩武器発射");
        //既にリクエストされていた場合は、以降の処理を行わない
        if (m_isTrigger) return;

        //フラグ設定(使わない機能のフラグは更新しない)
        m_isTrigger = true;
        if(m_useRotate) m_isRotated = false;
        if(m_useDeceleration) m_isDecelerated = false;

        //ターゲットがいる場合、ターゲットの座標取得
        if (m_lockOn != null && m_lockOn.CurrentTarget != null)
            m_targetPos = m_lockOn.CurrentTarget.position;
        //ターゲットがいない場合、前方をターゲットに設定
        else
            m_targetPos = transform.root.position + transform.root.forward * 10f;

        //敵方向の目標回転を計算
        Vector3 dir = (m_targetPos - transform.root.position).normalized;
        dir.y = 0f; //水平方向だけ向くように
        m_targetRot = Quaternion.LookRotation(dir);
    }
}
