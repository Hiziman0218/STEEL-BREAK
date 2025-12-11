using Game.Enum;
using UnityEngine;

public class Weapon_Shooting : MonoBehaviour, IWeapon
{
    [Header("設定")]
    [Tooltip("銃の性能(GunStatusDataを設定)")]
    [SerializeField] private GunStatusData m_statusData;  //銃の性能(インスペクタで設定)
    [Tooltip("弾丸が生成される銃口")]
    [SerializeField] private Transform m_muzzleTransform; //発射口
    [Tooltip("持つ位置を調整するオフセット値")]
    [SerializeField] private Vector3 m_attachOffsetPos;   //銃を持つ位置の調整用オフセット(不要？現在は全ての武器が0, 0, 0)
    [Tooltip("SE再生用(銃本体を設定)")]
    [SerializeField] private AudioSource m_audioSource;   //音声データ

    private float m_elapsedTime; //経過時間計測用

    private bool m_isFireInternal = false;    //発射可能フラグ(内部管理)
    private bool m_isFireExternal = false;    //発射可能フラグ(外部管理)
    private bool m_isExternalControl = false; //発射可否を外部管理をするか
    private bool m_isReloading;      //リロード中フラグ(リロード中か)
    private bool m_isCoolTime = false;  //発射レートのクールタイム中なら
    private bool m_isUsing;          //使用中フラグ(発射したかに関わらず、使用されようとしたか)
    private bool m_isFireComplete;   //発射完了フラグ(発射が完了した1フレームのみ)
    private bool m_isReloadComplete; //リロード完了フラグ(リロードが完了した1フレームのみ)
    private bool m_isIKFinished;     //IK完了フラグ
    private bool m_canPlayEmptySE = true; //空撃ち音声が鳴らせるか

    private bool m_isBackWeapon = false;  //自身が背面武装か
    private bool m_isBackWeaponReady;     //背面武装の発射の準備が完了したか

    private string m_myTeam; //武器の所有者が所属するチーム
    private string m_checkPoint = "GripPoint"; //装備するときに探索するポイントの名称

    private Vector3 m_shootDir;        //発射角度保存用
    private Transform m_currentTarget; //現在のターゲット保存用

    private GunStatus m_status;            //銃の性能(インスペクタで設定したものを代入)
    private BulletManager m_bulletManager; //発射処理を委譲するマネージャー(現在オブジェクトプール未使用)
    private LockOn lockOn;                 //ロックオン機能

    private void Awake()
    {
        //銃のステータスを設定
        m_status = new GunStatus(m_statusData);

        //自身が背面武装かを、Weapon_Backがアタッチされているかで確認
        var backComp = GetComponent<Weapon_Back>();
        if (backComp != null)
        {
            m_isBackWeapon = true;
            m_isIKFinished = true; //背面武装はIKを使わない
        }

        //最初から撃てるように設定
        m_elapsedTime = m_status.GetRate();
        m_isFireInternal = true;

        //最上位の親オブジェクトのプレイヤーにアタッチされたLockOnを取得
        lockOn = transform.root.GetComponent<LockOn>();
    }

    private void Update()
    {
        /*
        //発射不可能の場合
        if (!m_isFireInternal)
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
                m_isFireInternal = true;
                m_isCoolTime = false;
                m_elapsedTime = 0f;
            }
            //リロード中ではなく、発射レート中なら
            else
            {
                m_isCoolTime = true;
            }
        }*/

        // 毎フレーム時間を加算
        m_elapsedTime += Time.deltaTime;

        // リロード処理中
        if (m_isReloading)
        {
            if (m_elapsedTime >= m_status.GetReloadTime())
            {
                ReloadComplete();
            }
            return;
        }

        // クールタイム管理
        if (!m_isFireInternal)
        {
            if (m_elapsedTime >= m_status.GetRate())
            {
                // クールタイム終了
                m_isFireInternal = true;
                m_isCoolTime = false;
                m_elapsedTime = 0f;
            }
            else
            {
                // クールタイム中
                m_isCoolTime = true;
            }
        }
    }

    private void LateUpdate()
    {
        //使用中フラグ更新
        m_isUsing = false;
        //発射完了フラグ更新
        m_isFireComplete = false;
        //リロード完了フラグをfalseに設定
        m_isReloadComplete = false;
    }

    /// <summary>
    /// 武装を装備させる
    /// </summary>
    /// <param name="point">装備させるポイント</param>
    /// <param name="side">どちらに装備させるか</param>
    public void AttachToPoint(Transform point, AttachSide side)
    {
        //持たせる手が左手か判定
        bool isLeft = (side == AttachSide.Left);

        //GripPointを検索
        Transform grip = transform.Find(m_checkPoint);
        if (grip == null)
        {
            Debug.LogError($"{name} に接続箇所が見つかりません。");
            return;
        }

        //手のTransformを親に設定
        //SetParentの第2引数falseにより、ワールド座標を維持せずローカル座標を手基準に再計算
        transform.SetParent(point, false);

        //GripPointのローカル回転を打ち消す(★重要★)
        //GripPointのローカル回転を逆にかけることで、GripPointの向きを手の向きと一致
        transform.localRotation = Quaternion.Inverse(grip.localRotation);

        //GripPointのローカル位置を打ち消す
        //GripPointがPivotからどれだけ離れているか(ローカル位置)を反転して適用することで、
        //GripPointの位置が手の原点(hand.position)と一致するよう補正
        transform.localPosition = -grip.localPosition;

        //左右のオフセットを適用
        //左右の手で対称にしたい場合、x軸方向を反転
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= isLeft ? -1f : 1f;

        //最終的に補正を加える
        //上記の打ち消し＋回転補正を行ったあとで微調整値を加算
        transform.localPosition += offsetPos;

        //反転機能を使う銃モデルなら
        if (m_status.GetUseMirror())
        {
            //反転条件が一致していればモデルのスケールと座標のxを反転
            if (m_status.GetMirrorWhenHeld() == side)
            {
                // モデル反転
                Vector3 scale = transform.localScale;
                scale.x *= -1f;
                transform.localScale = scale;

                // 位置補正
                Vector3 pos = transform.localPosition;
                pos.x *= -1f;
                transform.localPosition = pos;
            }
        }
    }

    /// <summary>
    /// 武器使用
    /// </summary>
    public void Use()
    {
        //フラグ更新
        m_isUsing = true;
        m_isFireComplete = false;

        //内部管理の発射可否チェック
        if (!m_isFireInternal || !m_isIKFinished)
        {
            //if (!m_isFireInternal) Debug.Log("内部処理的に発射できない");

            //リロード中なら空撃ち音を再生
            if (m_isReloading && m_canPlayEmptySE && m_elapsedTime >= m_status.GetRate())
            {
                if (m_status.GetEmptyFireSE())
                {
                    PlayFireSE(m_status.GetEmptyFireSE());
                }
                m_canPlayEmptySE = false;
            }

            return;
        }

        //外部管理をしているなら、外部管理の発射可否チェック
        if(m_isExternalControl)
        {
            if (!m_isFireExternal)
            {
                //Debug.Log("外部処理的に発射できない");
                return;
            }
        }

        //自身が背面武装なら、発射準備ができているか確認
        if(m_isBackWeapon)
        {
            if (!m_isBackWeaponReady) return;
        }

        Vector3 shootDir;

        //ターゲットがいる場合
        if (lockOn.CurrentTarget != null)
        {
            //ターゲットを設定
            m_currentTarget = lockOn.CurrentTarget;

            //銃口の制御を行う場合
            if (m_status.GetUseMuzzleControl())
            {
                //敵のBPを狙う
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

                    //銃口を敵に向ける
                    m_muzzleTransform.rotation = Quaternion.LookRotation(shootDir);
                }
                else
                {
                    //BP が見つからない場合はとりあえず位置のみで狙う
                    Vector3 targetPos = lockOn.CurrentTarget.transform.position;
                    shootDir = (targetPos - m_muzzleTransform.position).normalized;
                    m_muzzleTransform.rotation = Quaternion.LookRotation(shootDir);
                }
            }
            //行わない場合
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

        //発射角度を保存
        m_shootDir = shootDir;

        //弾を有効化
        if (m_status.GetBulletPrefab())
        {
            BulletBase bullet = Instantiate(m_status.GetBulletPrefab(), m_muzzleTransform.position, m_muzzleTransform.rotation);
            //弾の要素を設定
            bullet.SetShooting(this);
            bullet.SetTeam(m_myTeam);
            bullet.SetDamage(m_status.GetDamage());
            bullet.SetSpeed(m_status.GetSpeed());
            bullet.SetStaggerValue(m_status.GetStaggerPower());

            //ターゲットがいる場合、弾丸のターゲットに設定
            if (lockOn.CurrentTarget != null) bullet.SetTarget(lockOn.CurrentTarget);

            //弾の初速を velocity で設定
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.linearVelocity = shootDir * m_status.GetSpeed();
            }
        }

        //マズルフラッシュのエフェクトを有効化
        if (m_status.GetMuzzleFlashEffect())
        {
            GameObject MuzzleFlash = Instantiate(m_status.GetMuzzleFlashEffect(), m_muzzleTransform.position, m_muzzleTransform.rotation);
            Destroy(MuzzleFlash, 0.1f);
        }

        //発射音を再生
        if (m_status.GetFireSE())
        {
            PlayFireSE(m_status.GetFireSE());
        }

        //弾数減少/フラグ更新
        m_status.SetAmmo(m_status.GetAmmo() - 1);
        m_isFireInternal = false;
        m_isFireComplete = true;
        m_elapsedTime = 0f;
        if (m_status.GetAmmo() <= 0)
        {
            Reload();
        }
    }

    /// <summary>
    /// 武器不使用
    /// </summary>
    public void NotUse()
    {
        //フラグ管理
        m_canPlayEmptySE = true;
    }

    /// <summary>
    /// 敵の移動を考慮した発射方向を計算する
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
    /// 発射可否を外部管理するよう設定
    /// </summary>
    public void ExternalControl()
    {
        m_isExternalControl = true;
    }

    /// <summary>
    /// 発射できるかを設定(外部管理)
    /// </summary>
    /// <param name="isFire"></param>
    public void SetIsFire(bool isFire)
    {
        m_isFireExternal = isFire;
    }

    /// <summary>
    /// リロード処理
    /// </summary>
    public void Reload()
    {
        //既にリロード中か、弾数が既に最大なら、以下の処理を行わない
        if(m_isReloading || m_status.GetAmmo() >= m_status.GetMaxAmmo()) return;

        //各フラグをリロード中の状態の物に設定
        m_isFireInternal = false;
        m_isReloading = true;
        m_isCoolTime = false;
    }

    /// <summary>
    /// リロード完了処理
    /// </summary>
    private void ReloadComplete()
    {
        //各フラグをリロード前の状態の物に設定し、弾丸と経過時間を初期化
        m_isReloading = false;
        m_isReloadComplete = true;
        m_status.SetAmmo(m_status.GetMaxAmmo());
        m_isFireInternal = true;
        m_canPlayEmptySE = true;
        m_elapsedTime = 0f;
    }

    /// <summary>
    /// SEを再生
    /// </summary>
    /// <param name="PlaySE">再生したいSE</param>
    /// <param name="Overwrite">上書き再生するか(デフォルトではしない)</param>
    public void PlayFireSE(AudioClip PlaySE, bool Overwrite = false)
    {
        //既に何かのSEを再生中なら、音声を再生しない
        if (!Overwrite && m_audioSource.isPlaying) return;

        //音声保存用変数に再生したいSEを設定し、再生
        m_audioSource.clip = PlaySE;
        AudioSource.PlayClipAtPoint(m_audioSource.clip, m_muzzleTransform.position);
    }

    /// <summary>
    /// 使用されているか
    /// </summary>
    /// <returns></returns>
    public bool GetIsUsing()
    {
        return m_isUsing;
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

    /// <summary>
    /// 自身のチームを取得
    /// </summary>
    /// <returns></returns>
    public string GetTeam()
    {
        return m_myTeam;
    }

    /// <summary>
    /// 装備時に確認するポイントの名称を設定
    /// </summary>
    /// <param name="checkPoint"></param>
    public void SetCheckPoint(string checkPoint)
    {
        m_checkPoint = checkPoint;
    }

    /// <summary>
    /// 銃口を取得
    /// </summary>
    /// <returns></returns>
    public Transform GetMuzzle()
    {
        return m_muzzleTransform;
    }

    /// <summary>
    /// 銃口を設定(銃口が複数ある場合に使用)
    /// </summary>
    /// <param name="nextMuzzle"></param>
    public void SetMuzzle(Transform nextMuzzle)
    {
        m_muzzleTransform = nextMuzzle;
    }

    /// <summary>
    /// 発射が完了したか取得
    /// </summary>
    /// <returns></returns>
    public bool GetIsFireComplete()
    {
        return m_isFireComplete;
    }

    /// <summary>
    /// 背面武装の発射準備ができているかを設定
    /// </summary>
    /// <param name="weaponBackReady"></param>
    public void SetWeaponBackReady(bool weaponBackReady)
    {
        m_isBackWeaponReady = weaponBackReady;
    }

    /// <summary>
    /// リロード中かを取得
    /// </summary>
    /// <returns></returns>
    public bool GetReloading()
    {
        return m_isReloading;
    }

    /// <summary>
    /// リロード中かを取得
    /// </summary>
    /// <returns></returns>
    public bool IsReloading()
    {
        return m_isReloading;
    }

    /// <summary>
    /// リロードが完了しているかを取得
    /// </summary>
    /// <returns></returns>
    public bool GetReloadCompleat()
    {
        return m_isReloadComplete;
    }

    /// <summary>
    /// 発射レートのクールタイム中かを取得
    /// </summary>
    /// <returns></returns>
    public bool GetIsCoolTime()
    {
        return m_isCoolTime;
    }

    /// <summary>
    /// 銃のステータスを取得
    /// </summary>
    /// <returns></returns>
    public GunStatus GetGunStatus()
    {
        return m_status;
    }

    /// <summary>
    /// 発射角度を取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetShootDir()
    {
        return m_shootDir;
    }

    /// <summary>
    /// 現在のターゲットを取得
    /// </summary>
    /// <returns></returns>
    public Transform GetCurrentTarget()
    {
        return m_currentTarget;
    }

    /// <summary>
    /// 武器名を取得
    /// </summary>
    /// <returns></returns>
    public string GetName() => m_status.GetName();

    public int GetAmmo()
    {
        return m_status.GetAmmo();
    }

    public int GetMaxAmmo()
    {
        return m_status.GetMaxAmmo();
    }
}
