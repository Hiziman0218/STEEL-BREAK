using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【AreaOfEffect】
    /// 範囲攻撃（Area of Effect, AoE）を実行するコンポーネント。
    /// ・オーナー（発動者）の DetectionLayerMask に一致するレイヤーのみを対象に検出
    /// ・派閥関係が「Enemy」の相手に対してのみ効果を適用
    /// ・ヒットエフェクト、ノックバック、スタン、ダメージ（DoT含む）に対応
    /// </summary>
    public class AreaOfEffect : MonoBehaviour
    {
        [Header("AoE 対象とするレイヤーマスク（敵レイヤー）。初期値は AI の DetectionLayerMask を使用")]
        public LayerMask Enemies;

        [Header("現在の範囲攻撃アビリティデータ（AreaOfEffectAbility）")]
        AreaOfEffectAbility CurrentAbilityData;

        [Header("この AoE を発動した所有者（発動元の GameObject）")]
        GameObject Owner;

        [Header("所有者の EmeraldSystem 参照（検知や派閥判定に使用）")]
        EmeraldSystem EmeraldComponent;

        /// <summary>
        /// AoE を初期化します。
        /// </summary>
        public void Initialize(GameObject owner, Transform AttackTransform, AreaOfEffectAbility abilityData)
        {
            EmeraldComponent = owner.GetComponent<EmeraldSystem>();
            Enemies = EmeraldComponent.DetectionComponent.DetectionLayerMask;
            CurrentAbilityData = abilityData;
            Owner = owner;
            IntitailizeInternal(Owner, AttackTransform);
        }

        /// <summary>
        /// 内部初期化処理。
        /// </summary>
        void IntitailizeInternal(GameObject Owner, Transform AttackTransform)
        {
            // AI の DetectionLayerMask と同じレイヤーを持つ対象のみを検出する。
            List<Collider> DetectedAOETargets = Physics.OverlapSphere(AttackTransform.position, CurrentAbilityData.AreaOfEffectSettings.Radius, Enemies).ToList();
            // 検出対象にオーナー自身のコライダーが含まれる場合は除外する。
            DetectedAOETargets.Remove(Owner.GetComponent<Collider>());

            for (int i = 0; i < DetectedAOETargets.Count; i++)
            {
                // オーナーから見て派閥関係が「Enemy」の相手にのみ効果を適用する。
                if (EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, DetectedAOETargets[i].transform) == "Enemy")
                {
                    ICombat m_ICombat = DetectedAOETargets[i].GetComponent<ICombat>();

                    // ヒットエフェクトが設定されている場合、回避・防御中でなく、テレポート中でない相手に対して再生する。
                    if (CurrentAbilityData.AreaOfEffectSettings.HitTargetEffect != null)
                    {
                        if (m_ICombat != null && !m_ICombat.IsDodging() && !m_ICombat.IsBlocking() && DetectedAOETargets[i].transform.localScale != Vector3.one * 0.003f)
                            EmeraldObjectPool.SpawnEffect(CurrentAbilityData.AreaOfEffectSettings.HitTargetEffect, DetectedAOETargets[i].GetComponent<ICombat>().DamagePosition(), DetectedAOETargets[i].transform.rotation, CurrentAbilityData.AreaOfEffectSettings.HitTargetEffectTimeoutSeconds);
                    }

                    DamageTarget(DetectedAOETargets[i].gameObject, m_ICombat);
                }
            }
        }

        /// <summary>
        /// ヒット対象にダメージを与えます（IDamageable を持つことが前提）。
        /// </summary>
        void DamageTarget(GameObject Target, ICombat ICombatRef)
        {
            // テレポート中の対象は無視する。
            if (Target.transform.localScale == Vector3.one * 0.003f) return;

            // ノックバックが有効なら、確率でノックバックを適用する。
            if (CurrentAbilityData.KnockbackSettings.Enabled && CurrentAbilityData.KnockbackSettings.RollForKnockback())
            {
                Vector3 Direction = (ICombatRef.TargetTransform().transform.position - Owner.transform.position).normalized;
                if (ICombatRef != null) Owner.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(CurrentAbilityData.KnockbackSettings.KnockbackSequence(Direction, ICombatRef.TargetTransform(), ICombatRef));
            }

            // スタンが有効なら、確率でスタンを付与する。
            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (ICombatRef != null) ICombatRef.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合はここで終了。
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                m_IDamageable.Damage(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);
            }
            else
            {
                // 対象に IDamageable および/または ICombat が存在しない場合の通知。
                Debug.Log(Target.gameObject + " には IDamageable または ICombat コンポーネントがありません。追加してください。");
            }
        }
    }
}
