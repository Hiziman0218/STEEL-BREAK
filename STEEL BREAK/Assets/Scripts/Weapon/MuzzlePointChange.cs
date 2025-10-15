using UnityEngine;
using System.Collections.Generic;

public class MuzzlePointChange : MonoBehaviour
{
    [Tooltip("eŒû‚ª•¡”‚ ‚ée–{‘Ì")]
    [SerializeField] private Weapon_Shooting m_shooting;
    [Tooltip("‘S‚Ä‚ÌeŒû")]
    [SerializeField] private List<Transform> m_muzzlePoints = new List<Transform>();

    private int m_currentIndex = 0;
    private bool m_prevIsFired = false;

    private void Start()
    {
        if(!m_shooting) m_shooting = GetComponent<Weapon_Shooting>();

        //‰Šú‚ÌeŒû‚ğİ’è
        if (m_shooting && m_muzzlePoints.Count > 0)
            m_shooting.SetMuzzle(m_muzzlePoints[m_currentIndex]);
    }

    private void Update()
    {
        if (m_shooting == null || m_muzzlePoints.Count == 0) return;

        bool currentIsFired = m_shooting.GetIsFireComplete();

        //‘OƒtƒŒ[ƒ€‚Åfalse¨¡ƒtƒŒ[ƒ€‚Åtrue‚É‚È‚Á‚½uŠÔ
        if (!m_prevIsFired && currentIsFired)
        {
            //Ÿ‚ÌeŒû‚ÉØ‚è‘Ö‚¦
            m_currentIndex = (m_currentIndex + 1) % m_muzzlePoints.Count;

            m_shooting.SetMuzzle(m_muzzlePoints[m_currentIndex]);
        }

        m_prevIsFired = currentIsFired;
    }
}
