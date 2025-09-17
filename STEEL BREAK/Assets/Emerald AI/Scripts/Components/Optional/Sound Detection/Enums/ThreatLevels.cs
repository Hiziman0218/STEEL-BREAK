namespace EmeraldAI.SoundDetection
{
    // 【列挙体の概要】ThreatLevels：
    //  Emerald のサウンド検知や警戒ロジックにおける「脅威レベル（知覚段階）」を表す区分です。
    //  AI が音/移動を検知していない「未感知」→気配に反応する「疑念」→完全に警戒した「警戒」と段階的に遷移します。

    /// <summary>
    /// （日本語）ThreatLevels：AI の知覚状態（脅威度）を示す列挙体。
    /// 未感知（Unaware）→ 疑念（Suspicious）→ 警戒（Aware）の順で強度が上がります。
    /// </summary>
    public enum ThreatLevels
    {
        Unaware,    // 未感知：脅威なし。通常行動（徘徊/待機）を行う段階
        Suspicious, // 疑念   ：不審を感じて注意を向ける段階（リアクション1回発火など）
        Aware,      // 警戒   ：脅威を強く認識した段階（戦闘移行や追跡等のトリガ）
    }
}
