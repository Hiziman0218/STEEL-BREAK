using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Centering : MonoBehaviour
{
    /// <summary>
    /// 旋回警戒用AI行動
    /// </summary>
    public static Vector3 CenterPoint(GameObject m_CenterMarker, Transform m_My,Transform m_Player,float m_Muki, float m_AttackDistance)
    {
        ///まずは、プレイヤー(ターゲット)の位置を取得
        Vector3 TargetPosition = m_Player.position;
        ///ターゲットのY軸を揃える
        TargetPosition.y = m_My.position.y;
        ///センターポイントの座標を(Y補正付き)ターゲットに合わせる
        m_CenterMarker.transform.position = TargetPosition;
        ///センターポイントの向きをNPCへ向けさせる
        m_CenterMarker.transform.LookAt(m_My.position);
        ///１回分の旋回角度分回転
        m_CenterMarker.transform.Rotate(new Vector3(0, 10f * m_Muki, 0));
        ///センターポイントをターゲットから指定分遠ざける(相対距離位置指定)
        m_CenterMarker.transform.Translate(new Vector3(0, 0, m_AttackDistance - 4f));
        ///その地点をNPCの目標地点とする
        return m_CenterMarker.transform.position;
    }

    /// <summary>
    /// 立体的な旋回 ※現状使っていない
    /// </summary>
    /// <param name="m_GuardPointer">守護位置</param>
    /// <param name="time">時間（回転の角度に使う）</param>
    /// <param name="m_radius">半径（遠さ）</param>
    /// <param name="m_RotSpeed">回転速度</param>
    /// <param name="m_Vertical">上下揺れの幅</param>
    /// <param name="m_Twist_x">X軸のねじれの幅</param>
    /// <returns></returns>
    public static Vector3 RotAroundGuardPoint3DFixed(Vector3 m_GuardPointer, float time, float m_radius, float m_RotSpeed, float m_Vertical, float m_Twist_x)
    {
        float angle = time * m_RotSpeed;

        // 回転角から方向ベクトルを生成
        Vector3 horizontal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * m_radius;

        // 垂直方向にゆらぎを加える
        float y = Mathf.Sin(angle * 3.9f) * m_Vertical;

        // さらにねじれ（X軸方向）も加える
        float xTwist = Mathf.Sin(angle * 3.3f) * m_Twist_x;

        Vector3 offset = new Vector3(horizontal.x + xTwist, y, horizontal.z);
        return m_GuardPointer + offset;
    }

}
