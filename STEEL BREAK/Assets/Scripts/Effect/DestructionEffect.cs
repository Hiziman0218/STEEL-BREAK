using System;
using UnityEngine;

public class DestructionEffect : MonoBehaviour
{
    private GameObject m_owner;  //自身を生成したキャラクター
    private event Action OnDied; //死亡時のイベント

    private void OnDestroy()
    {
        //自身を生成したキャラクターを削除
        if(m_owner != null) Destroy(m_owner);
        //死亡イベントがあれば実行
        OnDied?.Invoke();
    }

    /// <summary>
    /// 自身を生成したキャラクターを設定
    /// </summary>
    /// <param name="owner"></param>
    public void SetOwner(GameObject owner, Action DiedEvent = null)
    {
        m_owner = owner;
        OnDied = DiedEvent;
    }
}
