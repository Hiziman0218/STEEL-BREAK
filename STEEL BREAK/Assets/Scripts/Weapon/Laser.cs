using System.Collections.Generic;
using UnityEngine;

public class Laser : BulletBase
{
    [Header("レーザー設定")]
    [SerializeField] private float m_damageInterval = 0.3f; //ダメージ間隔
    [SerializeField] private Collider m_hitCollider;        //自身の判定を除外する場合に設定
                                                            
    private readonly Dictionary<CharaBase, float> m_hitTimer = new(); //ヒット管理用
    private Player m_player;     //プレイヤー
    private Transform m_parent;  //発射時に自身の親にしたい部分

    private void Start()
    {
        //自身を銃口の子供に設定
        if(m_shooting != null)
        {
            Transform muzzle = m_shooting.GetMuzzle();
            if (muzzle != null)
            {
                transform.SetParent(muzzle);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        //銃が無ければ(敵などの場合)
        else
        {
            if(m_parent != null)
            {
                transform.SetParent(m_parent);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }   
        }

        //使用者がプレイヤーなら、プレイヤーにレーザー使用を通知
        m_player = transform.root.GetComponent<Player>();
        if(m_player != null)
        {
            m_player.FireLaser();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //自身は除外対象
        if (other == m_hitCollider) return;

        //キャラクター確認
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;

        //同チームは無視
        if (chara.GetTeam() == m_myTeam) return;

        //よろけ値は毎フレーム加算
        Enemy enemy = chara.GetComponent<Enemy>();
        if (enemy != null) enemy.GetStaggerValue(m_staggerValue);

        float now = Time.time;

        //初回 or 前回ダメージから一定時間経過したら
        if (!m_hitTimer.ContainsKey(chara) || now - m_hitTimer[chara] >= m_damageInterval)
        {
            //ダメージ処理
            chara.GetDamage(m_damage);

            //タイマー更新
            m_hitTimer[chara] = now;
        }
    }

    private void OnDestroy()
    {
        //プレイヤーにレーザー使用終了を通知
        if(m_player != null)
        {
            m_player.EndLaser();
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出される削除関数
    /// アニメーションが終了すると削除
    /// </summary>
    public void OnLaserEnd()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 親にしたいオブジェクトを設定
    /// </summary>
    /// <param name="parent"></param>
    public void SetParent(Transform parent)
    {
        m_parent = parent;
    }
}
