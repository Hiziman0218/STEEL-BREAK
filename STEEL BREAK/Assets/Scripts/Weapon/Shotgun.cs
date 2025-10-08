using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [Header("ŠgUİ’è")]
    [Tooltip("•ª—ô”")]
    [SerializeField] private int m_count = 10; //•ª—ô”
    [Tooltip("ŠgU‚ÌÅ‘åŠp“x")]
    [SerializeField] private float m_maxRange; //Å‘åŠp“x
    [Tooltip("ŠgU‚ÌÅ¬Šp“x")]
    [SerializeField] private float m_minRange; //Å¬Šp“x
    [Tooltip("ŠgU‘O‚Ì’eŠÛ(©g)")]
    [SerializeField] private Bullet m_bullet;  //ŠgU‘O‚Ì’eŠÛ
    [Tooltip("ŠgUŒã‚Ì’eŠÛ")]
    [SerializeField] private Bullet m_bulletPrefab; //ŠgUŒã‚Ì’eŠÛƒvƒŒƒnƒu

    private void Start()
    {
        //©g‚ªİ’è‚³‚ê‚Ä‚¢‚È‚¯‚ê‚ÎA’eŠÛƒNƒ‰ƒX‚ğæ“¾
        if(m_bullet == null)
        {
            Debug.LogError("ŠgU‘O‚Ì’eŠÛ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            m_bullet = GetComponent<Bullet>();
        }
        
        //‚·‚®‚É©g‚ğíœ
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        //ŠgU‚·‚é”‚¾‚¯ŒJ‚è•Ô‚µ
        for(int i = 0; i < m_count; i++)
        {
            //ŠgUŒã‚Ì’eŠÛ‚ªİ’è‚³‚ê‚Ä‚¢‚È‚¯‚ê‚ÎAˆÈ~‚Ìˆ—‚ğs‚í‚È‚¢
            if (m_bulletPrefab == null)
            {
                Debug.LogError("ŠgUŒã‚Ì’eŠÛƒvƒŒƒnƒu‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
                return;
            }

            //ŠgU—pƒvƒŒƒnƒu‚ğ¶¬
            Bullet Dummy = Instantiate(m_bulletPrefab, transform.position, transform.rotation);
            //’e‚ÌŠ‘®ƒ`[ƒ€‚Æƒ_ƒ[ƒW—Ê‚ğİ’è
            Dummy.SetTeam(m_bullet.GetTeam());
            Dummy.SetDamage(m_bullet.GetDamage());
            //w’è”ÍˆÍ“à‚Ìƒ‰ƒ“ƒ_ƒ€‚È•ûŒü‚ÖŒü‚¯A‚»‚Ì•ûŒü‚Ö”­Ë
            Dummy.transform.Rotate(new Vector3(Random.Range(m_minRange, m_maxRange), Random.Range(m_minRange, m_maxRange), 0));
            Dummy.GetComponent<Rigidbody>().linearVelocity = Dummy.transform.forward * m_bullet.GetSpeed();
            //10•bŒã‚Éíœ
            Destroy(Dummy, 10f);
        }
    }
}
