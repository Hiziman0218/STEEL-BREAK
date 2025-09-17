using UnityEngine;  // Unity の基本API

namespace EmeraldAI
{
    /// <summary>
    /// 【LocationBasedDamageArea（部位別ダメージ用ヒットエリア）】
    /// AIの「部位別ダメージ」機能と連動するコライダー領域用コンポーネント。
    /// ・この部位に攻撃が命中したとき、ダメージに倍率（DamageMultiplier）を適用して与える。
    /// ・EmeraldSystem（本体側）の位置に Location Based Damage スクリプトをアタッチし、
    ///   そのコンポーネントの「Get Colliders」ボタンで各コライダーを取得しておくこと。
    /// </summary>
    public class LocationBasedDamageArea : MonoBehaviour
    {
        [Header("この部位に命中したときのダメージ倍率（1で等倍、>1で増加、<1で減少）。HideInInspectorのためインスペクタ非表示")]
        [HideInInspector] public float DamageMultiplier = 1;

        [Header("このヒットエリアが属しているAI本体（EmeraldSystem）への参照。HideInInspectorのためインスペクタ非表示")]
        [HideInInspector] public EmeraldSystem EmeraldComponent;

        /// <summary>
        /// 【DamageArea】
        /// 部位別ダメージ用コンポーネントに対してダメージを与え、受けたダメージに倍率を適用します。
        /// パラメータは EmeraldAISystem の Damage 関数と同一です。
        /// 部位別ダメージ（Location Based Damage）機能を利用したい場合は、この関数を呼び出してください。
        /// 事前条件：
        /// ・ダメージ対象のAI本体（EmeraldAISystem がある場所）に Location Based Damage スクリプトがアタッチされていること
        /// ・Location Based Damage コンポーネントで「Get Colliders」ボタンを押し、コライダーを取得済みであること
        /// </summary>
        public void DamageArea(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 0, bool CriticalHit = false)
        {
            int DamageDealt = DamageAmount;                                              // 最終的に与えるダメージ
            if (DamageMultiplier > 1) DamageDealt = Mathf.RoundToInt(DamageAmount * DamageMultiplier); // 倍率適用（>1 のとき）
            IDamageable m_IDamageable = EmeraldComponent.GetComponent<IDamageable>();    // ダメージ適用インターフェースを取得
            m_IDamageable.Damage(DamageDealt, AttackerTransform, RagdollForce, CriticalHit); // ダメージを与える

            // 防御（IsBlocking）でなければ、ヒットエフェクトを生成
            if (!EmeraldComponent.AnimationComponent.IsBlocking) CreateImpactEffect(transform.position, EmeraldComponent.HealthComponent.AttachHitEffects);

            // 体力が0以下になった場合、この部位（Transform）をラグドール化の基準Transformとして設定
            if (m_IDamageable.Health <= 0)
                EmeraldComponent.CombatComponent.RagdollTransform = transform;
        }

        /// <summary>
        /// 【CreateImpactEffect】
        /// 指定した座標（ImpactPosition）にヒットエフェクトを生成します。
        /// 生成するエフェクトは、AI本体の「Hit Effects List」（設定：Settings > Combat > Hit Effect）からランダムに選択されます。
        /// </summary>
        public void CreateImpactEffect(Vector3 ImpactPosition, bool SetAIAsEffectParent = true)
        {
            if (EmeraldComponent.HealthComponent.UseHitEffect == YesOrNo.Yes && EmeraldComponent.HealthComponent.HitEffectsList.Count > 0)
            {
                GameObject RandomBloodEffect = EmeraldComponent.HealthComponent.HitEffectsList[Random.Range(0, EmeraldComponent.HealthComponent.HitEffectsList.Count)];
                if (RandomBloodEffect != null)
                {
                    GameObject SpawnedBlood = EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(
                        RandomBloodEffect,
                        ImpactPosition,
                        Quaternion.LookRotation(transform.forward, Vector3.up),
                        EmeraldComponent.HealthComponent.HitEffectTimeoutSeconds
                    ) as GameObject;

                    // エフェクトのParent設定：AI本体にぶら下げるか、共通オブジェクトプール配下に置くか
                    if (SetAIAsEffectParent)
                        SpawnedBlood.transform.SetParent(transform);
                    else
                        SpawnedBlood.transform.SetParent(EmeraldSystem.ObjectPool.transform);
                }
            }
        }
    }
}
