using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [Header("プレイヤー生成データ")]
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform m_playerSpawnPoint;

    [Header("ウェーブデータリスト")]
    [Tooltip("各ウェーブごとの敵生成データ")]
    public List<WaveData> waveDataList = new List<WaveData>();

    private GameObject m_playerInstance; //プレイヤー
    private int m_currentWaveIndex = 0;  //現在のウェーブ

    private List<Enemy> m_aliveEnemies = new List<Enemy>(); //生存している敵のリスト

    private void Awake()
    {
        SpawnPlayer();
        StartCoroutine(StartWaveSequence());
    }

    /// <summary>
    /// プレイヤーを生成
    /// </summary>
    private void SpawnPlayer()
    {
        if (m_playerPrefab != null && m_playerSpawnPoint != null)
        {
            m_playerInstance = Instantiate(m_playerPrefab, m_playerSpawnPoint.position, m_playerSpawnPoint.rotation);
            GameManager.Instance.OnPlayerSpawned(m_playerInstance);
        }
    }

    /// <summary>
    /// ウェーブ進行管理コルーチン
    /// </summary>
    private IEnumerator StartWaveSequence()
    {
        while (m_currentWaveIndex < waveDataList.Count)
        {
            yield return StartCoroutine(SpawnWave(waveDataList[m_currentWaveIndex]));

            // 次のウェーブへ
            m_currentWaveIndex++;
        }

        GameData.ShowGameClear();
        Debug.Log("すべてのウェーブが終了");
    }

    /// <summary>
    /// 指定ウェーブを生成して全滅を待つ
    /// </summary>
    private IEnumerator SpawnWave(WaveData wave)
    {
        Debug.Log($"Wave {m_currentWaveIndex + 1} 開始");

        //敵を生成
        foreach (var enemyData in wave.enemySpawnList)
        {
            if (enemyData.enemyPrefab == null || enemyData.spawnTransform == null) continue;

            Enemy enemy = Instantiate(enemyData.enemyPrefab, enemyData.spawnTransform.position, enemyData.spawnTransform.rotation);
            m_aliveEnemies.Add(enemy);

            //敵が倒れた時に通知できるようにする
            if (enemy != null)
            {
                enemy.OnDiedField += HandleEnemyDied;
            }
        }

        //全滅待ち
        yield return new WaitUntil(() => m_aliveEnemies.Count == 0);

        Debug.Log($"Wave {m_currentWaveIndex + 1} 終了");
        yield return new WaitForSeconds(2f); //次ウェーブまでの待機
    }

    /// <summary>
    /// 敵死亡時に呼ばれるイベント
    /// </summary>
    private void HandleEnemyDied(Enemy enemy)
    {
        m_aliveEnemies.Remove(enemy);
    }
}