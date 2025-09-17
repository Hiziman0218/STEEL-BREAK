namespace EmeraldAI
{
    /// <summary>
    /// 【AnimationStateTypes】
    /// アニメーション状態を表す列挙体。<see cref="System.FlagsAttribute"/> によりビットフラグとして扱われ、
    /// 複数の状態を同時に保持できます（例：Moving | TurningLeft）。
    /// 各値は 1 を左シフトして定義され、相互に重複しないビットを持ちます。
    /// </summary>
    [System.Flags]
    public enum AnimationStateTypes
    {
        /// <summary>
        /// いずれの状態にも該当しない（フラグなし）
        /// </summary>
        None = 0,

        /// <summary>
        /// アイドル（待機）状態
        /// </summary>
        Idling = 1 << 1,

        /// <summary>
        /// 移動中
        /// </summary>
        Moving = 1 << 2,

        /// <summary>
        /// 後退中
        /// </summary>
        BackingUp = 1 << 3,

        /// <summary>
        /// 左旋回中
        /// </summary>
        TurningLeft = 1 << 4,

        /// <summary>
        /// 右旋回中
        /// </summary>
        TurningRight = 1 << 5,

        /// <summary>
        /// 攻撃中
        /// </summary>
        Attacking = 1 << 6,

        /// <summary>
        /// ストレイフ（横移動）中
        /// </summary>
        Strafing = 1 << 7,

        /// <summary>
        /// ガード（ブロック）中
        /// </summary>
        Blocking = 1 << 8,

        /// <summary>
        /// 回避（ドッジ）中
        /// </summary>
        Dodging = 1 << 9,

        /// <summary>
        /// 反動（リコイル）中
        /// </summary>
        Recoiling = 1 << 10,

        /// <summary>
        /// スタン（気絶）中
        /// </summary>
        Stunned = 1 << 11,

        /// <summary>
        /// 被弾中（ヒット反応）
        /// </summary>
        GettingHit = 1 << 12,

        /// <summary>
        /// 武器を装備中（取り出し中など）
        /// </summary>
        Equipping = 1 << 13,

        /// <summary>
        /// 武器の切り替え中
        /// </summary>
        SwitchingWeapons = 1 << 14,

        /// <summary>
        /// 戦闘不能（死亡）状態
        /// </summary>
        Dead = 1 << 15,

        /// <summary>
        /// エモート（感情表現）中
        /// </summary>
        Emoting = 1 << 16,

        /// <summary>
        /// すべてのフラグを含む（ビット反転による全ビットON）
        /// </summary>
        Everything = ~0,
    }
}
