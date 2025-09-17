using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    /// <summary>
    /// 手に持つ
    /// </summary>
    /// <param name="hand"></param>
    public void AttachToHand(Transform hand, bool left);

    ///<summary>
    ///武装使用
    ///</summary>
    void Use();

    /// <summary>
    /// 武器リロード
    /// </summary>
    void Reload();

    /// <summary>
    /// IKの完了/未完了を通達
    /// </summary>
    void SetIKFinished(bool IKFinished);

    ///<summary>
    ///名前を取得
    ///</summary>
    ///<returns>武装の名前</returns>
    string GetName();

    void SetTeam(string team);
}
