using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEngine.UI.GridLayoutGroup;

public class RandomMove : MonoBehaviour
{
    private Vector3 directionToPlayer;
    private Vector3 combatTargetPosition;
    private bool hasCombatTarget = false;
    //0 = 前進 1 = 後退 2 = 左 3 = 右 
    private int moveDirection;
    //指定する移動距離
    [SerializeField]
    private float moveDistance;

    void Start()
    {
        //コルーチンを開始
        StartCoroutine(RandomMoveRoutine());
    }

    //一定間隔で移動
    IEnumerator RandomMoveRoutine()
    {
        while (true) // 無限ループ
        {
            hasCombatTarget = false;
            //0〜3の値(移動方向)をランダムに決定
            moveDirection = Random.Range(0, 4);
            //2秒待機して次の移動処理へ
            yield return new WaitForSeconds(2.0f);
        }
    }

    public void StartRandomMove()
    {
        //新しい移動先を決定
        if (!hasCombatTarget)
        {
            //0〜3の値をランダムに決定
            moveDirection = Random.Range(0, 4);
            Debug.Log("方向決定："+moveDirection);
            //プレイヤーとアタッチされたオブジェクトの距離を正規化
            directionToPlayer = (GameObject.FindWithTag("Player").transform.position - transform.position).normalized;

            switch (moveDirection)
            {
                case 0: //前進
                    combatTargetPosition = transform.position + directionToPlayer * moveDistance;
                    break;
                case 1: //後退
                    combatTargetPosition = transform.position - directionToPlayer * moveDistance;
                    break;
                case 2: //左移動
                    combatTargetPosition = transform.position + new Vector3(-directionToPlayer.z, 0, directionToPlayer.x) * moveDistance;
                    break;
                case 3: //右移動
                    combatTargetPosition = transform.position + new Vector3(directionToPlayer.z, 0, -directionToPlayer.x) * moveDistance;
                    break;
            }

            //NavMeshの中にある座標を取得
            NavMeshHit hit;
            if (NavMesh.SamplePosition(combatTargetPosition, out hit, 2.0f, NavMesh.AllAreas))
            {
                combatTargetPosition = hit.position;
                hasCombatTarget = true;
            }
        }

        GetComponent<NavMeshAgent>().SetDestination(combatTargetPosition);
    }
    public void Exit()
    {
        hasCombatTarget = false;
    }
}
