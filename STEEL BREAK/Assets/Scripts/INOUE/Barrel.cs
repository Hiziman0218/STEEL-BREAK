using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("ƒoƒŒƒ‹İ’è")]
    [Tooltip("‰ñ“]Šî•”")]
    [SerializeField] private Transform m_gunBarrel;
    [Tooltip("–Cg‚Ì”")]
    [SerializeField] private int m_gunBurrelNumber;
    [Tooltip("‰ñ“]‘¬“x(•b‘¬)")]
    [SerializeField] private float m_rotateSpeed;

    private bool m_isUse = false;  //Œ»İg—p‚³‚ê‚Ä‚¢‚é‚©
    private float m_fireIntervalAngle;     //’e‚ğ”­Ë‚·‚é‰ñ“]”
    private float m_accumulatedAngle = 0f; //‘O‰ñ‚Ì”­Ë‚©‚ç‰½“x‰ñ“]‚µ‚½‚©

    private Weapon_Shooting m_shooting;    //eƒNƒ‰ƒX

    private void Start()
    {
        m_shooting = GetComponent<Weapon_Shooting>(); //eƒNƒ‰ƒX

        //’e‚ğ”­Ë‚·‚é‰ñ“]”‚ğŒvZ
        m_fireIntervalAngle = 360f / m_gunBurrelNumber;
    }

    private void Update()
    {
        m_shooting.SetIsFire(false);

        //g—p’†‚Íí‚É‰ñ“]
        if (m_isUse)
        {
            m_gunBarrel.Rotate(m_rotateSpeed * Time.deltaTime, 0f, 0f);
            //‰ñ“]—Ê‚ğ‰ÁZ
            m_accumulatedAngle += m_rotateSpeed * Time.deltaTime;

            //‰ñ“]‚µ‚½—Ê‚ª’e‚ğ”­Ë‚·‚é‰ñ“]”‚É“’B‚µ‚½‚çA’e‚ğ”­Ë‚µŒ»İ‚Ì‰ñ“]—Ê‚ğƒŠƒZƒbƒg
            if(m_accumulatedAngle >= m_fireIntervalAngle)
            {
                m_shooting.SetIsFire(true);
                m_accumulatedAngle -= m_fireIntervalAngle;
            }
        }
    }
}
