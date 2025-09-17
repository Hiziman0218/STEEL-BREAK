using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawntest : MonoBehaviour
{
    IEnumerator SpawnWithInterval(
        GameObject m_Fairys,
        int m_MaxFairys,
        List<GameObject> m_SpawnPoints,
        List<GameObject> spawnedEnemies,
        float m_SpawnPer,
        float waitSeconds,
        FairysAI m_Fairys_Component
    )
    {
        foreach (GameObject point in m_SpawnPoints)
        {
            //生成上限かどうか確認
            if (spawnedEnemies.Count >= m_MaxFairys)
            {
                Debug.Log("上限に達したため、これ以上生成できません。");
                break;
            }

            //フェアリーの生成
            GameObject enemy = GameObject.Instantiate(m_Fairys, point.transform.position, Quaternion.identity);

            if (m_Fairys_Component != null)
            {
                m_Fairys_Component.m_Role = (UnityEngine.Random.value < m_SpawnPer) ? EnemyRole.Guardian : EnemyRole.Soldier;
            }

            spawnedEnemies.Add(enemy);

            // 数秒間待つ
            yield return new WaitForSeconds(waitSeconds);
        }

        Debug.Log("フェアリーをすべて徐々に生成しました！");
    }
}