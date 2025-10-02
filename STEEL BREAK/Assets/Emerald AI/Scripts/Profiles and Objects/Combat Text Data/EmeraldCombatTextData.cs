using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 【EmeraldCombatTextData】
    /// コンバットテキスト（ダメージ数値・回復量など）の外観と挙動をまとめて設定する ScriptableObject。
    /// インスペクターでの可読性向上のため、各メンバーに日本語の [Header] を付与しています。
    /// </summary>
    [System.Serializable]
    public class EmeraldCombatTextData : ScriptableObject
    {
        //[Header("コンバットテキストの有効/無効（Enabled=表示、Disabled=非表示）")]
        public enum CombatTextStateEnum { Enabled, Disabled };
        public CombatTextStateEnum CombatTextState = CombatTextStateEnum.Disabled;

        [Header("プレイヤーが与えたダメージのテキスト色")]
        public Color PlayerTextColor = Color.white;

        [Header("プレイヤーのクリティカルヒット時のテキスト色")]
        public Color PlayerCritTextColor = Color.red;

        [Header("プレイヤーがダメージを受けた際のテキスト色")]
        public Color PlayerTakeDamageTextColor = Color.red;

        [Header("AI（敵/味方）の通常ダメージ表示テキスト色")]
        public Color AITextColor = Color.white;

        [Header("AI のクリティカルヒット時のテキスト色")]
        public Color AICritTextColor = Color.red;

        [Header("回復量テキストの色")]
        public Color HealingTextColor = Color.green;

        [Header("テキストに使用するフォント")]
        public Font TextFont;

        [Header("基準のフォントサイズ")]
        public int FontSize = 20;

        [Header("フォントサイズ拡大時の最大倍率（相対値）")]
        public int MaxFontSize = 6;

        //[Header("テキストのアニメーションタイプ（跳ねる/上昇/放射/V1/V2/静止）")]
        public enum AnimationTypeEnum { Bounce, Upwards, OutwardsV1, OutwardsV2, Stationary };
        public AnimationTypeEnum AnimationType = AnimationTypeEnum.Bounce;

        //[Header("表示対象（プレイヤーとAI/プレイヤーのみ/AIのみ）")]
        public enum CombatTextTargetEnum { PlayerAndAI, PlayerOnly, AIOnly };
        public CombatTextTargetEnum CombatTextTargets = CombatTextTargetEnum.PlayerAndAI;

        //[Header("アウトライン効果の有効/無効")]
        public enum OutlineEffectEnum { Enabled, Disabled };
        public OutlineEffectEnum OutlineEffect = OutlineEffectEnum.Enabled;

        //[Header("フォントサイズのアニメーション有効/無効")]
        public enum UseAnimateFontSizeEnum { Enabled, Disabled };
        public UseAnimateFontSizeEnum UseAnimateFontSize = UseAnimateFontSizeEnum.Disabled;

        [Header("テキストを表示する高さ（キャラクター位置からのYオフセット, メートル）")]
        public float DefaultHeight = 1.75f;

        [Header("VR向け補正の有効/無効（VR Support）")]
        public bool VRSupport = false;
    }
}
