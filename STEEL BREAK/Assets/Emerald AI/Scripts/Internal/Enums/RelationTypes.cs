namespace EmeraldAI
{
    /// <summary>
    /// 【RelationTypes】
    /// 派閥・対象との関係性を表す列挙体です。
    /// AIの敵対/中立/友好判定や、振る舞い分岐の条件として使用します。
    /// </summary>
    public enum RelationTypes
    {
        /// <summary>
        /// 敵対（Enemy）。攻撃対象として扱う関係。
        /// </summary>
        Enemy = 0,

        /// <summary>
        /// 中立（Neutral）。原則として敵対もしないし、支援もしない関係。
        /// </summary>
        Neutral = 1,

        /// <summary>
        /// 友好（Friendly）。支援・同陣営として扱う関係。
        /// </summary>
        Friendly = 2
    }
}
