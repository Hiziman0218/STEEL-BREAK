using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [Header("�o��������G�̃v���n�u�i������ނ�ݒ�j")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("�G���o��������|�C���g")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("�o���Ԋu�i�b�j")]
    [SerializeField] private float spawnInterval = 3f;

    [Header("�o���\�Ȏc�萔")]
    [Tooltip("������ 0 �ɂȂ���������X�|�[�����܂���")]
    [SerializeField] private int maxEnemies = 1;

    // ���݃V�[�����ɑ��݂���G��
    private int currentEnemyCount = 0;

    // �N���A��������x�����ĂԂ��߂̃t���O
    private bool clearCalled = false;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // �u�c��o���\�� > 0�v���u�������ݐ� < 1�v�łȂ�
            if (maxEnemies > 0
                && currentEnemyCount < maxEnemies
                && spawnPoints.Length > 0
                && enemyPrefabs.Length > 0)
            {
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var go = Instantiate(prefab, point.position, point.rotation);

                currentEnemyCount++;
                maxEnemies--;  // �o���\�����P���炷
            }

            // �o���\���� 0 �ɂȂ�����������[�v���񂳂Ȃ�
            if (maxEnemies <= 0)
                yield break;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// �G�����S�����Ƃ��� Enemy ������Ă΂��
    /// </summary>
    public void OnEnemyDestroyed()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);

        // �����c��o���\����0 ���V�[�����ɓG���������݂��Ȃ���΃N���A
        if (!clearCalled
            && maxEnemies <= 0
            && FindObjectsOfType<Enemy>().Length == 0)
        {
            clearCalled = true;
            GameData.ShowGameClear();
        }
    }
}
