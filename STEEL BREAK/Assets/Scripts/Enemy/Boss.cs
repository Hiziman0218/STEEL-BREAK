using System;
using UnityEngine;

public class Boss : MonoBehaviour
{
    private Enemy m_enemy;       //自身のEnemyコンポーネント
    private ProgressBar m_HPBar; //HPバー

    private float m_HPRate;      //現在の耐久割合

    private void Start()
    {
        m_enemy = GetComponent<Enemy>();

        m_enemy.OnStagger += null;

        //ボス用HPバーを設定
        m_HPBar = GameManager.Instance.GetBossHPBar();
    }

    private void LateUpdate()
    {
        UpdateRate();
    }

    /// <summary>
    /// 各種割合を計算し、対応したUIに反映
    /// </summary>
    private void UpdateRate()
    {
        //HPバーが設定されていたら
        if (m_HPBar != null)
        {
            //現在のHP割合を計算
            m_HPRate = m_enemy.GetStatus().GetHP() / m_enemy.GetStatus().GetMaxHP() * 100f;
            //HPバーに反映(体力が完全に0になるまでは最低1%として表示)
            m_HPBar.BarValue = (m_enemy.GetStatus().GetHP() > 0f) ? Math.Max(1f, MathF.Floor(m_HPRate)) : 0f;
            //死亡状態なら確実に0%として表示
            if (m_enemy.IsAlive == false) m_HPBar.BarValue = 0f;
        }
    }
}
