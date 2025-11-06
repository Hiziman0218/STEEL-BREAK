using Game.Enum;
using System.Collections.Generic;
using UnityEngine;

public class Wepon_Fighting : MonoBehaviour , IWeapon
{
    [Header("基本設定")]
    [SerializeField] private string m_name; //武装の名前
    private bool m_isIKFinished;            //IKが完了しているかフラグ
    [SerializeField] private Collider m_attackCollider; //当たり判定のコライダー
    private List<CharaBase> m_hitList = new List<CharaBase>(); //一度の攻撃内で当たった敵のリスト(多段ヒット対策)

    [SerializeField] private Vector3 m_attachOffsetPos;

    private string m_myTeam;

    /// <summary>
    /// 当たり判定
    /// </summary>
    /// <param name="collision">当たったオブジェクト</param>
    public void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        CharaBase chara = other.GetComponent<CharaBase>();
        //命中したオブジェクトがキャラなら
        if (chara != null)
        {
            //命中したキャラがリスト内に存在しなければ
            if (!m_hitList.Contains(chara))
            {
                //ダメージを与え、ヒットリストに追加(初期化されるまではヒットしない)
                chara.GetDamage(1.0f);
                m_hitList.Add(chara);
            }
        }
    }

    /// <summary>
    /// 攻撃開始の処理
    /// </summary>
    public void AttackStart()
    {
        //リストを初期化
        m_hitList.Clear();
        //コライダーを有効に
        m_attackCollider.enabled = true;
    }

    /// <summary>
    /// 攻撃終了の処理
    /// </summary>
    public void AttackEnd()
    {
        //コライダーを無効に
        m_attackCollider.enabled = false;
    }

    /// <summary>
    /// 武装を装備させる
    /// </summary>
    /// <param name="point">装備させるポイント</param>
    /// <param name="side">どちらに装備させるか</param>
    public void AttachToPoint(Transform hand, AttachSide heldHand)
    {
        //持たせる手が左手か判定
        bool isLeft = (heldHand == AttachSide.Left);

        /* m_statusを実装してから
        //反転機能を使う銃モデルなら
        if (m_status.GetUseMirror())
        {
            //反転条件が一致していればモデルを反転
            if (m_status.GetMirrorWhenHeld() == heldHand)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        */

        //GripPointを検索
        Transform grip = transform.Find("GripPoint");
        if (grip == null)
        {
            Debug.LogWarning($"{name} に GripPoint が見つかりません。");
            return;
        }

        //手のTransformを親に設定
        //SetParentの第2引数falseにより、ワールド座標を維持せずローカル座標を手基準に再計算
        transform.SetParent(hand, false);

        //GripPointのローカル回転を打ち消す(★重要★)
        //GripPointのローカル回転を逆にかけることで、GripPointの向きを手の向きと一致
        transform.localRotation = Quaternion.Inverse(grip.localRotation);

        //GripPointのローカル位置を打ち消す
        //GripPointがPivotからどれだけ離れているか(ローカル位置)を反転して適用することで、
        //GripPointの位置が手の原点(hand.position)と一致するよう補正
        transform.localPosition = -grip.localPosition;

        //左右のオフセットを適用
        //左右の手で対称にしたい場合、x軸方向を反転
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= isLeft ? -1f : 1f;

        //最終的に補正を加える
        //上記の打ち消し＋回転補正を行ったあとで微調整値を加算
        transform.localPosition += offsetPos;
    }

    public void Use()
    {
        AttackStart();
    }

    public void NotUse()
    {

    }

    public void Reload()
    {

    }

    public void SetIKFinished(bool IKFinished)
    {
        m_isIKFinished = IKFinished;
    }

    public void SetTeam(string team)
    {
        m_myTeam = team;
    }

    public string GetName() => m_name;

    public int GetAmmo()
    {
        return 1;
    }

    public int GetMaxAmmo()
    {
        return 1;
    }
}
