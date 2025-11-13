using System.Collections.Generic;
using UnityEngine;
using Game.Enum;

[System.Serializable]
public class WaveData
{
    [Header("このウェーブの敵生成データ")]
    public List<EnemySpawnData> enemySpawnList = new List<EnemySpawnData>();

    [Header("このウェーブを進行させるための追加条件")]
    public WaveCondition condition = WaveCondition.None;
}
