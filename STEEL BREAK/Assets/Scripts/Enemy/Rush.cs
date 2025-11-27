using System.Collections.Generic;
using UnityEngine;

public class Rush : MonoBehaviour
{
    [SerializeField] private Collider m_collider;           //当たり判定に使用するコライダー
    [SerializeField] private float m_knockbackPower = 3.0f; //相手を跳ね飛ばす力
    [SerializeField] private float m_damage = 3.0f;         //与えるダメージ
    [SerializeField] private float m_damageInterval = 0.3f; //ダメージ間隔

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
            CharaBase chara = GetComponentInParent<CharaBase>();
            if (chara != null)
            {
                m_myTeam = chara.GetTeam();
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
            //タイマー更新
            m_hitTimer[chara] = now;
        }
    }

    /// <summary>
    /// 突進攻撃開始
    /// </summary>
    public void StartRush()
    {
        m_collider.enabled = true;
    }

    /// <summary>
    /// 突進攻撃終了
    /// </summary>
    public void EndRush()
    {
        m_collider.enabled = false;
        m_hitTimer.Clear();
    }

    /// <summary>
    /// 所属チームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }
}
