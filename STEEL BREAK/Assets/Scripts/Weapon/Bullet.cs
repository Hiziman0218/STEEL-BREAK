using System.Collections.Generic;
using UnityEngine;

public class Bullet : BulletBase
{
    [Header("弾丸判定設定")]
    [Tooltip("弾の半径(レイキャストを使用した当たり判定に使用)")]
    [SerializeField] private float m_bulletRadius = 0.1f;  //当たり判定の半径
    [Tooltip("ヒットエフェクト")]
    [SerializeField] private GameObject m_hitEffect;       //ヒット時のエフェクト
    [Tooltip("ヒットしたら消えるか/貫通しないか")]
    [SerializeField] private bool m_disappearOnHit = true; //ヒットしたら消えるか

    private Vector3 m_prevPos;  //前フレームでの位置
    private List<CharaBase> m_hitList = new List<CharaBase>(); //多段ヒット防止

    public System.Action<Vector3> OnHit; // 命中時に座標を渡すイベント

    private void Start()
    {
        //初期位置を保存
        m_prevPos = transform.position;

        //10秒後に削除
        Destroy(gameObject, 10.0f);
    }

    private void Update()
    {
        Vector3 currentPos = transform.position;
        Vector3 move = currentPos - m_prevPos;
        float dist = move.magnitude;

        //動いている場合のみ判定
        if (dist > 0.0001f)
        {
            Vector3 dir = move.normalized;

            //SphereCast で移動経路を判定
            if (Physics.SphereCast(m_prevPos, m_bulletRadius, dir, out RaycastHit hit, dist))
            {
                //当たった対象からCharaBaseコンポーネントを取得
                var chara = hit.collider.GetComponentInParent<CharaBase>();

                //ヒット座標を補正
                Vector3 hitPos = hit.point + hit.normal * m_bulletRadius;

                //キャラクターだった場合
                if (chara != null)
                {
                    //自身と同じチームではないかつ、まだ当たっていない敵なら
                    if (chara.GetTeam() != m_myTeam && !m_hitList.Contains(chara))
                    {
                        //ダメージを与え、リストに追加した後、エフェクトを生成
                        chara.GetDamage(m_damage);
                        m_hitList.Add(chara);
                        if (m_hitEffect) Instantiate(m_hitEffect, hit.point, Quaternion.identity);

                        //ヒットしたら消えるオブジェクトなら削除
                        if (m_disappearOnHit)
                        {
                            OnHit?.Invoke(hitPos);
                            Destroy(gameObject);
                            return;
                        }
                    }
                }
                //キャラクター以外の場合
                else
                {
                    OnHit?.Invoke(hit.point); //イベント発火
                    //ヒットエフェクトを生成し、自身を削除
                    if (m_hitEffect) Instantiate(m_hitEffect, hitPos, Quaternion.identity);
                    Destroy(gameObject);
                    return;
                }
            }
        }

        //移動後の位置を次の判定開始点に保存
        m_prevPos = currentPos;
    }
}