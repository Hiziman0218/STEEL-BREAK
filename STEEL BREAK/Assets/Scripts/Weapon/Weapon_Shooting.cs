using UnityEngine;

public class Weapon_Shooting : MonoBehaviour, IWeapon
{
    [Header("設定")]
    [Tooltip("銃の性能(GunStatusDataを設定)")]
    [SerializeField] private GunStatusData m_statusData;  //銃の性能(インスペクタで設定)

    [Tooltip("弾丸が生成される銃口")]
    [SerializeField] private Transform m_muzzleTransform; //発射口

    [Tooltip("持つ位置を調整するオフセット値")]
    [SerializeField] private Vector3 m_attachOffsetPos;   //銃を持つ位置の調整用

    private float m_elapsedTime; //経過時間計測用

    private bool m_isFire;       //発射可能フラグ
    private bool m_isFireNew = true; //外部管理用発射可能フラグ(外部からしか触らないので、ミニガン等外部から変更されないなら常にtrue)
    private bool m_isReloading;  //リロード中フラグ
    private bool m_isIKFinished; //IKが完了しているかフラグ

    private string m_myTeam;     //武器の所有者が所属するチーム

    private GunStatus m_status;            //銃の性能(インスペクタで設定したものを代入)
    private BulletManager m_bulletManager; //発射処理を委譲するマネージャー(現在オブジェクトプール未使用)
    private LockOn lockOn;                 //ロックオン機能

    private void Awake()
    {
        //銃のステータスを設定
        m_status = new GunStatus(m_statusData);

        //最初から撃てるように設定
        m_elapsedTime = m_status.GetRate();
        m_isFire = true;

        // プレイヤーにアタッチされたLockOnを取得
        lockOn = transform.root.GetComponent<LockOn>();
    }

    private void Update()
    {
        //発射不可能の場合
        if (!m_isFire)
        {
            //経過時間を計測
            m_elapsedTime += Time.deltaTime;

            //リロード中の場合
            if (m_isReloading)
            {
                //経過時間がリロード時間を超えていたら
                if (m_elapsedTime >= m_status.GetReloadTime())
                {
                    //リロード完了
                    ReloadComplete();
                }
            }
            //経過時間が発射レートを超えていたら
            else if (m_elapsedTime >= m_status.GetRate())
            {
                //発射可能処理
                m_isFire = true;
                m_elapsedTime = 0f;
            }
        }
    }

    /// <summary>
    /// 武器を手に持ち、装備させる
    /// </summary>
    /// <param name="hand">手のトランスフォーム</param>
    /// <param name="left">左手か(右手か左手の二択なのでフラグ管理)</param>
    public void AttachToHand(Transform hand, bool left)
    {
        //GripPointを検索し、見つからなければ以降の処理を行わない
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        //各種設定
        transform.SetParent(hand, false);
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= left ? -1f : 1f;
        transform.localPosition = offsetPos;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
    }

    /// <summary>
    /// 武器使用
    /// </summary>
    public void Use()
    {
        // 発射可否チェック
        if (!m_isFire || !m_isFireNew || !m_isIKFinished)
            return;

        Vector3 shootDir;

        //ターゲットがいる場合
        if (lockOn.CurrentTarget != null)
        {
            // 敵のBPを狙う
            Transform bp = lockOn.CurrentTarget.transform.Find("BP");

            if (bp != null)
            {
                Rigidbody targetRb = lockOn.CurrentTarget.GetComponent<Rigidbody>();
                shootDir = CalculateInterceptDirection(
                    m_muzzleTransform.position,
                    bp.position,
                    targetRb != null ? targetRb.linearVelocity : Vector3.zero,
                    m_status.GetSpeed()
                );

                // 銃口を敵に向ける
                m_muzzleTransform.rotation = Quaternion.LookRotation(shootDir);
            }
            else
            {
                // BP が見つからない場合はとりあえず位置のみで狙う
                Vector3 targetPos = lockOn.CurrentTarget.transform.position;
                shootDir = (targetPos - m_muzzleTransform.position).normalized;
                m_muzzleTransform.rotation = Quaternion.LookRotation(shootDir);
            }
        }
        //ターゲットがいない場合
        else
        {
            //親オブジェクトを取得
            GameObject rootObj = transform.root.gameObject;
            //親オブジェクトの回転を取得
            Quaternion forward = rootObj.transform.rotation;
            //銃口を元に戻す
            m_muzzleTransform.rotation = forward;
            shootDir = m_muzzleTransform.forward;
        }

        //弾を有効化
        NewBullet Dummy = Instantiate(m_status.GetBulletPrefab(), m_muzzleTransform.position, m_muzzleTransform.rotation);
        //弾のチームを自身と同じものに設定
        Dummy.SetTeam(m_myTeam);
        //ターゲットがいる場合、弾丸のターゲットに設定
        if(lockOn.CurrentTarget != null)
        {
            Dummy.SetTarget(lockOn.CurrentTarget);
        }
        //マズルフラッシュのエフェクトを有効化
        GameObject MuzzleFlash = Instantiate(m_status.GetMuzzleFlashEffect(), m_muzzleTransform.position, m_muzzleTransform.rotation);
        Destroy(MuzzleFlash, 0.1f);

        //弾の初速を velocity で設定
        Rigidbody rb = Dummy.GetComponent<Rigidbody>();
        rb.linearVelocity = shootDir * m_status.GetSpeed();

        //10秒後に削除
        Destroy(Dummy.gameObject, 10.0f);

        //既存の弾数減少／フラグ更新
        m_status.SetAmmo(m_status.GetAmmo() - 1);
        m_isFire = false;
        m_elapsedTime = 0f;
        if (m_status.GetAmmo() <= 0)
        {
            m_isReloading = true;
        }
    }

    /// <summary>
    /// 敵の移動を考慮した迎撃方向を計算する
    /// </summary>
    /// <param name="shooterPos">銃口の位置</param>
    /// <param name="targetPos">敵の現在位置</param>
    /// <param name="targetVel">敵の速度ベクトル</param>
    /// <param name="bulletSpeed">弾速</param>
    /// <returns>狙う方向</returns>
    private Vector3 CalculateInterceptDirection(Vector3 shooterPos, Vector3 targetPos, Vector3 targetVel, float bulletSpeed)
    {
        Vector3 displacement = targetPos - shooterPos;

        float a = Vector3.Dot(targetVel, targetVel) - bulletSpeed * bulletSpeed;
        float b = 2f * Vector3.Dot(displacement, targetVel);
        float c = Vector3.Dot(displacement, displacement);

        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0f || Mathf.Abs(a) < 0.001f)
        {
            // 解なし → 現在位置を狙う
            return displacement.normalized;
        }

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDisc) / (2f * a);
        float t2 = (-b - sqrtDisc) / (2f * a);

        float t = Mathf.Min(t1, t2);
        if (t < 0f) t = Mathf.Max(t1, t2);
        if (t < 0f) return displacement.normalized;

        Vector3 aimPoint = targetPos + targetVel * t;
        return (aimPoint - shooterPos).normalized;
    }

    /// <summary>
    /// リロード処理
    /// </summary>
    public void Reload()
    {
        //各フラグをリロード中の状態の物に設定
        m_isFire = false;
        m_isReloading = true;
    }

    /// <summary>
    /// リロード完了処理
    /// </summary>
    void ReloadComplete()
    {
        //各フラグをリロード前の状態の物に設定し、弾丸と経過時間を初期化
        m_isReloading = false;
        m_status.SetAmmo(m_status.GetMaxAmmo());
        m_isFire = true;
        m_elapsedTime = 0f;
    }

    /// <summary>
    /// IKの設定が終了したか設定(終了している場合のみ弾を発射できる)
    /// </summary>
    /// <param name="IKFinished"></param>
    public void SetIKFinished(bool IKFinished)
    {
        m_isIKFinished = IKFinished;
    }

    /// <summary>
    /// 自身のチームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    public void SetIsFire(bool isFire)
    {
        m_isFireNew = isFire;
    }

    /// <summary>
    /// 武器名を取得
    /// </summary>
    /// <returns></returns>
    public string GetName() => m_status.GetName();
}