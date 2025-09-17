using UnityEngine;

/// <summary>
/// バンカーセンサー
#region バンカーセンサーとは?
/// バンカーセンサーとは、ある意味【鳴子】の事で、プレイヤーがこのセンサーエリアに侵入した際に
/// バンカーセットへ侵入の旨を伝える
/// 但し、今回の方式は【センサーマーカー式】で、あくまでセンサーとしての機能しかなく、
/// 侵入したから即座に撤退指示をバンカーセットに出すのではない。(バンカーセットが「気が付く」式)
/// バンカーセンサーが侵入警報を鳴らす事で、どの方向から侵入したのかがわかり易く、
/// 結果逃走経路を確保した上での撤退が可能となる。
/// 但し複数人数による侵攻には向いていないし、誤ってセンサーを押す事による誤報もある
#endregion
/// </summary>
public class BankerSensor : MonoBehaviour
{
    public bool m_PlayerHit = false;

    /// <summary>
    /// 空間内にプレイヤーが接触し続けている状態
    /// </summary>
    /// <param name="other">接触者</param>
    private void OnTriggerStay(Collider other)
    {
        ///相手がプレイヤーならアラームONそれ以外はアラームOFF
        if (other.tag != "Player")
            m_PlayerHit = false;
        else
            m_PlayerHit = true;
    }
    /// <summary>
    /// 空間内からプレイヤーが出る場合
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        ///アラーム終了
        m_PlayerHit = false;
    }
}
