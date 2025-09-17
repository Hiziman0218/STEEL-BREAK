namespace EmeraldAI
{
    /// <summary>
    /// 【YesOrNo】
    /// はい／いいえ を表す単純な列挙体。
    /// インスペクタ上のトグル相当の設定や、条件分岐の明示に利用します。
    /// </summary>
    public enum YesOrNo
    {
        /// <summary>
        /// いいえ（無効・使用しない）
        /// </summary>
        No = 0,

        /// <summary>
        /// はい（有効・使用する）
        /// </summary>
        Yes = 1
    };
}
