using UnityEngine;
using System.Collections;

//同時射撃
public class Attack_Shots : MonoBehaviour
{
    /// <summary>
    /// エネミースクリプト、クールダウンスクリプト、付与するクールタイムの秒数
    /// </summary>
    /// <param name="m_Enemy">エネミースクリプト</param>
    /// <param name="m_CoolDown">クールダウンスクリプト</param>
    /// <param name="CoolTime">付与するクールタイムの秒数</param>

    //同時射撃（単発）
    public static void Execute(Enemy m_Enemy, CoolDown m_CoolDown, float CoolTime)
    {

        Debug.Log("射撃");

        //該当するコンポーネントがあれば
        if (m_Enemy != null)
        {
            if (m_Enemy.weaponR != null)
            {
                m_Enemy.UseR();
            }

            if (m_Enemy.weaponL != null)
            {
                m_Enemy.UseL();
            }

        }

        //クールダウン設定
        m_CoolDown.StartCoolDown("Attack", CoolTime);
    }

    /// <summary>
    /// 同時連射　エネミースクリプト,クールダウンスクリプト,付与するクールタイム,最大連射数,射撃間隔
    /// </summary>
    /// <param name="m_Enemy">エネミースクリプト</param>
    /// <param name="m_CoolDown">クールダウンスクリプト</param>
    /// <param name="CoolTime">付与するクールタイム</param>
    /// <param name="MaxRange">最大連射数</param>
    /// <param name="interval">射撃間隔</param>
    public static IEnumerator ExecuteBurst(Enemy m_Enemy, CoolDown m_CoolDown, float CoolTime, int MaxRange, float interval)
    {
        int shots = Random.Range(1, MaxRange);
        //連射処理
        for (int i = 0; i < shots; i++)
        {
            if (m_Enemy.weaponR != null)
            {
                m_Enemy.UseR();
            }

            if (m_Enemy.weaponL != null)
            {
                m_Enemy.UseL();
            }

            yield return new WaitForSeconds(interval); // 連射間隔
        }

        m_CoolDown.StartCoolDown("Attack", CoolTime);
    }

}
