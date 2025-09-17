using UnityEngine;  // Unity の基本API

namespace EmeraldAI  // EmeraldAI 用の名前空間
{
    /// <summary>
    /// 【ProjectileEffectsClass】
    /// 弾（Projectile）から利用されるエフェクト参照をキャッシュし、
    /// 必要に応じて有効化／無効化できるように保持するためのデータクラス。
    /// </summary>
    [System.Serializable]  // シリアライズしてインスペクタに表示・保存できるようにする属性
    public class ProjectileEffectsClass
    {
        [Header("弾が使用するパーティクルのレンダラー参照（有効/無効の切替対象）")]
        public ParticleSystemRenderer EffectParticle;  // エフェクト表示に用いる ParticleSystemRenderer

        [Header("エフェクトをまとめたルート GameObject（活性/非活性を切り替える対象）")]
        public GameObject EffectObject;                // 実体となるエフェクトの GameObject

        /// <summary>
        /// コンストラクタ：パーティクルレンダラーとエフェクトオブジェクトを受け取り、各フィールドへ設定します。
        /// </summary>
        public ProjectileEffectsClass(ParticleSystemRenderer m_EffectParticle, GameObject m_EffectObject)
        {
            EffectParticle = m_EffectParticle;  // 受け取ったパーティクルレンダラーを保持
            EffectObject = m_EffectObject;    // 受け取ったエフェクトのルートオブジェクトを保持
        }
    }
}
