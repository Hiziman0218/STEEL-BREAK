using UnityEngine;
using Game.Enum;

public interface IWeapon
{
    /// <summary>
    /// 手に持ち、装備させる
    /// </summary>
    /// <param name="hand">手のトランスフォーム</param>
    /// <param name="heldHand">どちらの手に持つか</param>
    public void AttachToHand(Transform hand, HandSide heldHand);

    ///<summary>
    ///武装使用
    ///</summary>
    void Use();

    /// <summary>
    /// 武器リロード
    /// </summary>
    void Reload();

    /// <summary>
    /// IKの完了/未完了を設定
    /// </summary>
    /// <param name="IKFinished">IKが完了したか</param>
    void SetIKFinished(bool IKFinished);

    ///<summary>
    ///名前を取得
    ///</summary>
    ///<returns>武装の名前</returns>
    string GetName();

    /// <summary>
    /// チームを設定
    /// </summary>
    /// <param name="team">現在のチーム</param>
    void SetTeam(string team);
}
