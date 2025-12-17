using UnityEngine;

public class Centering : MonoBehaviour
{
    /// <summary>
    /// 旋回警戒用AI行動
    /// </summary>
    /// <param name="m_CenterMarker">センターマーカーのオブジェクト</param>
    /// <param name="m_My">自分の位置</param>
    /// <param name="m_Player">プレイヤーの位置</param>
    /// <param name="m_Muki">方向指定用の数値</param>
    /// <param name="m_AttackDistance">攻撃可能距離</param>
    /// <returns></returns>
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
        m_CenterMarker.transform.Translate(new Vector3(0, 0, m_AttackDistance - 7f));
        ///その地点をNPCの目標地点とする
        return m_CenterMarker.transform.position;
    }
}
