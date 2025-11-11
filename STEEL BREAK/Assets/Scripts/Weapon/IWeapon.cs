using UnityEngine;
using Game.Enum;

public interface IWeapon
{
    /// <summary>
    /// 武装を装備させる
    /// </summary>
    /// <param name="point">装備させるポイント</param>
    /// <param name="side">どちらに装備させるか</param>
    public void AttachToPoint(Transform point, AttachSide side);

    ///<summary>
    ///武装使用
    ///</summary>
    void Use();

    /// <summary>
    /// 武器不使用
    /// </summary>
    void NotUse();

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

    /// <summary>
    /// 残弾数を取得
    /// </summary>
    /// <returns></returns>
    int GetAmmo();

    /// <summary>
    /// 最大弾数を取得
    /// </summary>
    /// <returns></returns>
    int GetMaxAmmo();

    /// <summary>
    /// リロード中か取得
    /// </summary>
    /// <returns></returns>
    bool IsReloading();
}
