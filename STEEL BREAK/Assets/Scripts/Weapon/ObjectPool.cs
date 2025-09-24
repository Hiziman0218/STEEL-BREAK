using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Queue<T> objects = new Queue<T>();

    public ObjectPool(T prefab, int initialSize)
    {
        this.prefab = prefab;

        //��������
        for (int i = 0; i < initialSize; i++)
        {
            T obj = GameObject.Instantiate(prefab);
            obj.gameObject.SetActive(false);
            objects.Enqueue(obj);
        }
    }

    //�I�u�W�F�N�g�擾
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

    //�I�u�W�F�N�g��߂�
    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }

    //�V�K����
    private void AddObject()
    {
        T obj = GameObject.Instantiate(prefab);
        obj.gameObject.SetActive(false);
        objects.Enqueue(obj);
    }
}
