using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Player2DMove : MonoBehaviour
{

    [Header("ぬこの移動パターン")]
    public List<Sprite> m_XMoveAnime;

    [Header("物理")]
    public Rigidbody m_Rigidbody;

    [Header("今何番目のAnimationが実行中か")]
    public int m_AnimeNo = 0;

    [Header("Material")]
    public Material m_Material;

    public float m_PageTime = 0.1f;
    public float m_MaxPageTime = 0.1f;

    public Quaternion m_Rot;

    public bool m_LRFlag = false;
    void Start()
    {
        m_Rot = transform.rotation;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_PageTime = m_MaxPageTime;
    }

    void Update()
    {
        float X = Input.GetAxis("Horizontal");
        Animaters(X);
        Move(X);

    }
    public void Move(float X)
    {
        transform.rotation = m_Rot;
        if (X < 0)
        {
            X *= -1;
            m_LRFlag = true;
        }
        else if (X > 0)
            m_LRFlag = false;

        if(m_LRFlag)
            transform.Rotate(new Vector3(0, 180, 0));

        //m_Rigidbody.linearVelocity = transform.right * (X * 10.0f);
    }
    public void Animaters(float X)
    {
        if (X != 0)
        {
            if (m_PageTime <= 0)
            {
                m_AnimeNo++;
                m_PageTime = m_MaxPageTime;
                if (m_AnimeNo == m_XMoveAnime.Count)
                    m_AnimeNo = 0;
            }
            else
                m_PageTime -= Time.deltaTime;

            m_Material.mainTexture = m_XMoveAnime[m_AnimeNo].texture;
        }
    }
}
