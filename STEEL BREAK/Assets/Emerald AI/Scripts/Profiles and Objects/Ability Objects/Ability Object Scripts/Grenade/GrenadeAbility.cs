using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【GrenadeAbility】
    /// グレネード系アビリティの定義用 ScriptableObject。
    /// ・投擲/爆発設定
    /// ・ノックバック/スタン/ダメージ設定
    /// をまとめて管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "グレネード アビリティ", menuName = "Emerald AI/アビリティ/グレネード アビリティ")]
    public class GrenadeAbility : EmeraldAbilityObject
    {
        [Header("グレネード設定（プレハブ/爆発半径/爆発時間/回転/レイヤー 等）")]
        public AbilityData.GrenadeData GrenadeSettings;

        [Header("ノックバック設定（有効/確率/力/時間 等）")]
        public AbilityData.KnockbackData KnockbackSettings;

        [Header("スタン付与設定（有効/確率/時間 等）")]
        public AbilityData.StunnedData StunnedSettings;

        [Header("ダメージ設定（基礎ダメージ/DoT/クリティカル 等）")]
        public AbilityData.DamageData DamageSettings;

        /// <summary>
        /// アビリティの実行処理：グレネードを生成して初期化します。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            SpawnProjectiles(Owner, AttackTransform);
        }

        /// <summary>
        /// グレネードを生成し、ターゲットへ向けて初期化します。
        /// </summary>
        void SpawnProjectiles(GameObject Owner, Transform AttackTransform)
        {
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);

            Vector3 SpawnPosition = AttackTransform.position;
            GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(GrenadeSettings.GrenadeObject, SpawnPosition, GrenadeSettings.GrenadeObject.transform.rotation);
            SpawnedProjectile.transform.localScale = GrenadeSettings.GrenadeObject.transform.localScale;
            SpawnedProjectile.name = GrenadeSettings.GrenadeObject.name;

            AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);
        }

        /// <summary>
        /// 新しく生成したグレネードに Grenade スクリプトを割り当てます。
        /// </summary>
        public Grenade AssignScript(GameObject SpawnedProjectile)
        {
            var grenade = SpawnedProjectile.GetComponent<Grenade>();
            if (grenade == null) grenade = SpawnedProjectile.AddComponent<Grenade>();
            grenade.enabled = true;
            return grenade;
        }
    }
}
