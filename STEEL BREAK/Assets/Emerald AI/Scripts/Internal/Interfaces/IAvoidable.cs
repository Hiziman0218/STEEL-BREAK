using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【IAvoidable】
    /// AI が「回避すべき対象（オブジェクト／アビリティ）」を検出できるようにするためのインターフェイス。
    /// AbilityTarget は、そのアビリティが「誰（どの Transform）を狙っているか」を表します。
    /// ※インターフェイスのため、インスペクタに表示されるフィールドは持たず、[Header] 属性の付与対象もありません。
    /// </summary>
    public interface IAvoidable
    {
        /// <summary>
        /// このアビリティが意図するターゲット（Transform）。
        /// AI はこの値を参照して、回避対象かどうかを判断します。
        /// </summary>
        Transform AbilityTarget { get; set; }
    }
}
