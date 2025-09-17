namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// （日本語）ReactionTypes：サウンド検知／誘引（Attract）時などに実行される「反応の種類」を表す列挙体。
    /// ここでの数値（0, 25, 50, …）は主にエディタ上の並び制御や整理のために用いられる識別値です。
    /// </summary>
    // 【列挙体の概要】ReactionTypes：
    //  ・EmeraldSoundDetector / AttractModifier などが、この種類に応じて具体的な処理を選択します。
    //  ・各リアクションで使う値（IntValue1/IntValue2/FloatValue/StringValue/BoolValue/SoundRef/MovementState 等）は
    //    Reaction.cs の説明どおり、種類に応じて意味が切り替わります。
    public enum ReactionTypes
    {
        None = 0,                           // 何もしない（デフォルト）
        AttractModifier = 25,               // AttractModifier 経由の専用リアクション（誘引元を基準に動作）
        DebugLogMessage = 50,               // Unity コンソールへメッセージ出力（StringValue を使用）
        Delay = 75,                         // 次のリアクション実行を遅延（FloatValue=遅延秒）
        EnterCombatState = 100,             // 戦闘状態へ移行
        ExitCombatState = 125,              // 非戦闘状態へ移行
        ExpandDetectionDistance = 150,      // 検知距離を一時的に拡張（IntValue1=加算距離）
        FleeFromLoudestTarget = 162,        // 最も騒音が大きいターゲットから逃走（臆病AI向け）
        LookAtLoudestTarget = 175,          // 最も騒音が大きいターゲットの方向を見る（IntValue1=注視秒）
        MoveAroundCurrentPosition = 200,    // 現在位置を基準に巡回（半径=IntValue1, 総数=IntValue2, 待機=FloatValue, 到達待ち=BoolValue）
        MoveAroundLoudestTarget = 225,      // “最大騒音ターゲット”周辺を巡回（半径/数/待機/到達待ちは上と同様）
        MoveToLoudestTarget = 250,          // “最大騒音ターゲット”へ移動（待機=FloatValue, 到達まで次を遅延=BoolValue）
        SetLoudestTargetAsCombatTarget = 260, // “最大騒音ターゲット”を戦闘ターゲットに設定
        PlayEmoteAnimation = 275,           // エモートアニメーション再生（IntValue1=Emote ID）
        PlaySound = 300,                    // 効果音を再生（SoundRef=AudioClip, FloatValue=音量）
        ResetAllToDefault = 325,            // すべて初期状態へ戻す（LookAt/検知距離/移動状態/戦闘状態）
        ResetDetectionDistance = 350,       // 検知距離を初期値へリセット
        ResetLookAtPosition = 375,          // Look At 位置を初期値へリセット
        ReturnToStartingPosition = 400,     // 開始位置へ戻る
        SetMovementState = 425,             // 移動状態を設定（MovementState を使用）
    }
}
