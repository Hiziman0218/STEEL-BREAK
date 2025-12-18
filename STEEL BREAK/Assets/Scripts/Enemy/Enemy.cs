using System;
using UnityEngine;

public class Enemy : CharaBase
{
    [SerializeField] private float m_staggerThreshold = 1; //よろけるのに必要なよろけ値
    [SerializeField] private bool m_isUseRandomWeapon = false; //武器をランダムで設定するか
    [SerializeField] private Transform m_attachPointR; //右の装備位置
    [SerializeField] private Transform m_attachPointL; //左の装備位置

    public event Action<Enemy> OnStagger; //よろけイベント
    public event Action<Enemy> OnDeath;   //死亡イベント
    public event Action<Enemy> OnDiedField; //死亡時のフィールド用イベント

    public EnemyGun weaponR; //右武器
    public EnemyGun weaponL; //左武器

    private EnemyGunFactory m_randomWeaponSystem; //敵の武器をランダムに取得するシステム

    private Rigidbody rb;

    private float m_currentStagger; //現在のよろけ値

    public bool IsAlive { get; private set; } = true; //生存中か

    protected override void Initialize()
    {
        //基底クラスの初期化処理呼び出し
        base.Initialize();

        m_randomWeaponSystem = GetComponent<EnemyGunFactory>();
        rb = GetComponent<Rigidbody>();

        OperationArea area = FindAnyObjectByType<OperationArea>();
        if (area != null)
        {
            area.RegisterEnemy(this);
        }

        //自身の武器のチームを設定
        //武器が設定されていなければ、ランダムシステムと装備ポイントが設定されていれば
        //自動的にランダムに取得
        if (weaponR) weaponR.SetTeam(m_status.GetTeam());
        else
        {
            if(m_randomWeaponSystem != null && m_attachPointR != null)
            {
                weaponR = m_randomWeaponSystem.CreateRandomGun(m_attachPointR);
                weaponR.SetTeam(m_status.GetTeam());
            }
            
        }
        if (weaponL) weaponL.SetTeam(m_status.GetTeam());
        else
        {
            if(m_randomWeaponSystem != null && m_attachPointL != null)
            {
                weaponL = m_randomWeaponSystem.CreateRandomGun(m_attachPointL);
                weaponL.SetTeam(m_status.GetTeam());
            }
            
        }
    }

    private void Update()
    {
        //よろけ値が最大になればよろけイベント
        if(m_currentStagger >= m_staggerThreshold)
        {
            OnStagger?.Invoke(this);
            m_currentStagger = 0;
            Debug.Log("よろけイベント呼び出し");
        }

        //HPが0以下なら、死亡
        if (m_status.GetHP() <= 0f)
        {
            //死亡イベント
            OnDeath?.Invoke(this);
            //OnDiedField?.Invoke(this);

            //フラグをfalseにし、死亡処理
            IsAlive = false;
            Die();
        }
    }

    private void OnDestroy()
    {
        OnDiedField?.Invoke(this);

        OperationArea area = FindAnyObjectByType<OperationArea>();
        if (area != null)
        {
            area.UnregisterEnemy(this);
        }
    }

    /// <summary>
    /// 右に装備された武装を使用
    /// </summary>
    public void UseR()
    {
        weaponR ?.Fire();
    }

    /// <summary>
    /// 左に装備された武装を使用
    /// </summary>
    public void UseL()
    {
        weaponL ?.Fire();
    }

    /// <summary>
    /// よろけ値を加算
    /// </summary>
    /// <param name="StaggerValue">与えるよろけ値</param>
    public void GetStaggerValue(float StaggerValue)
    {
        m_currentStagger += StaggerValue;
    }

    /// <summary>
    /// OperationArea からの強制位置指定
    /// </summary>
    /// <param name="pos"></param>
    public void ForceSetPosition(Vector3 pos)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
        }
        else
        {
            transform.position = pos;
        }
    }

    /// <summary>
    /// 範囲に阻まれた時のフック（自爆しない！）
    /// </summary>
    public virtual void OnBlockedByOperationArea()
    {
        // デフォルトは何もしない
        // 突進中断・向き変更などをここに書ける
    }
}