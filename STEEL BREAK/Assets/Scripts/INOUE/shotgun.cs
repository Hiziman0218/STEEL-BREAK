using UnityEngine;

public class shotgun : MonoBehaviour
{
    public int m_count = 10; //分裂数
    public float m_maxRange; //最大角度
    public float m_minRange; //最小角度
    public GameObject m_bulletPrefab; //弾プレハブ
    [SerializeField] private GameObject m_hitEffect; //ヒット時のエフェクト

    private void Start()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        for(int i = 0; i <= m_count; i++)
        {
            //拡散用プレハブを生成
            GameObject Dummy = Instantiate(m_bulletPrefab, transform.position, transform.rotation);
            Dummy.transform.Rotate(new Vector3(Random.Range(m_minRange, m_maxRange), Random.Range(m_minRange, m_maxRange), 0));
            Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 10000.0f);
            Destroy(Dummy, 10f);
        }
    }

    /// <summary>
    /// 当たり判定
    /// </summary>
    /// <param name="other">当たったオブジェクト</param>
    private void OnTriggerEnter(Collider other)
    {
        var chara = other.GetComponentInParent<CharaBase>();
        //キャラクターにダメージを与え、ヒットエフェクトを生成した後、自身を削除
        if (chara != null)
        {
            chara.GetDamage(1.0f);
            Instantiate(m_hitEffect, chara.transform.position, Quaternion.Inverse(transform.rotation));
            Destroy(gameObject);
        }
    }
}
