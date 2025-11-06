using UnityEngine;

public class Weapon_Back : MonoBehaviour
{
    [Tooltip("発射前に機体回転機能を使うか")]
    [SerializeField] private bool m_usePlayerRotate = false;
    [Tooltip("発射時に武器回転機能を使うか")]
    [SerializeField] private bool m_useWeaponRotate = false;
    [Tooltip("発射前に減速機能を使うか")]
    [SerializeField] private bool m_useDeceleration = false;
    [Tooltip("向きを変える速さ")]
    [SerializeField] private float m_rotateSpeed = 5f;    //向きを変える速さ
    [Tooltip("減速の速さ")]
    [SerializeField] private float m_decelerateRate = 5f; //減速の速さ(大きいほど急減速)

    [Header("武器回転制限(上下左右)")]
    [Tooltip("上方向")]
    [SerializeField] private float m_maxPitch = 30f;
    [Tooltip("下方向")]
    [SerializeField] private float m_minPitch = -10f;
    [Tooltip("右方向")]
    [SerializeField] private float m_maxYaw = 45f;
    [Tooltip("左方向")]
    [SerializeField] private float m_minYaw = -45f;

    private bool m_isTrigger;         //発射入力を受けたか
    private bool m_isRotated;         //敵方向への回転が完了したか
    private bool m_isDecelerated;     //減速が完了したか

    private Vector3 m_targetPos;      //発射時に取得した敵の位置
    private Quaternion m_targetRot;   //向きたい方向
    private Quaternion m_defaultLocalRot; // 初期ローカル回転

    private float m_minMoveThreshold = 0.05f; //減速完了とみなす速度閾値

    private Weapon_Shooting m_shooting;
    private InputManager m_inputManager;
    private LockOn m_lockOn;

    private void Awake()
    {
        //参照取得
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
        m_inputManager = transform.root.GetComponent<InputManager>();
        m_lockOn = transform.root.GetComponent<LockOn>();

        //初期姿勢を記録
        m_defaultLocalRot = transform.localRotation;

        //各機能を使うかでフラグを管理(使わない機能のフラグは常にtrue)
        if (!m_usePlayerRotate) m_isRotated = true;
        if (!m_useDeceleration) m_isDecelerated = true;
    }

    private void Update()
    {
        //銃クラスが無い場合は、以降の処理を行わない
        if (m_shooting == null) return;

        //リロード中もしくは発射レートのクールタイム中なら、トリガーをfalseに
        if (m_shooting.GetReloading() || m_shooting.GetIsCoolTime()) m_isTrigger = false;
        
        //発射の入力を受け取ってるかつ、弾丸が0ではないなら
        if (m_isTrigger && m_shooting.GetGunStatus().GetAmmo() > 0)
        {
            //機体を敵方向へ向かせる
            if (m_usePlayerRotate) RotateTowardTarget();
            //武装を敵方向へ向かせる
            if (m_useWeaponRotate)
            {
                //ターゲットが存在している場合は敵の方へ、いない場合は正面へ回転
                if (m_lockOn.CurrentTarget) RotateWeaponBody();
                else RotateForward();

            }
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
        else
        {
            //武装の回転を行う武装の場合、正面へ回転
            if(m_useWeaponRotate) RotateForward();
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
        if (angleDiff < 1f) //1度未満なら完了とみなす
        {
            root.rotation = m_targetRot;
            m_isRotated = true;
        }
    }

    /// <summary>
    /// 武装自体を上下左右に制限付きで回転させる（ローカル空間で角度制限と補間を行う）
    /// - 重要: 目標方向はまずワールド→親のローカル空間に変換して扱うことで座標系の混乱を防ぐ
    /// </summary>
    private void RotateWeaponBody()
    {
        // safety
        if (m_lockOn == null || m_lockOn.CurrentTarget == null) return;
        if (transform.parent == null) return; // 親がないとローカル基準が不明

        // World-space direction to target (from weapon origin)
        Vector3 worldDir = (m_targetPos - transform.position);
        float worldDirMag = worldDir.magnitude;
        if (worldDirMag < 0.0001f) return; // ほぼ同位置なら回転不要

        worldDir /= worldDirMag; // normalize

        // --- ここが重要 ---
        // 武器の回転制御は "親のローカル空間" で行う（transform.localRotation に制御を入れるため）
        // worldDir を親ローカル空間へ変換する
        Vector3 localDir = transform.parent.InverseTransformDirection(worldDir);
        // localDir が Z 軸方向を向くようにターゲット回転を作る
        Quaternion targetLocalRot = Quaternion.LookRotation(localDir, Vector3.up);

        // targetLocalRot をローカルのオイラーに変換し、ピッチ(X), ヨー(Y) を取り出す
        Vector3 targetLocalEuler = targetLocalRot.eulerAngles;
        // eulerAngles は 0..360 なので -180..180 に正規化する
        float targetPitch = NormalizeAngle(targetLocalEuler.x); // 上下
        float targetYaw = NormalizeAngle(targetLocalEuler.y);   // 左右

        // 制限をローカル軸で適用（Inspector で与えた範囲はローカル角度として解釈）
        float clampedPitch = Mathf.Clamp(targetPitch, m_minPitch, m_maxPitch);
        float clampedYaw = Mathf.Clamp(targetYaw, m_minYaw, m_maxYaw);

        // クランプした角度から「目標のローカル回転」を作る
        Quaternion clampedLocalRot = Quaternion.Euler(clampedPitch, clampedYaw, 0f);

        // 現在のローカル回転
        Quaternion currentLocalRot = transform.localRotation;

        // 補間：最短回転経路を取る Quaternion.RotateTowards を使用
        float maxDegThisFrame = m_rotateSpeed * Time.deltaTime;
        Quaternion newLocalRot = Quaternion.RotateTowards(currentLocalRot, clampedLocalRot, maxDegThisFrame);

        // 最終セット
        transform.localRotation = newLocalRot;
    }

    /// <summary>
    /// 武装を正面方向に戻す
    /// </summary>
    private void RotateForward()
    {
        //未使用時は正面方向へ戻す
        if (m_useWeaponRotate)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                m_defaultLocalRot,
                Time.deltaTime * m_rotateSpeed * 0.05f //戻るスピードは少しゆっくり
            );
        }
    }

    /// <summary>
    /// 角度を -180～180 に変換
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
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
        //既にリクエストされていた場合は、以降の処理を行わない
        if (m_isTrigger) return;

        //フラグ設定(使わない機能のフラグは更新しない)
        m_isTrigger = true;
        if(m_usePlayerRotate) m_isRotated = false;
        if(m_useDeceleration) m_isDecelerated = false;

        //ターゲットがいる場合、ターゲットの座標取得
        if (m_lockOn != null && m_lockOn.CurrentTarget != null)
            m_targetPos = m_lockOn.CurrentTarget.position;
        //ターゲットがいない場合、前方をターゲットに設定
        else
            m_targetPos = transform.root.position + transform.root.forward * 10f;

        //敵方向の目標回転を計算
        Vector3 dir = (m_targetPos - transform.root.position).normalized;
        //dir.y = 0f; //水平方向だけ向くように
        m_targetRot = Quaternion.LookRotation(dir);
    }

    /// <summary>
    /// 回転機能を使うかを取得
    /// </summary>
    /// <returns></returns>
    public bool GetUseRotate()
    {
        return m_usePlayerRotate;
    }
}
