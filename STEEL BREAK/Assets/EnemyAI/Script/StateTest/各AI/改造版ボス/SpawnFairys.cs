using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnFairys_T : MonoBehaviour
{
    //一体一体生成していく
    public static IEnumerator SpawnWithInterval(
    GameObject m_Fairys,                        //生成するモデル
    List<GameObject> m_SpawnPoints,             //スポーンポイント
    List<GameObject> spawnedEnemies,            //全体の生成数
    List<GameObject> spawnedAttackEnemies,      //攻撃型の数
    List<GameObject> spawnedDefensEnemies,      //防御型の数
    float m_SpawnPer,                           //生成割合
    float m_waitSeconds,                        //生成待ち時間
    int m_MaxFairys,                            //ゲーム上で生成できる最大数
    System.Action onComplete
)
    {
        while (spawnedEnemies.Count < m_MaxFairys)
        {
            //スポーンポイントを取得
            GameObject point = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];

            //フェアリーの生成
            GameObject enemy = GameObject.Instantiate(m_Fairys, point.transform.position, Quaternion.identity);

            if (Random.value < m_SpawnPer)
            {
                var comp = enemy.AddComponent<GyardianFairysAI>();
                spawnedDefensEnemies.Add(enemy);
            }
            else
            {
                var comp = enemy.AddComponent<SoldierFairysAI>();
                spawnedAttackEnemies.Add(enemy);
            }

            //現在生成されている敵を記録
            spawnedEnemies.Add(enemy);
            //待ち時間を返す
            yield return new WaitForSeconds(m_waitSeconds);
        }

        Debug.Log("フェアリーの生成が完了しました！");
        onComplete?.Invoke();
    }
}
