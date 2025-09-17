using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    //プール本体
    private ObjectPool<Bullet> bulletPool;
    //このプールを使う使用者
    private GameObject shooter;

    ///<summary>
    ///プールを初期化 Weapon_Shooting から呼び出し
    ///</summary>
    ///<param name="shooter">使用者(PlayerやEnemy)</param>
    ///<param name="bulletPrefab">弾丸プレハブ</param>
    ///<param name="initialSize">プールの初期サイズ</param>
    public void Initialize(GameObject shooter, Bullet bulletPrefab, int initialSize = 20)
    {
        this.shooter = shooter;
        bulletPool = new ObjectPool<Bullet>(bulletPrefab, initialSize);
    }

    ///<summary>
    ///弾を発射
    ///</summary>
    public void Fire(Vector3 position, Quaternion rotation)
    {
        //プールから取り出し、位置/回転をセット
        var b = bulletPool.Get();
        b.transform.position = position;
        b.transform.rotation = rotation;

        //弾に、誰が撃ったかとどのプール所属かを教える
        b.Initialize(shooter, bulletPool);

        //当たり判定開始
        b.AttackStart();
    }
}