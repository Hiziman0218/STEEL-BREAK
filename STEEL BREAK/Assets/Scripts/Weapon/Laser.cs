using System.Collections.Generic;
using UnityEngine;

public class Laser : BulletBase
{
    [Header("レーザー設定")]
    [SerializeField] private float m_damageInterval = 0.3f; //ダメージ間隔
    [SerializeField] private Collider m_hitCollider;        //自身の判定を除外する場合に設定

    private readonly Dictionary<CharaBase, float> m_hitTimer = new(); //ヒット管理用
    private Player m_player; //プレイヤー

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

        //プレイヤーにレーザー使用を通知
        m_player = transform.root.GetComponent<Player>();
        if(m_player != null)
        {
            m_player.FireLaser();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log(name + " OnTriggerStay with " + other.name);

        //自身は除外対象
        if (other == m_hitCollider) return;

        //キャラクター確認
        var chara = other.GetComponentInParent<CharaBase>();
        if (chara == null) return;

        //同チームは無視
        if (chara.GetTeam() == m_myTeam) return;

        float now = Time.time;

        //初回 or 前回ダメージから一定時間経過したら
        if (!m_hitTimer.ContainsKey(chara) || now - m_hitTimer[chara] >= m_damageInterval)
        {
            //ダメージ処理
            chara.GetDamage(m_damage);

            var hitStop = chara.GetComponent<HitStop>();
            if (hitStop == null) hitStop = chara.gameObject.AddComponent<HitStop>();
            hitStop.StartHitStop(10f);

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
}
