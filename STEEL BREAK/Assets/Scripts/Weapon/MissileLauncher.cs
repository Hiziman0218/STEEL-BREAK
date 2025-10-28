using UnityEngine;
using System.Collections.Generic;

public class MissileLauncher : MonoBehaviour
{
    [Tooltip("武装本体")]
    [SerializeField] private Weapon_Shooting m_shooting;
    [Tooltip("全ての見た目用ミサイル")]
    [SerializeField] private List<Transform> m_missiles = new List<Transform>();

    private void Update()
    {
        if (m_shooting == null) return;

        //リロードが完了していたら
        if (m_shooting.GetReloadCompleat())
        {
            //全てのミサイルを表示
            foreach (Transform missile in m_missiles)
            {
                missile.gameObject.SetActive(true);
                missile.localRotation = Quaternion.Euler(0f, 0f, 90f); // 正面方向にリセット
            }
        }

        //銃の発射が完了したら
        if (m_shooting.GetIsFireComplete())
        {
            int index = 0;
            foreach (Transform missile in m_missiles)
            {
                //最初の要素なら、非表示に設定のみ
                if(index == 0)
                {
                    missile.gameObject.SetActive(false);
                    index++;
                    continue;
                }

                //弾を有効化
                if (m_shooting.GetGunStatus().GetBulletPrefab())
                {
                    //非表示に設定
                    missile.gameObject.SetActive(false);

                    //発生位置は正面方向が違うため、回転させる
                    Quaternion missileRotation = missile.parent.rotation * Quaternion.Euler(0f, -90f, 0f);

                    Bullet Dummy = Instantiate(m_shooting.GetGunStatus().GetBulletPrefab(), missile.position, missileRotation);
                    //弾の所属チームとダメージ量と弾速を設定
                    Dummy.SetTeam(m_shooting.GetTeam());
                    Dummy.SetDamage(m_shooting.GetGunStatus().GetDamage());
                    Dummy.SetSpeed(m_shooting.GetGunStatus().GetSpeed());
                    //発射時に設定されたターゲットがいる場合、弾丸のターゲットに設定
                    if (m_shooting.GetCurrentTarget() != null)
                    {
                        Dummy.SetTarget(m_shooting.GetCurrentTarget());
                    }

                    //弾の初速を velocity で設定
                    Rigidbody rb = Dummy.GetComponent<Rigidbody>();
                    rb.linearVelocity = m_shooting.GetShootDir() * m_shooting.GetGunStatus().GetSpeed();

                    //10秒後に削除
                    Destroy(Dummy.gameObject, 10.0f);

                    //弾数を減少
                    m_shooting.GetGunStatus().SetAmmo(m_shooting.GetGunStatus().GetAmmo() - 1);
                    //リロード処理
                    m_shooting.Reload();

                    index++;
                }
            }
        }
    }
}
