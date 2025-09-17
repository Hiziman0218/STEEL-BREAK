using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


public class SpawnFairys_T : MonoBehaviour
{
    //一体一体生成していく
    public static IEnumerator SpawnWithInterval(
    GameObject m_Fairys,
    List<GameObject> m_SpawnPoints,
    List<GameObject> spawnedEnemies,
    float m_SpawnPer,
    float m_waitSeconds,
    int m_MaxFairys,
    System.Action onComplete
)
    {
        while (spawnedEnemies.Count < m_MaxFairys)
        {
            //スポーンポイントを取得
            GameObject point = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];

            //フェアリーの生成
            GameObject enemy = GameObject.Instantiate(m_Fairys, point.transform.position, Quaternion.identity);
            FairyAI fairyComponent = enemy.GetComponent<FairyAI>();

            //コンポーネントがnullじゃなければ役職決め
            if (fairyComponent != null)
            {
                //m_SpawnPerが低いほどソルジャーが多くなる　高いほどガーディアンが多くなる
                fairyComponent.m_Role = (Random.value < m_SpawnPer) ? EnemyRole.Guardian : EnemyRole.Soldier;
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
