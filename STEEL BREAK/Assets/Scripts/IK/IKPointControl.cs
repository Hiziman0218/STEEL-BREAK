using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IKPointControl : MonoBehaviour
{
    [Header("参照設定")]
    [Tooltip("LockOn スクリプト（手動アサイン可）")]
    public LockOn lockOn;

    [Tooltip("プレイヤー本体の Transform")]
    public Transform player;

    [Header("敵未ロック時のオフセット")]
    [Tooltip("敵がいないときに伸ばす距離")]
    public float forwardDistance = 2.0f;

    [Tooltip("敵がいないときに伸ばす高さ")]
    public float heightOffset = 1.2f;

    void Start()
    {
        // 必要なら動的に探す
        if (lockOn == null)
            lockOn = FindObjectOfType<LockOn>();
        if (player == null)
            player = transform.root; // IKPoint を子にしていた場合の例
    }

    void LateUpdate()
    {
        //IKpointの座標を更新
        UpdateIKPoint();
    }

    /// <summary>
    /// ターゲットの有無によって座標を更新
    /// </summary>
    private void UpdateIKPoint()
    {
        Transform target = lockOn != null ? lockOn.CurrentTarget : null;

        if (target != null)
        {
            // 敵とプレイヤーの中点を計算し、位置と回転を反映
            Vector3 midPoint = (player.position + target.position) * 0.5f;
            transform.position = midPoint;

            Vector3 dir = target.position - transform.position;
            if (dir.sqrMagnitude > 0.0001f) // ゼロ除算防止
            {
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }
        else
        {
            // プレイヤー正面方向へ前方オフセット
            Vector3 forwardPoint = player.position + player.forward * forwardDistance;
            Quaternion forwardRotate = player.rotation;
            //高さを加算
            forwardPoint.y += heightOffset;
            transform.position = forwardPoint;
            transform.rotation = forwardRotate;
        }
    }
}
