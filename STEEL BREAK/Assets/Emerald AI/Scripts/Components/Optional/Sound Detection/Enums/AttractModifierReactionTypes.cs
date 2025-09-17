namespace EmeraldAI.SoundDetection
{
    // 【列挙体の概要】AttractModifierReactionTypes：
    //  AttractModifier（誘引トリガ）に対して、AI がどの種類の反応をするかを定義する列挙体。
    //  値（0, 25, 50）はエディタ上の並び・重み付け調整などに用いられる想定で、ゲーム挙動の区別子です。
    public enum AttractModifierReactionTypes
    {
        LookAtAttractSource = 0,        // 誘引元（音源など）の方向を「見る」
        MoveAroundAttractSource = 25,   // 誘引元の「周囲を巡回」する（半径・ポイント数はリアクション側の設定に依存）
        MoveToAttractSource = 50,       // 誘引元へ「移動」する（到達待機などの挙動はリアクション側の設定に依存）
    }
}
