using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCamera : MonoBehaviour
{
    [Header("注視点:カメラが何処を見ているか")]
    public Transform m_GazingPoint;
    [Header("カメラ台:カメラが何処に移動しているか")]
    public Transform m_Target;


    private void Start()
    {
        //プレイヤーとの親子関係を解除
        transform.SetParent(null);   
    }

    /// <summary>
    /// フレームではなく時間で起動するアップデート
    /// 重い処理とかに使用するが、場合によって処理をすっ飛ばす可能性がある
    /// </summary>
    private void FixedUpdate()
    {
        if (m_GazingPoint && m_Target)
        {
            ///フローティングカメラの向きをプレイヤーの位置情報から向き情報を獲得し、分割してゆっくり回転
            ///Slerpは、AからBまでの補完を行い、分割して値を提出する
            this.transform.rotation = Quaternion.Lerp(
                this.transform.rotation,
                Quaternion.LookRotation(m_GazingPoint.position - this.transform.position),
                0.1f);
            ///フローティングカメラの位置をフローティングカメラ台にゆっくり移動させる
            this.transform.position = Vector3.Lerp(
                this.transform.position,
                m_Target.position,
                0.1f);
        }
    }
}
