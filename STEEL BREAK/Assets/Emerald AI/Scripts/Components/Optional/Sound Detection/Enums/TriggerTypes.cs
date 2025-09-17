namespace EmeraldAI.SoundDetection
{
    // 【列挙体の概要】TriggerTypes：
    //  AttractModifier などの「誘引の発火条件」を表す区分。
    //  いつリアクション（ReactionObject）を起動するかを、この列挙値で指定します。

    /// <summary>
    /// （日本語）アトラクト（誘引）やサウンド検知の「発火タイミング」を表す列挙体。
    /// </summary>
    public enum TriggerTypes
    {
        /// <summary>（日本語）コンポーネントの Start 時に発火。</summary>
        OnStart = 0,

        /// <summary>（日本語）Trigger コライダー侵入時に発火。</summary>
        OnTrigger = 5,

        /// <summary>（日本語）通常の衝突（非 Trigger）時に発火。</summary>
        OnCollision = 10,

        /// <summary>（日本語）スクリプトから明示的に呼び出したときに発火（例：ActivateAttraction）。</summary>
        OnCustomCall = 15,
    }
}
