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
    /// 武器を手に持ち、装備させる
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="left"></param>
    public void AttachToHand(Transform hand, bool left)
    {
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        transform.SetParent(hand, false);
        //transform.localPosition = -grip.localPosition;
        Vector3 offsetPos = m_attachOffsetPos;
        offsetPos.x *= left ? -1f : 1f;
        transform.localPosition = offsetPos;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
    }

    public void Use()
    {
        AttackStart();
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
}
