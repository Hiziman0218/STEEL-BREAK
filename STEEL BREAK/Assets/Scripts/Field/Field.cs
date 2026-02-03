using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [Header("プレイヤー生成データ")]
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private Transform m_playerSpawnPoint;

    [Header("敵のランダム生成データ")]
    [SerializeField] private List<EnemyPrefabEntry> enemyPrefabs = new List<EnemyPrefabEntry>();

    [Header("ウェーブデータリスト")]
    [Tooltip("各ウェーブごとの敵生成データ")]
    public List<WaveData> waveDataList = new List<WaveData>();

    private GameObject m_playerInstance; //プレイヤー
    private int m_currentWaveIndex = 0;  //現在のウェーブ
    
    public static event Action<int, int> OnWaveChanged; //Wave変更通知

    private List<Enemy> m_aliveEnemies = new List<Enemy>(); //生存している敵のリスト

    private void Start()
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
        int currentWave = m_currentWaveIndex + 1;
        int totalWave = waveDataList.Count;

        // UIへ通知
        OnWaveChanged?.Invoke(currentWave, totalWave);

        Debug.Log($"Wave {m_currentWaveIndex + 1} 開始");

        //敵を生成
        foreach (var enemyData in wave.enemySpawnList)
        {
            //プレハブか座標が設定されていなければコンティニュー
            if (enemyData.enemyPrefab == null || enemyData.spawnTransform == null) continue;

            Enemy enemy = null;
            
            //ランダム生成する場合は、このフィールドに登録された敵のプレハブをランダムに取得し生成
            if (enemyData.isRandom)
            {
                enemy = Instantiate(GetRandomEnemyPrefab(), enemyData.spawnTransform.position, enemyData.spawnTransform.rotation);
            }
            //ランダム生成ではない場合は、データに登録されたプレハブをそのまま使用する
            else
            {
                enemy = Instantiate(enemyData.enemyPrefab, enemyData.spawnTransform.position, enemyData.spawnTransform.rotation);
            }

            m_aliveEnemies.Add(enemy);

            //敵が上手く生成できていたら、敵を生存リストに追加し、敵の死亡イベントを設定
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
    /// 確率を元にランダムで敵プレハブを取得する
    /// </summary>
    public Enemy GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("敵プレハブリストが設定されていません。");
            return null;
        }

        //合計重みを算出
        float totalWeight = 0f;
        foreach (var entry in enemyPrefabs)
            totalWeight += entry.spawnWeight;

        //0〜totalWeight の乱数を生成
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        //重みに応じて該当プレハブを返す
        foreach (var entry in enemyPrefabs)
        {
            cumulative += entry.spawnWeight;
            if (randomValue <= cumulative)
                return entry.enemyPrefab;
        }

        //万が一、誤差で何も選ばれなかった場合
        return enemyPrefabs[enemyPrefabs.Count - 1].enemyPrefab;
    }

    /// <summary>
    /// 敵死亡時に呼ばれるイベント
    /// </summary>
    private void HandleEnemyDied(Enemy enemy)
    {
        m_aliveEnemies.Remove(enemy);
    }
}