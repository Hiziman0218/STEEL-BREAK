using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MissileLauncher : MonoBehaviour
{
    [Tooltip("武装本体")]
    [SerializeField] private Weapon_Shooting m_shooting;
    [Tooltip("全ての見た目用ミサイル")]
    [SerializeField] private List<Transform> m_missiles = new List<Transform>();
    [Tooltip("発射間隔(秒)")]
    [SerializeField] private float m_fireInterval = 0.2f;

    private bool m_isFiring = false; //コルーチンを使った発射管理

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
        if (m_shooting.GetIsFireComplete() && !m_isFiring)
        {
            //コルーチン開始
            StartCoroutine(FireMissilesSequentially());
        }
    }

    private IEnumerator FireMissilesSequentially()
    {
        m_isFiring = true;

        for (int i = 0; i < m_missiles.Count; i++)
        {
            Transform missile = m_missiles[i];

            //待機
            yield return new WaitForSeconds(m_fireInterval);

            //最初の要素だけ非表示設定のみでスキップ
            if (i == 0)
            {
                missile.gameObject.SetActive(false);
                continue;
            }

            //通常の銃と同じ発射処理
            if (m_shooting.GetGunStatus().GetBulletPrefab())
            {
                missile.gameObject.SetActive(false);

                Quaternion missileRotation = missile.parent.rotation * Quaternion.Euler(0f, -90f, 0f);
                BulletBase Dummy = Instantiate(m_shooting.GetGunStatus().GetBulletPrefab(), missile.position, missileRotation);

                Dummy.SetTeam(m_shooting.GetTeam());
                Dummy.SetDamage(m_shooting.GetGunStatus().GetDamage());
                Dummy.SetSpeed(m_shooting.GetGunStatus().GetSpeed());

                if (m_shooting.GetCurrentTarget() != null)
                    Dummy.SetTarget(m_shooting.GetCurrentTarget());

                Rigidbody rb = Dummy.GetComponent<Rigidbody>();
                rb.linearVelocity = m_shooting.GetShootDir() * m_shooting.GetGunStatus().GetSpeed();

                Destroy(Dummy.gameObject, 10.0f);
                m_shooting.GetGunStatus().SetAmmo(m_shooting.GetGunStatus().GetAmmo() - 1);
                m_shooting.Reload();
            }
        }
        //フラグ設定
        m_isFiring = false;
    }
}
