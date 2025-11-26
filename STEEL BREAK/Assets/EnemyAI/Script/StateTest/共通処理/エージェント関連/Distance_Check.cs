using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Distance_Check : MonoBehaviour
{
    // プレイヤーとの距離と角度を計算する処理
    /// <summary>
    /// 【使い方】
    /// Distance_Check.Check(自分のTransform, プレイヤーのTransform);
    /// 
    /// 返り値:
    /// - float distance: プレイヤーとの距離
    /// - float directionP: プレイヤーから見た方向
    /// - float directionE: 敵から見た方向
    ///
    /// 【例】
    /// (float distance, float direction) = Distance_Check.Check(owner.transform, owner.m_Player);
    /// これでdistanceとdirectionを取得することができる
    /// 
    /// sideの取得が必要ない場合は
    /// (float distance, _)
    /// のようにすると数値を無視できるらしい
    /// </summary>


    public static (float distance, float directionP, float directionE) Check(Transform m_My, Transform m_Player)
    {
        ///プレイヤーの正面ベクトルを取得
        Vector3 playerForward = m_Player.forward;

        // 敵の forward ベクトルを取得
        Vector3 enemyForward = m_My.forward;

        // 敵からプレイヤーへの方向ベクトル
        Vector3 directionToPlayer = (m_Player.position - m_My.position).normalized;

        // プレイヤーから見た敵の位置の Dot 値
        float directionP = Vector3.Dot(playerForward, directionToPlayer);

        // 敵から見たプレイヤーの位置の Dot 値
        float directionE = Vector3.Dot(enemyForward, directionToPlayer);

        ///プレイヤーとの相対距離チェック
        float distance = Vector3.Distance(m_My.position, m_Player.position);

        return (distance, directionP, directionE);

    }
}
