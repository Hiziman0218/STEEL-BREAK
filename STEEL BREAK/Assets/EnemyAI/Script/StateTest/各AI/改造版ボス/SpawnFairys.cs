using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


public class SpawnFairys_T : MonoBehaviour
{
    //��̈�̐������Ă���
    public static IEnumerator SpawnWithInterval(
    GameObject m_Fairys,
    List<GameObject> m_SpawnPoints,
    List<GameObject> spawnedEnemies,
    float m_SpawnPer,
    float m_waitSeconds,
    int m_MaxFairys,
    System.Action onComplete
)
    {
        while (spawnedEnemies.Count < m_MaxFairys)
        {
            //�X�|�[���|�C���g���擾
            GameObject point = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];

            //�t�F�A���[�̐���
            GameObject enemy = GameObject.Instantiate(m_Fairys, point.transform.position, Quaternion.identity);
            FairyAI fairyComponent = enemy.GetComponent<FairyAI>();

            //�R���|�[�l���g��null����Ȃ���Ζ�E����
            if (fairyComponent != null)
            {
                //m_SpawnPer���Ⴂ�قǃ\���W���[�������Ȃ�@�����قǃK�[�f�B�A���������Ȃ�
                fairyComponent.m_Role = (Random.value < m_SpawnPer) ? EnemyRole.Guardian : EnemyRole.Soldier;
            }

            //���ݐ�������Ă���G���L�^
            spawnedEnemies.Add(enemy);
            //�҂����Ԃ�Ԃ�
            yield return new WaitForSeconds(m_waitSeconds);
        }

        Debug.Log("�t�F�A���[�̐������������܂����I");
        onComplete?.Invoke();
    }
}
