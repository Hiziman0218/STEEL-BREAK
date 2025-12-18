using System.Collections.Generic;
using UnityEngine;

public class Rush : MonoBehaviour
{
    [SerializeField] private Collider m_collider;           //当たり判定に使用するコライダー
    [SerializeField] private float m_knockbackPower = 3.0f; //相手を跳ね飛ばす力
    [SerializeField] private float m_damage = 3.0f;         //与えるダメージ
    [SerializeField] private float m_damageInterval = 0.3f; //ダメージ間隔
    [SerializeField] private AudioSource m_audioSource;     //音声データ

    private string m_myTeam; //自身の所属するチーム
    private readonly Dictionary<CharaBase, float> m_hitTimer = new(); //ヒット管理用

    private void Start()
    {
        //チームを取得
        CharaBase chara = GetComponentInParent<CharaBase>();
        if (chara != null)
        {
            m_myTeam = chara.GetTeam();
        }

        //最初は当たり判定をオフに設定
        //m_collider.enabled = false;
        m_collider.gameObject.SetActive(false);
    }

    private void Update()
    {
        //チームが設定されていなかった場合、取得
        if(m_myTeam == null)
        {
            Debug.Log("チームがnullでした。");
            CharaBase chara = GetComponentInParent<CharaBase>();
            if (chara != null)
            {
                m_myTeam = chara.GetTeam();
                Debug.Log("チームを取得できました。");
            }
            else
            {
                Debug.Log("チームを取得できませんでした。");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //キャラクターでなければ以降の処理を行わない
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;

        //所属チームが同じなら以降の処理を行わない
        if (chara.GetTeam() == m_myTeam) return;

        //ヒット位置を取得し、SEを再生
        Vector3 hitPos = other.ClosestPoint(transform.position);
        PlayRushSE(hitPos);

        //相手を吹き飛ばす
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (chara.transform.position - transform.position).normalized;
            rb.AddForce(dir * m_knockbackPower, ForceMode.Impulse);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //自身は除外対象
        if (other == m_collider) return;

        //キャラクター確認
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;

        //同チームは無視
        if (chara.GetTeam() == m_myTeam) return;

        float now = Time.time;

        //初回 or 前回ダメージから一定時間経過したら
        if (!m_hitTimer.ContainsKey(chara) || now - m_hitTimer[chara] >= m_damageInterval)
        {
            //ダメージ処理
            chara.GetDamage(m_damage);
            //ヒット位置を取得し、SEを再生
            Vector3 hitPos = other.ClosestPoint(transform.position);
            PlayRushSE(hitPos);
            //タイマー更新
            m_hitTimer[chara] = now;
        }
    }

    /// <summary>
    /// 突進攻撃開始
    /// </summary>
    public void StartRush()
    {
        m_collider.gameObject.SetActive(true);
    }

    /// <summary>
    /// 突進攻撃終了
    /// </summary>
    public void EndRush()
    {
        m_collider.gameObject.SetActive(false);
        m_hitTimer.Clear();
    }

    /// <summary>
    /// 音声再生
    /// </summary>
    public void PlayRushSE(Vector3 hitPosition)
    {
        //AudioSourceが無ければ、以降の処理は行わない
        if (m_audioSource == null) return;

        //ヒット時SE再生
        AudioSource.PlayClipAtPoint(m_audioSource.clip, hitPosition);
    }
}
