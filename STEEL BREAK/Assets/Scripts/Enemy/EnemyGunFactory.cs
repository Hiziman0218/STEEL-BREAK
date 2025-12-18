using UnityEngine;

public class EnemyGunFactory : MonoBehaviour
{
    [SerializeField] private EnemyGun[] m_gunPrefabs; //ランダムで装備させたい武器のリスト

    /// <summary>
    /// リストからランダムで取得した武器を取得
    /// </summary>
    /// <returns></returns>
    public EnemyGun CreateRandomGun(Transform parent)
    {
        int index = Random.Range(0, m_gunPrefabs.Length);
        return Instantiate(m_gunPrefabs[index], parent);
    }
}
