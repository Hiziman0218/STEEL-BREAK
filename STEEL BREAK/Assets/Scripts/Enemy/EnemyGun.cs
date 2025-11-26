using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    [SerializeField] private GunStatusData m_statusData;  //銃の性能(インスペクタで設定)
    [SerializeField] private Transform m_muzzleTransform;
    
    private GunStatus m_status;    //銃の性能(インスペクタで設定したものを代入)

    private Transform targetPoint; //目標とするポイント

    private string m_myTeam;       //自身の所属するチーム

    private float m_elapsedTime;   //経過時間計測用

    private bool m_isFire = false; //発射可能フラグ
    private bool m_isReloading = false; //リロード中フラグ

    private void Awake()
    {
        //銃のステータスを設定
        m_status = new GunStatus(m_statusData);
    }

    private void Update()
    {
        //毎フレーム時間を加算
        m_elapsedTime += Time.deltaTime;

        //リロード処理中
        if (m_isReloading)
        {
            if (m_elapsedTime >= m_status.GetReloadTime())
            {
                ReloadComplete();
            }
            return;
        }

        //クールタイム管理
        if (!m_isFire)
        {
            if (m_elapsedTime >= m_status.GetRate())
            {
                //クールタイム終了
                m_isFire = true;
                m_elapsedTime = 0f;
            }
        }
    }

    /// <summary>
    /// 弾丸を発射
    /// </summary>
    public void Fire()
    {
        //発射できないなら以降の処理を行わない
        if (!m_isFire) return;

        //ターゲットポイントが設定されていない場合、プレイヤーのBPを探索
        if (targetPoint == null)
        {
            FindPlayerBP();
            //プレイヤーのBPが見つからなければ、以降の処理を行わない
            if (targetPoint == null) return;
        }

        Vector3 dir = (targetPoint.position - m_muzzleTransform.position).normalized;

        //弾を有効化
        if (m_status.GetBulletPrefab())
        {
            //弾丸発射処理
            BulletBase bullet = Instantiate(m_status.GetBulletPrefab(), m_muzzleTransform.position, Quaternion.LookRotation(dir));
            bullet.SetTeam(m_myTeam);
            bullet.SetDamage(m_status.GetDamage());
            bullet.SetSpeed(m_status.GetSpeed());

            //レーザー専用の処理
            Laser laser = bullet.GetComponent<Laser>();
            if (laser != null)
            {
                laser.SetParent(transform);
            }

            //弾の初速を velocity で設定
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = dir * m_status.GetSpeed();
            }
        }

        //マズルフラッシュのエフェクトを有効化
        if (m_status.GetMuzzleFlashEffect())
        {
            GameObject MuzzleFlash = Instantiate(m_status.GetMuzzleFlashEffect(), m_muzzleTransform.position, m_muzzleTransform.rotation);
            Destroy(MuzzleFlash, 0.1f);
        }

        //弾数減少/フラグ更新
        m_status.SetAmmo(m_status.GetAmmo() - 1);
        m_isFire = false;
        m_elapsedTime = 0f;
        if (m_status.GetAmmo() <= 0)
        {
            Reload();
        }
    }

    /// <summary>
    /// プレイヤーからBPを探索
    /// </summary>
    private void FindPlayerBP()
    {
        //プレイヤーを探索し、見つからなければ以降の処理を行わない
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform bp = player.transform.Find("BP");
        if (bp != null)
        {
            targetPoint = bp;
        }
        else
        {
            targetPoint = player.transform;
        }
    }

    /// <summary>
    /// リロード処理
    /// </summary>
    public void Reload()
    {
        //既にリロード中か、弾数が既に最大なら、以下の処理を行わない
        if (m_isReloading || m_status.GetAmmo() >= m_status.GetMaxAmmo()) return;

        //各フラグをリロード中の状態の物に設定
        m_isFire = false;
        m_isReloading = true;
    }

    /// <summary>
    /// リロード完了処理
    /// </summary>
    private void ReloadComplete()
    {
        //各フラグをリロード前の状態の物に設定し、弾丸と経過時間を初期化
        m_isReloading = false;
        m_status.SetAmmo(m_status.GetMaxAmmo());
        m_isFire = true;
        m_elapsedTime = 0f;
    }

    /// <summary>
    /// 自身の所属チームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }
}
