using Ilumisoft.RadarSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlayer : PlayerBase
{
    //デバッグ用 武装をインスペクタで設定
    [SerializeField] private MonoBehaviour m_righthandWeaponMono; //右手武装(デバッグ)
    [SerializeField] private MonoBehaviour m_lefthandWeaponMono;  //左手武装(デバッグ)

    protected override void Initialize()
    {
        //基底クラスの初期化呼び出し
        base.Initialize();
    }
}