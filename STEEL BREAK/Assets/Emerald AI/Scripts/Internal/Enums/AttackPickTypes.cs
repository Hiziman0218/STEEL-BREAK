namespace EmeraldAI
{
    /// <summary>
    /// 【AttackPickTypes】
    /// AIが複数の攻撃候補（アニメ・アビリティ等）から「どの攻撃を使うか」を決める
    /// 選択方式を表す列挙体です。通常は AttackClass/AttackData と併用します。
    /// </summary>
    public enum AttackPickTypes
    {
        /// <summary>
        /// 重み（確率）に基づく抽選方式。
        /// 例：各攻撃の「抽選率（%）」に応じて重み付きランダムで選択。
        /// </summary>
        Odds,

        /// <summary>
        /// 定義順に順番で選択する方式。
        /// 例：先頭→次→…→末尾→先頭…のように周回します。
        /// </summary>
        Order,

        /// <summary>
        /// 一様ランダムに選択する方式。
        /// 例：候補から等確率で1つを選びます（重みは無視）。
        /// </summary>
        Random
    };
}
