using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Weapon_Shooting : MonoBehaviour, IWeapon
{
    [Header("基本設定")]
    [SerializeField] private string m_name;               // 武装名
    [SerializeField] private Bullet m_bulletPrefab;       // 弾丸プレハブ
    [SerializeField] private NewBullet m_bulletPrefab_;   // 真の弾丸プレハブ
    [SerializeField] private Transform m_muzzleTransform; // 発射口
    [SerializeField] private GameObject m_muzzleFlashEffect; //マズルフラッシュのエフェクト

    [Header("性能設定")]
    [SerializeField] private float m_rate;       // 発射間隔
    [SerializeField] private float m_maxAmmo;    // 最大弾数
    [SerializeField] private float m_reloadTime; // リロード時間

    [Header("回転設定")]
    [Tooltip("見た目の武器モデルを回転させるTransform")]
    [SerializeField] private Transform m_weaponModelTransform; //武器モデル用Transform
    [Tooltip("武器モデルが敵を向くスピード")]
    [SerializeField] private float m_rotationSpeed = 10f;      //回転速度
    [SerializeField] private Vector3 m_attachOffsetPos;

    [SerializeField]
    private float m_ammo;           //現在の弾数
    private float m_elapsedTime;    //経過時間計測用
    private bool m_isFire;          //発射可能フラグ
    private bool m_isReloading;     //リロード中フラグ
    private bool m_isUsing = false; //使用されているかフラグ
    private bool m_isIKFinished;    //IKが完了しているかフラグ

    private BulletManager m_bulletManager;  //発射処理を委譲するマネージャー
    private LockOn lockOn;                  //ロックオン機能
    private Transform m_prevTarget;         //前フレームのターゲット保持
    private Quaternion m_defaultLocalRot;   //初期ローカル回転保持
    private GameObject m_currentMuzzleFlashEffect;

    private string m_myTeam;

    private bool _isResettingRotation = false; //補完フラグ
    private float _resetLerpTime = 0f;         //補完用タイマー
    private const float ResetDuration = 0.7f;  //リセットにかける時間（秒）

    private Enemy _trackedEnemy; //死亡イベント購読用

    private void Awake()
    {
        // オブジェクトプール初期化
        //m_bulletManager = gameObject.AddComponent<BulletManager>();
        //m_bulletManager.Initialize(transform.root.gameObject, m_bulletPrefab);

        m_ammo = m_maxAmmo;     // 弾数を最大に設定
        m_elapsedTime = m_rate; // 最初から撃てるように設定
        m_isFire = true;        // 同上

        // プレイヤーにアタッチされたLockOnを取得
        lockOn = transform.root.GetComponent<LockOn>();

        // 初期回転を保持
        if (m_weaponModelTransform != null)
        {
            m_defaultLocalRot = m_weaponModelTransform.localRotation;
        }
    }

    private void Update()
    {
        //フラグを初期化
        m_isUsing = false;

        //現在のエフェクトを削除
        //Destroy(m_currentMuzzleFlashEffect);

        /*
        if (m_weaponModelTransform != null)
        {
            var current = lockOn != null ? lockOn.CurrentTarget : null;

            // --- ターゲット切り替えイベント登録 ---
            if (m_prevTarget != current)
            {
                RegisterTargetDeathEvent(current);
                m_prevTarget = current;
            }

            
            if (_isResettingRotation)
            {
                _resetLerpTime += Time.deltaTime;
                float t = Mathf.Clamp01(_resetLerpTime / ResetDuration);
                m_weaponModelTransform.localRotation = Quaternion.Slerp(
                    m_weaponModelTransform.localRotation,
                    m_defaultLocalRot,
                    t
                );

                if (t >= 1f)
                    _isResettingRotation = false;
            }
            else if (current != null && m_isUsing)
            {
                RotateModelTowards(current.position); // 敵方向に回転
            }
            else
            {
                RotateModelForward(); // 非ロック時
            }
            
        }
        */

        // --- 発射可能状態の管理 ---
        //発射不可能の場合
        if (!m_isFire)
        {
            //経過時間を計測
            m_elapsedTime += Time.deltaTime;

            //リロード中の場合
            if (m_isReloading)
            {
                //経過時間がリロード時間を超えていたら
                if (m_elapsedTime >= m_reloadTime)
                {
                    //リロード完了
                    ReloadComplete();
                }
            }
            //経過時間が発射レートを超えていたら
            else if (m_elapsedTime >= m_rate)
            {
                //発射可能処理
                m_isFire = true;
                m_elapsedTime = 0f;
            }
        }
    }

    private void LateUpdate()
    {
        //フラグを初期化
        m_isUsing = false;
    }

    public void AttachToHand(Transform hand, bool left)
    {
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        transform.SetParent(hand, false);
        //transform.localPosition = -grip.localPosition;
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= left ? -1f : 1f;
        transform.localPosition = offsetPos;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
    }

    public void Use()
    {
        //使用中として設定
        m_isUsing = true;

        // 発射可否チェック
        if (!m_isFire || !m_isIKFinished)
            return;

        //ターゲットがいる場合
        if(lockOn.CurrentTarget != null)
        {
            //銃口を強制的に敵に向ける
            //敵への方向を取得(ここは敵のBPに変更)
            Vector3 targetPos = lockOn.CurrentTarget.transform.Find("BP").position;
            Vector3 dir = (targetPos - m_muzzleTransform.position).normalized;
            //敵の方向へ銃口を回転
            m_muzzleTransform.rotation = Quaternion.LookRotation(dir);
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
        }

        //弾を有効化
        NewBullet Dummy = Instantiate(m_bulletPrefab_, m_muzzleTransform.position, m_muzzleTransform.rotation);
        //弾のチームを設定
        Dummy.SetTeam(m_myTeam);
        //マズルフラッシュのエフェクトを有効化
        m_currentMuzzleFlashEffect = Instantiate(m_muzzleFlashEffect, m_muzzleTransform.position, m_muzzleTransform.rotation);
        Destroy(m_currentMuzzleFlashEffect, 0.1f);
        //弾に力を加えて移動させる(AddForse)
        Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 1000.0f);
        //10秒後に削除
        Destroy(Dummy.gameObject, 10.0f);

        //既存の弾数減少／フラグ更新
        m_ammo--;
        m_isFire = false;
        m_elapsedTime = 0f;
        if (m_ammo <= 0)
            m_isReloading = true;
    }

    public void Reload()
    {
        //各フラグをリロード状態の物に設定
        m_isFire = false;
        m_isReloading = true;
    }

    /// <summary>
    /// リロード完了処理
    /// </summary>
    void ReloadComplete()
    {
        //リロード処理
        m_isReloading = false;
        m_ammo = m_maxAmmo;
        m_isFire = true;
        m_elapsedTime = 0f;
    }

    /*
    /// <summary>
    /// ロック中に武器モデルを敵方向へスムーズ回転
    /// </summary>
    private void RotateModelTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - m_weaponModelTransform.position;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        m_weaponModelTransform.rotation = Quaternion.Slerp(
            m_weaponModelTransform.rotation,
            lookRot,
            Time.deltaTime * m_rotationSpeed
        );
    }

    /// <summary>
    /// ターゲットの死亡イベントを解除
    /// </summary>
    private void RegisterTargetDeathEvent(Transform newTarget)
    {
        // 旧ターゲットのイベント解除
        if (_trackedEnemy != null)
        {
            _trackedEnemy.OnDeath -= OnTargetDeath;
            _trackedEnemy = null;
        }

        if (newTarget != null)
        {
            var enemy = newTarget.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.OnDeath += OnTargetDeath;
                _trackedEnemy = enemy;
            }
        }
    }
    */


    /// <summary>
    /// ロックオン対象の死亡通知
    /// </summary>
    private void OnTargetDeath(Enemy deadEnemy)
    {
        if (_trackedEnemy == deadEnemy)
        {
            ResetModelRotation();
            _trackedEnemy.OnDeath -= OnTargetDeath;
            _trackedEnemy = null;
        }
    }

    /*
    /// <summary>
    /// 非ロック時に武器モデルを親の forward 方向にスムーズ回転
    /// </summary>
    private void RotateModelForward()
    {
        Quaternion targetRot = Quaternion.LookRotation(transform.forward);
        m_weaponModelTransform.rotation = Quaternion.Slerp(
            m_weaponModelTransform.rotation,
            targetRot,
            Time.deltaTime * m_rotationSpeed
        );
    }
    */

    /// <summary>
    /// ターゲット消失時にスムーズにリセット
    /// </summary>
    private void ResetModelRotation()
    {
        if (m_weaponModelTransform != null)
        {
            _isResettingRotation = true;
            _resetLerpTime = 0f;
        }
    }

    public void SetIKFinished(bool IKFinished)
    {
        m_isIKFinished = IKFinished;
    }

    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    public string GetName() => m_name;
}
