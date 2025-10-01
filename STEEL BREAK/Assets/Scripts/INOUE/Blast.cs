using System.Collections.Generic;
using UnityEngine;

public class Blast : MonoBehaviour
{
    [Header("爆発設定")]
    [Tooltip("爆風の最終半径")]
    [SerializeField] private float m_maxRadius = 5f;  //爆風の最終半径
    [Tooltip("爆風が与えるダメージ(単発)")]
    [SerializeField] private float m_damage = 50f;    //与えるダメージ
    [Tooltip("爆風が広がりきるまでの寿命")]
    [SerializeField] private float m_lifetime = 1.0f; //爆風が存在する時間

    [Header("見た目用エフェクト")]
    [Tooltip("爆風エフェクト")]
    [SerializeField] private GameObject m_visualEffectPrefab; //爆風の見た目Prefab

    private GameObject m_visualEffectInstance;
    private ParticleSystem m_particle;

    private SphereCollider m_collider;   //爆風のコライダー
    private float m_elapsedTime = 0f;    //経過時間
    private float m_currentRadius = 0f;  //現在の爆風半径
    private string m_myTeam;             //所属チーム
    private List<CharaBase> m_hitList = new List<CharaBase>(); //多重ダメージ防止用

    private void Awake()
    {
        //当たり判定設定
        m_collider = GetComponent<SphereCollider>();
        m_collider.isTrigger = true;
        m_collider.radius = 0f; //初期はゼロから

        //見た目エフェクトを生成
        if (m_visualEffectPrefab != null)
        {
            m_visualEffectInstance = Instantiate(
                m_visualEffectPrefab, transform.position, Quaternion.identity, transform
            );
            m_particle = m_visualEffectInstance.GetComponentInChildren<ParticleSystem>();
        }
    }

    private void Update()
    {
        //経過時間を更新
        m_elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(m_elapsedTime / m_lifetime);

        //t=0 → 半径0、t=1 → m_maxRadius
        m_currentRadius = Mathf.Lerp(0f, m_maxRadius, t);
        m_collider.radius = m_currentRadius;

        //見た目のスケールを連動
        if (m_visualEffectInstance != null)
        {
            m_visualEffectInstance.transform.localScale = Vector3.one * (m_currentRadius * 2f);
        }

        //寿命終了処理
        if (m_elapsedTime >= m_lifetime)
        {
            if (m_particle != null) m_particle.Stop();
            Destroy(gameObject, 1.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara != null)
        {
            //味方は無視、かつ未ヒットのみ
            if (chara.GetTeam() != m_myTeam && !m_hitList.Contains(chara))
            {
                chara.GetDamage(m_damage);
                m_hitList.Add(chara);
            }
        }
    }

    /// <summary>
    /// チームを設定(発射元の弾から設定)
    /// </summary>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }
}