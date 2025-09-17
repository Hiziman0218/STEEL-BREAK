using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMS : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] private NewBullet m_bulletPrefab;   //弾丸プレハブ
    [SerializeField] private Transform m_muzzleTransform; //発射口
    [SerializeField] private GunStatusData m_statusData;  //銃の性能(インスペクタで設定)

    private GunStatus m_status;      //銃の性能(インスペクタで設定したものを代入)
    private float m_ElapsedTime;     //経過時間
    private bool m_FireFlag = false; //発射可能か
    private bool m_Reload = false;   //リロード中か

    void Start()
    {
        //銃のステータスを設定
        m_status = new GunStatus(m_statusData);
        //最初はすぐに撃てるように設定
        m_ElapsedTime = m_status.GetRate();
    }

    void Update()
    {
        //発射可能フラグをfalseに設定
        m_FireFlag = false;
        //経過時間を計測
        m_ElapsedTime += Time.deltaTime;

        /*リロード処理*/
        //リロード中なら
        if (m_Reload)
        {
            //経過時間がリロード時間以上なら
            if (m_ElapsedTime > m_status.GetReloadTime())
            {
                //リロードを完了
                ReloadComplete();
            }
            return; //以降の処理を行わない
        }

        /*クールタイム処理*/
        //経過時間が発射間隔以上なら
        if (m_ElapsedTime >= m_status.GetRate())
        {
            //発射可能フラグをtrueに設定
            m_FireFlag = true;
        }
    }

    /// <summary>
    /// 発射
    /// </summary>
    public void OnFire()
    {
        //発射可能なら
        if (m_FireFlag)
        {
            //弾を生成
            NewBullet Dummy = Instantiate(m_bulletPrefab, m_muzzleTransform.position, m_muzzleTransform.rotation);
            //弾に力を加えて移動させる(AddForse)
            Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 1000.0f);
            //10秒後に削除
            Destroy(Dummy, 10.0f);
            //弾数を減少
            m_status.SetAmmo(m_status.GetAmmo() - 1);
            //弾数が残っているか確認、残っていないならリロード開始
            if(m_status.GetAmmo() <= 0)
            {
                ReloadStart();
            }
            //経過時間をリセット
            m_ElapsedTime = 0f;
        }
    }

    /// <summary>
    /// リロード開始
    /// </summary>
    public void ReloadStart()
    {
        //リロードフラグをtrueに設定
        m_Reload = true;
    }

    /// <summary>
    /// リロード完了
    /// </summary>
    private void ReloadComplete()
    {
        //リロードフラグをfalseに設定
        m_Reload = false;
        //弾数を最大に設定
        m_status.SetAmmo(m_status.GetMaxAmmo());
    }

    /// <summary>
    /// 自身を子供に設定
    /// </summary>
    /// <param name="hand">親になるトランスフォーム</param>
    public void AttachToHand(Transform hand)
    {
        
        Transform grip = transform.Find("GripPoint");
        if (grip == null) return;

        transform.SetParent(hand, false);
        transform.localPosition = -grip.localPosition;
        transform.localRotation = Quaternion.Inverse(grip.localRotation);
        
        //transform.SetParent(hand);
    }
}
