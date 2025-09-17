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
            //����������ǂ����m�F
            if (spawnedEnemies.Count >= m_MaxFairys)
            {
                Debug.Log("����ɒB�������߁A����ȏ㐶���ł��܂���B");
                break;
            }

            //�t�F�A���[�̐���
            GameObject enemy = GameObject.Instantiate(m_Fairys, point.transform.position, Quaternion.identity);

            if (m_Fairys_Component != null)
            {
                m_Fairys_Component.m_Role = (UnityEngine.Random.value < m_SpawnPer) ? EnemyRole.Guardian : EnemyRole.Soldier;
            }

            spawnedEnemies.Add(enemy);

            // ���b�ԑ҂�
            yield return new WaitForSeconds(waitSeconds);
        }

        Debug.Log("�t�F�A���[�����ׂď��X�ɐ������܂����I");
    }
}