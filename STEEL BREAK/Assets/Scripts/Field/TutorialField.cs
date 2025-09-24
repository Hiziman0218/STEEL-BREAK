using System.Collections.Generic;
using UnityEngine;

public class TutorialField : MonoBehaviour
{
    [Header("出現位置")]
    [SerializeField] private Transform m_playerSpawnPoint;       //プレイヤーの出現位置
    [SerializeField] private List<Transform> m_enemySpawnPoints; //敵の出現位置

    [Header("プレハブ")]
    [SerializeField] private GameObject m_playerPrefab; //プレイヤーのプレハブ
    [SerializeField] private GameObject m_enemyPrefab;  //敵のプレハブ

    private GameObject m_playerInstance; //プレイヤーのインスタンス
    private List<GameObject> m_enemyInstances = new List<GameObject>(); //敵のリスト


    void Start()
    {
        //プレイヤーと敵を生成
        SpawnPlayer();
        SpawnEnemies();
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
    /// 敵を生成
    /// </summary>
    private void SpawnEnemies()
    {
        //敵のプレハブと生成位置が設定されていなければ、以降の処理を行わない
        if (m_enemyPrefab == null || m_enemySpawnPoints == null)
        {
            return;
        }

        //生成位置に敵を生成
        foreach (Transform spawnPoint in m_enemySpawnPoints)
        {
            if (spawnPoint != null)
            {
                GameObject enemy = Instantiate(m_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                m_enemyInstances.Add(enemy);
            }
        }
    }
}
