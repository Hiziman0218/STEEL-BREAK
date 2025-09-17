using Plugins.RaycastPro.Demo.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    [Header("���ʖ�")]
    public string key;
    [Header("�v���n�u")]
    public GameObject prefab;
    [Header("����������")]
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
        else Destroy(gameObject); // �d���h�~

        // �e��ނ̃v���n�u���Ƃ�Queue���쐬
        foreach (var item in poolItems)
        {
            var queue = new Queue<GameObject>();

            // �����������������Ĕ�A�N�e�B�u��Ԃ�Queue�ɓ����
            for (int i = 0; i < item.size; i++)
            {
                var obj = Instantiate(item.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            // key��Queue�������ɓo�^
            pools[item.key] = queue;
        }
    }

    /// <summary>
    /// �v�[���}�l�[�W���[�̎擾
    /// </summary>
    /// <param name="key">�擾�������v�[���}�l�[�W���̖��O</param>
    /// <param name="position">�ǂ��ɒu����</param>
    /// <param name="target">�����������ʒu</param>
    /// <returns></returns>
    public GameObject Get(string key, Vector3 position, Transform target)
    {
        // �Ή�����L�[���Ȃ���Ή����Ԃ��Ȃ�
        if (!pools.ContainsKey(key)) return null;

        // �g�p�ł���i��A�N�e�B�u�j�ȃI�u�W�F�N�g������΍ė��p�A����ȊO�͐V��������
        GameObject obj = (pools[key].Count > 0 && !pools[key].Peek().activeInHierarchy)
            ? pools[key].Dequeue()
            : Instantiate(poolItems.Find(p => p.key == key).prefab);

        // �ʒu�ݒ聕�A�N�e�B�u��
        obj.transform.position = position;
        obj.SetActive(true);

        // SteeringController ������΃^�[�Q�b�g�ݒ聕����ON
        var controller = obj.GetComponent<SteeringController>();
        if (controller != null)
        {
            controller.detector.destination = target;
            controller.enabled = true;
        }

        // Queue�ɖ߂��Ď�����g����悤�Ɂi�ė��p�̂��߁j
        pools[key].Enqueue(obj);
        return obj;
    }

    /// <summary>
    /// �g�p�ς݃I�u�W�F�N�g���v�[���ɕԋp�i�g�p��~�{��\���j
    /// </summary>
    /// <param name="key">�����������v�[���}�l�[�W���̖��O</param>
    /// <param name="obj">�v�[���}�l�[�W����t�����I�u�W�F�N�g</param>
    public void Return(string key, GameObject obj)
    {
        var controller = obj.GetComponent<SteeringController>();
        if (controller != null) controller.enabled = false;

        obj.SetActive(false);
    }
}
