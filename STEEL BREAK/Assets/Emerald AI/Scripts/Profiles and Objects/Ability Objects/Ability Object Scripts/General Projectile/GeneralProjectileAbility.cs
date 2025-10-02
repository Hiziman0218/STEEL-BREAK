using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【GeneralProjectileAbility】
    /// 汎用プロジェクタイル系アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・発射間隔・弾数・拡散（水平半径／ランダム）などのスポーン設定
    /// ・ホーミング、コライダー、ノックバック、スタン、ダメージ設定 などを一括管理
    /// </summary>
    [CreateAssetMenu(fileName = "汎用プロジェクタイル アビリティ", menuName = "Emerald AI/アビリティ/汎用プロジェクタイル アビリティ")]
    public class GeneralProjectileAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("プロジェクタイル全般の設定（エフェクト/発射遅延/寿命 等）")]
        public AbilityData.ProjectileData ProjectileSettings;

        [Header("汎用プロジェクタイル設定（速度/最大発射角/発射間隔 等）")]
        [UnityEngine.Serialization.FormerlySerializedAs("LinearProjectileSettings")] public AbilityData.GeneralProjectileData GeneralProjectileSettings;

        [Header("ホーミング設定（有効/速度/追尾時間/最小距離 等）")]
        public AbilityData.HomingData HomingSettings;

        [Header("ターゲット種別の設定（現在のターゲット/複数ランダム 等）")]
        public AbilityData.TargetTypeData TargetTypeSettings;

        [Header("拡散（Spread）設定（水平半径/ランダム/角度/距離 等）")]
        public AbilityData.SpreadData SpreadSettings;

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
        /// アビリティの実行処理：生成エフェクト再生後、プロジェクタイル生成コルーチンを開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(SpawnProjectiles(Owner, AttackTransform, GeneralProjectileSettings.TimeBetweenProjectiles));
        }

        /// <summary>
        /// プロジェクタイルを複数生成して順次発射します。
        /// </summary>
        IEnumerator SpawnProjectiles(GameObject Owner, Transform AttackTransform, float Delay)
        {
            Transform Target = GetTarget(Owner, TargetTypeSettings.TargetType);

            for (int i = 0; i < GeneralProjectileSettings.TotalProjectiles; i++)
            {
                EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
                if (EmeraldComponent != null)
                {
                    // 回避中または被弾中は発射を中断
                    if (EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit) yield break;
                }

                // ターゲット種別が「複数ランダム」の場合、生成のたびにターゲットを取り直す
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies) Target = GetTarget(Owner, TargetTypeSettings.TargetType);

                Vector3 SpawnPosition = AttackTransform.position;

                // 水平半径の拡散が有効な場合、発射位置を所有者の頭上へ
                if (SpreadSettings.Enabled && SpreadSettings.SpreadType == AbilityData.SpreadTypes.HorizontalRadius) SpawnPosition = Owner.transform.position + Owner.transform.localScale.y * Vector3.up;

                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

                if (SpreadSettings.Enabled)
                {
                    // 指定角度に基づく均等な拡散（水平半径）
                    if (SpreadSettings.SpreadType == AbilityData.SpreadTypes.HorizontalRadius)
                    {
                        float AnglePerStepX = ((SpreadSettings.SpreadAngleX / 2f) * 2) / (float)GeneralProjectileSettings.TotalProjectiles;
                        SpawnedProjectile.transform.LookAt(Owner.transform.position + Owner.transform.forward);
                        Vector3 AimDir = new Vector3(-SpreadSettings.TiltAngleY, (-(SpreadSettings.SpreadAngleX / 2f) + AnglePerStepX / 2f) + AnglePerStepX * i, 0);
                        SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;
                    }
                    // ランダム拡散オフセット
                    else if (SpreadSettings.SpreadType == AbilityData.SpreadTypes.Random)
                    {
                        float spreadX = Random.Range(SpreadSettings.MinSpreadX, SpreadSettings.MaxSpreadX);
                        float spreadY = Random.Range(SpreadSettings.MinSpreadY, SpreadSettings.MaxSpreadY);
                        if (Target != null) SpawnedProjectile.transform.LookAt(Target.position);
                        Vector3 AimDir = new Vector3(-spreadY, spreadX, 0);
                        SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;
                    }
                }

                AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }
        }

        /// <summary>
        /// 新しく生成したプロジェクタイルへ GeneralProjectile スクリプトを割り当てます。
        /// </summary>
        public GeneralProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var generalProjectile = SpawnedProjectile.GetComponent<GeneralProjectile>();
            if (generalProjectile == null) generalProjectile = SpawnedProjectile.AddComponent<GeneralProjectile>();
            generalProjectile.enabled = true;
            return generalProjectile;
        }
    }
}
