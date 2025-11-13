using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [Header("敵プレハブ")]
    public Enemy enemyPrefab;

    [Header("出現位置")]
    public Transform spawnTransform;

    [Header("ランダム生成にするか")]
    [Tooltip("trueならランダム生成、falseなら設定した敵プレハブで固定")]
    public bool isRandom;
}
