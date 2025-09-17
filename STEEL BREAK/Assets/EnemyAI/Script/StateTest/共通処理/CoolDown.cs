using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
//using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
///クールダウンの使い方
///例） m_CoolDown.StartCoolDown("Attack",10);
///のようにクールダウンの名前と秒数を決める
///例）!IsCoolDown("Attack")
///とすることでAttackのクールダウンが終わったかどうかを確認することができる
/// </summary>

public class CoolDown : MonoBehaviour
{
    //各行動のクールダウン時間を取得
    private Dictionary<string, float> CoolDowns = new Dictionary<string, float>();
    [Header("クールダウン時間")]
    private Dictionary<string, float> CoolTimes = new Dictionary<string, float>();
    //クールダウン開始
    public void StartCoolDown(string actionName, float duration)
    {
        //Debug.Log("クールダウン開始");
        CoolDowns[actionName] = duration;
        CoolTimes[actionName] = Time.time;
    }

    //クールダウン中かどうか
    public bool IsCoolDown(string actionName)
    {
        //開始記録がないなら
        if (!CoolDowns.ContainsKey(actionName) || !CoolTimes.ContainsKey(actionName))
        {
            //クールダウンデータなし＝クールダウン中ではない
            return false;
        }

        return Time.time - CoolTimes[actionName] < CoolDowns[actionName];
    }

    // クールダウンの状態をログに出す
    //使い方　例）owner.m_CoolDown.DebugCoolDownProgress("Attack");
    public void DebugCoolDownProgress(string actionName)
    {
        if (!CoolDowns.ContainsKey(actionName) || !CoolTimes.ContainsKey(actionName))
        {
            Debug.Log($"[{actionName}] クールダウンなし");
            return;
        }

        float elapsedTime = Time.time - CoolTimes[actionName];
        float remainingTime = CoolDowns[actionName] - elapsedTime;

        Debug.Log($"[{actionName}] 経過時間: {elapsedTime:F2} 秒 / 残り時間: {remainingTime:F2} 秒");
    }

}