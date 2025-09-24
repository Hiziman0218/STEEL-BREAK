using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Queue<T> objects = new Queue<T>();

    public ObjectPool(T prefab, int initialSize)
    {
        this.prefab = prefab;

        //初期生成
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab);
            obj.gameObject.SetActive(false);
            objects.Enqueue(obj);
        }
    }

    //オブジェクト取得
    public T Get()
    {
        if (objects.Count == 0)
        {
            AddObject();
        }

        T obj = objects.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    //オブジェクトを戻す
    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }

    //新規生成
    private void AddObject()
    {
        T obj = GameObject.Instantiate(prefab);
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }
}
