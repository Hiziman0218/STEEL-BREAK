using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【GroundProjectileAbility】
    /// 地上（グラウンド）を這うタイプのプロジェクタイル用アビリティ定義。
    /// ・チャージ/生成演出
    /// ・プロジェクタイル一般設定、地上追従（地形アライン）、ホーミング
    /// ・ターゲット選択、コライダー、ノックバック、スタン、ダメージ
    /// をまとめて管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "地上プロジェクタイル アビリティ", menuName = "Emerald AI/アビリティ/地上プロジェクタイル アビリティ")]
    public class GroundProjectileAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("プロジェクタイル全般の設定（エフェクト/発射遅延/寿命 等）")]
        public AbilityData.ProjectileData ProjectileSettings;

        [Header("地上プロジェクタイル設定（速度/距離/地形アライン/角度拡散 等）")]
        public AbilityData.GroundProjectileData GroundProjectileSettings;

        [Header("ホーミング設定（有効/速度/追尾時間/最小距離 等）")]
        public AbilityData.HomingData HomingSettings;

        [Header("ターゲット種別の設定（現在ターゲット/複数ランダム 等）")]
        public AbilityData.TargetTypeData TargetTypeSettings;

        [Header("コライダー設定（半径/奥行オフセット/レイヤー/衝突タイムアウト 等）")]
        public AbilityData.ColliderData ColliderSettings;

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
        /// アビリティの実行処理：ターゲット取得、生成エフェクト再生、プロジェクタイル生成コルーチン開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, TargetTypeSettings.TargetType);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(SpawnProjectiles(Owner, Target, GroundProjectileSettings.TimeBetweenProjectiles));
        }

        /// <summary>
        /// プロジェクタイルを複数生成して順次発射します。
        /// </summary>
        IEnumerator SpawnProjectiles(GameObject Owner, Transform Target, float Delay)
        {
            for (int i = 0; i < GroundProjectileSettings.TotalProjectiles; i++)
            {
                // ターゲット種別が「複数ランダム」の場合、生成のたびにターゲットを取り直す
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies) Target = GetTarget(Owner, TargetTypeSettings.TargetType);

                Vector3 SpawnPosition = Owner.transform.position;
                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

                // 水平角度の均等拡散
                float AnglePerStepX = ((GroundProjectileSettings.AngleSpread / 2f) * 2) / (float)GroundProjectileSettings.TotalProjectiles;
                SpawnedProjectile.transform.LookAt(Owner.transform.position + Owner.transform.forward);
                Vector3 AimDir = new Vector3(0, (-(GroundProjectileSettings.AngleSpread / 2f) + AnglePerStepX / 2f) + AnglePerStepX * i, 0);
                SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;

                AssignAbilityScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }

            yield return new WaitForSeconds(0f);
        }

        /// <summary>
        /// 新しく生成したプロジェクタイルへ GroundProjectile スクリプトを割り当てます。
        /// </summary>
        public GroundProjectile AssignAbilityScript(GameObject SpawnedAbility)
        {
            var groundProjectile = SpawnedAbility.GetComponent<GroundProjectile>();
            if (groundProjectile == null) groundProjectile = SpawnedAbility.AddComponent<GroundProjectile>();
            return groundProjectile;
        }
    }
}
