using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Attack_Side : MonoBehaviour
{
    //横からの攻撃の処理
    public static void Execute(Transform m_My, Transform m_EnemyModel, Transform m_Player, CoolDown m_CoolDown)
    {
        Debug.Log("側面から攻撃！");
        //モデルの向きをプレイヤー方向へ向かせる
        m_My.LookAt(m_Player.position);
        m_EnemyModel.transform.LookAt(m_Player.position);

        //攻撃
        m_My.Translate(new Vector3(0, 0, 1));

        //クールダウン設定
        m_CoolDown.StartCoolDown("Attack", 2);
    }



}
