using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Center_Rush : MonoBehaviour
{
    /// <summary>
    /// 突進行動
    /// </summary>
    public static Vector3 CenterRush(GameObject CenterMarker, Transform m_My, Transform m_Player, float m_AttackDistance)
    {
        ///まずは、プレイヤー(ターゲット)の位置を取得
        Vector3 TargetPosition = m_Player.position;
        ///センターポイントの座標を(Y補正付き)ターゲットに合わせる
        CenterMarker.transform.position = TargetPosition;
        ///センターポイントの向きをNPCへ向けさせる
        CenterMarker.transform.LookAt(m_My.position);
        ///センターポイントをターゲットから指定分遠ざける(相対距離位置指定)
        CenterMarker.transform.position = TargetPosition - CenterMarker.transform.forward * (m_AttackDistance + 5f);
        ///その地点をNPCの目標地点とする
        return CenterMarker.transform.position;
    }
}
