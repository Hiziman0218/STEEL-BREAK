using StateMachineAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFairys_T : MonoBehaviour
{
    //一体一体生成していく
    public static IEnumerator SpawnWithInterval(
    GameObject soldierPrefab,
    GameObject guardianPrefab,
    List<GameObject> m_SpawnPoints,             //スポーンポイント
    List<GameObject> spawnedEnemies,            //全体の生成数
    List<GameObject> spawnedAttackEnemies,      //攻撃型の数
    List<GameObject> spawnedDefensEnemies,      //防御型の数
    float m_SpawnPer,                           //生成割合
    float m_waitSeconds,                        //生成待ち時間
    int m_MaxFairys,                            //ゲーム上で生成できる最大数
    int m_MaxDefensFairys,                      //ボスを守る雑魚の上限
    System.Action onComplete
    )
    {
        GameObject enemy;

        //場に存在できる最大数より少なければ処理が走る
        while (spawnedEnemies.Count < m_MaxFairys)
        {
            //スポーンポイントを取得
            GameObject point = m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];

            //確率で攻撃型か防御型を選出（防御型は上限を設けている）
            if (Random.value < m_SpawnPer && spawnedDefensEnemies.Count < m_MaxDefensFairys)
            {
                //フェアリーの生成
                enemy = GameObject.Instantiate(guardianPrefab, point.transform.position, Quaternion.identity);

                //防御型のカウンターに追加
                spawnedDefensEnemies.Add(enemy);
            }
            else
            {
                //フェアリーの生成
                enemy = GameObject.Instantiate(soldierPrefab, point.transform.position, Quaternion.identity);

                //攻撃型のカウンターに追加
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
