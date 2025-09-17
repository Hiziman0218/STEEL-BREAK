using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [Header("出現させる敵のプレハブ（複数種類を設定）")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("敵を出現させるポイント")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("出現間隔（秒）")]
    [SerializeField] private float spawnInterval = 3f;

    [Header("出現可能な残り数")]
    [Tooltip("ここが 0 になったらもうスポーンしません")]
    [SerializeField] private int maxEnemies = 1;

    // 現在シーン内に存在する敵数
    private int currentEnemyCount = 0;

    // クリア処理を一度だけ呼ぶためのフラグ
    private bool clearCalled = false;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 「残り出現可能数 > 0」かつ「同時存在数 < 1」でなく
            if (maxEnemies > 0
                && currentEnemyCount < maxEnemies
                && spawnPoints.Length > 0
                && enemyPrefabs.Length > 0)
            {
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var go = Instantiate(prefab, point.position, point.rotation);

                currentEnemyCount++;
                maxEnemies--;  // 出現可能数を１減らす
            }

            // 出現可能数が 0 になったらもうループを回さない
            if (maxEnemies <= 0)
                yield break;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 敵が死亡したときに Enemy 側から呼ばれる
    /// </summary>
    public void OnEnemyDestroyed()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);

        // もし残り出現可能数＝0 かつシーン内に敵がもう存在しなければクリア
        if (!clearCalled
            && maxEnemies <= 0
            && FindObjectsOfType<Enemy>().Length == 0)
        {
            clearCalled = true;
            GameData.ShowGameClear();
        }
    }
}
