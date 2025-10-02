using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    /// <summary>
    /// 【AreaOfEffectAbility】
    /// 範囲攻撃（Area of Effect, AoE）アビリティの定義用 ScriptableObject。
    /// ・チャージ演出／生成演出
    /// ・AoE半径・高さ・遅延などのパラメータ
    /// ・ノックバック／スタン／ダメージ（DoT含む）
    /// をまとめて管理します。
    /// </summary>
    [CreateAssetMenu(fileName = "範囲攻撃アビリティ", menuName = "Emerald AI/アビリティ/範囲攻撃アビリティ")]
    public class AreaOfEffectAbility : EmeraldAbilityObject
    {
        [Header("チャージ時の設定（エフェクト等）")]
        public AbilityData.ChargeSettingsData ChargeSettings;

        [Header("生成直前の設定（エフェクト等）")]
        public AbilityData.CreateSettingsData CreateSettings;

        [Header("AoE の基本設定（半径/高さ/ディレイ/エフェクト等）")]
        public AbilityData.AreaOfEffectData AreaOfEffectSettings;

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
        /// アビリティの実行処理：ターゲット取得、生成エフェクト再生、AoEの開始。
        /// </summary>
        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(StartAOE(Owner, AttackTransform));
        }

        /// <summary>
        /// AoE の発動（一定の遅延後に AoE エフェクトを生成し、AreaOfEffect を初期化）。
        /// </summary>
        IEnumerator StartAOE(GameObject Owner, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(AreaOfEffectSettings.Delay);

            Vector3 SpawnPosition = new Vector3(AttackTransform.position.x, Owner.transform.position.y, AttackTransform.position.z) + Vector3.up * AreaOfEffectSettings.HeightOffset;
            GameObject SpawnedAbility = AreaOfEffectSettings.SpawnAOEEffect(Owner, SpawnPosition);
            AssignScript(SpawnedAbility).Initialize(Owner, AttackTransform, this);
        }

        /// <summary>
        /// 生成した AoE エフェクトに AreaOfEffect スクリプトを割り当てます。
        /// </summary>
        public AreaOfEffect AssignScript(GameObject SpawnedAbility)
        {
            var areaOfEffect = SpawnedAbility.GetComponent<AreaOfEffect>();
            if (areaOfEffect == null) areaOfEffect = SpawnedAbility.AddComponent<AreaOfEffect>();
            return areaOfEffect;
        }
    }
}
