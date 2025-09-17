using UnityEngine;  // Unity の基本API

namespace EmeraldAI
{
    /// <summary>
    /// 【AnimationClass】
    /// アニメーションの再生速度・参照クリップ・左右反転の有無をまとめて扱うシリアライズ用データクラス。
    /// Animator のステート遷移や外部設定から、1セットの再生パラメータとして利用する想定。
    /// </summary>
    [System.Serializable]
    public class AnimationClass
    {
        /// <summary>
        /// コンストラクタ：再生速度、使用する AnimationClip、左右反転の有無を受け取って初期化します。
        /// </summary>
        public AnimationClass(float NewAnimationSpeed, AnimationClip NewAnimationClip, bool NewMirror)
        {
            AnimationSpeed = NewAnimationSpeed;   // 引数の再生速度を反映
            AnimationClip = NewAnimationClip;     // 使用するクリップ参照を反映
            Mirror = NewMirror;                   // 左右反転の有無を反映
        }

        //[Header("アニメーションの再生速度（1=通常、0.5=半分、2=倍速 など）")]
        public float AnimationSpeed = 1;          // 既定値は1（等速再生）

        //[Header("再生する AnimationClip（モーション本体の参照）")]
        public AnimationClip AnimationClip;       // ここに再生対象のクリップを割り当てる

        [Header("アニメーションを左右反転（Mirror）して再生するかどうか")]
        public bool Mirror = false;               // trueで左右反転、falseで通常（既定）
    }
}
