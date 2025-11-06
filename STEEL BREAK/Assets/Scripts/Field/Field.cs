using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [Header("プレイヤー生成データ")]
    [Tooltip("プレイヤーのプレハブ")]
    [SerializeField] private GameObject m_playerPrefab;
    [Tooltip("プレイヤーの出現位置")]
    [SerializeField] private Transform m_playerSpawnPoint;

    [Header("敵生成データリスト")]
    [Tooltip("敵のプレハブとその敵を生成したい位置のリスト")]
    public List<EnemySpawnData> spawnDataList = new List<EnemySpawnData>();

    private GameObject m_playerInstance; //プレイヤーのインスタンス

    private void Awake()
    {
        SpawnPlayer();
        SpawnAllEnemies();
    }

    /// <summary>
    /// プレイヤーを生成
    /// </summary>
    private void SpawnPlayer()
    {
        if (m_playerPrefab != null && m_playerSpawnPoint != null)
        {
            //プレイヤー生成
            m_playerInstance = Instantiate(m_playerPrefab, m_playerSpawnPoint.position, m_playerSpawnPoint.rotation);

            //GameManagerへ通知
            GameManager.Instance.OnPlayerSpawned(m_playerInstance);
        }
    }

    /// <summary>
    /// 全ての敵を生成
    /// </summary>
    private void SpawnAllEnemies()
    {
        foreach (var data in spawnDataList)
        {
            if (data.enemyPrefab == null)
            {
                Debug.LogWarning("敵プレハブが設定されていません。");
                continue;
            }

            foreach (var point in data.spawnPoints)
            {
                if (point == null) continue;

                //敵を生成
                Instantiate(data.enemyPrefab, point.position, point.rotation);
            }
        }
    }
}