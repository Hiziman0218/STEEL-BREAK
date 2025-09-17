using Plugins.RaycastPro.Demo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    [Header("識別名")]
    public string key;
    [Header("プレハブ")]
    public GameObject prefab;
    [Header("初期生成数")]
    public int size;
}


public class PoolManager : MonoBehaviour
{
    [SerializeField] private List<PoolItem> poolItems;
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    public static PoolManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // 重複防止

        // 各種類のプレハブごとにQueueを作成
        foreach (var item in poolItems)
        {
            var queue = new Queue<GameObject>();

            // 初期数だけ生成して非アクティブ状態でQueueに入れる
            for (int i = 0; i < item.size; i++)
            {
                var obj = Instantiate(item.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            // keyとQueueを辞書に登録
            pools[item.key] = queue;
        }
    }

    /// <summary>
    /// プールマネージャーの取得
    /// </summary>
    /// <param name="key">取得したいプールマネージャの名前</param>
    /// <param name="position">どこに置くか</param>
    /// <param name="target">生成したい位置</param>
    /// <returns></returns>
    public GameObject Get(string key, Vector3 position, Transform target)
    {
        // 対応するキーがなければ何も返さない
        if (!pools.ContainsKey(key)) return null;

        // 使用できる（非アクティブ）なオブジェクトがあれば再利用、それ以外は新しく生成
        GameObject obj = (pools[key].Count > 0 && !pools[key].Peek().activeInHierarchy)
            ? pools[key].Dequeue()
            : Instantiate(poolItems.Find(p => p.key == key).prefab);

        // 位置設定＆アクティブ化
        obj.transform.position = position;
        obj.SetActive(true);

        // SteeringController があればターゲット設定＆挙動ON
        var controller = obj.GetComponent<SteeringController>();
        if (controller != null)
        {
            controller.detector.destination = target;
            controller.enabled = true;
        }

        // Queueに戻して次回も使えるように（再利用のため）
        pools[key].Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// 使用済みオブジェクトをプールに返却（使用停止＋非表示）
    /// </summary>
    /// <param name="key">解除したいプールマネージャの名前</param>
    /// <param name="obj">プールマネージャを付けたオブジェクト</param>
    public void Return(string key, GameObject obj)
    {
        var controller = obj.GetComponent<SteeringController>();
        if (controller != null) controller.enabled = false;

        obj.SetActive(false);
    }
}
