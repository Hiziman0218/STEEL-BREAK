using UnityEngine;
using System;

/// <summary>
/// 【DrawIfAttribute】
/// 条件が満たされた場合にのみ、対象のフィールド/プロパティをインスペクタ上に描画する属性。
/// 「あるプロパティの値」と「指定値（comparedValue）」を比較し、合致した場合だけ描画します。
/// </summary>
// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{
    #region Fields

    /// <summary>
    /// 比較対象となるプロパティ名（大文字小文字は区別されます）。
    /// 例: "AttackType" など
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// 比較に用いる値。比較対象プロパティの値がこれと一致した場合に描画します。
    /// bool / enum 等に対応。
    /// </summary>
    public object comparedValue { get; private set; }

    /// <summary>
    /// 条件を満たさない場合の無効化方法。
    /// </summary>
    public DisablingType disablingType { get; private set; }

    /// <summary>
    /// 無効化方法の種類。
    /// </summary>
    public enum DisablingType
    {
        /// <summary>
        /// 読み取り専用として表示（編集不可）
        /// </summary>
        ReadOnly = 2,

        /// <summary>
        /// そもそも描画しない（非表示）
        /// </summary>
        DontDraw = 3
    }

    #endregion

    /// <summary>
    /// 条件が満たされた場合にのみ、対象のフィールドを描画します（bool/enumに対応）。
    /// </summary>
    /// <param name="comparedPropertyName">比較対象プロパティ名（厳密一致・大文字小文字区別）。</param>
    /// <param name="comparedValue">比較値（この値と一致した場合に描画）。</param>
    /// <param name="disablingType">条件不一致時の無効化方法（既定は <see cref="DisablingType.DontDraw"/>）。</param>
    public DrawIfAttribute(string comparedPropertyName, object comparedValue, DisablingType disablingType = DisablingType.DontDraw)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.disablingType = disablingType;
    }
}
