namespace EmeraldAI
{
    /// <summary>
    /// 【PickTargetTypes】
    /// ターゲットの選択方式を表す列挙体です。
    /// AIが検知した候補の中から、どのターゲットを採用するかの基準を指定します。
    /// </summary>
    public enum PickTargetTypes
    {
        /// <summary>
        /// 最も近いターゲットを選択します。
        /// </summary>
        Closest = 0,

        /// <summary>
        /// 最初に検知したターゲットを選択します。
        /// </summary>
        FirstDetected = 1,

        /// <summary>
        /// 候補の中からランダムに選択します。
        /// </summary>
        Random = 2
    }
}
