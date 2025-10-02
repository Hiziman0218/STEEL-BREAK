using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【BulletProjectileAbility】
    /// 弾丸（Bullet）系プロジェクタイルのアビリティ定義。
    /// ・チャージ演出
    /// ・弾速／拡散／発射間隔
    /// ・ノックバック／スタン／ダメージ
    /// などを一括で管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "弾丸プロジェクタイル アビリティ", menuName = "Emerald AI/アビリティ/弾丸プロジェクタイル アビリティ")]
    public class BulletProjectileAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("弾丸プロジェクタイル設定（本数/発射間隔/拡散/弾速 等）")]
        public AbilityData.BulletProjectileData BulletProjectileSettings;

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
        /// アビリティの実行処理：発射コルーチンを開始します。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            OwnerMonoBehaviour.StartCoroutine(SpawnProjectiles(Owner, AttackTransform, BulletProjectileSettings.TimeBetweenBullets));
        }

        /// <summary>
        /// 弾丸を複数生成して順次発射します。
        /// </summary>
        IEnumerator SpawnProjectiles(GameObject Owner, Transform AttackTransform, float Delay)
        {
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);

            yield return new WaitForSeconds(0.005f);

            for (int i = 0; i < BulletProjectileSettings.TotalBullets; i++)
            {
                EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
                if (EmeraldComponent != null)
                {
                    if (EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit || EmeraldComponent.AnimationComponent.IsTurning || EmeraldComponent.AnimationComponent.IsDead) { yield break; }
                    ;
                }

                Vector3 SpawnPosition = AttackTransform.position;
                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(BulletProjectileSettings.BulletObject, SpawnPosition, AttackTransform.rotation);
                SpawnedProjectile.transform.localScale = BulletProjectileSettings.BulletObject.transform.localScale;
                SpawnedProjectile.name = BulletProjectileSettings.BulletObject.name;

                // 発射音は、発射間隔（Delay）が 0 の場合は最初の 1 回だけ再生します。
                // これにより、1 回の発射で不要に複数回再生されることを防ぎます。
                if (Delay == 0 && i == 0) BulletProjectileSettings.SpawnBulletEffect(Owner, SpawnedProjectile.transform.position, EmeraldComponent.CombatComponent.CurrentAttackTransform);
                else if (Delay > 0) BulletProjectileSettings.SpawnBulletEffect(Owner, SpawnedProjectile.transform.position, EmeraldComponent.CombatComponent.CurrentAttackTransform);

                AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }
        }

        /// <summary>
        /// 生成したプロジェクタイルへ BulletProjectile スクリプトを割り当てます。
        /// </summary>
        public BulletProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var bulletProjectile = SpawnedProjectile.GetComponent<BulletProjectile>();
            if (bulletProjectile == null) bulletProjectile = SpawnedProjectile.AddComponent<BulletProjectile>();
            bulletProjectile.enabled = true;
            return bulletProjectile;
        }
    }
}
