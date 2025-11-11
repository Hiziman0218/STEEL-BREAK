using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    [Header("このウェーブの敵生成データ")]
    public List<EnemySpawnData> enemySpawnList = new List<EnemySpawnData>();
}
