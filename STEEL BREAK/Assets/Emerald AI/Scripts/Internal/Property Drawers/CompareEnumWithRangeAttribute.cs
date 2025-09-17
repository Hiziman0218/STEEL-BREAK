using UnityEngine;
using System;

/// <summary>
/// 【CompareEnumWithRangeAttribute】
/// 比較用の別プロパティの値（列挙値など）に応じて、対象のフィールド/プロパティを
/// 「インスペクタ上で表示するかどうか」や「スライダー範囲」を制御するための属性です。
/// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CompareEnumWithRangeAttribute : PropertyAttribute
{
    #region Fields
    /// <summary>
    /// 比較対象となるプロパティ名（大文字/小文字は区別されます）。
    /// 例: "AttackType" など
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// 比較に使用する列挙値/値（1つ目）。一致した場合に本属性の条件が成立します。
    /// </summary>
    public object comparedValue1 { get; private set; }

    /// <summary>
    /// 比較に使用する列挙値/値（2つ目・任意）。
    /// </summary>
    public object comparedValue2 { get; private set; }

    /// <summary>
    /// 比較に使用する列挙値/値（3つ目・任意）。
    /// </summary>
    public object comparedValue3 { get; private set; }

    /// <summary>
    /// スライダーの種類（浮動小数/整数）を指定します。
    /// </summary>
    public StyleType styleType { get; private set; }

    [Header("スライダーの最小値（Min）。StyleType に応じて float/int として解釈されます")]
    /// <summary>
    /// スライダーの最小値。
    /// </summary>
    public float min;

    [Header("スライダーの最大値（Max）。StyleType に応じて float/int として解釈されます")]
    /// <summary>
    /// スライダーの最大値。
    /// </summary>
    public float max;

    /// <summary>
    /// 変数に適用できるスライダーの種類。
    /// </summary>
    public enum StyleType
    {
        /// <summary>
        /// 小数（float）スライダー
        /// </summary>
        FloatSlider = 1,

        /// <summary>
        /// 整数（int）スライダー
        /// </summary>
        IntSlider = 2
    }
    #endregion

    /// <summary>
    /// 【コンストラクタ】
    /// 条件が満たされた場合にのみ、対象のフィールド/プロパティを描画します。
    /// </summary>
    /// <param name="comparedPropertyName">比較対象のプロパティ名（厳密一致・大文字小文字区別）。</param>
    /// <param name="min">スライダーの最小値。</param>
    /// <param name="max">スライダーの最大値。</param>
    /// <param name="styleType">スライダーの種類（Float/Int）。</param>
    /// <param name="comparedValue1">比較値1（この値と一致したら表示）。</param>
    /// <param name="comparedValue2">比較値2（任意）。</param>
    /// <param name="comparedValue3">比較値3（任意）。</param>
    public CompareEnumWithRangeAttribute(string comparedPropertyName, float min, float max, StyleType styleType, object comparedValue1, object comparedValue2 = null, object comparedValue3 = null)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
        this.comparedValue1 = comparedValue1;
        this.comparedValue2 = comparedValue2;
        this.comparedValue3 = comparedValue3;
    }
}
