using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Header("敵プレハブ")]
    public Enemy enemyPrefab;

    [Header("出現位置")]
    public Transform spawnTransform;

    /*
    [Header("出現位置リスト")]
    public List<Transform> spawnPoints = new List<Transform>();*/
}
