using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBullet : MonoBehaviour
{
    [SerializeField] private Collider m_attackCollider; //当たり判定のコライダー
    [SerializeField] private GameObject m_hitEffect;    //ヒット時のエフェクト
    [SerializeField] private float m_speed;             //弾速(削除？)
    [SerializeField] private bool m_disappearOnHit = true; //ヒット時に消えるか
    private List<CharaBase> m_hitList = new List<CharaBase>(); //一度の攻撃内で当たった敵のリスト(多段ヒット対策)←もし弾丸が敵に当たってすぐ消えるならいらない
    private string m_myTeam; //自身のチーム

    /// <summary>
    /// 自身の所属するチームを設定
    /// </summary>
    /// <param name="team"></param>
    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    /// <summary>
    /// 当たり判定
    /// </summary>
    /// <param name="other">当たったオブジェクト</param>
    private void OnTriggerEnter(Collider other)
    {
        //キャラとして取得、キャラではないなら、以降の処理を行わない
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;
        //ヒットしたキャラが自身と同じチームなら、以降の処理を行わない
        if (chara.GetTeam() == m_myTeam) return;
        //当たったオブジェクトがキャラクターかつ、ヒットリストに無ければ、
        //キャラクターにダメージを与え、ヒットリストに追加
        if (chara != null && !m_hitList.Contains(chara))
        {
            chara.GetDamage(1.0f);
            m_hitList.Add(chara);
            //ヒットエフェクトを有効化
            Instantiate(m_hitEffect, chara.transform.position, Quaternion.Inverse(transform.rotation));
            //ヒット時に消える弾なら、自身を削除
            if (m_disappearOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
