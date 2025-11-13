using UnityEngine;

[System.Serializable]
public class EnemyPrefabEntry
{
    [Header("敵プレハブ")]
    public Enemy enemyPrefab;

    [Header("敵の出現確率")]
    [Tooltip("値が大きいほど出現確率が高い")]
    [Range(0f, 1f)] public float spawnWeight = 1f;
}
