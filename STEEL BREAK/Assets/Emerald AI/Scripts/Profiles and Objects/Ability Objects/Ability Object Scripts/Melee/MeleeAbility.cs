using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【MeleeAbility】
    /// 近接攻撃（Melee）アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・攻撃角度・距離判定
    /// ・ノックバック／スタン／ダメージ（DoT含む）
    /// をまとめて管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "近接アビリティ", menuName = "Emerald AI/アビリティ/近接アビリティ")]
    public class MeleeAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("近接攻撃の各種設定（角度/距離/ヒット演出 等）")]
        public AbilityData.MeleeData MeleeSettings;

        [Header("ノックバック設定（有効/確率/力/時間 等）")]
        public AbilityData.KnockbackData KnockbackSettings;

        [Header("スタン付与設定（有効/確率/時間 等）")]
        public AbilityData.StunnedData StunnedSettings;

        [Header("ダメージ設定（基礎ダメージ/DoT/クリティカル 等）")]
        public AbilityData.DamageData DamageSettings;

        /// <summary>
        /// アビリティのチャージ処理（チャージエフェクトの再生など）。
        /// </summary>
        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        /// <summary>
        /// アビリティの実行処理。
        /// 攻撃角度・距離・武器コリジョンの状態を確認し、条件を満たす場合にダメージ／効果を適用します。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            EmeraldWeaponCollision WeaponCollision = EmeraldComponent.CombatComponent.CurrentWeaponCollision;
            Transform Target = EmeraldComponent.CombatTarget;
            float TargetAngle = EmeraldComponent.CombatComponent.TargetAngle;
            float TargetDistance = EmeraldComponent.CombatComponent.DistanceFromTarget;

            // ダメージ角度や距離が条件を満たさない場合、または武器コライダーが既に有効な場合、あるいはターゲットが存在しない場合は終了。
            if (TargetAngle > MeleeSettings.MaxDamageAngle || TargetDistance > MeleeSettings.MaxDamageDistance || WeaponCollision != null || Target == null) return;

            var m_ICombat = Target.GetComponentInParent<ICombat>();

            // ノックバックが有効なら、確率でノックバックを適用
            if (KnockbackSettings.Enabled && KnockbackSettings.RollForKnockback())
            {
                Vector3 Direction = m_ICombat.TargetTransform().position - Owner.transform.position;
                if (m_ICombat != null) Owner.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(KnockbackSettings.KnockbackSequence(Direction, m_ICombat.TargetTransform(), m_ICombat));
            }

            // スタンが有効なら、確率でスタンを付与
            if (StunnedSettings.Enabled && StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合はここで終了
            if (!DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = DamageSettings.GenerateCritHit();
                m_IDamageable.Damage(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking() && !m_ICombat.IsDodging()) AbilityData.SpawnEffectAndSound(Owner, Target.GetComponent<ICombat>().DamagePosition(), MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
            }
            else
            {
                Debug.Log(Target.gameObject + " には IDamageable コンポーネントがありません。追加してください。");
            }
        }

        /// <summary>
        /// 【MeleeDamage】
        /// EmeraldWeaponCollision によるターゲット衝突判定時に呼ばれます。
        /// （本アビリティは EmeraldWeaponCollision スクリプトに依存します。）
        /// </summary>
        public void MeleeDamage(GameObject Owner, GameObject Target, Transform TargetRoot)
        {
            // ターゲットがテレポート中の場合は処理しない
            if (TargetRoot.transform.localScale == Vector3.one * 0.003f) return;

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();
            ICombat m_ICombat = TargetRoot.GetComponent<ICombat>();

            // スタンが有効なら、確率でスタンを付与
            if (StunnedSettings.Enabled && StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(StunnedSettings.StunLength);
            }

            // ダメージが無効化されている場合は終了
            if (!DamageSettings.Enabled) return;

            if (m_LocationBasedDamageArea == null)
            {
                var m_IDamageable = TargetRoot.GetComponent<IDamageable>();

                if (m_IDamageable != null)
                {
                    bool IsCritHit = DamageSettings.GenerateCritHit();
                    m_IDamageable.Damage(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                    DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                    EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                    if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking()) AbilityData.SpawnEffectAndSound(Owner, Target.GetComponent<ICombat>().DamagePosition(), MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
                }
                else
                {
                    Debug.Log(Target.gameObject + " には IDamageable コンポーネントがありません。追加してください。");
                }
            }
            else if (m_LocationBasedDamageArea != null)
            {
                bool IsCritHit = DamageSettings.GenerateCritHit();
                m_LocationBasedDamageArea.DamageArea(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking() && !m_ICombat.IsDodging()) AbilityData.SpawnEffectAndSound(Owner, Target.transform.position, MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
            }
        }

        /// <summary>
        /// 【GetTargetRoot】
        /// ターゲットの Root Transform を取得します。
        /// ICombat の参照取得や、ヒット済みターゲットの追跡に使用します。
        /// </summary>
        public Transform GetTargetRoot(GameObject Target)
        {
            Transform TargetTransform = null;
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();
            if (m_LocationBasedDamageArea != null && m_LocationBasedDamageArea.EmeraldComponent.transform.localScale == Vector3.one * 0.003f || Target.transform.localScale == Vector3.one * 0.003f) return null;
            var m_ICombat = Target.GetComponentInParent<ICombat>();
            if (m_ICombat != null) TargetTransform = m_ICombat.TargetTransform();
            return TargetTransform;
        }
    }
}
