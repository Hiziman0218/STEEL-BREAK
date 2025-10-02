using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【ArrowProjectileAbility】
    /// 矢（Arrow）系プロジェクタイルのアビリティ定義。
    /// ・チャージ演出、生成演出
    /// ・プロジェクタイル挙動、ノックバック、スタン、ダメージ などの設定を一括管理
    /// </summary>
    [CreateAssetMenu(fileName = "矢プロジェクタイル アビリティ", menuName = "Emerald AI/アビリティ/矢プロジェクタイル アビリティ")]
    public class ArrowProjectileAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("プロジェクタイル全般の設定（エフェクト/速度/タイムアウト等）")]
        public AbilityData.ProjectileData ProjectileSettings;

        [Header("矢プロジェクタイル固有の設定（速度/付着/その他挙動）")]
        public AbilityData.ArrowProjectileData ArrowProjectileSettings;

        [Header("コライダー設定（半径/レイヤー/衝突タイムアウト等）")]
        public AbilityData.ColliderData ColliderSettings;

        [Header("ノックバック設定（有効/確率/力/時間等）")]
        public AbilityData.KnockbackData KnockbackSettings;

        [Header("スタン付与設定（有効/確率/時間等）")]
        public AbilityData.StunnedData StunnedSettings;

        [Header("ダメージ設定（基礎ダメージ/DoT/クリティカル等）")]
        public AbilityData.DamageData DamageSettings;

        /// <summary>
        /// アビリティのチャージ処理（チャージエフェクトの再生など）。
        /// </summary>
        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        /// <summary>
        /// アビリティの実行処理：生成エフェクト再生後、プロジェクタイル生成を行う。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            SpawnProjectiles(Owner, AttackTransform);
        }

        /// <summary>
        /// プロジェクタイル（矢）を生成し、ターゲットへ向けて初期化する。
        /// </summary>
        void SpawnProjectiles(GameObject Owner, Transform AttackTransform)
        {
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            if (EmeraldComponent != null)
            {
                // 回避中または被弾中は発射しない
                if (EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit) return;
            }

            Vector3 SpawnPosition = AttackTransform.position;
            GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
            SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
            SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

            AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);
        }

        /// <summary>
        /// 生成したプロジェクタイルに ArrowProjectile スクリプトを割り当てる。
        /// </summary>
        public ArrowProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var arrowProjectile = SpawnedProjectile.GetComponent<ArrowProjectile>();
            if (arrowProjectile == null) arrowProjectile = SpawnedProjectile.AddComponent<ArrowProjectile>();
            arrowProjectile.enabled = true;
            return arrowProjectile;
        }
    }
}
