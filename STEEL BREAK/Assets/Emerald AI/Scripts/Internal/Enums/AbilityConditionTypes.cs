namespace EmeraldAI
{
    /// <summary>
    /// 【ConditionTypes】
    /// 条件判定の種類を示す列挙体。AIの振る舞い分岐などで参照します。
    /// </summary>
    public enum ConditionTypes
    {
        /// <summary>
        /// 自身の体力が低い（Self Low Health）
        /// </summary>
        SelfLowHealth,

        /// <summary>
        /// 味方の体力が低い（Ally Low Health）
        /// </summary>
        AllyLowHealth,

        /// <summary>
        /// ターゲットとの距離（Distance From Target）
        /// </summary>
        DistanceFromTarget,

        /// <summary>
        /// 現在アクティブな召喚体が存在しない（No Current Summons）
        /// </summary>
        NoCurrentSummons,
    }
}
