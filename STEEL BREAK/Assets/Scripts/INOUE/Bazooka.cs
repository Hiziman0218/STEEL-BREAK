using UnityEngine;

public class Bazooka : MonoBehaviour
{
    [Header("誘導設定")]
    [Tooltip("誘導力/旋回力")]
    [SerializeField] private float m_homingStrength = 5f;   //誘導力
    [Tooltip("爆風プレハブ")]
    [SerializeField] private GameObject m_explosionPrefab;  //爆風プレハブ

    private Transform m_target; //追尾対象
    private Rigidbody m_rb;     //物理挙動
    private NewBullet m_bullet; //弾丸の基本機能

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_bullet = GetComponent<NewBullet>();

        //命中イベントに爆発処理を登録
        m_bullet.OnHit += Explode;
    }

    private void OnDestroy()
    {
        //イベント購読解除(メモリリーク防止)
        if (m_bullet != null)
            m_bullet.OnHit -= Explode;
    }

    private void FixedUpdate()
    {
        //NewBulletがターゲットを持っているなら参照
        m_target = m_bullet.GetTarget();
        if (m_target == null) return;

        //目標方向を計算
        Vector3 dir = (m_target.position - transform.position).normalized;

        //現在の進行方向を補正（誘導力で調整）
        Vector3 newVel = Vector3.Lerp(m_rb.linearVelocity.normalized, dir, Time.fixedDeltaTime * m_homingStrength);

        //弾速を維持したまま方向だけを修正
        m_rb.linearVelocity = newVel.normalized * m_rb.linearVelocity.magnitude;

        //弾頭を進行方向に向ける
        transform.rotation = Quaternion.LookRotation(m_rb.linearVelocity);
    }

    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode(Vector3 hitPoint)
    {
        //爆発を生成
        GameObject exp = Instantiate(m_explosionPrefab, hitPoint, Quaternion.identity);

        //爆発にチーム情報を渡す
        Blast explosion = exp.GetComponent<Blast>();
        if (explosion != null)
        {
            explosion.SetTeam(m_bullet.GetTeam());
        }

        //自身を削除
        Destroy(gameObject);
    }
}