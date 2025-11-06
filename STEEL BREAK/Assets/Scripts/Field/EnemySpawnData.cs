using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Header("敵プレハブ")]
    public GameObject enemyPrefab;

    [Header("出現位置リスト")]
    public List<Transform> spawnPoints = new List<Transform>();
}
