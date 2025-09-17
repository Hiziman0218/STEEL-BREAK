using UnityEngine;                          // Unity の基本API
using System.Collections.Generic;           // List を使用
using UnityEngine.Serialization;            // 旧シリアライズ名との互換に使用（本ファイルでは参照のみ）

namespace EmeraldAI
{
    /// <summary>
    /// 【AnimationParentClass（アニメーション親クラス）】
    /// 多くのアニメーションはカテゴリごと（NonCombat / Type 1 / Type 2）に繰り返し構成されるため、
    /// それぞれのカテゴリ用に共通の親クラスを用意して管理しやすくしています。
    /// 未使用のアニメーションはインスペクタ上に表示されません。
    /// 将来のアップデートで新しいアニメーションを追加する場合も、拡張が容易になります。
    /// </summary>
    [System.Serializable]
    public class AnimationParentClass
    {
        /// <summary>
        /// 非戦闘時のアイドルアニメーション一覧（ループ候補）
        /// </summary>
        [Header("非戦闘時のアイドルアニメーション一覧（ループ候補）")]
        public List<AnimationClass> IdleList = new List<AnimationClass>();   // 非戦闘用の待機アニメ群

        /// <summary>
        /// 足を止めた静止アイドル
        /// </summary>
        [Header("足を止めた静止アイドル（その場待機）")]
        public AnimationClass IdleStationary;                                // その場での待機

        /// <summary>
        /// 警戒アイドル（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("警戒アイドル（Type 1 / Type 2 のみ）")]
        public AnimationClass IdleWarning;                                   // 周囲警戒の待機

        /// <summary>
        /// 歩行アニメーション
        /// </summary>
        [Header("歩行（左・前・右・後ろ）")]
        public AnimationClass WalkLeft, WalkForward, WalkRight, WalkBack;    // 歩行4方向

        /// <summary>
        /// 走行アニメーション
        /// </summary>
        [Header("走行（左・前・右）")]
        public AnimationClass RunLeft, RunForward, RunRight;                 // 走行3方向

        /// <summary>
        /// その場旋回アニメーション
        /// </summary>
        [Header("その場旋回（左・右）")]
        public AnimationClass TurnLeft, TurnRight;                           // 旋回2方向

        /// <summary>
        /// 被弾アニメーション一覧
        /// </summary>
        [Header("被弾アニメーション一覧")]
        public List<AnimationClass> HitList = new List<AnimationClass>();    // 被弾時の反応

        /// <summary>
        /// 死亡アニメーション一覧
        /// </summary>
        [Header("死亡アニメーション一覧")]
        public List<AnimationClass> DeathList = new List<AnimationClass>();  // 死亡演出

        /// <summary>
        /// ストレイフ（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("ストレイフ横移動（左・右）※Type 1 / Type 2 のみ")]
        public AnimationClass StrafeLeft, StrafeRight;                       // 横移動

        /// <summary>
        /// ガード（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("ガード（待機・被弾）※Type 1 / Type 2 のみ")]
        public AnimationClass BlockIdle, BlockHit;                           // 盾受け等

        /// <summary>
        /// 回避（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("回避（左・後・右）※Type 1 / Type 2 のみ")]
        public AnimationClass DodgeLeft, DodgeBack, DodgeRight;              // ステップ回避

        /// <summary>
        /// 反動（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("反動/のけぞり ※Type 1 / Type 2 のみ")]
        public AnimationClass Recoil;                                        // 反動

        /// <summary>
        /// スタン（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("スタン（気絶）※Type 1 / Type 2 のみ")]
        public AnimationClass Stunned;                                       // 行動不能

        /// <summary>
        /// カバー（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("カバー（掩体待機・被弾）※Type 1 / Type 2 のみ")]
        public AnimationClass CoverIdle, CoverHit;                           // 掩体動作

        /// <summary>
        /// 武器の収納/抜刀（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("武器の収納・抜刀（PutAway / PullOut）※Type 1 / Type 2 のみ")]
        public AnimationClass PutAwayWeapon, PullOutWeapon;                  // 武器出し入れ

        /// <summary>
        /// 攻撃アニメーション一覧（Type 1 / Type 2 のみ）
        /// </summary>
        [Header("攻撃アニメーション一覧（Type 1 / Type 2 のみ）")]
        public List<AnimationClass> AttackList = new List<AnimationClass>(); // 攻撃手段の候補
    }
}
