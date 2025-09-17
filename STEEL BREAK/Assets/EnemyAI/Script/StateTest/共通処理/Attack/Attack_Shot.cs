using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

//射撃
public class Attack_Shot : MonoBehaviour
{
    public static void Execute(Enemy m_Enemy, CoolDown m_CoolDown)
    {
        Debug.Log("射撃");

        //該当するコンポーネントがあれば
        if (m_Enemy != null)
        {
            //ランダム回数射撃をする
            for(int i = 0; i<Random.Range(1, 7); i++)
            {
                m_Enemy.UseR();
            }
        }

        //クールダウン設定
        m_CoolDown.StartCoolDown("Attack", 4);

    }
}
