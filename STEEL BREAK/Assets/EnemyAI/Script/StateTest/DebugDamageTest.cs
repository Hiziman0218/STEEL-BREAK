using UnityEngine;

public class DebugDamageTester : MonoBehaviour
{
    public CharaBase target;   // ダメージを与える対象
    public float damage = 1f; // 与えるダメージ量
    public KeyCode key = KeyCode.Space; // 押すキー

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            if (target != null)
            {
                target.GetDamage(damage);
                Debug.Log($"{target.name} に {damage} ダメージを与えた");
            }
        }
    }
}
