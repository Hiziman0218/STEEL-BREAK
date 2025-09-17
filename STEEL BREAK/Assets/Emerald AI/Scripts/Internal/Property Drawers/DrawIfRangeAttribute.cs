using UnityEngine;
using System;

/// <summary>
/// 【DrawIfRangeAttribute】
/// 指定した「比較対象プロパティ」の値と <see cref="comparedValue"/> を
/// <see cref="comparisonType"/> の条件で比較し、**条件が満たされた場合にのみ**
/// 対象のフィールド/プロパティをインスペクタに描画する属性です。
/// さらに、描画されるフィールドに対して、<see cref="styleType"/> に応じた
/// **スライダー（範囲）** を適用します。
///
/// 参考: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfRangeAttribute : PropertyAttribute
{
    #region Fields

    /// <summary>
    /// 比較対象となるプロパティ名（大文字小文字は区別されます）。
    /// 例: "AttackType" など
    /// </summary>
    public string comparedPropertyName { get; private set; }

    /// <summary>
    /// 比較に用いる値。比較対象プロパティの値とこの値を照合します。
    /// </summary>
    public object comparedValue { get; private set; }

    /// <summary>
    /// 比較の方法を指定します（等しい／より大きい／以下 など）。
    /// </summary>
    public ComparisonType comparisonType { get; private set; }

    /// <summary>
    /// スライダーの種類（既定/小数スライダー/整数スライダー）を指定します。
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
        /// 既定（スライダーなし、通常描画）
        /// </summary>
        Default = 1,

        /// <summary>
        /// 小数（float）スライダー
        /// </summary>
        FloatSlider = 2,

        /// <summary>
        /// 整数（int）スライダー
        /// </summary>
        IntSlider = 3
    }

    /// <summary>
    /// 比較の種類。
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// 等しい
        /// </summary>
        Equals = 1,

        /// <summary>
        /// 等しくない
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// より大きい
        /// </summary>
        GreaterThan = 3,

        /// <summary>
        /// より小さい
        /// </summary>
        SmallerThan = 4,

        /// <summary>
        /// 以下（小さいまたは等しい）
        /// </summary>
        SmallerOrEqual = 5,

        /// <summary>
        /// 以上（大きいまたは等しい）
        /// </summary>
        GreaterOrEqual = 6
    }

    #endregion

    /// <summary>
    /// 【コンストラクタ】
    /// 条件が満たされた場合にのみ対象フィールドを描画し、同時にスライダー範囲を適用します。
    /// 列挙体・bool 等の比較に対応します。
    /// </summary>
    /// <param name="comparedPropertyName">比較対象プロパティ名（厳密一致・大文字小文字区別）。</param>
    /// <param name="comparedValue">比較値（この値と比較して条件判定）。</param>
    /// <param name="comparisonType">比較方法（等しい・より大きい 等）。</param>
    /// <param name="min">スライダー最小値。</param>
    /// <param name="max">スライダー最大値。</param>
    /// <param name="styleType">スライダー種類（既定/小数/整数）。</param>
    public DrawIfRangeAttribute(string comparedPropertyName, object comparedValue, ComparisonType comparisonType, float min, float max, StyleType styleType)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.comparisonType = comparisonType;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
    }
}
