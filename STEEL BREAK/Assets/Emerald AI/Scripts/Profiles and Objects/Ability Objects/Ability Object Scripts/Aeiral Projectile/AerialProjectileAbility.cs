using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "空中プロジェクタイル アビリティ", menuName = "Emerald AI/アビリティ/空中プロジェクタイル アビリティ")]
    public class AerialProjectileAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("プロジェクタイル全般の設定（エフェクト/速度/タイムアウト等）")]
        public AbilityData.ProjectileData ProjectileSettings;

        [Header("ホーミング設定（有効/速度/追尾時間 等）")]
        public AbilityData.HomingData HomingSettings;

        [Header("空中プロジェクタイル設定（上空生成、半径/高さ/本数/射出角 等）")]
        public AbilityData.AerialProjectileData AerialProjectileSettings;

        [Header("ターゲット種別の設定（単体/複数ランダム 等）")]
        public AbilityData.TargetTypeData TargetTypeSettings;

        [Header("コライダー設定（レイヤー/半径/衝突タイムアウト 等）")]
        public AbilityData.ColliderData ColliderSettings;

        [Header("スタン付与設定（確率/時間 等）")]
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
        /// アビリティの実行処理。ターゲット取得、生成エフェクトの再生、プロジェクタイル生成コルーチンの開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, TargetTypeSettings.TargetType);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(SpawnAerialProjectiles(Owner, Target, AerialProjectileSettings.TimeBetweenProjectiles));
        }

        /// <summary>
        /// 空中プロジェクタイルを複数生成して順次射出します。
        /// </summary>
        IEnumerator SpawnAerialProjectiles(GameObject Owner, Transform Target, float Delay)
        {
            yield return new WaitForSeconds(0.5f);

            if (Target == null) Target = Owner.GetComponent<EmeraldSystem>().CombatTarget;
            // 現在のターゲット取得後も null の場合、アビリティを中断
            if (Target == null) yield break;

            // 有効な場合、上空エフェクトを生成
            AerialProjectileSettings.SpawnAerialEffect(Owner, Target);

            // アビリティ呼出時点の基準位置（上空生成の中心）を取得
            Vector3 StartingPosition = GetStartingPosition(Owner, Target);

            yield return new WaitForSeconds(0.25f);

            // 黄金角（均等配置用）
            float theta = Mathf.PI * (3 - Mathf.Sqrt(5));

            for (int i = 0; i < AerialProjectileSettings.TotalProjectiles; i++)
            {
                // ターゲット種別が「複数ランダム」の場合、毎回ターゲットを取り直す
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies)
                {
                    Target = GetTarget(Owner, TargetTypeSettings.TargetType);
                    if (AerialProjectileSettings.SpawnSource != AbilityData.AerialProjectileData.SpawnSources.AboveSelf)
                        AerialProjectileSettings.SpawnAerialEffect(Owner, Target);
                }

                // 均等に分布させたスポーン位置を算出
                Vector3 SpawnPosition = GetSpawnPosition(StartingPosition, theta, i);

                // プロジェクタイルの生成（Object Pool から）
                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;
                SpawnedProjectile.transform.LookAt(Vector3.down);

                // 初期の射出角をランダムに設定
                Vector3 AimDir = new Vector3(90 + Random.Range(-AerialProjectileSettings.LaunchAngle, AerialProjectileSettings.LaunchAngle), Random.Range(0, 360), 0);
                SpawnedProjectile.transform.eulerAngles = AimDir;

                // AerialProjectile スクリプトを割り当て、初期化
                AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }

            yield return new WaitForSeconds(0f);
        }

        /// <summary>
        /// 新しく生成されたプロジェクタイルに AerialProjectile スクリプトを割り当てます。
        /// </summary>
        public AerialProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var aerialProjectile = SpawnedProjectile.GetComponent<AerialProjectile>();
            if (aerialProjectile == null) aerialProjectile = SpawnedProjectile.AddComponent<AerialProjectile>();
            aerialProjectile.enabled = true;
            return aerialProjectile;
        }

        /// <summary>
        /// アビリティ呼び出し時の開始位置を取得します（開始後にオブジェクトが移動しても、生成中心がぶれないようにするため）。
        /// </summary>
        Vector3 GetStartingPosition(GameObject Owner, Transform Target)
        {
            if (AerialProjectileSettings.SpawnSource == AbilityData.AerialProjectileData.SpawnSources.AboveTarget)
            {
                return Target.transform.position;
            }
            else
            {
                return Owner.transform.position;
            }
        }

        /// <summary>
        /// インデックスと総数に応じて均等に分布するスポーン位置を算出します。
        /// </summary>
        Vector3 GetSpawnPosition(Vector3 StartingPosition, float theta, int Index)
        {
            float r = (AerialProjectileSettings.Radius) * Mathf.Sqrt(Index) / Mathf.Sqrt(AerialProjectileSettings.TotalProjectiles);
            float a = theta * Index;
            return StartingPosition + new Vector3(Mathf.Cos(a) * r, AerialProjectileSettings.HeightOffset, Mathf.Sin(a) * r);
        }
    }
}
